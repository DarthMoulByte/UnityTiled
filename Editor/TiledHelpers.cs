using UnityEngine;
using System.IO;

public static class TiledHelpers
{
    public static string GetAssetPath(string file)
    {
		// Ensure we have a full path to the file
        file = Path.GetFullPath(file);

		// Replace all backslashes with forward slashes in both the file and the data path
		file = file.Replace("\\", "/");
		var dataPath = Application.dataPath.Replace ("\\", "/");

		// Remove the data path from the start of the file path
		file = file.Replace(dataPath, "");

		// If the file doesn't have a slash, add one
		if (!file.StartsWith ("/"))
			file = "/" + file;
       
		// Then put Assets at the front
		return "Assets" + file;
    }
}