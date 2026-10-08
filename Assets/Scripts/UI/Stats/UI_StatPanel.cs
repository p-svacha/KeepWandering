using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_StatPanel : MonoBehaviour
{
    private const int STATS_PER_ROW = 2;

    [Header("Elements")]
    public UI_Stat Morale;
    public UI_Stat Strength;
    public UI_Stat Survival;
    public UI_Stat Dexterity;
    public UI_Stat Social;

    [Header("Prefabs")]
    private Dictionary<StatDef, UI_Stat> StatDisplays;

    public void Init(Game game)
    {
        StatDisplays = new Dictionary<StatDef, UI_Stat>();

        // Stats
        Morale.Init(game.Player.Stats[StatDefOf.Morale], fixedColor: true);
        Strength.Init(game.Player.Stats[StatDefOf.Strength]);
        Survival.Init(game.Player.Stats[StatDefOf.Survival]);
        Dexterity.Init(game.Player.Stats[StatDefOf.Dexterity]);
        Social.Init(game.Player.Stats[StatDefOf.Social]);

        // Cache
        StatDisplays.Add(StatDefOf.Morale, Morale);
        StatDisplays.Add(StatDefOf.Strength, Strength);
        StatDisplays.Add(StatDefOf.Survival, Survival);
        StatDisplays.Add(StatDefOf.Dexterity, Dexterity);
        StatDisplays.Add(StatDefOf.Social, Social);
    }

    public void Refresh()
    {
        foreach (UI_Stat stat in StatDisplays.Values) stat.Refresh();
    }

    public void HightlightStat(StatDef stat, Color color)
    {
        StatDisplays[stat].Highlight(color);
    }
    public void UnhighlightStat(StatDef stat)
    {
        StatDisplays[stat].Unhighlight();
    }

    public void UnhighlightAll()
    {
        foreach (UI_Stat stat in StatDisplays.Values) stat.Unhighlight();
    }
}
