using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Missions : MonoBehaviour
{
    public GameObject MissionsContainer;
    public TextMeshProUGUI TitleText;
    public TextMeshProUGUI MissionTextPrefab;

    public void UpdateList(List<Mission> missions)
    {
        // Clear old elements
        foreach (Transform t in MissionsContainer.transform) Destroy(t.gameObject);

        // Display new elements
        TitleText.gameObject.SetActive(missions.Count > 0);

        foreach(Mission mission in missions)
        {
            TextMeshProUGUI missionDisplay = Instantiate(MissionTextPrefab, MissionsContainer.transform);
            missionDisplay.text = mission.Text;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(MissionsContainer.GetComponent<RectTransform>());
    }
}
