using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_WorldMapMenu : MonoBehaviour
{
    [Header("Elements")]
    public UI_LabelValueRow Biome;
    public UI_LabelValueRow Encounter;
    public UI_LabelValueRow DangerLevel;

    public UI_LabelValueRow HexDistance;
    public UI_LabelValueRow ShortestPathDistance;
    public UI_LabelValueRow LastVisited;

    public Toggle DangerOverlayToggle;

    public RawImage MapImage;
    public Button CloseButton;

    private void Awake()
    {
        CloseButton.onClick.AddListener(() => GameUI.Instance.ToggleWorldMap());
        DangerOverlayToggle.onValueChanged.AddListener(ToggleDangerOverlay);
    }

    public void ShowTileInfo(WorldMapTile tile)
    {
        if (tile == null)
        {
            Biome.SetContentVisible(false);
            Encounter.SetContentVisible(false);
            DangerLevel.SetContentVisible(false);
            HexDistance.SetContentVisible(false);
            ShortestPathDistance.SetContentVisible(false);
            LastVisited.SetContentVisible(false);
            return;
        }

        Biome.Init("Biome", tile.Biome.LabelCapWord);
        Encounter.Init("Location", tile.HasEncounter ? tile.Encounter.Label : "Undiscovered");
        DangerLevel.Init("Danger Level", tile.DangerLevel.LabelCapWord);
        DangerLevel.ValueText.color = tile.DangerLevel.Color;

        HexDistance.Init("Hex Distance", $"{tile.GetHexDistance(Game.Instance.CurrentPosition)}");
        int shortestPath = tile.GetShortestPath(Game.Instance.CurrentPosition);
        ShortestPathDistance.Init("Shortest Path Distance", shortestPath >= 0 ? shortestPath.ToString() : "Unreachable");

        string lastVisited = "";
        if (tile.HasEncounter && tile.NumVisits > 0)
        {
            int daysDifference = Game.Instance.Day - tile.Encounter.LastVisitDay;
            if (daysDifference == 0) lastVisited = "Today";
            else if (daysDifference == 1) lastVisited = "Yesterday";
            else lastVisited = $"{daysDifference} days ago (day {tile.Encounter.LastVisitDay})";
        }
        else lastVisited = "Never";

        LastVisited.Init("Last Visited", lastVisited);
    }

    private void ToggleDangerOverlay(bool value)
    {
        WorldMapRenderer.Instance.SetDangerOverlayVisible(value);
    }
}
