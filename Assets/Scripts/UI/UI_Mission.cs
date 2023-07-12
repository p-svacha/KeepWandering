using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_Mission : MonoBehaviour
{
    private Mission Mission;

    [Header("Elements")]
    public TextMeshProUGUI MissionText;
    public Button LocationButton;

    public void Init(Mission mission)
    {
        Mission = mission;
        MissionText.text = "- " + Mission.Text;
        LocationButton.gameObject.SetActive(Mission.IsLocationBased);
        if (Mission.IsLocationBased) LocationButton.onClick.AddListener(LocationButton_OnClick);
    }

    private void LocationButton_OnClick()
    {
        Game.Singleton.UI.OpenWorldMap(focusTile: Mission.Location);
    }
}
