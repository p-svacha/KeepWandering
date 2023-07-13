using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutInjury : Injury
{
    public override InjuryId Type => InjuryId.Cut;
    public override Sprite SpriteBase => ResourceManager.Singleton.Cut_Base;
    public override Sprite SpriteInfectMinor => ResourceManager.Singleton.Cut_Infect_Minor;
    public override Sprite SpriteInfectMajor => ResourceManager.Singleton.Cut_Infect_Major;
    public override Sprite SpriteTended => ResourceManager.Singleton.Cut_Tended;
}
