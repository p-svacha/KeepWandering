using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E002_Dog : Event
{
    private const float PetSuccessChance = 0.2f;

    private static Dictionary<Location, float> LocationProbabilityTable = new Dictionary<Location, float>()
    {
        {Location.Suburbs, 2f},
        {Location.City, 0.5f},
    };

    public static float GetProbability(Game game)
    {
        if (game.Player.HasDog) return 0;
        else return 2f * LocationProbabilityTable[game.CurrentLocation];
    }

    public static E002_Dog GetEventInstance(Game game)
    {
        // Sprite
        ResourceManager.Singleton.E002_Dog.SetActive(true);

        bool petSuccess = Random.value < PetSuccessChance;

        List<EventOption> options = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Pet the dog
        if (petSuccess) options.Add(new EventOption("Pet the dog", () => RecruitDog(game), new EventStep("The dog loves the pets and decides to follow you on your journey.", null, null)));
        else options.Add(new EventOption("Pet the dog", null, new EventStep("The dog enjoys the pets and keeps watching into the distance.", null, null)));

        // Dialogue Option - Ignore dog
        options.Add(new EventOption("Ignore the dog", null, new EventStep("The dog mirrors your reaction and ignores you too.", null, null)));

        // Item Option (bone) - Offer bone
        itemOptions.Add(new EventItemOption(ItemType.Bone, "Offer to dog", RecruitDogWithBone, new EventStep("The dog happily takes the bone and decides to follow you on your journey.", null, null)));

        EventStep initialStep = new EventStep("You encounter a dog that looks friendly towards you.", options, itemOptions);
        return new E002_Dog(initialStep);
    }

    private static void RecruitDogWithBone(Game game, Item bone)
    {
        game.DestroyOwnedItem(bone);
        RecruitDog(game);
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
