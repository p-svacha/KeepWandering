using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E001_Crate : Event
{
    // Static
    public override int Id => 1;
    protected override float BaseProbability => 8f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 1.1f},
        {LocationType.City, 0.2f},
        {LocationType.Woods, 0.9f},
    };

    private const float CUT_CHANCE = 0.1f;

    private static LootTable ItemTable = new LootTable(
        new(ItemType.Beans, 10),
        new(ItemType.WaterBottle, 10),
        new(ItemType.Bandage, 5),
        new(ItemType.Antibiotics, 5),
        new(ItemType.Bone, 3),
        new(ItemType.Knife, 3),
        new(ItemType.NutSnack, 10),
        new(ItemType.MedicalKit, 1)
    );

    // Instance
    private Item CrateItem;

    public E001_Crate(Game game) : base(game) { }
    public override Event GetEventInstance => new E001_Crate(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E001_Crate);

        // Crate item
        CrateItem = GetLocationLootTable(ItemTable).GetItem();
        CrateItem.transform.position = new Vector3(6, 0f, 0f);
        CrateItem.transform.rotation = Quaternion.Euler(0f, 0f, -30f);
    }
    protected override EventStep GetInitialStep()
    {
        // Dialogue Options
        List<EventDialogueOption> options = new List<EventDialogueOption>();
        options.Add(new EventDialogueOption("Take the " + CrateItem.Name + ".", TakeItem)); // Take item
        options.Add(new EventDialogueOption("Don't take the " + CrateItem.Name + ".", DontTakeItem)); // Don't take item

        // Item Options
        List<EventItemOption> itemOptions = new List<EventItemOption>();      

        return new EventStep("You stumble upon a crate that looks to have a " + CrateItem.Name + " inside.", options, itemOptions);
    }
    protected override void OnEventEnd()
    {
        if (!CrateItem.IsPlayerOwned) GameObject.Destroy(CrateItem.gameObject);
    }

    private EventStep TakeItem()
    {
        string text = "You reach into the crate and take out the " + CrateItem.Name + ".";
        Game.AddItemToInventory(CrateItem);
        if(Random.value < CUT_CHANCE)
        {
            Game.AddCutWound();
            text += " Upon taking out your hand you scratch yourself on a loose nail.";
        }
        return new EventStep(text);
    }
    private EventStep DontTakeItem()
    {
        return new EventStep("You didn't take the " + CrateItem.Name + ".");
    }


}
