using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parrot : Companion
{
    public override string Name => "Parrot";

    private float Nutrition;

    private bool StatusEffectsInitialized;
    private StatusEffect SE_Hungry;
    private StatusEffect SE_VeryHungry;
    private StatusEffect SE_Starving;

    private void InitStatusEffects()
    {
        StatusEffectsInitialized = true;

        SE_Hungry = new StatusEffect("Hungry", "Needs nuts", ResourceManager.Singleton.SE_Bad, Color.clear);
        SE_VeryHungry = new StatusEffect("Very Hungry", "Needs nuts urgently", ResourceManager.Singleton.SE_VeryBad, Color.clear);
        SE_Starving = new StatusEffect("Starving", "Needs nuts or it will die", ResourceManager.Singleton.SE_ExtremelyBad, Color.clear);
    }

    protected override void OnInit()
    {
        Nutrition = 5f;
    }

    public void AddNutrition(float value)
    {
        Nutrition += value;
    }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        Nutrition -= 1f;
        if (Nutrition <= 0)
        {
            Starve(game);
            morningReport.NightEvents.Add("Your parrot died from malnutrition.");
        }
    }

    private void Starve(Game game)
    {
        game.RemoveParrot();
        game.RemoveMission(MissionId.M001_CareParrot);
    }

    public override void UpdateStatusEffects()
    {
        if (!StatusEffectsInitialized) InitStatusEffects();

        StatusEffects.Clear();

        if (Nutrition <= 1f) StatusEffects.Add(SE_Starving);
        else if (Nutrition <= 2f) StatusEffects.Add(SE_VeryHungry);
        else if (Nutrition <= 3f) StatusEffects.Add(SE_Hungry);
    }
}
