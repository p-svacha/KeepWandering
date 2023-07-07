using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An instance of this class contains all information about a single hex tile on the world map.
/// </summary>
public class WorldMapTile
{
    // World
    public WorldMap World;
    public Vector2Int Coordinates;
    public Vector2 WorldPosition;

    public Location Location { get; private set; }

    public WorldMapTile(WorldMap world, Vector2Int coordinates)
    {
        World = world;
        Coordinates = coordinates;
        Vector3 worldPos = World.HexGrid.CellToWorld(new Vector3Int(Coordinates.x, Coordinates.y, 0));
        WorldPosition = new Vector2(worldPos.x, worldPos.y);
    }

    public void SetLocation(Location loc)
    {
        Location = loc;
    }

    #region Getters

    /// <summary>
    /// Returns the adjacent tile in a specified direction
    /// </summary>
    public WorldMapTile GetAdjacentTile(Direction dir)
    {
        return World.GetTile(HelperFunctions.GetAdjacentHexCoordinates(Coordinates, dir));
    }

    /// <summary>
    /// Returns all existing adjacent tiles of this tile.
    /// </summary>
    public List<WorldMapTile> GetAdjacentTiles()
    {
        List<WorldMapTile> adjacentTiles = new List<WorldMapTile>();
        foreach(Direction dir in HelperFunctions.GetAdjacentHexDirections())
        {
            WorldMapTile adjacentTile = World.GetTile(HelperFunctions.GetAdjacentHexCoordinates(Coordinates, dir));
            if (adjacentTile != null) adjacentTiles.Add(adjacentTile);
        }

        return adjacentTiles;
    }

    /// <summary>
    /// Returns if this tile has an adjacent tile in the specifies direction.
    /// </summary>
    public bool HasAdjacentTile(Direction dir)
    {
        return World.GetTile(HelperFunctions.GetAdjacentHexCoordinates(Coordinates, dir)) != null;
    }

    public bool IsPassable()
    {
        return true;
    }

    public Vector2 North => WorldPosition + new Vector2(0f, 0.5f);
    public Vector2 NorthEast => WorldPosition + new Vector2(HelperFunctions.HEXAGON_SIDE2SIDE / 2f, 0.25f);
    public Vector2 SouthEast => WorldPosition + new Vector2(HelperFunctions.HEXAGON_SIDE2SIDE / 2f, -0.25f);
    public Vector2 South => WorldPosition + new Vector2(0f, -0.5f);
    public Vector2 SouthWest => WorldPosition + new Vector2(-HelperFunctions.HEXAGON_SIDE2SIDE / 2f, -0.25f);
    public Vector2 NorthWest => WorldPosition + new Vector2(-HelperFunctions.HEXAGON_SIDE2SIDE / 2f, 0.25f);

    public override string ToString()
    {
        return Location.ToString();
    }


    #endregion
}
