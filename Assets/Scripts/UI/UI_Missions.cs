using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Missions : MonoBehaviour
{
    [Header("Elements")]
    public GameObject MissionsContainer;

    [Header("Prefabs")]
    public UI_Mission MissionPrefab;

    public void UpdateList(List<Mission> missions)
    {
        // Clear old elements
        HelperFunctions.DestroyAllChildredImmediately(MissionsContainer);

        // Display new elements
        foreach(Mission mission in missions)
        {
            UI_Mission missionDisplay = Instantiate(MissionPrefab, MissionsContainer.transform);
            missionDisplay.Init(mission);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MissionsContainer.GetComponent<RectTransform>());
    }
}
