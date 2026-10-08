using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UI_Missions : MonoBehaviour
{
    [Header("Elements")]
    public GameObject MissionsContainer;

    [Header("Prefabs")]
    public UI_Mission MissionPrefab;

    public void UpdateList(List<Quest> quests)
    {
        // Clear old elements
        HelperFunctions.DestroyAllChildredImmediately(MissionsContainer);

        // Display new elements
        foreach(Quest quest in quests.Where(q => q.State == QuestState.Active))
        {
            UI_Mission missionDisplay = Instantiate(MissionPrefab, MissionsContainer.transform);
            missionDisplay.Init(quest);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MissionsContainer.GetComponent<RectTransform>());
    }
}
