using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parrot : Companion
{
    private float Nutrition;

    private static string SourceName = "Parrot";
    private bool StatusEffectsInitialized;
    private StatusEffect SE_Hungry;
    private StatusEffect SE_VeryHungry;
    private StatusEffect SE_Starving;

    private void InitStatusEffects()
    {
        StatusEffectsInitialized = true;

        SE_Hungry = new StatusEffect(SourceName, "Hungry", "Needs nuts", ResourceManager.Singleton.SE_Bad, Color.clear);
        SE_VeryHungry = new StatusEffect(SourceName, "Hungry", "Needs nuts urgently", ResourceManager.Singleton.SE_VeryBad, Color.clear);
        SE_Starving = new StatusEffect(SourceName, "Hungry", "Needs nuts or it will die", ResourceManager.Singleton.SE_ExtremelyBad, Color.clear);
    }

    protected override void OnInit()
    {
        Nutrition = 5f;
    }

    public void AddNutrition(float value)
    {
        Nutrition += value;
    }

    public override List<string> OnEndDay(Game game)
    {
        List<string> morningReport = new List<string>();
        Nutrition -= 1f;
        if(Nutrition <= 0)
        {
            game.RemoveParrot();
            morningReport.Add("Your parrot died from malnutrition.");
        }
        return morningReport;
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
