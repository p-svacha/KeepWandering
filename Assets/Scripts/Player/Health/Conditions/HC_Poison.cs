using UnityEngine;

public class HC_Poison : HealthCondition
{
    public bool IsPoisoned; // If true, poison countdown will decrease every day
    public int PoisonCountdown; // Death upon reaching zero

    protected override void OnInit() { }

    public override void OnUpdate() { }

    public override void OnEndDay(Game game, MorningReport morningReport)
    {
        if (IsPoisoned) PoisonCountdown--;
    }

    public override string IsFatal()
    {
        if (IsPoisoned && PoisonCountdown <= 0) return "You died of poisoning.";
        return "";
    }

    public void ApplyPoison()
    {
        if (IsPoisoned) PoisonCountdown -= PlayerCharacter.REPOISON_STRENGTH;
        else
        {
            IsPoisoned = true;
            PoisonCountdown = PlayerCharacter.POISON_COUNTDOWN_START;
        }
    }
    public void HealPoison()
    {
        IsPoisoned = false;
    }

    public override string GetReportLabel() => IsPoisoned ? $"{LabelCapWord} ({PoisonCountdown} days left)" : LabelCapWord;
}
