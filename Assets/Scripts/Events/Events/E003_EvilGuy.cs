using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E003_EvilGuy : Event
{
    // Static
    private const float BaseProbability = 4f;

    private const float BaseFightSuccess = 0.2f;
    private const float AddChance_Dog = 0.6f;
    private const float AddChance_Knife = 0.3f;
    private const float AddChance_Injured = -0.3f;

    private const float ItemThrowSuccessChance = 0.4f;

    private static Dictionary<LocationType, float> LocationProbabilityTable = new Dictionary<LocationType, float>()
    {
        {LocationType.MainRoad, 0.5f},
        {LocationType.City, 2f},
        {LocationType.Woods, 0.1f},
        {LocationType.GroceryStore, 1.5f},
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
        { ItemType.Bone, 2},
        { ItemType.Bandage, 4},
        { ItemType.Antibiotics, 4},
    };



    // Instance
    private List<Item> RewardItems;
    private Item RansomItem;

    public E003_EvilGuy(Game game) : base(game) { }
    public override Event GetEventInstance => new E003_EvilGuy(Game);

    public override float GetEventProbability()
    {
        if (Game.Inventory.Count == 0) return 0;
        else return BaseProbability * LocationProbabilityTable[Game.CurrentPosition.Location.Type];
    }
    public override void OnEventStart()
    {
        // Sprites
        ResourceManager.Singleton.E003_EvilGuy.SetActive(true);

        // Ransom item
        RansomItem = Game.Inventory[Random.Range(0, Game.Inventory.Count)];

        // Reward item(s)
        int numRewards = HelperFunctions.GetWeightedRandomElement<int>(NumRewardsTable);
        RewardItems = new List<Item>();
        for (int i = 0; i < numRewards; i++)
        {
            ItemType itemType = HelperFunctions.GetWeightedRandomElement<ItemType>(FightRewardTable);
            Item item = Game.GetItemInstance(itemType);
            item.GetComponent<SpriteRenderer>().enabled = false;
            RewardItems.Add(item);
        }
    }
    public override EventStep GetInitialStep()
    {
        string initialStep = "You encounter a very angry and dangerous looking guy. He tells you to give him your " + RansomItem.Name + " or he's gonna punch you.";
        return GetStandoffStep(initialStep);
    }
    public override void OnEventEnd()
    {
        ResourceManager.Singleton.E003_EvilGuy.SetActive(false);
        ResourceManager.Singleton.E003_EvilGuy_KO.SetActive(false);
        foreach (Item item in RewardItems)
            if (!item.IsPlayerOwned) GameObject.Destroy(item.gameObject);
    }

    private EventStep GetStandoffStep(string text)
    {
        // Dialogue Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        dialogueOptions.Add(new EventDialogueOption("Fight", Fight)); // Fight

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();
        itemOptions.Add(new EventItemOption(RansomItem.Type, "Give to the evil guy", PayRansom)); // Pay Ransom
        foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
            itemOptions.Add(new EventItemOption(type, "Throw", ThrowItem)); // Throw Item

        return new EventStep(text, null, null, dialogueOptions, itemOptions, allowItems: false);
    }
    private EventStep Fight()
    {
        // Success chance
        float fightSuccessChance = BaseFightSuccess;
        List<string> fightModifiers = new List<string>();
        if (Game.Player.HasDog)
        {
            fightSuccessChance += AddChance_Dog;
            fightModifiers.Add("Dog (++)");
        }
        if (Game.Inventory.Any(x => x.Type == ItemType.Knife))
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

        // Outcome handling
        if(fightSuccess && !gotInjured)
        {
            WinFight();
            return new EventStep("You jump on the guy and manage to knock him unconcious before he can hurt you. You take his stuff and leave.", RewardItems, null, null, null);
        }
        else if(fightSuccess && gotInjured)
        {
            GetInjured();
            WinFight();
            return new EventStep("You jump on the guy and a dirty fight ensues. You manage to knock him about but take a punch in the process. You take his stuff and leave.", RewardItems, null, null, null);
        }
        else
        {
            string text = "You jump on the guy but quickly realize that you underestimated his strength.";
            GetInjured();
            if (RansomItem != null)
            {
                Game.DestroyOwnedItem(RansomItem);
                text += " He punches you and takes away your " + RansomItem.Name + " by force.";
            }
            text += " Defeated and injured you decide it's better to move on.";
            return new EventStep(text, null, RansomItem == null ? null : new List<Item>() { RansomItem }, null, null);
        }
    }
    private EventStep PayRansom(Item ransomItem)
    {
        if (ransomItem != null) Game.DestroyOwnedItem(ransomItem);
        return new EventStep("You give the guy your " + ransomItem.Name + ". He thanks and wishes you a nice day.", null, new List<Item>() { ransomItem }, null, null);
    }
    private void WinFight()
    {
        ResourceManager.Singleton.E003_EvilGuy.SetActive(false);
        ResourceManager.Singleton.E003_EvilGuy_KO.SetActive(true);
        foreach (Item item in RewardItems) Game.AddItemToInventory(item);
    }
    private void GetInjured()
    {
        Game.AddBruiseWound();
    }
    private EventStep ThrowItem(Item throwItem)
    {
        bool throwSuccessful = Random.value < ItemThrowSuccessChance;

        Game.DestroyOwnedItem(throwItem);

        if (throwSuccessful)
        {
            WinFight();
            return new EventStep("Bullseye! The " + throwItem.Name + " knocked the guy straight to the floor. You take his stuff and leave.", RewardItems, new List<Item>() { throwItem }, null, null);
        }
        else
        {
            string text = "The " + throwItem.Name + " missed the guy and gets destroyed upon hitting the floor.";
            EventStep startStep = GetStandoffStep(text);
            startStep.RemovedItems = new List<Item>() { throwItem };
            return startStep;
        }
    }

    
}
