using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System;

namespace TiledUtilities
{
    /*
    * This component scans all tiles in the map looking for those with a Collider property set to true.
    * It then reads the CollisionSides property from the tile to know which sides should generate collision
    * edges. Islands are built from segments that are connected and then edge colliders are created for
    * each of those islands.
    */
    public class MapCollisionGenerator : MonoBehaviour
    {    
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
        
        void OnTmxMapImported()
        {
            // Find all the collision tiles in the map
            var colliderTiles = GetComponentsInChildren<TileProperties>()
                                    .Where(p => p.ContainsProperty("Collider") && p.GetBool("Collider"))
                                    .Select(p => new CollisionTile { transform = p.transform, collisionSides = p.GetEnum<CollisionSides>("CollisionSides") });
                                    
            if (!colliderTiles.Any())
                return;
                
            // Going to build up a set of edge colliders based on our collision layer
            var collisionRoot = new GameObject("_GeneratedCollision");
            collisionRoot.isStatic = true;
            collisionRoot.transform.SetParent(transform);

            // Build up the list of all segments that need to be created into one big list
            var segments = new List<Point[]>();
            foreach (var tile in colliderTiles)
            {
                var pos = (Vector2)tile.transform.localPosition;
                var topLeft = new Point(pos);
                var topRight = new Point(pos + new Vector2(1, 0));
                var bottomRight = new Point(pos + new Vector2(1, -1));
                var bottomLeft = new Point(pos + new Vector2(0, -1));

                if ((tile.collisionSides & CollisionSides.Top) == CollisionSides.Top)
                    segments.Add(new[] { topLeft, topRight });
                if ((tile.collisionSides & CollisionSides.Right) == CollisionSides.Right)
                    segments.Add(new[] { topRight, bottomRight });
                if ((tile.collisionSides & CollisionSides.Bottom) == CollisionSides.Bottom)
                    segments.Add(new[] { bottomRight, bottomLeft });
                if ((tile.collisionSides & CollisionSides.Left) == CollisionSides.Left)
                    segments.Add(new[] { bottomLeft, topLeft });
            }
        
            // Combine segments into line lists based on vertices that touch
            var lineLists = new List<List<Point>>();
            var currentList = new List<Point>(segments[0]);
            lineLists.Add(currentList);
            segments.RemoveAt(0);
            do
            {
                var nextSegment = segments.FirstOrDefault(s => s[0].Equals(currentList.Last())); 
                if (nextSegment != null)
                {
                    currentList.Add(nextSegment[1]);
                    segments.Remove(nextSegment);
                }
                else
                {
                    currentList = new List<Point>(segments[0]);
                    lineLists.Add(currentList);
                    segments.RemoveAt(0);
                }
            } while (segments.Count > 0);

            // Take each line list and build an edge collider around it
            foreach (var lineList in lineLists)
            {
                // First we optimize away unnecessary vertices between segments that are continuous, e.g.
                // if we have two tiles with bottom collisions touching, we remove the middle point so we
                // have one segment span both tiles, rather than two separate segments.
                if (lineList.Count >= 3)
                {
                    for (int i = 1; i < lineList.Count - 1; i++)
                    {
                        var a = lineList[i - 1];
                        var b = lineList[i];
                        var c = lineList[i + 1];

                        if ((a.x == b.x && b.x == c.x) || (a.y == b.y && b.y == c.y))
                        {
                            lineList.RemoveAt(i);
                            i--;
                        }
                    }
                }

                // Use the line list to create an edge collider
                var colliderObj = new GameObject("Collider");
                colliderObj.isStatic = true;
                colliderObj.transform.SetParent(collisionRoot.transform);
                var edgeCollider = colliderObj.AddComponent<EdgeCollider2D>();
                edgeCollider.points = lineList.Select(p => new Vector2(p.x, p.y)).ToArray();
            }
        }
    }
}