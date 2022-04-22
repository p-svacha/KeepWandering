using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E002_Dog : Event
{
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

    public static E002_Dog GetEventInstance(Game game)
    {
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
        itemOptions.Add(new EventItemOption(ItemType.Bone, "Offer to dog", (game, item) => OfferBone(game, item)));

        EventStep initialStep = new EventStep("You encounter a dog that looks friendly towards you.", null, null, options, itemOptions);
        return new E002_Dog(initialStep);
    }

    private static EventStep PetDog(Game game)
    {
        bool petSuccess = Random.value < PetSuccessChance;

        EventStep nextEventStep;
        if (petSuccess)
        {
            RecruitDog(game);
            nextEventStep = new EventStep("The dog loves the pets and decides to follow you on your journey.", null, null, null, null);
        }
        else nextEventStep = new EventStep("The dog enjoys the pets and keeps watching into the distance.", null, null, null, null);
        return nextEventStep;
    }

    private static EventStep IgnoreDog(Game game)
    {
        return new EventStep("The dog mirrors your reaction and ignores you too.", null, null, null, null);
    }

    private static EventStep OfferBone(Game game, Item bone)
    {
        game.DestroyOwnedItem(bone);
        RecruitDog(game);
        return new EventStep("The dog happily takes the bone and decides to follow you on your journey.", null, new List<Item>() { bone }, null, null);
    }

    private static void RecruitDog(Game game)
    {
        ResourceManager.Singleton.E002_Dog.SetActive(false);
        game.AddDog();
    }

    public E002_Dog(EventStep initialStep) : base(
        EventType.E002_Dog,
        initialStep,
        new List<Item>() {  },
        new List<GameObject>() { ResourceManager.Singleton.E002_Dog },
        itemActionsAllowed: true)
    { }
}
