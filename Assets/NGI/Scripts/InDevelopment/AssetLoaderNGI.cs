using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AssetLoaderNGI : MonoBehaviour
{

    string AssetName;
    public string BundleFullURL;
    public string ABName;
    public Preloader preloader;
    ModelManager modelManager;
    Mover mover;

    //Temporary variable for using external file service, needs a better solution
    //TODO: Fix a custom Url for both iOS and Android bundles
    public string customUrl;

    void Start()
    {
        AssetName = ABName;//"obj";
        GameObject man = GameObject.FindGameObjectWithTag("Manager");
        modelManager = man.GetComponent<ModelManager>();
        mover = GetComponent<Mover>();
        StartLoadAB();
    }

    public void StartLoadAB()
    {
        if (modelManager.loadingBunles.Contains(ABName))
        {
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

    public void StopLoad()
    {
        StopCoroutine(LoadAsset());
    }

    IEnumerator LoadAsset()
    {
        Caching.ClearCache();
        modelManager.loadingBunles.Add(ABName);

        //TODO: Fix an URL scheme that works for both android and iOS assetbundels, so the app downloads the correct one
#if UNITY_IOS
        customUrl = customUrl + ".ios";
#endif
#if PLATFORM_ANDROID
        customUrl = customUrl + ".android";
#endif

        Debug.Log("Load Bundle Path = " + customUrl);
        if (string.IsNullOrEmpty(customUrl))
        {
            yield break;
        }

        CachedAssetBundle cab = new CachedAssetBundle(ABName, new Hash128(0, 0));
        using (UnityWebRequest uwr = UnityWebRequestAssetBundle.GetAssetBundle(customUrl, cab))
        {
            preloader.LoadPercent(uwr);
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.ConnectionError || uwr.result == UnityWebRequest.Result.ProtocolError)
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

                string[] names = bundle.GetAllAssetNames();

                foreach (var item in names)
                {
                    Debug.Log("Name of asset: " + item);
                }

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
