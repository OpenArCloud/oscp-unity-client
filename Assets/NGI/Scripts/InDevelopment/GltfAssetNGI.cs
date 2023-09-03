using GLTFast;
using GLTFast.Loading;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GltfAssetNGI : GltfAssetBase
{
    [Tooltip("URL to load the glTF from.")]
    public string url;

    [Tooltip("Automatically load at start.")]
    public bool loadOnStartup = true;

    [Tooltip("Override scene to load (-1 loads glTFs default scene)")]
    public int sceneId = -1;

    [Tooltip("If checked, url is treated as relative StreamingAssets path.")]
    public bool streamingAsset = false;

    public GameObjectInstantiator.SceneInstance sceneInstance
    {
        get;
        protected set;
    }

    public string FullUrl => streamingAsset ? Path.Combine(Application.streamingAssetsPath, url) : url;

    protected virtual async void Start()
    {
        if (loadOnStartup && !string.IsNullOrEmpty(url))
        {
            await Load(FullUrl);
        }
    }

    public override async Task<bool> Load(string url, IDownloadProvider downloadProvider = null, IDeferAgent deferAgent = null, IMaterialGenerator materialGenerator = null, ICodeLogger logger = null)
    {
        logger = (logger ?? new ConsoleLogger());
        bool success = await base.Load(url, downloadProvider, deferAgent, materialGenerator, logger);
        if (success)
        {
            if (deferAgent != null)
            {
                await deferAgent.BreakPoint();
            }

            if (sceneId >= 0)
            {
                InstantiateScene(sceneId, logger);
            }
            else
            {
                Instantiate(logger);
            }
        }

        return success;
    }

    protected override IInstantiator GetDefaultInstantiator(ICodeLogger logger)
    {
        return new GameObjectInstantiator(importer, base.transform, logger);
    }

    protected override void PostInstantiation(IInstantiator instantiator, bool success)
    {
        sceneInstance = (instantiator as GameObjectInstantiator).sceneInstance;
        base.PostInstantiation(instantiator, success);
    }

    public override void ClearScenes()
    {
        foreach (Transform item in base.transform)
        {
            Object.Destroy(item.gameObject);
        }

        sceneInstance = null;
    }
}

public class GLBDownloader : IDownloadProvider
{
    public Task<IDownload> Request(System.Uri url)
    {
        throw new System.NotImplementedException();
    }

    public Task<ITextureDownload> RequestTexture(System.Uri url, bool nonReadable)
    {
        throw new System.NotImplementedException();
    }
}
