using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    InDayTransition,
    DayTransitionFadeIn,
    DayTransitionFadeOut,
    EndEventTransitionIn,
    EndEventTransitionOut,
    EndMorningReportTransitionIn,
    EndMorningReportTransitionOut,
    InGame,
    GameOver
}
