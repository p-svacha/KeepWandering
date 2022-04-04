using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E003_EvilGuy : Event
{
    private const float BaseProbability = 4f;

    private const float BaseFightSuccess = 0.2f;
    private const float AddChance_Dog = 0.6f;
    private const float AddChance_Knife = 0.3f;
    private const float AddChance_Injured = -0.3f;

    private const float ItemThrowSuccessChance = 0.4f;

    private static Dictionary<Location, float> LocationProbabilityTable = new Dictionary<Location, float>()
    {
        {Location.Suburbs, 0.5f},
        {Location.City, 2f},
    };
    private static Dictionary<int, float> NumRewardsTable = new Dictionary<int, float>()
    {
        {1, 60 },
        {2, 30 },
        {3, 10 }
    };
    private static Dictionary<ItemType, float> FightRewardTable = new Dictionary<ItemType, float>()
    {
        { ItemType.Beans, 10},
        { ItemType.WaterBottle, 10},
        { ItemType.Bone, 5},
        { ItemType.Knife, 2},
    };

    public E003_EvilGuy(EventStep initialStep, List<Item> rewardItems) : base(
        EventType.E003_EvilGuy,
        initialStep,
        rewardItems,
        new List<GameObject>() { ResourceManager.Singleton.E003_EvilGuy, ResourceManager.Singleton.E003_EvilGuy_KO },
        itemActionsAllowed: false)
    { }

    public static float GetProbability(Game game)
    {
        if (game.Inventory.Count == 0) return 0;
        else return BaseProbability * LocationProbabilityTable[game.CurrentLocation];
    }

    public static E003_EvilGuy GetEventInstance(Game game)
    {
        // Event Resources
        ResourceManager.Singleton.E003_EvilGuy.SetActive(true);
        List<Item> eventItems = new List<Item>();

        // Ransom item
        Item ransomItem = game.Inventory[Random.Range(0, game.Inventory.Count)];

        // Reward item(s)
        int numRewards = HelperFunctions.GetWeightedRandomElement<int>(NumRewardsTable);
        List<Item> rewardItems = new List<Item>();
        for(int i = 0; i < numRewards; i++)
        {
            ItemType itemType = HelperFunctions.GetWeightedRandomElement<ItemType>(FightRewardTable, debug: true);
            Item item = game.GetItemInstance(itemType);
            item.GetComponent<SpriteRenderer>().enabled = false;
            rewardItems.Add(item);
            eventItems.Add(item);
        }

        // Event
        string eventText = "You encounter a very angry and dangerous looking guy. He tells you to give him your " + ransomItem.Name + " or he's gonna punch you.";

        EventStep initialStep = GetInitialStep(game, eventText, ransomItem, rewardItems);
        return new E003_EvilGuy(initialStep, eventItems);
    }

    private static void PayRansom(Game game, Item ransomItem)
    {
        if (ransomItem != null) game.DestroyOwnedItem(ransomItem);
    }

    private static void WinFight(Game game, List<Item> rewards)
    {
        ResourceManager.Singleton.E003_EvilGuy.SetActive(false);
        ResourceManager.Singleton.E003_EvilGuy_KO.SetActive(true);
        foreach (Item item in rewards) game.AddItemToInventory(item);
    }

    private static void WinFightWithInjury(Game game, List<Item> rewards)
    {
        GetInjured(game);
        WinFight(game, rewards);
    }

    private static void GetInjured(Game game)
    {
        game.AddBruiseWound();
    }

    private static void LoseFight(Game game, Item ransomItem)
    {
        GetInjured(game);
        PayRansom(game, ransomItem);
    }

    private static void ThrowItem(Game game, Item throwItem, Item ransomItem, List<Item> rewardItems)
    {
        bool throwSuccessful = Random.value < ItemThrowSuccessChance;
        game.DestroyOwnedItem(throwItem);
        if (throwSuccessful)
        {
            WinFight(game, rewardItems);
            game.DisplayEventStep(new EventStep("Bullseye! The " + throwItem.Name + " knocked the guy straight to the floor. You take his stuff (" + HelperFunctions.GetItemListAsString(rewardItems) + ") and leave.", null, null));
        }
        else
        {
            string text = "The " + throwItem.Name + " missed the guy and gets destroyed upon hitting the floor.";
            EventStep startStep = GetInitialStep(game, text, ransomItem, rewardItems);
            game.DisplayEventStep(startStep);
        }
    }

    private static EventStep GetInitialStep(Game game, string text, Item ransomItem, List<Item> rewardItems)
    {
        // Successful fight calculations
        float fightSuccessChance = BaseFightSuccess;
        List<string> fightModifiers = new List<string>();
        if (game.Player.HasDog)
        {
            fightSuccessChance += AddChance_Dog;
            fightModifiers.Add("Dog (++)");
        }
        if (game.Inventory.Any(x => x.Type == ItemType.Knife))
        {
            fightSuccessChance += AddChance_Knife;
            fightModifiers.Add("Knife (+)");
        }
        /*
        if(game.Player.IsInjured)
        {
            fightSuccessChance += AddChance_Injured;
            fightModifiers.Add("Injured (-)");
        }
        */
        string fightModifiersText = "";
        if (fightModifiers.Count > 0)
        {
            fightModifiersText += " (Chance affected by";
            foreach (string s in fightModifiers) fightModifiersText += " " + s + ",";
            fightModifiersText = fightModifiersText.TrimEnd(',');
            fightModifiersText += ")";
        }

        bool fightSuccess = Random.value < fightSuccessChance;
        bool gotInjured = !fightSuccess || Random.value < fightSuccessChance;

        List<EventOption> dialogueOptions = new List<EventOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Fight him
        string successText = fightSuccess ? "You manage to knock him unconcious and take his stuff (" + HelperFunctions.GetItemListAsString(rewardItems) + ")." : "He is stronger than you thought and takes away the " + ransomItem.Name + " by force.";
        string injuryText = gotInjured ? " You got injured during the fight." : "";
        EventStep fightResultStep = new EventStep("You jump onto the guy and a dirty fight occurs. " + successText + injuryText + "You decide it's better to leave.", null, null);
        System.Action fightAction;
        if (fightSuccess && !gotInjured) fightAction = () => WinFight(game, rewardItems);
        else if (fightSuccess && gotInjured) fightAction = () => WinFightWithInjury(game, rewardItems);
        else fightAction = () => LoseFight(game, ransomItem);
        dialogueOptions.Add(new EventOption("Fight him " + fightModifiersText, fightAction, fightResultStep));

        // Item Option - Pay him
        itemOptions.Add(new EventItemOption(ransomItem.Type, "Give to the evil guy", PayRansom, new EventStep("You give the guy your " + ransomItem.Name + ". He thanks and wishes you a nice day.", null, null)));

        // Item Option - Throw item at him
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
            itemOptions.Add(new EventItemOption(type, "Throw", (game, item) => ThrowItem(game, item, ransomItem, rewardItems), null));

        return new EventStep(text, dialogueOptions, itemOptions);
    }
}
