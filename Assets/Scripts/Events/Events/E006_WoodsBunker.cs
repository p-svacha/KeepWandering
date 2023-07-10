using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class E006_WoodsBunker : Event
{
    // Static
    public override int Id => 6;
    protected override float BaseProbability => 2f;

    public int RequiredFood;
    public int RequiredWater;

    // Instance
    public E006_WoodsBunker(Game game) : base(game) { }
    public override Event GetEventInstance => new E006_WoodsBunker(Game);

    // Base
    public override float GetEventProbability()
    {
        if (Game.CurrentPosition.Location.Type != LocationType.Woods) return 0; // only happens in woods
        if (!Game.PlayerIsOnQuarantinePerimeter) return 0; // only happens on perimeter
        return BaseProbability;
    }
    protected override void OnEventStart()
    {
        // Sprites
        ShowEventSprite(ResourceManager.Singleton.E006_WoodsBunker);

        // Required items
        RequiredFood = Random.Range(1, 2);
        RequiredWater = Random.Range(1, 2);
    }
    protected override EventStep GetInitialStep()
    {
        string eventText = "You come across a secret tunnel entry that seems like it will lead to the outside of the quarantine zone.";
        if (RequiredFood > 0 || RequiredWater > 0) eventText += " A voice from inside assures you that they will let you through if you give them " + RequiredFood + " food and " + RequiredWater + " water.";
        return GetInitialStep(eventText);
    }

    private EventStep GetInitialStep(string eventText)
    {
        // Options
        List<EventDialogueOption> dialogueOptions = new List<EventDialogueOption>();
        List<EventItemOption> itemOptions = new List<EventItemOption>();

        // Dialogue Option - Enter Bunker
        if (RequiredFood == 0 && RequiredWater == 0)
        {
            dialogueOptions.Add(new EventDialogueOption("Enter Tunnel", EnterBunker));
        }

        // Dialogue Option - Continue
        dialogueOptions.Add(new EventDialogueOption("Ignore Tunnel", Continue));

        // Item Option - Give Food/Water
        List<ItemType> handledTypes = new List<ItemType>();
        foreach (Item item in Game.Inventory)
        {
            if (RequiredFood > 0 && item.IsEdible && !handledTypes.Contains(item.Type))
            {
                handledTypes.Add(item.Type);
                itemOptions.Add(new EventItemOption(item.Type, "Give to bunker", GiveFood));
            }
            if (RequiredWater > 0 && item.IsDrinkable && !handledTypes.Contains(item.Type))
            {
                handledTypes.Add(item.Type);
                itemOptions.Add(new EventItemOption(item.Type, "Give to bunker", GiveWater));
            }
        }

        // Event
        return new EventStep(eventText, dialogueOptions, itemOptions);
    }
    private EventStep EnterBunker()
    {
        // Get position of other side of the tunnel
        WorldMapTile targetTile = null;
        foreach(WorldMapTile tile in Game.CurrentPosition.GetAdjacentTiles())
        {
            if(!Game.QuarantineZone.IsInArea(tile))
            {
                targetTile = tile;
                break;
            }
        }

        // Go outside of quarantine zone
        Game.SetPosition(targetTile);
        Game.CheckGameOver();
        return null;
    }
    private EventStep Continue()
    {
        return new EventStep("You walk past the bunker.");
    }
    private EventStep GiveFood(Item item)
    {
        Game.DestroyOwnedItem(item);
        RequiredFood--;
        EventStep nextStep = GetInitialStep("You gave the " + item.Name + " to the bunker.");
        return nextStep;
    }
    private EventStep GiveWater(Item item)
    {
        Game.DestroyOwnedItem(item);
        RequiredWater--;
        EventStep nextStep = GetInitialStep("You gave the " + item.Name + " to the bunker.");
        return nextStep;
    }
}
