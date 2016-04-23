using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityTiled
{
    /*
    * This component scans all tiles in the map looking for those with a Collider property set to true.
    * It then reads the CollisionSides property from the tile to know which sides should generate collision
    * edges. Islands are built from segments that are connected and then edge colliders are created for
    * each of those islands.
    */
    public class MapCollisionGenerator : MonoBehaviour
    {    
#if UNITY_EDITOR
        [Flags]
        private enum CollisionSides
        {
            None = 0,
            Top = 1,
            Left = 2,
            Right = 4,
            Bottom = 8    
        }
        
        private class CollisionTile
        {
            public Transform transform;
            public CollisionSides collisionSides;
        }
        
        private struct Point
        {
            public int x;
            public int y;
            
            public Point(Vector2 v) : this()
            {
                x = Mathf.RoundToInt(v.x);
                y = Mathf.RoundToInt(v.y);
            }
        }
        
        private class PointCache
        {
            private List<Point> _points = new List<Point>();
            
            public int AddPoint(Vector2 v)
            {
                var p = new Point(v);
                var index = _points.IndexOf(p);
                if (index >= 0)
                    return index;
                _points.Add(p);
                return _points.Count - 1;
            }
            
            public Point GetPoint(int index)
            {
                return _points[index];
            }
        }
        
        void OnTmxMapImported()
        {
            try
            {
                const int stepsPerLayer = 4;
                
                // Iterate all children and generate colliders per layer
                for (int layerIndex = 0; layerIndex < transform.childCount; layerIndex++)
                {
                    var layer = transform.GetChild(layerIndex);
                    
                    // Find all the collision tiles in the map
                    EditorUtility.DisplayProgressBar(
                        "Map Collision Generator", 
                        "Looking for collision tiles in layer " + layer.name, 
                        (float)(layerIndex * stepsPerLayer + 0) / (float)(transform.childCount * stepsPerLayer));
                    var colliderTiles = layer.gameObject.GetComponentsInChildren<TileProperties>()
                                            .Where(p => p.ContainsProperty("CollisionSides"))
                                            .Select(p => new CollisionTile { transform = p.transform, collisionSides = p.GetEnum<CollisionSides>("CollisionSides") });
                                            
                    if (!colliderTiles.Any())
                    {
                        Debug.LogFormat("No collision tiles found on layer {0}", layer.name);
                        continue;
                    }
                        
                    // Build up the list of all segments that need to be created into one big list
                    EditorUtility.DisplayProgressBar(
                        "Map Collision Generator", 
                        "Creating line segments for layer " + layer.name, 
                        (float)(layerIndex * stepsPerLayer + 1) / (float)(transform.childCount * stepsPerLayer));
                    var pointCache = new PointCache();
                    var segments = new Queue<int[]>();
                    foreach (var tile in colliderTiles)
                    {
                        var topLeftVector = (Vector2)tile.transform.localPosition + new Vector2(-0.5f, 0.5f);
                        var topLeft = pointCache.AddPoint(topLeftVector);
                        var topRight = pointCache.AddPoint(topLeftVector + new Vector2(1, 0));
                        var bottomRight = pointCache.AddPoint(topLeftVector + new Vector2(1, -1));
                        var bottomLeft = pointCache.AddPoint(topLeftVector + new Vector2(0, -1));

                        if ((tile.collisionSides & CollisionSides.Top) == CollisionSides.Top)
                            segments.Enqueue(new[] { topLeft, topRight });
                        if ((tile.collisionSides & CollisionSides.Right) == CollisionSides.Right)
                            segments.Enqueue(new[] { topRight, bottomRight });
                        if ((tile.collisionSides & CollisionSides.Bottom) == CollisionSides.Bottom)
                            segments.Enqueue(new[] { bottomRight, bottomLeft });
                        if ((tile.collisionSides & CollisionSides.Left) == CollisionSides.Left)
                            segments.Enqueue(new[] { bottomLeft, topLeft });
                    }
                
                    // Combine segments into line lists based on vertices that touch
                    EditorUtility.DisplayProgressBar(
                        "Map Collision Generator", 
                        "Combining line segments for layer " + layer.name, 
                        (float)(layerIndex * stepsPerLayer + 2) / (float)(transform.childCount * stepsPerLayer));
                    var lineLists = new List<List<int>>();
                    while (segments.Count > 0)
                    {
                        var segment = segments.Dequeue();
                        bool foundList = false;
                        
                        foreach (var list in lineLists)
                        {
                            if (list.Last() == segment[0])
                            {
                                foundList = true;
                                list.Add(segment[1]);
                                break;
                            }
                            else if (list.First() == segment[1])
                            {
                                foundList = true;
                                list.Insert(0, segment[0]);
                                break;
                            }
                        }
                        
                        if (!foundList)
                        {
                            var list = new List<int>();
                            list.AddRange(segment);
                            lineLists.Add(list);
                        }
                    }
                    
                    // Try to combine our lists to reduce the collider count
                    for (int listIndex1 = 0; listIndex1 < lineLists.Count; listIndex1++)
                    {
                        var list1 = lineLists[listIndex1];
                        
                        for (int listIndex2 = listIndex1 + 1; listIndex2 < lineLists.Count; listIndex2++)
                        {
                            var list2 = lineLists[listIndex2];
                            
                            if (list1.Last() == list2.First())
                            {
                                list1.AddRange(list2.Skip(1));
                                lineLists.RemoveAt(listIndex2);
                                listIndex2--;
                            }
                            else if (list1.First() == list2.Last())
                            {
                                list2.AddRange(list1.Skip(1));
                                lineLists.RemoveAt(listIndex1);
                                listIndex1--;
                                break;
                            }
                        }
                    }

                    // Take each line list and build an edge collider around it
                    EditorUtility.DisplayProgressBar(
                        "Map Collision Generator", 
                        "Creating edge collider for layer " + layer.name, 
                        (float)(layerIndex * stepsPerLayer + 3) / (float)(transform.childCount * stepsPerLayer));
                    foreach (var lineList in lineLists)
                    {
                        // First we optimize away unnecessary vertices between segments that are continuous, e.g.
                        // if we have two tiles with bottom collisions touching, we remove the middle point so we
                        // have one segment span both tiles, rather than two separate segments.
                        if (lineList.Count >= 3)
                        {
                            for (int i = 1; i < lineList.Count - 1; i++)
                            {
                                var a = pointCache.GetPoint(lineList[i - 1]);
                                var b = pointCache.GetPoint(lineList[i]);
                                var c = pointCache.GetPoint(lineList[i + 1]);

                                if ((a.x == b.x && b.x == c.x) || (a.y == b.y && b.y == c.y))
                                {
                                    lineList.RemoveAt(i);
                                    i--;
                                }
                            }
                        }

                        // Use the line list to create an edge collider
                        var edgeCollider = layer.gameObject.AddComponent<EdgeCollider2D>();
                        edgeCollider.points = lineList.Select(i => pointCache.GetPoint(i)).Select(p => new Vector2(p.x, p.y)).ToArray();
                    }
                }
            }
            finally
            {               
                EditorUtility.ClearProgressBar();
            }
        }
#endif // UNITY_EDITOR
    }
}