using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E003_EvilGuy : Event
{
    // Static
    public override int Id => 3;

    protected override float BaseProbability => 5f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 0.5f},
        {LocationType.City, 2f},
        {LocationType.Woods, 0.1f},
    };

    private const float FIGHT_WIN_BASE_CHANCE = 0.3f;
    private const float FIGHTING_STAT_AFFECTION = 1f; // +1% in fighting means +1% chance

    private const float ITEM_THROW_SUCCESS_CHANCE = 0.5f;
    
    private static Dictionary<int, float> NumRewardsTable = new Dictionary<int, float>()
    {
        {1, 60 },
        {2, 30 },
        {3, 10 }
    };

    private static LootTable FightRewardTable = new LootTable(
        new(ItemType.Beans, 10),
        new(ItemType.WaterBottle, 10),
        new(ItemType.Bone, 2),
        new(ItemType.Bandage, 4),
        new(ItemType.Antibiotics, 4)
    );



    // Instance
    private List<Item> RewardItems;
    private Item RansomItem;

    public E003_EvilGuy(Game game) : base(game) { }
    public override Event GetEventInstance => new E003_EvilGuy(Game);

    public override float GetEventProbability()
    {
        if (Game.Inventory.Count == 0) return 0;
        else return GetDefaultEventProbability();
    }
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E003_EvilGuy);

        // Ransom item
        RansomItem = Game.RandomInventoryItem;

        // Reward item(s)
        int numRewards = HelperFunctions.GetWeightedRandomElement<int>(NumRewardsTable);
        RewardItems = GetLocationLootTable(FightRewardTable).GetItems(numRewards, hide: true);
    }

    protected override EventStep GetInitialStep()
    {
        string initialStep = "You encounter a very angry and dangerous looking guy. He tells you to give him your " + RansomItem.Name + " or he's gonna punch you.";
        return GetStandoffStep(initialStep);
    }
    protected override void OnEventEnd()
    {
        foreach (Item item in RewardItems)
            if (!item.IsPlayerOwned) GameObject.Destroy(item.gameObject);
    }

    private EventStep GetStandoffStep(string text)
    {
        // Dialogue Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        dialogueOptions.Add(new EventDialogueOption("Fight", Fight, StatId.Fighting)); // Fight

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();
        itemOptions.Add(new EventItemOption(RansomItem.Type, "Give to the evil guy", PayRansom)); // Pay Ransom
        foreach (Item item in Game.Inventory.Where(x => x.IsWeapon)) itemOptions.Add(new EventItemOption(item.Type, "Throw", ThrowItem)); // Throw Item


        return new EventStep(text, dialogueOptions, itemOptions, allowDefaultItemInteractions: false);
    }
    private EventStep Fight()
    {
        // Success chance
        float fightSuccessChance = FIGHT_WIN_BASE_CHANCE;
        float fightSuccessModifier = (100 + (Game.Stats[StatId.Fighting].GetRelativeValue() * FIGHTING_STAT_AFFECTION)) / 100f;
        fightSuccessChance *= fightSuccessModifier;

        bool fightSuccess = Random.value < fightSuccessChance;
        bool gotInjured = !fightSuccess || Random.value < fightSuccessChance;

        // Outcome handling
        if(fightSuccess && !gotInjured)
        {
            WinFight();
            return new EventStep("You jump on the guy and manage to knock him unconcious before he can hurt you. You take his stuff and leave.");
        }
        else if(fightSuccess && gotInjured)
        {
            GetInjured();
            WinFight();
            return new EventStep("You jump on the guy and a dirty fight ensues. You manage to knock him about but take a punch in the process. You take his stuff and leave.");
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
            return new EventStep(text);
        }
    }
    private EventStep PayRansom(Item ransomItem)
    {
        if (ransomItem != null) Game.DestroyOwnedItem(ransomItem);
        return new EventStep("You give the guy your " + ransomItem.Name + ". He thanks and wishes you a nice day.");
    }
    private void WinFight()
    {
        HideEventSprite(ResourceManager.Singleton.E003_EvilGuy);
        ShowEventSprite(ResourceManager.Singleton.E003_EvilGuy_KO);
        foreach (Item item in RewardItems) Game.AddItemToInventory(item);
    }
    private void GetInjured()
    {
        Game.AddBruiseWound();
    }
    private EventStep ThrowItem(Item throwItem)
    {
        bool throwSuccessful = Random.value < ITEM_THROW_SUCCESS_CHANCE;

        Game.DestroyOwnedItem(throwItem);

        if (throwSuccessful)
        {
            WinFight();
            return new EventStep("Bullseye! The " + throwItem.Name + " knocked the guy straight to the floor. You take his stuff and leave.", null, null);
        }
        else
        {
            string text = "The " + throwItem.Name + " missed the guy and gets destroyed upon hitting the floor.";
            EventStep startStep = GetStandoffStep(text);
            return startStep;
        }
    }

    
}
