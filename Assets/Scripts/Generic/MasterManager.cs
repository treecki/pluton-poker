using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

[CreateAssetMenu(menuName = "Singleton/MasterManager")]
public class MasterManager : SingletonScriptableObject<MasterManager>
{
    [SerializeField]
    private GameSettings gameSettings;
    public static GameSettings GameSettings { get { return Instance.gameSettings; } }

    [SerializeField]
    private List<NetworkedPrefab> networkedPrefabs = new List<NetworkedPrefab>();

    public static GameObject NetworkInstantiate(GameObject obj, Vector3 pos, Quaternion rot)
    {
        foreach(NetworkedPrefab networkedPrefab in Instance.networkedPrefabs)
        {
            if (networkedPrefab.Prefab == obj)
            {
                if (networkedPrefab.Path != string.Empty)
                {
                    GameObject result = PhotonNetwork.Instantiate(networkedPrefab.Path, pos, rot);
                    return result;
                }
                else
                {
                    Debug.LogError("Path is empty for gameobject name " + networkedPrefab.Prefab);
                    return null;
                }

            }
        }

        return null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void PopulateNetworkedPrefabs()
    {

#if UNITY_EDITOR
        Instance.networkedPrefabs.Clear();

        GameObject[] results = Resources.LoadAll<GameObject>("");
        for (int i = 0; i < results.Length; i++)
        {
            if (results[i].GetComponent<PhotonView>() != null)
            {
               string path = AssetDatabase.GetAssetPath(results[i]);
                Instance.networkedPrefabs.Add(new NetworkedPrefab(results[i], path));
            }
        }
#endif
    }
}
