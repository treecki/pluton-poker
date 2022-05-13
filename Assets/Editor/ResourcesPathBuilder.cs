#if UNITY_EDITOR
using UnityEngine;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class ResourcesPathBuilder : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        MasterManager.PopulateNetworkedPrefabs();
    }
}
#endif
