using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_StatPanel : MonoBehaviour
{
    private const int STATS_PER_ROW = 2;

    [Header("Elements")]
    public GameObject StatContainer;

    [Header("Prefabs")]
    public UI_StatRow RowPrefab;

    private Dictionary<StatDef, UI_Stat> StatDisplays;

    public void Init(Game game)
    {
        HelperFunctions.DestroyAllChildredImmediately(StatContainer, skipElements: 1);

        StatDisplays = new Dictionary<StatDef, UI_Stat>();
        UI_StatRow currentRow = null;
        List<Stat> playerStats = game.PlayerStats.Values.ToList();
        for (int i = 0; i < playerStats.Count; i += STATS_PER_ROW)
        {
            if (i % STATS_PER_ROW == 0)
            {
                currentRow = Instantiate(RowPrefab, StatContainer.transform);
                Stat rowStat1 = playerStats[i];
                Stat rowStat2 = (i + 1 < playerStats.Count) ? playerStats[i + 1] : null;
                currentRow.Init(rowStat1, rowStat2);
            }
        }
    }

    public void Refresh()
    {
        foreach (UI_Stat stat in StatDisplays.Values) stat.Refresh();
    }

    public void HightlightStat(StatDef stat)
    {
        StatDisplays[stat].Highlight();
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
