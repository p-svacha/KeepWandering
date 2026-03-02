using UnityEngine;

public class HC_CutWound : Wound
{
    public override Sprite SpriteBase => ResourceManager.LoadSprite("Characters/Injuries/Cut_Base");
    public override Sprite SpriteInfectMinor => ResourceManager.LoadSprite("Characters/Injuries/Cut_InfectedMinor");
    public override Sprite SpriteInfectMajor => ResourceManager.LoadSprite("Characters/Injuries/Cut_InfectedMajor");
    public override Sprite SpriteTended => ResourceManager.LoadSprite("Characters/Injuries/Cut_Tended");

    protected override string GetUntendedEffectString() => " While untended, this wound causes bleeding.";
}
