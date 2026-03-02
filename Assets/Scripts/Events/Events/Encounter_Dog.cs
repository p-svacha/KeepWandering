using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
public class Encounter_Dog : Encounter
{
    // Static
    public override int DefName => 2;
    private const float PetSuccessChance = 0.2f;

    protected override float BaseProbability => 2f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 2f},
        {LocationType.City, 0.3f},
        {LocationType.Woods, 0.5f},
    };
     
    // Instance
    public Encounter_Dog(Game game) : base(game) { }
    public override Encounter GetEventInstance => new Encounter_Dog(Game);

    // Base
    public override float GetEventProbability()
    {
        if (Game.Player.HasDog) return 0;
        else return GetDefaultEventProbability();
    }
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager_Old.Singleton.E002_Dog);
    }
    protected override EncounterStep GetInitialStep()
    {
        // Dialogue Options
        List<EventDialogueOption> options = new List<EventDialogueOption>();
        options.Add(new EventDialogueOption("Pet the dog", PetDog)); // Pet dog
        options.Add(new EventDialogueOption("Ignore the dog", IgnoreDog)); // Ignore dog

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();
        itemOptions.Add(new EventItemOption(ItemType.Bone, "Offer to dog", OfferBone)); // Offer bone

        return new EncounterStep("You encounter a dog that looks friendly towards you.", options, itemOptions);
    }

    private EncounterStep PetDog()
    {
        bool petSuccess = Random.value < PetSuccessChance;

        EncounterStep nextEventStep;
        if (petSuccess)
        {
            RecruitDog();
            nextEventStep = new EncounterStep("The dog loves the pets and decides to follow you on your journey.");
        }
        else nextEventStep = new EncounterStep("The dog enjoys the pets and keeps watching into the distance.");
        return nextEventStep;
    }
    private EncounterStep IgnoreDog()
    {
        return new EncounterStep("The dog mirrors your reaction and ignores you too.");
    }
    private EncounterStep OfferBone(Item bone)
    {
        Game.DestroyOwnedItem(bone);
        RecruitDog();
        return new EncounterStep("The dog happily takes the bone and decides to follow you on your journey.");
    }
    private void RecruitDog()
    {
        HideEventSprite(ResourceManager_Old.Singleton.E002_Dog);
        Game.AddDog();
    }


}
*/