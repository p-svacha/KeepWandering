using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E012_ItemStash : Event
{
    // Static
    public override int Id => 12;
    protected override float BaseProbability => 0.1f;

    public static Dictionary<MissionId, ItemType> ForcedItems = new Dictionary<MissionId, ItemType>();

    // Instance
    private Item Item;

    public E012_ItemStash(Game game) : base(game) { }
    public override Event GetEventInstance => new E012_ItemStash(Game);

    // Base
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E012_ItemStashClosed);

        // Item
        if (Mission != null && ForcedItems.ContainsKey(Mission.Id))
        {
            Item = Game.Singleton.GetItemInstance(ForcedItems[Mission.Id]);
            ForcedItems.Remove(Mission.Id);
        }
        else Item = Game.GetStandardLootTable().GetItem();

        Item.Hide();

        // Mission
        if (Mission != null) Game.RemoveMission(Mission.Id);
    }
    protected override EventStep GetInitialStep()
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Take item
        dialogueOptions.Add(new EventDialogueOption("Take " + Item.Name, TakeItem));

        // Event
        string eventText = "You find an item stash.";
        return new EventStep(eventText, dialogueOptions, itemOptions);
    }

    // Steps
    private EventStep TakeItem()
    {
        ShowEventSprite(ResourceManager.Singleton.E012_ItemStashOpen);
        HideEventSprite(ResourceManager.Singleton.E012_ItemStashClosed);

        string text = "You take the " + Item.Name + ".";
        Game.AddItemToInventory(Item);
        return new EventStep(text);
    }
}
