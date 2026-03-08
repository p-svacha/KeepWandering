using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerCharacterRenderer : MonoBehaviour
{
    public static PlayerCharacterRenderer Instance;
    public PlayerCharacter Character => Game.Instance.Player;

    [Header("Sprites")]
    public GameObject Head;

    public GameObject Torso;
    public GameObject DehydrationOverlay;
    public LimbRenderer LegFront;
    public LimbRenderer LegBack;
    public LimbRenderer RightArm;

    [Header("Wounds")]
    public GameObject BruiseWoundsContainer;
    public GameObject CutWoundsContainer;
    private Dictionary<HealthConditionDef, List<WoundRenderer>> WoundRenderers;

    private void Awake()
    {
        Instance = this;
    }

    public void Init()
    {
        WoundRenderers = new Dictionary<HealthConditionDef, List<WoundRenderer>>();
        InitWoundRenderers(HealthConditionDefOf.Bruise, BruiseWoundsContainer);
        InitWoundRenderers(HealthConditionDefOf.Cut, CutWoundsContainer);
    }

    private void InitWoundRenderers(HealthConditionDef woundDef, GameObject container)
    {
        List<WoundRenderer> renderers = container.GetComponentsInChildren<WoundRenderer>(true).ToList();
        WoundRenderers.Add(woundDef, renderers);
    }

    /// <summary>
    /// Sets all children of the given object to inactive except for the child with the given index, which is set to active.
    /// If index is negative, all children are set to inactive.
    /// </summary>
    public void SetActiveSprite(GameObject obj, int index)
    {
        for (int i = 0; i < obj.transform.childCount; i++)
        {
            obj.transform.GetChild(i).gameObject.SetActive(i == index);
        }
    }

    public WoundRenderer GetUnusedWoundRenderer(HealthConditionDef woundDef)
    {
        return WoundRenderers[woundDef].Where(wr => wr.Wound == null).ToList().RandomElement();
    }


    public void SetCharacterColor(Color c)
    {
        for (int i = 0; i < Torso.transform.childCount; i++)
        {
            Torso.transform.GetChild(i).GetComponent<SpriteRenderer>().color = c;
        }
        Head.GetComponent<SpriteRenderer>().color = c;
    }
}
