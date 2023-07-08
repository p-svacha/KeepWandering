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
    public TextMeshProUGUI MissionTextPrefab;

    public void UpdateList(List<Mission> missions)
    {
        // Clear old elements
        HelperFunctions.DestroyAllChildredImmediately(MissionsContainer);

        // Display new elements
        foreach(Mission mission in missions)
        {
            TextMeshProUGUI missionDisplay = Instantiate(MissionTextPrefab, MissionsContainer.transform);
            missionDisplay.text = mission.Text;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MissionsContainer.GetComponent<RectTransform>());
    }
}
