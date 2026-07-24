using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Initializing,

    InDayTransition,
    DayTransitionFadeIn,
    DayTransitionFadeOut,

    EndEncounterTransitionIn,
    EndEncounterTransitionOut,

    EndMorningTransitionIn,
    EndMorningTransitionOut,

    InGame,
    GameOver
}
