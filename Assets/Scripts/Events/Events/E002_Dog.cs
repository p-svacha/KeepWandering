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
        {LocationType.Suburbs, 2f},
        {LocationType.City, 0.3f},
        {LocationType.Woods, 0.5f},
        {LocationType.GroceryStore, 0f},
    };

    public static float GetProbability(Game game)
    {
        if (game.Player.HasDog) return 0;
        else return BaseProbability * LocationProbabilityTable[game.CurrentLocation.Type];
    }

    // Instance

    public E002_Dog(Game game) : base(game, EventType.E002_Dog) { }

    public override void InitEvent()
    {
        // Attributes
        ItemActionsAllowed = true;

        // Sprite
        ResourceManager.Singleton.E002_Dog.SetActive(true);

        // Event
        List<EventOption> options = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Pet the dog
        options.Add(new EventOption("Pet the dog", PetDog));

        // Dialogue Option - Ignore dog
        options.Add(new EventOption("Ignore the dog", IgnoreDog));

        // Item Option (bone) - Offer bone
        itemOptions.Add(new EventItemOption(ItemType.Bone, "Offer to dog", OfferBone));

        InitialStep = new EventStep("You encounter a dog that looks friendly towards you.", null, null, options, itemOptions);
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
