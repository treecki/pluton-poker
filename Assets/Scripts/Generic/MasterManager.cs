using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Singleton/MasterManager")]
public class MasterManager : SingletonScriptableObject<MasterManager>
{

    [SerializeField]
    [Tooltip("")]
    private GameSettings gameSettings;
    public static GameSettings GameSettings { get { return Instance.gameSettings; } }
}
