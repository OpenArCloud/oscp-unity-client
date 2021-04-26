#if UNITY_EDITOR
using UnityEditor;
#endif

public class ABBuilder
{
	#if UNITY_EDITOR

    [MenuItem("Assets/Build AB/Windows")]
    static void BuildAllAssetBundlesWindows()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Windows", BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
    [MenuItem("Assets/Build AB/Android")]
    static void BuildAllAssetBundlesAndroid()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles/Android", BuildAssetBundleOptions.None, BuildTarget.Android);
    }
    [MenuItem("Assets/Build AB/IOS")]
    static void BuildAllAssetBundlesIOS()
    {
        BuildPipeline.BuildAssetBundles("Assets/AssetBundles/iOS", BuildAssetBundleOptions.None, BuildTarget.iOS);
    }
	#endif
}
