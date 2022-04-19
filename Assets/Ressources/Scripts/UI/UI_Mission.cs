using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Mission : MonoBehaviour
{
    public Text MissionText;

    public void Init(Mission mission)
    {
        MissionText.text = mission.Text;
    }
}
