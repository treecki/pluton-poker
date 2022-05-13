using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiateExample : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;

    private void Awake()
    {
        MasterManager.NetworkInstantiate(prefab, transform.position, transform.rotation);
    }
}
