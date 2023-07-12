using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class E011_SurvivorNeedsItemFromLocation : Event
{
    // Static
    public override int Id => 11;
    protected override float BaseProbability => 2.5f;
    protected override Dictionary<LocationType, float> LocationProbabilityTable => new Dictionary<LocationType, float>()
    {
        {LocationType.Farmland, 0.5f},
        {LocationType.City, 1.5f},
        {LocationType.Woods, 0.1f},
    };

    private static LootTable RequestedItemLootTable = new LootTable(
        new(ItemType.Antidote, 10),
        new(ItemType.MedicalKit, 4),
        new(ItemType.Bandage, 4),
        new(ItemType.Antibiotics, 4)
        );

    private static bool ItemLocationRevealed = false;
    private static bool SurvivorMetAlready = false;
    private static Item RequestedItemDummy;

    // Instance
    public E011_SurvivorNeedsItemFromLocation(Game game) : base(game) { }
    public override Event GetEventInstance => new E011_SurvivorNeedsItemFromLocation(Game);

    // Base
    public override float GetEventProbability()
    {
        if (Game.IsMissionActive(MissionId.E011_ItemStash)) return 0f;
        if (Game.IsMissionActive(MissionId.E011_BringItemToSurvivor)) return 0f;
        return base.GetEventProbability();
    }

    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E011_Survivor);

        // Requested Item
        if(!SurvivorMetAlready) RequestedItemDummy = RequestedItemLootTable.GetItem(hide: true);
    }
    protected override EventStep GetInitialStep()
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Offer help
        if(!SurvivorMetAlready) dialogueOptions.Add(new EventDialogueOption("Offer to help", OfferHelp));

        // Dialogue Option - Ignore
        dialogueOptions.Add(new EventDialogueOption("Ignore and move on", Ignore));

        // Item Option - Help immediately
        itemOptions.Add(new EventItemOption(RequestedItemDummy.Type, "Give", GiveItem));

        // Event
        string eventText = "You come across a weary survivor huddled near a crumbling building. They appear injured and desperate for help. You cautiously approach, and they plead for your assistance, mentioning a vital " + RequestedItemDummy.Name + " they need from a location not far away.";
        if (SurvivorMetAlready) eventText = "You come back to the survivor that you promised to help. They are still looking for " + RequestedItemDummy.Name;

        SurvivorMetAlready = true;

        return new EventStep(eventText, dialogueOptions, itemOptions);
    }

    // Steps
    private EventStep OfferHelp()
    {
        RevealItemLocation();
        ActivateBringBackItemQuest();

        return new EventStep("You offer to help the survivor and promise to get the " + RequestedItemDummy.Name + " as quickly as possible");
    }

    private EventStep GiveItem(Item item)
    {
        Game.DestroyOwnedItem(item);

        if (!ItemLocationRevealed)
        {
            RevealItemLocation();
            CompleteBringBackItemQuest();
            return new EventStep("The survivor thanks you. Even though they no longer need the " + RequestedItemDummy.Name + " they tell you the location of where to find it so you can keep it for yourself.");
        }
        else
        {
            CompleteBringBackItemQuest();
            return new EventStep("The survivor thanks you.");
        }
    }

    private EventStep Ignore()
    {
        return new EventStep("Not wanting to waste your time helping them you move on.");
    }

    private void RevealItemLocation()
    {
        ItemLocationRevealed = true;
        WorldMapTile targetTile = Game.WorldMap.GetRandomQuarantineTile();

        Mission mission = new Mission(MissionId.E011_ItemStash, "Get " + RequestedItemDummy.Name + " from item stash", targetTile, eventId: 12, ResourceManager.Singleton.TileMarkerItem);
        E012_ItemStash.MissionItems.Add(mission.Id, RequestedItemDummy.Type);

        Game.AddMission(mission);
    }

    private void ActivateBringBackItemQuest()
    {
        WorldMapTile targetTile = Game.CurrentPosition;
        Mission mission = new Mission(MissionId.E011_BringItemToSurvivor, "Bring " + RequestedItemDummy.Name + " to survivor", targetTile, eventId: 11, ResourceManager.Singleton.TileMarkerPerson);
        Game.AddMission(mission);
    }

    private void CompleteBringBackItemQuest()
    {
        Game.RemoveMission(MissionId.E011_BringItemToSurvivor);
        Game.DestroyItem(RequestedItemDummy);

        ItemLocationRevealed = false;
        SurvivorMetAlready = false;
    }
}
