
// Used to save scripts to json that implement ISaveable
// based on https://github.com/UnityTechnologies/UniteNow20-Persistent-Data
using UnityEngine;

public static class SaveDataManager
{
    public static void SaveJsonData(ISaveable saveable)
    {
        if (FileManager.WriteToFile(saveable.FileNameToUseForData(), saveable.ToJson()))
        {
            Debug.Log("Save successful");
        }
    }

    public static void LoadJsonData(ISaveable saveable)
    {
        if (FileManager.LoadFromFile(saveable.FileNameToUseForData(), out var json))
        {
            saveable.LoadFromJson(json);
            Debug.Log("Load complete");
        }
    }
}

public interface ISaveable
{
    string ToJson();
    void LoadFromJson(string a_Json);
    string FileNameToUseForData();
}