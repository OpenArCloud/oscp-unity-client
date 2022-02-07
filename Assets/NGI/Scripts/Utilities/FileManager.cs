using System;
using System.IO;
using UnityEngine;

// Saves a file to the operating system's persistent data path
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data
public static class FileManager
{
   public static bool WriteToFile(string a_FileName, string a_FileContents)
   {
      var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);
      // Debug.Log(fullPath);

      try
      {
         File.WriteAllText(fullPath, a_FileContents);
         return true;
      }
      catch (Exception e)
      {
         Debug.LogError($"Failed to write to {fullPath} with exception {e}");
         return false;
      }
   }

   public static bool LoadFromFile(string a_FileName, out string result)
   {
      var fullPath = Path.Combine(Application.persistentDataPath, a_FileName);

      try
      {
         result = File.ReadAllText(fullPath);
         return true;
      }
      catch (Exception e)
      {
         Debug.LogWarning($"Failed to read from {fullPath}, file should be created after a successful login, can probably ignore. Exception {e}.");
         result = "";
         return false;
      }
   }
}