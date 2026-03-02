using System.Collections.Generic;
using UnityEngine;

public class UI_StatPanel : MonoBehaviour
{
    private const int STATS_PER_ROW = 2;

    [Header("Elements")]
    public GameObject StatContainer;

    [Header("Prefabs")]
    public GameObject RowPrefab;
    public UI_Stat StatPrefab;

    private Dictionary<StatDef, UI_Stat> StatDisplays;

    public void Init(Game game)
    {
        StatDisplays = new Dictionary<StatDef, UI_Stat>();
        GameObject currentRow = null;
        int statIndex = 0;
        foreach (Stat stat in game.PlayerStats.Values)
        {
            if (statIndex % STATS_PER_ROW == 0)
            {
                currentRow = Instantiate(RowPrefab, StatContainer.transform);
            }

            var statUI = Instantiate(StatPrefab, currentRow.transform);
            statUI.Init(stat);
            StatDisplays[stat.Def] = statUI;
            statIndex++;
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
