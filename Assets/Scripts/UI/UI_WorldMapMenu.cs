using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_WorldMapMenu : MonoBehaviour
{
    [Header("Elements")]
    public TextMeshProUGUI BiomeText;
    public TextMeshProUGUI EncounterText;
    public TextMeshProUGUI DangerLevelText;

    public TextMeshProUGUI CoordinatesText;
    public TextMeshProUGUI HexDistanceText;
    public TextMeshProUGUI ShortestPathDistanceText;

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
            BiomeText.text = string.Empty;
            EncounterText.text = string.Empty;
            DangerLevelText.text = string.Empty;
            CoordinatesText.text = string.Empty;
            HexDistanceText.text = string.Empty;
            ShortestPathDistanceText.text = string.Empty;
            return;
        }

        BiomeText.text = tile.Biome.LabelCapWord;
        EncounterText.text = tile.HasEncounter ? tile.Encounter.Label : "Undiscovered";
        DangerLevelText.text = tile.DangerLevel.LabelCapWord;
        DangerLevelText.color = tile.DangerLevel.Color;

        CoordinatesText.text = $"Coordinates: {tile.Coordinates.x}, {tile.Coordinates.y}";
        HexDistanceText.text = $"Hex Distance: {tile.GetHexDistance(Game.Instance.CurrentPosition)}";
        int shortestPath = tile.GetShortestPath(Game.Instance.CurrentPosition);
        ShortestPathDistanceText.text = $"Shortest Path Distance: {(shortestPath >= 0 ? shortestPath.ToString() : "Unreachable")}";
    }

    private void ToggleDangerOverlay(bool value)
    {
        WorldMapRenderer.Instance.SetDangerOverlayVisible(value);
    }
}
