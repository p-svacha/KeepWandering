using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public enum InfectionStage
{
    None,
    [Description("Infected")] Minor,
    [Description("Severely Infected")] Major,
    Fatal
}
