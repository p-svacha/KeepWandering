using UnityEngine;

public class UI_StatRow : MonoBehaviour
{
    [Header("Elements")]
    public UI_Stat LeftStat;
    public UI_Stat RightStat;

    public void Init(Stat stat1,  Stat stat2)
    {
        LeftStat.Init(stat1);
        if (stat2 != null)
        {
            RightStat.Init(stat2);
        }
        else
        {
            RightStat.gameObject.SetActive(false);
        }
    }
}
