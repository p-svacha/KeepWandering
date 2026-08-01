using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_StatPanel : MonoBehaviour
{
    private const int STATS_PER_ROW = 2;

    [Header("Elements")]
    public UI_Stat Morale;
    public GameObject StatContainer;

    [Header("Prefabs")]
    public UI_StatRow RowPrefab;

    private Dictionary<StatDef, UI_Stat> StatDisplays;

    public void Init(Game game)
    {
        StatDisplays = new Dictionary<StatDef, UI_Stat>();

        // Morale
        Morale.Init(game.Player.Stats[StatDefOf.Morale], fixedColor: true);
        StatDisplays.Add(StatDefOf.Morale, Morale);

        // Skills
        HelperFunctions.DestroyAllChildredImmediately(StatContainer, skipElements: 1);

        
        UI_StatRow currentRow = null;
        List<Stat> playerStats = game.Player.Stats.Values.Where(stat => stat.Def != StatDefOf.Morale).ToList();
        for (int i = 0; i < playerStats.Count; i += STATS_PER_ROW)
        {
            if (i % STATS_PER_ROW == 0)
            {
                currentRow = Instantiate(RowPrefab, StatContainer.transform);
                Stat rowStat1 = playerStats[i];
                Stat rowStat2 = (i + 1 < playerStats.Count) ? playerStats[i + 1] : null;
                currentRow.Init(rowStat1, rowStat2);

                StatDisplays.Add(rowStat1.Def, currentRow.LeftStat);
                if (rowStat2 != null) StatDisplays.Add(rowStat2.Def, currentRow.RightStat);
            }
        }
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
