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

    [Header("Backgrounds")]
    public Sprite BG_Neighbourhood;
    public Sprite BG_City;

    public Sprite GetLocationSprite(Location location)
    {
        switch (location)
        {
            case Location.Suburbs: return BG_Neighbourhood;
            case Location.City: return BG_City;

            default: throw new System.Exception("No background found for location " + location.ToString());
        }
    }

    public static ResourceManager Singleton { get { return GameObject.Find("ResourceManager").GetComponent<ResourceManager>(); } }
}
