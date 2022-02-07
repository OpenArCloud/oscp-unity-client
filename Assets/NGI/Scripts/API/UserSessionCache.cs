using UnityEngine;

// Use this class to store user session data to use for refreshing tokens
[System.Serializable]
public class UserSessionCache : ISaveable
{
    [SerializeField] private string _idToken;
    [SerializeField] private string _accessToken;
    [SerializeField] private string _refreshToken;

    public UserSessionCache() { }

    public UserSessionCache(string idToken, string accessToken, string refreshToken)
    {
        _idToken = idToken;
        _accessToken = accessToken;
        _refreshToken = refreshToken;
    }

    public string getIdToken()
    {
        return _idToken;
    }

    public string getAccessToken()
    {
        return _accessToken;
    }

    public string getRefreshToken()
    {
        return _refreshToken;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public void LoadFromJson(string jsonToLoadFrom)
    {
        JsonUtility.FromJsonOverwrite(jsonToLoadFrom, this);
    }

    public string FileNameToUseForData()
    {
        return "data_01.dat";
    }
}
