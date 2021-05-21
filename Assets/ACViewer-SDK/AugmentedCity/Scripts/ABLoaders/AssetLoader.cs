using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class AssetLoader : MonoBehaviour
{
    public string BundleFullURL;
    string AssetName;
    public Preloader preloader;
    public string ABName;
    ModelManager modelManager;
    Mover mover;

    void Start()
    {
        AssetName = "obj";
        GameObject man = GameObject.FindGameObjectWithTag("Manager");
        modelManager = man.GetComponent<ModelManager>();
        mover = GetComponent<Mover>();
        StartLoadAB();
    }

    public void StartLoadAB()
    {
        if (modelManager.loadingBunles.Contains(ABName)) {
            StartCoroutine(WaitToLoad());
        }
        else
        {
            bool bundleTaken = false;
            foreach (AssetBundle ab in modelManager.bundles)
            {
                Debug.Log("ab.name = " + ab.name);
                if (ab.name.Equals(ABName))
                {
                    Instantiate(ab.LoadAssetAsync(AssetName).asset, gameObject.transform);
                    Debug.Log("Loaded twice or MORE");
                    preloader.Loaded();
                    bundleTaken = true;
                }
            }
            if (!bundleTaken)
            {
                StartCoroutine(LoadAsset());
            }
        }
    }

    public void StopLoad() {
        StopCoroutine(LoadAsset());
    }

    IEnumerator LoadAsset()
    {
        modelManager.loadingBunles.Add(ABName);

#if UNITY_IOS
        BundleFullURL = PlayerPrefs.GetString("ApiUrl") + "/media/3d/" + ABName + "/ios/bundle";
#endif
#if PLATFORM_ANDROID
        BundleFullURL = PlayerPrefs.GetString("ApiUrl") + "/media/3d/" + ABName + "/android/bundle";
#endif
        Debug.Log("Load Bundle Path = " + BundleFullURL);

        CachedAssetBundle cab = new CachedAssetBundle(ABName, new Hash128(0,0));
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(BundleFullURL, cab))
        {
            preloader.LoadPercent(uwr);
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
                preloader.CantLoad();
                preloader.Loaded();
                GetComponent<Collider>().enabled = false;
                GetComponent<Mover>().enabled = false;
            }
            else
            {
                // Get downloaded asset bundle
                AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                if (bundle.Contains(AssetName))
                {
                    Instantiate(bundle.LoadAssetAsync(AssetName).asset, gameObject.transform);
                    modelManager.bundles.Add(bundle);
                    modelManager.loadingBunles.Remove(ABName);
                    mover.modelName = ABName;
                    Debug.Log("is OBJ");
                    preloader.Loaded();
                }
                else
                {
                    Debug.Log("Check !loaded Asset name: " + AssetName);
                    preloader.CantLoad();
                    preloader.Loaded();
                    GetComponent<Collider>().enabled = false;
                    GetComponent<Mover>().enabled = false;
                }
            }
        }
    }

    IEnumerator WaitToLoad()
    {
        yield return new WaitForSeconds(0.1f);
        StartLoadAB();
    }
}
