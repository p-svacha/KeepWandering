using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BruiseInjury : Injury
{
    public override InjuryId Type => InjuryId.Bruise;
    public override Sprite SpriteBase => ResourceManager_Old.Singleton.Bruise_Base;
    public override Sprite SpriteInfectMinor => ResourceManager_Old.Singleton.Bruise_Infect_Minor;
    public override Sprite SpriteInfectMajor => ResourceManager_Old.Singleton.Bruise_Infect_Major;
    public override Sprite SpriteTended => ResourceManager_Old.Singleton.Bruise_Tended;
}
