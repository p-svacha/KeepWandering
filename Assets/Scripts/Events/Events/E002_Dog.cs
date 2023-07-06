using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E002_Dog : Event
{
    // Static
    private const float PetSuccessChance = 0.2f;

    private static float BaseProbability = 2f;
    private static Dictionary<LocationType, float> LocationProbabilityTable = new Dictionary<LocationType, float>()
    {
        {LocationType.MainRoad, 2f},
        {LocationType.City, 0.3f},
        {LocationType.Woods, 0.5f},
        {LocationType.GroceryStore, 0f},
    };

    public E002_Dog(Game game) : base(game) { }
    public override Event GetEventInstance => new E002_Dog(Game);

    public override float GetEventProbability()
    {
        if (Game.Player.HasDog) return 0;
        else return BaseProbability * LocationProbabilityTable[Game.CurrentPosition.Location.Type];
    }
    public override void OnEventStart()
    {
        // Sprites
        ResourceManager.Singleton.E002_Dog.SetActive(true);
    }
    public override EventStep GetInitialStep()
    {
        // Dialogue Options
        List<EventDialogueOption> options = new List<EventDialogueOption>();
        options.Add(new EventDialogueOption("Pet the dog", PetDog)); // Pet dog
        options.Add(new EventDialogueOption("Ignore the dog", IgnoreDog)); // Ignore dog

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();
        itemOptions.Add(new EventItemOption(ItemType.Bone, "Offer to dog", OfferBone)); // Offer bone

        return new EventStep("You encounter a dog that looks friendly towards you.", null, null, options, itemOptions);
    }
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E002_Dog.SetActive(false);
    }

    private EventStep PetDog()
    {
        bool petSuccess = Random.value < PetSuccessChance;

        EventStep nextEventStep;
        if (petSuccess)
        {
            RecruitDog();
            nextEventStep = new EventStep("The dog loves the pets and decides to follow you on your journey.", null, null, null, null);
        }
        else nextEventStep = new EventStep("The dog enjoys the pets and keeps watching into the distance.", null, null, null, null);
        return nextEventStep;
    }
    private EventStep IgnoreDog()
    {
        return new EventStep("The dog mirrors your reaction and ignores you too.", null, null, null, null);
    }
    private EventStep OfferBone(Item bone)
    {
        Game.DestroyOwnedItem(bone);
        RecruitDog();
        return new EventStep("The dog happily takes the bone and decides to follow you on your journey.", null, new List<Item>() { bone }, null, null);
    }
    private void RecruitDog()
    {
        ResourceManager.Singleton.E002_Dog.SetActive(false);
        Game.AddDog();
    }


}
