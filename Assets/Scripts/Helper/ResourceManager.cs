using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

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
    public GameObject E007_Trader;
    public List<TextMeshPro> E007_Prices;
    public GameObject E008_DistressedPerson;

    [Header("Backgrounds")]
    public Location LOC_Suburbs;
    public Location LOC_City;
    public Location LOC_Woods;
    public Location LOC_GroceryStore;

    [Header("Companions")]
    public Dog Dog;
    public Parrot Parrot;

    [Header("Colors")]
    public Color SE_Neutral;
    public Color SE_Good;
    public Color SE_VeryGood;
    public Color SE_ExtremelyGood;
    public Color SE_Bad;
    public Color SE_VeryBad;
    public Color SE_ExtremelyBad;


    [Header("World Map")]
    public Texture2D SuburbsTexture;
    public Texture2D WoodsTexture;
    public Texture2D CityTexture;

    private Dictionary<LocationType, TileBase> LocationTiles;


    void Awake()
    {
        LocationTiles = new Dictionary<LocationType, TileBase>();
        LocationTiles.Add(LocationType.Suburbs, TileGenerator.CreateTileFromTexture(SuburbsTexture));
        LocationTiles.Add(LocationType.Woods, TileGenerator.CreateTileFromTexture(WoodsTexture));
        LocationTiles.Add(LocationType.City, TileGenerator.CreateTileFromTexture(CityTexture));
        Singleton = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
    }

    public TileBase GetLocationTile(LocationType loc)
    {
        return LocationTiles[loc];
    }

    public static ResourceManager Singleton;
}
