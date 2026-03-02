using UnityEngine;

public class HC_BruiseWound : Wound
{
    public override Sprite SpriteBase => ResourceManager.LoadSprite("Character/Injuries/Bruise_Base");
    public override Sprite SpriteInfectMinor => ResourceManager.LoadSprite("Character/Injuries/Bruise_InfectedMinor");
    public override Sprite SpriteInfectMajor => ResourceManager.LoadSprite("Character/Injuries/Bruise_InfectedMajor");
    public override Sprite SpriteTended => ResourceManager.LoadSprite("Character/Injuries/Bruise_Tended");

    protected override string GetUntendedEffectString() => " While untended, this wound prevents bone fractures from healing.";
}
