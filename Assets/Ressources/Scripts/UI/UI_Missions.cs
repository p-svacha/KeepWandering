using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Missions : MonoBehaviour
{
    public Text TitleText;
    public UI_Mission MissionPrefab;
    public GameObject MissionsContainer;

    public void UpdateList(List<Mission> missions)
    {
        // Clear old elements
        foreach (Transform t in MissionsContainer.transform) Destroy(t.gameObject);

        // Display new elements
        TitleText.gameObject.SetActive(missions.Count > 0);

        foreach(Mission mission in missions)
        {
            UI_Mission missionDisplay = Instantiate(MissionPrefab, MissionsContainer.transform);
            missionDisplay.Init(mission);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MissionsContainer.GetComponent<RectTransform>());
    }
}
