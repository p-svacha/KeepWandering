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

    public GameObject Divider;

    public UI_LabelValueRow HexDistance;
    public UI_LabelValueRow ShortestPathDistance;
    public UI_LabelValueRow LastVisited;

    public Toggle DangerOverlayToggle;
    public Toggle PathHistoryToggle;
    public Toggle AreaLabelToggle;
    public Toggle MapDetailsToggle;

    public RawImage MapImage;
    public Button CloseButton;

    private void Awake()
    {
        CloseButton.onClick.AddListener(() => GameUI.Instance.ToggleWorldMap());
        DangerOverlayToggle.onValueChanged.AddListener(ToggleDangerOverlay);
        PathHistoryToggle.onValueChanged.AddListener(TogglePathHistory);
        AreaLabelToggle.onValueChanged.AddListener(ToggleAreaLabels);
        MapDetailsToggle.onValueChanged.AddListener(ToggleMapDetails);
    }

    private void Start()
    {
        // Set default toggle states
        ToggleDangerOverlay(false);
        TogglePathHistory(true);
        ToggleAreaLabels(true);
        ToggleMapDetails(true);
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

            Divider.SetActive(false);
            return;
        }

        Divider.SetActive(true);

        Biome.Init("Biome", tile.GetBiomeAreaLabel());
        Encounter.Init("Location", tile.HasEncounter ? tile.Encounter.Label : "Undiscovered");
        DangerLevel.Init("Danger Level", tile.BaseDangerLevel.LabelCapWord);
        DangerLevel.ValueText.color = tile.BaseDangerLevel.Color;

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
        BaseToggleEffect(value, DangerOverlayToggle);
        WorldMapRenderer.Instance.SetDangerOverlayVisible(value);
    }
    private void TogglePathHistory(bool value)
    {
        BaseToggleEffect(value, PathHistoryToggle);
        WorldMapRenderer.Instance.SetPathHistoryVisible(value);
    }
    private void ToggleAreaLabels(bool value)
    {
        BaseToggleEffect(value, AreaLabelToggle);
        WorldMapRenderer.Instance.SetAreaLabelsVisible(value);
    }
    private void ToggleMapDetails(bool value)
    {
        BaseToggleEffect(value, MapDetailsToggle);
        WorldMapRenderer.Instance.SetScatteredTileSpritesVisible(value);
    }

    private void BaseToggleEffect(bool value, Toggle toggle)
    {
        AudioManager.PlayStandardToggleClick(value);
        toggle.transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite = value ? ResourceManager.LoadSprite("UiSprites/Checkmark") : ResourceManager.LoadSprite("UiSprites/Cross");
    }
}
