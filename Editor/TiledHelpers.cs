using UnityEngine;
using System.IO;

public static class TiledHelpers
{
    public static string GetAssetPath(string file)
    {
        file = Path.GetFullPath(file);
        file = file.Replace(Application.dataPath, "");
        if (file.StartsWith("/") || file.StartsWith("\\"))
        {
            file = file.Substring(1);
        }
        file = Path.Combine("Assets", file);
        return file;
    }
}