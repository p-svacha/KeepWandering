using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mission
{
    public MissionId Id;
    public string Text;

    public Mission(MissionId id, string text)
    {
        Id = id;
        Text = text;
    }
}
