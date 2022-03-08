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


#if false // Decompilation log
'254' items in cache
------------------
Resolve: 'netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Found single assembly: 'netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\NetStandard\ref\2.0.0\netstandard.dll'
------------------
Resolve: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.CoreModule.dll'
------------------
Resolve: 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.AnimationModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.AnimationModule.dll'
------------------
Resolve: 'glTFastSchema, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'glTFastSchema, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\Projects\3DInteractive\oscp-unity-client\Library\ScriptAssemblies\glTFastSchema.dll'
------------------
Resolve: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Mathematics, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\Projects\3DInteractive\oscp-unity-client\Library\ScriptAssemblies\Unity.Mathematics.dll'
------------------
Resolve: 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.PhysicsModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.PhysicsModule.dll'
------------------
Resolve: 'Ktx, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Ktx, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\Projects\3DInteractive\oscp-unity-client\Library\ScriptAssemblies\Ktx.dll'
------------------
Resolve: 'glTFastFakeSchema, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'glTFastFakeSchema, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\Projects\3DInteractive\oscp-unity-client\Library\ScriptAssemblies\glTFastFakeSchema.dll'
------------------
Resolve: 'Draco, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Draco, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\Projects\3DInteractive\oscp-unity-client\Library\ScriptAssemblies\Draco.dll'
------------------
Resolve: 'Unity.Burst, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'Unity.Burst, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\Projects\3DInteractive\oscp-unity-client\Library\ScriptAssemblies\Unity.Burst.dll'
------------------
Resolve: 'UnityEngine.UnityWebRequestModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.UnityWebRequestModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.UnityWebRequestModule.dll'
------------------
Resolve: 'Unity.Meshopt.Decompress, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Could not find by name: 'Unity.Meshopt.Decompress, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
------------------
Resolve: 'UnityEngine.JSONSerializeModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.JSONSerializeModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.JSONSerializeModule.dll'
------------------
Resolve: 'UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEditor.CoreModule.dll'
------------------
Resolve: 'UnityEngine.UnityWebRequestTextureModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.UnityWebRequestTextureModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.UnityWebRequestTextureModule.dll'
------------------
Resolve: 'UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'UnityEngine.ImageConversionModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\Managed\UnityEngine\UnityEngine.ImageConversionModule.dll'
------------------
Resolve: 'mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
WARN: Version mismatch. Expected: '2.0.0.0', Got: '4.0.0.0'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\NetStandard\compat\2.0.0\shims\netfx\mscorlib.dll'
------------------
Resolve: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Found single assembly: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Load from: 'E:\GameEngines\2021.1.28f1\Editor\Data\NetStandard\compat\2.0.0\shims\netfx\mscorlib.dll'
------------------
Resolve: 'Microsoft.Win32.Registry, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'Microsoft.Win32.Registry, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.Security.Principal.Windows, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Security.Principal.Windows, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
------------------
Resolve: 'System.Security.AccessControl, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Could not find by name: 'System.Security.AccessControl, Version=4.1.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
#endif

