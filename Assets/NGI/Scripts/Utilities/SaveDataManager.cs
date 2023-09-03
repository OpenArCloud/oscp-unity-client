// Used to save scripts to json that implement ISaveable
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data
using UnityEngine;
using System;

public static class SaveDataManager
{
    public static bool SaveJsonData(ISaveable saveable)
    {
        string fullPath = saveable.FileNameToUseForData();
        try {
            if (FileManager.WriteToFile(fullPath, saveable.ToJson())) {
                return true;
            }
            return false;
        } catch (Exception e) {
            Debug.LogError($"Failed to write to {fullPath} with exception {e}");
            return false;
        }
    }

    public static bool LoadJsonData(ISaveable saveable)
    {
        string fullPath = saveable.FileNameToUseForData();
        try {
            if (FileManager.LoadFromFile(fullPath, out var json)) {
                saveable.LoadFromJson(json);
                return true;
            }
            return false;
        } catch (Exception e) {
            Debug.LogError($"Failed to read from {fullPath} with exception {e}");
            return false;
        }
    }
}

public interface ISaveable
{
    string ToJson();
    void LoadFromJson(string a_Json);
    string FileNameToUseForData();
}