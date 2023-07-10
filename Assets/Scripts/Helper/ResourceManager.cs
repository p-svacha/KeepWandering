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

    public GameObject E009_Shelter;
    public GameObject E009_TrapOpen;
    public GameObject E009_TrapClosed;
    public GameObject E009_WindowBlood;

    public GameObject E010_FenceForeground;
    public GameObject E010_FenceBackground;

    [Header("Backgrounds")]
    public SpriteRenderer FarmlandBackground;
    public SpriteRenderer CityBackground;
    public SpriteRenderer WoodsBackground;
    public SpriteRenderer GroceryStoreBackground;

    [Header("Characters")]
    public GameObject PlayerCharacter;
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
    public Texture2D FarmlandTexture;
    public Texture2D WoodsTexture;
    public Texture2D CityTexture;
    public Texture2D LakeTexture;

    public TileBase WhiteTile;
    public TileBase TileMarkerTransparentWhite;
    public TileBase TileMarkerGreen;
    public TileBase TileMarkerBlue;
    public TileBase TileMarkerRed;

    public Material PathHistoryMaterial;
    public Material QuarantineZoneBorderMaterial;

    void Awake()
    {
        Singleton = GameObject.Find("ResourceManager").GetComponent<ResourceManager>();
    }

    public static ResourceManager Singleton;
}
