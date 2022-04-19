using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Events")]
    public GameObject E001_Crate;
    public GameObject E002_Dog;
    public GameObject E003_EvilGuy;
    public GameObject E003_EvilGuy_KO;
    public GameObject E004_Woman;
    public GameObject E004_Parrot;
    public GameObject E006_WoodsBunker;

    [Header("Backgrounds")]
    public Location LOC_Suburbs;
    public Location LOC_City;
    public Location LOC_Woods;
    public Location LOC_GroceryStore;

    [Header("Companions")]
    public Dog Dog;
    public Parrot Parrot;

    [Header("Colors")]
    public Color SE_Good;
    public Color SE_VeryGood;
    public Color SE_ExtremelyGood;
    public Color SE_Bad;
    public Color SE_VeryBad;
    public Color SE_ExtremelyBad;

    public static ResourceManager Singleton { get { return GameObject.Find("ResourceManager").GetComponent<ResourceManager>(); } }
}
