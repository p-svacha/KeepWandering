using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class WorldMapGenerator
{
    public static int MIN_CITY_SIZE = 3;
    public static int MAX_CITY_SIZE = 10;
    public static int MIN_BIOME_AREA_SIZE = 5;

    private const float EXISTING_ROAD_TILE_COST = 0.1f; // cost bias making pathing prefer merging into existing roads
    private const float NEW_TILE_COST = 1f;

    private static WorldMap WorldMap => WorldMap.Instance;
    private static WorldMapRenderer Renderer => WorldMapRenderer.Instance;

    private static PerlinNoise WaterNoise;
    private static PerlinNoise ForestNoise;
    private static PerlinNoise CityNoise;

    private static Dictionary<Vector2Int, WorldMapTile> Tiles;
    private static List<WorldMapTile> QuarantineZoneTiles;
    private static Area QuarantineZoneArea;
    private static List<WorldMapTile> OutsideZoneTiles;
    private static List<Area> Cities;
    private static List<Area> Forests;
    private static List<Area> Lakes;

    /// <summary>
    /// Generates a random world with a specified quarantine zone radius.
    /// <br/> The number of additional tiles will add random tiles to the perimeter to randomize the quarantine zone shape.
    /// </summary>
    public static WorldMap GenerateWorld(int zoneRadius, int numAdditionalTiles, int numCities)
    {
        // Initialize noisemaps
        WaterNoise = new PerlinNoise(scale: 0.15f);
        ForestNoise = new PerlinNoise(scale: 0.15f);
        CityNoise = new PerlinNoise(scale: 0.3f);

        // Add initial tiles
        Tiles = new Dictionary<Vector2Int, WorldMapTile>();

        AddTile(Vector2Int.zero);

        // Create base perimeter to have minimum radius of zone (-1 because we will expand a final time after adding random additional tiles)
        for (int i = 0; i < zoneRadius - 1; i++) ExpandMapEdge();

        // Expand random tiles along the perimeter
        for (int i = 0; i < numAdditionalTiles; i++) ExpandRandomTile();

        // Expand edge a final time to fill holes and smooth edges
        ExpandMapEdge();
        QuarantineZoneTiles = new List<WorldMapTile>(Tiles.Values); // all tiles generated so far will make up quarantine zone

        // Create the quarantine zone Area now - its Tiles list is already final, and this gives us
        // PerimeterTiles (fence tiles) early, needed by both connectivity validation and road generation.
        QuarantineZoneArea = new Area("Quarantine Zone", AreaTypeDefOf.QuarantineZone, QuarantineZoneTiles);

        // Create cities
        Cities = new List<Area>();
        GenerateCities(numCities);

        // Generate city labels
        foreach (Area city in Cities) Renderer.GenerateLabel(city);

        // Ensure every passable non-fence tile is reachable from the start without crossing impassable
        // or fence tiles. Must run after biomes (including cities) are set, and before biome-area grouping
        // below, since any carving here can change tile biomes.
        ValidateZoneConnectivity();

        // Group biome clusters into named areas
        Forests = GenerateBiomeAreas(BiomeDefOf.Woods, GetRandomForestName);
        Lakes = GenerateBiomeAreas(BiomeDefOf.Lake, GetRandomLakeName);

        // Show biome area labels
        foreach (Area area in Forests.Concat(Lakes)) Renderer.GenerateLabel(area);

        // Expand edge to create safety zone outside quarantine
        ExpandMapEdge();
        OutsideZoneTiles = Tiles.Values.Except(QuarantineZoneTiles).ToList();

        // Add roads
        AddRoads();

        // Draw quarantine zone fence
        QuarantineZoneArea.DrawPerimeterFence(ResourceManager.LoadMaterial("WorldMap/FenceMaterial"), 0.4f);

        // Create world map
        WorldMap worldMap = new WorldMap(Tiles, QuarantineZoneArea, Cities, Forests, Lakes);

        // Add fence encounters
        GenerateFenceEncounters();

        // Add landmarks to quarantine zone
        GenerateLandmarks();

        // Update renderer bounds
        Renderer.UpdateMapBounds(worldMap);

        // Render roads
        Renderer.RenderRoads(worldMap);

        // Populate background elements (trees, etc.)
        Renderer.PopulateTileElements(worldMap);

        // Clean up static state
        Tiles = null;
        QuarantineZoneArea = null;
        Cities = null;
        Forests = null;
        Lakes = null;
        WaterNoise = null;
        ForestNoise = null;
        CityNoise = null;

        return worldMap;
    }

    /// <summary>
    /// Adds a random tile at the edge of the map.
    /// </summary>
    private static void ExpandRandomTile()
    {
        List<Vector2Int> candidateCoordinates = new List<Vector2Int>();
        foreach (WorldMapTile tile in Tiles.Values)
        {
            foreach (Direction dir in HelperFunctions.GetAdjacentHexDirections())
            {
                if (!tile.HasAdjacentTile(dir))
                {
                    Vector2Int candidatePos = HelperFunctions.GetAdjacentHexCoordinates(tile.Coordinates, dir);
                    candidateCoordinates.Add(candidatePos);
                }
            }
        }

        Vector2Int chosenCoordinates = candidateCoordinates[Random.Range(0, candidateCoordinates.Count)];
        AddTile(chosenCoordinates);
    }

    /// <summary>
    /// Adds a layer of tiles at the edge of the map.
    /// </summary>
    private static void ExpandMapEdge()
    {
        // Identify all coordinates where a new tile needs to be added
        List<Vector2Int> coordinatesToExpand = new List<Vector2Int>();
        foreach (WorldMapTile tile in Tiles.Values)
        {
            foreach (Direction dir in HelperFunctions.GetAdjacentHexDirections())
            {
                if (!tile.HasAdjacentTile(dir))
                {
                    Vector2Int expandPos = HelperFunctions.GetAdjacentHexCoordinates(tile.Coordinates, dir);
                    if (!coordinatesToExpand.Contains(expandPos)) coordinatesToExpand.Add(expandPos);
                }
            }
        }

        // Add tile to all identified coordiantes
        foreach (Vector2Int coordinate in coordinatesToExpand) AddTile(coordinate);
    }

    /// <summary>
    /// Adds a tile at the specifies coordinates. Biome is set automatically.
    /// </summary>
    private static void AddTile(Vector2Int coordinates)
    {
        // Create Tile
        WorldMapTile newTile = new WorldMapTile(Tiles, coordinates);
        Tiles.Add(coordinates, newTile);

        // Set Biome (may be overriden in upcoming steps)
        BiomeDef biome = BiomeDefOf.Outskirts;
        if (WaterNoise.GetValue(coordinates) > 0.63f) biome = BiomeDefOf.Lake;
        else if (ForestNoise.GetValue(coordinates) > 0.63f) biome = BiomeDefOf.Woods;
        newTile.SetBiome(biome);
    }

    private static void GenerateCities(int numCities)
    {
        // Cities are generated by selecting a start tile and then randomly expanding out from it until the desired city size is reached. The start tile cannot be in a city and cities must have at least one tile between them.

        HashSet<WorldMapTile> fenceTiles = new HashSet<WorldMapTile>(QuarantineZoneArea.GetOrderedPerimeterTiles());
        HashSet<WorldMapTile> allCityTiles = new HashSet<WorldMapTile>();
        HashSet<WorldMapTile> cityBufferTiles = new HashSet<WorldMapTile>(); // tiles adjacent to a city (buffer zone)

        for (int i = 0; i < numCities; i++)
        {
            int targetSize = Random.Range(MIN_CITY_SIZE, MAX_CITY_SIZE + 1);

            // Find a valid start tile: not the start tile, not on the fence, not in a city, not in the buffer zone, and passable
            List<WorldMapTile> candidateStartTiles = Tiles.Values
                .Where(t => t.Coordinates != Vector2Int.zero
                    && t.IsPassable()
                    && !fenceTiles.Contains(t)
                    && !allCityTiles.Contains(t)
                    && !cityBufferTiles.Contains(t))
                .ToList();

            if (candidateStartTiles.Count == 0) break;

            WorldMapTile startTile = candidateStartTiles[Random.Range(0, candidateStartTiles.Count)];

            // Expand city from the start tile
            List<WorldMapTile> cityTiles = new List<WorldMapTile> { startTile };
            for (int j = 1; j < targetSize; j++)
            {
                // Collect all passable neighbor tiles of existing city tiles that are not yet part of any city or buffer zone
                List<WorldMapTile> expansionCandidates = new List<WorldMapTile>();
                foreach (WorldMapTile cityTile in cityTiles)
                {
                    foreach (WorldMapTile adj in cityTile.GetAdjacentTiles())
                    {
                        if (adj.IsPassable()
                            && !fenceTiles.Contains(adj)
                            && !allCityTiles.Contains(adj)
                            && !cityBufferTiles.Contains(adj)
                            && !cityTiles.Contains(adj))
                        {
                            expansionCandidates.Add(adj);
                        }
                    }
                }

                if (expansionCandidates.Count == 0) break;

                WorldMapTile chosenTile = expansionCandidates[Random.Range(0, expansionCandidates.Count)];
                cityTiles.Add(chosenTile);
            }

            // Set biome to City for all tiles in this city
            foreach (WorldMapTile tile in cityTiles)
            {
                tile.SetBiome(BiomeDefOf.City);
                allCityTiles.Add(tile);
            }

            // Add buffer zone (all neighbors of city tiles that are not in the city)
            foreach (WorldMapTile tile in cityTiles)
            {
                foreach (WorldMapTile adj in tile.GetAdjacentTiles())
                {
                    if (!allCityTiles.Contains(adj)) cityBufferTiles.Add(adj);
                }
            }

            // Create area for the city
            Area cityArea = new Area(GetRandomCityName(), AreaTypeDefOf.City, cityTiles);
            Cities.Add(cityArea);
        }
    }
    private static string GetRandomCityName()
    {
        string[] prefixes = { "New ", "Old ", "North ", "South ", "East ", "West " };
        string[] suffixes = { "ville", "town", "city", "burg", "polis", "grad" };
        string[] middles = { "wood", "field", "stone", "river", "hill", "port", "ford", "haven" };
        string[] names = { "Ash", "Bright", "Dark", "Green", "High", "Low", "Red", "White", "Wind", "Wolf" };

        bool hasPrefix = Random.value < 0.2f;
        bool hasSuffix = Random.value < 0.2f;
        string prefix = hasPrefix ? prefixes[Random.Range(0, prefixes.Length)] : "";
        string middle = middles[Random.Range(0, middles.Length)];
        string name = names[Random.Range(0, names.Length)];
        string suffix = hasSuffix ? suffixes[Random.Range(0, suffixes.Length)] : "";
        return prefix + name + middle + suffix;
    }

    /// <summary>
    /// Finds all clusters of adjacent tiles sharing a given biome and creates named areas for clusters above the minimum size.
    /// </summary>
    private static List<Area> GenerateBiomeAreas(BiomeDef biome, System.Func<string> nameGenerator)
    {
        List<Area> areas = new List<Area>();
        HashSet<WorldMapTile> visited = new HashSet<WorldMapTile>();

        foreach (WorldMapTile tile in Tiles.Values)
        {
            if (tile.Biome != biome || visited.Contains(tile)) continue;

            // Flood fill to find the full cluster
            List<WorldMapTile> cluster = new List<WorldMapTile>();
            Queue<WorldMapTile> queue = new Queue<WorldMapTile>();
            queue.Enqueue(tile);
            visited.Add(tile);

            while (queue.Count > 0)
            {
                WorldMapTile current = queue.Dequeue();
                cluster.Add(current);

                foreach (WorldMapTile adj in current.GetAdjacentTiles())
                {
                    if (adj.Biome == biome && !visited.Contains(adj))
                    {
                        visited.Add(adj);
                        queue.Enqueue(adj);
                    }
                }
            }

            if (cluster.Count >= MIN_BIOME_AREA_SIZE)
            {
                AreaTypeDef areaType = biome == BiomeDefOf.Woods ? AreaTypeDefOf.Forest : AreaTypeDefOf.Lake;

                areas.Add(new Area(nameGenerator(), areaType, cluster));
            }
        }

        return areas;
    }

    private static string GetRandomForestName()
    {
        string[] adjectives = { "Dark", "Green", "Whispering", "Silent", "Mossy", "Twisted", "Ancient", "Misty", "Hollow", "Shadowed" };
        string[] nouns = { "Forest", "Woods", "Thicket", "Grove", "Timberland" };

        string adjective = adjectives[Random.Range(0, adjectives.Length)];
        string noun = nouns[Random.Range(0, nouns.Length)];
        return adjective + " " + noun;
    }

    private static string GetRandomLakeName()
    {
        string[] adjectives = { "Crystal", "Still", "Deep", "Black", "Blue", "Silver", "Murky", "Hidden", "Frozen", "Sunken" };
        string[] nouns = { "Lake", "Pond", "Reservoir", "Basin", "Waters" };

        string adjective = adjectives[Random.Range(0, adjectives.Length)];
        string noun = nouns[Random.Range(0, nouns.Length)];
        return adjective + " " + noun;
    }

    private static void GenerateFenceEncounters()
    {
        // Add a fence encounter to each passable tile along the perimeter of the quarantine zone.
        foreach (WorldMapTile tile in WorldMap.QuarantineZone.GetOrderedPerimeterTiles())
        {
            if (tile.IsPassable() && tile.Encounter == null)
            {
                Game.Instance.SetLocationEncounter(tile, EncounterDefOf.QuarantineFence, hidden: true);
            }
        }
    }

    private static void GenerateLandmarks()
    {
        foreach (EncounterDef landmark in DefDatabase<EncounterDef>.AllDefs.Where(def => def.Type == EncounterType.Landmark))
        {
            int numOccurences = Random.Range(landmark.MinOccurences, landmark.MaxOccurences + 1);
            for (int i = 0; i < numOccurences; i++)
            {
                // Make dictionary of all candidate tiles and their probability
                Dictionary<WorldMapTile, float> candidateTiles = new Dictionary<WorldMapTile, float>();
                foreach (WorldMapTile tile in Tiles.Values)
                {
                    // Must be in quarantine zone
                    if (!WorldMap.QuarantineZone.ContainsTile(tile)) continue;

                    // Must be passable
                    if (!tile.IsPassable()) continue;

                    // Skip start tile
                    if (tile.Coordinates == Vector2Int.zero) continue;

                    // Distance from start
                    if (landmark.MinDistanceFromStart > 0 && tile.DistanceFromStart < landmark.MinDistanceFromStart) continue;

                    // Check if occupied already
                    if (tile.Encounter != null) continue;

                    // Distance from other occurences of this landmark
                    if (landmark.MinDistanceBetween > 0)
                    {
                        bool tooClose = false;
                        foreach (WorldMapTile otherTile in Tiles.Values)
                        {
                            if (otherTile.Encounter != null && otherTile.Encounter.Def == landmark)
                            {
                                int distance = tile.GetHexDistance(otherTile);
                                if (distance < landmark.MinDistanceBetween)
                                {
                                    tooClose = true;
                                    break;
                                }
                            }
                        }
                        if (tooClose) continue;
                    }

                    // Biome modifier
                    if (landmark.BiomeProbabilityOverrides.Count > 0)
                    {
                        if (landmark.BiomeProbabilityOverrides.ContainsKey(tile.Biome)) candidateTiles.Add(tile, landmark.BiomeProbabilityOverrides[tile.Biome]);
                    }
                    else candidateTiles.Add(tile, 1f); // If no biome requirements, all tiles are valid with equal probability
                }

                // Pick location
                if (candidateTiles.Count == 0)
                {
                    Debug.LogWarning($"No valid location found for landmark {landmark.Label}");
                    continue;
                }
                WorldMapTile location = candidateTiles.GetWeightedRandomElement();

                // Add encounter to tile
                Game.Instance.SetLocationEncounter(location, landmark);
            }
        }
    }

    #region Connectivity Validation

    /// <summary>
    /// Ensures every passable, non-fence tile within the quarantine zone is reachable from the start tile
    /// without crossing an impassable tile or a fence (perimeter) tile. If disconnected interior clusters
    /// are found, each is connected to the main cluster (the one containing the start tile) by carving a
    /// path of Outskirts tiles through impassable terrain - the carved path itself never crosses a fence tile.
    /// <br/>Fence tiles themselves are not required to route through non-fence tiles to be reachable; given
    /// the zone is grown as a single connected blob, they are naturally reachable once interior connectivity holds.
    /// </summary>
    private static void ValidateZoneConnectivity()
    {
        HashSet<WorldMapTile> fenceTiles = new HashSet<WorldMapTile>(QuarantineZoneArea.GetOrderedPerimeterTiles());
        HashSet<WorldMapTile> zoneTileSet = new HashSet<WorldMapTile>(QuarantineZoneTiles);

        // Candidate interior tiles: passable, in zone, not a fence tile
        List<WorldMapTile> interiorTiles = QuarantineZoneTiles
            .Where(t => t.IsPassable() && !fenceTiles.Contains(t))
            .ToList();

        List<List<WorldMapTile>> clusters = FindClusters(interiorTiles);
        if (clusters.Count <= 1) return; // already fully connected (or trivially empty)

        // Identify the main cluster (the one containing the start tile)
        WorldMapTile startTile = Tiles[Vector2Int.zero];
        List<WorldMapTile> mainCluster = clusters.First(c => c.Contains(startTile));
        HashSet<WorldMapTile> mainClusterSet = new HashSet<WorldMapTile>(mainCluster);

        foreach (List<WorldMapTile> cluster in clusters.Where(c => c != mainCluster))
        {
            ConnectClusterToMain(cluster, mainClusterSet, fenceTiles, zoneTileSet);
        }
    }

    /// <summary>
    /// Groups the given tiles into connected clusters, where adjacency is only considered within the given tile set.
    /// </summary>
    private static List<List<WorldMapTile>> FindClusters(List<WorldMapTile> candidateTiles)
    {
        HashSet<WorldMapTile> candidateSet = new HashSet<WorldMapTile>(candidateTiles);
        HashSet<WorldMapTile> visited = new HashSet<WorldMapTile>();
        List<List<WorldMapTile>> clusters = new List<List<WorldMapTile>>();

        foreach (WorldMapTile tile in candidateTiles)
        {
            if (visited.Contains(tile)) continue;

            List<WorldMapTile> cluster = new List<WorldMapTile>();
            Queue<WorldMapTile> queue = new Queue<WorldMapTile>();
            queue.Enqueue(tile);
            visited.Add(tile);

            while (queue.Count > 0)
            {
                WorldMapTile current = queue.Dequeue();
                cluster.Add(current);

                foreach (WorldMapTile adj in current.GetAdjacentTiles())
                {
                    if (candidateSet.Contains(adj) && !visited.Contains(adj))
                    {
                        visited.Add(adj);
                        queue.Enqueue(adj);
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    /// <summary>
    /// Finds the shortest route from the given cluster to the main cluster, allowed to pass through any
    /// zone tile that isn't a fence tile (regardless of current passability), then carves that route by
    /// switching any impassable tile along it to Outskirts biome.
    /// </summary>
    private static void ConnectClusterToMain(List<WorldMapTile> cluster, HashSet<WorldMapTile> mainClusterSet, HashSet<WorldMapTile> fenceTiles, HashSet<WorldMapTile> zoneTileSet)
    {
        Queue<WorldMapTile> frontier = new Queue<WorldMapTile>();
        Dictionary<WorldMapTile, WorldMapTile> cameFrom = new Dictionary<WorldMapTile, WorldMapTile>();

        foreach (WorldMapTile tile in cluster)
        {
            frontier.Enqueue(tile);
            cameFrom[tile] = null;
        }

        WorldMapTile reached = null;
        while (frontier.Count > 0)
        {
            WorldMapTile current = frontier.Dequeue();
            if (mainClusterSet.Contains(current))
            {
                reached = current;
                break;
            }

            foreach (WorldMapTile adj in current.GetAdjacentTiles())
            {
                if (fenceTiles.Contains(adj)) continue; // never cross fence tiles
                if (!zoneTileSet.Contains(adj)) continue; // stay within the zone
                if (cameFrom.ContainsKey(adj)) continue;
                cameFrom[adj] = current;
                frontier.Enqueue(adj);
            }
        }

        if (reached == null)
        {
            Debug.LogWarning("Could not find a connection path between a disconnected cluster and the main cluster.");
            return;
        }

        // Walk back from the reached tile to the cluster, carving impassable tiles along the way
        WorldMapTile step = reached;
        while (step != null && !cluster.Contains(step))
        {
            if (!step.IsPassable()) step.SetBiome(BiomeDefOf.Outskirts);
            step = cameFrom[step];
        }
    }

    #endregion

    #region Roads

    private static void AddRoads()
    {
        HashSet<WorldMapTile> fenceTiles = new HashSet<WorldMapTile>(QuarantineZoneArea.GetOrderedPerimeterTiles());

        List<WorldMapTile> pois = new List<WorldMapTile>();

        // Player start
        pois.Add(Tiles[Vector2Int.zero]);

        // Random passable fence tile that has at least one directly-adjacent interior (non-fence) tile,
        // guaranteeing a road can reach it without ever crossing another fence tile along the way.
        List<WorldMapTile> validGateCandidates = fenceTiles
            .Where(t => t.IsPassable() && t.GetAdjacentTiles().Any(adj => adj.IsPassable() && !fenceTiles.Contains(adj)))
            .ToList();

        if (validGateCandidates.Count > 0) pois.Add(validGateCandidates.RandomElement());
        else Debug.LogError("No valid fence gate candidate found with direct interior access - skipping fence gate road POI.");

        // A random tile within each city
        foreach (Area city in Cities) pois.Add(city.GetRandomPassableTile());

        GenerateRoadNetwork(pois, fenceTiles);

        // Add a road to one tile outside the quarantine zone connected to the fence gate
        List<WorldMapTile> outsideCandidates = OutsideZoneTiles.Where(t => t.IsPassable() && t.GetAdjacentTiles().Any(adj => adj.HasRoad)).ToList();
        if (outsideCandidates.Count > 0)
        {
            outsideCandidates.RandomElement().AddRoad();
        }
    }

    /// <summary>
    /// Connects all given points of interest with a road network. Builds a minimum spanning tree over the
    /// POIs (by shortest-path distance), then lays down actual road tiles along each MST edge in ascending
    /// distance order, using road-biased pathfinding so later edges prefer merging into roads built by
    /// earlier edges instead of carving new parallel paths.
    /// <br/>Paths never cross a fence tile, except where a POI itself is a fence tile (e.g. the fence gate).
    /// </summary>
    private static void GenerateRoadNetwork(List<WorldMapTile> pois, HashSet<WorldMapTile> fenceTiles)
    {
        if (pois.Count < 2) return;

        int n = pois.Count;

        // Pairwise distances (respecting the fence restriction) used purely for MST weighting
        float[,] distances = new float[n, n];
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                List<WorldMapTile> path = FindPath(pois[i], pois[j], fenceTiles);
                float dist = path != null ? path.Count : float.MaxValue;
                distances[i, j] = dist;
                distances[j, i] = dist;
            }
        }

        // Prim's algorithm to build MST edges over the POIs
        List<(int from, int to)> mstEdges = new List<(int, int)>();
        bool[] inTree = new bool[n];
        inTree[0] = true;
        int numInTree = 1;

        while (numInTree < n)
        {
            float bestDist = float.MaxValue;
            int bestFrom = -1, bestTo = -1;

            for (int i = 0; i < n; i++)
            {
                if (!inTree[i]) continue;
                for (int j = 0; j < n; j++)
                {
                    if (inTree[j]) continue;
                    if (distances[i, j] < bestDist)
                    {
                        bestDist = distances[i, j];
                        bestFrom = i;
                        bestTo = j;
                    }
                }
            }

            if (bestTo == -1 || bestDist == float.MaxValue) break; // remaining POIs unreachable
            mstEdges.Add((bestFrom, bestTo));
            inTree[bestTo] = true;
            numInTree++;
        }

        // Build shorter connections first, so longer ones have more existing road to merge into
        mstEdges = mstEdges.OrderBy(e => distances[e.from, e.to]).ToList();

        foreach (var (i, j) in mstEdges)
        {
            List<WorldMapTile> roadPath = FindRoadBiasedPath(pois[i], pois[j], fenceTiles);
            if (roadPath != null) AddRoad(roadPath);
        }
    }

    private static void AddRoad(List<WorldMapTile> road)
    {
        foreach (WorldMapTile tile in road)
        {
            if (!tile.HasRoad) tile.AddRoad();
        }
    }

    /// <summary>
    /// Same shortest-path search as FindPath, but each step onto a tile that already has a road costs much
    /// less than a step onto a fresh tile — biasing the route to merge into existing roads where reasonable
    /// rather than always taking the absolute shortest raw-distance route.
    /// <br/>If disallowedTiles is given, no intermediate tile may belong to it (the 'from'/'to' endpoints are exempt).
    /// </summary>
    public static List<WorldMapTile> FindRoadBiasedPath(WorldMapTile from, WorldMapTile to, HashSet<WorldMapTile> disallowedTiles = null)
    {
        if (from == null || to == null) return null;
        if (!from.IsPassable() || !to.IsPassable()) return null;
        if (from == to) return new List<WorldMapTile> { from };

        Dictionary<WorldMapTile, float> bestDist = new Dictionary<WorldMapTile, float>() { { from, 0f } };
        Dictionary<WorldMapTile, WorldMapTile> cameFrom = new Dictionary<WorldMapTile, WorldMapTile>();
        List<WorldMapTile> frontier = new List<WorldMapTile>() { from };

        while (frontier.Count > 0)
        {
            WorldMapTile current = frontier.OrderBy(t => bestDist[t]).First();
            frontier.Remove(current);
            if (current == to) break;

            foreach (WorldMapTile next in current.GetAdjacentTiles())
            {
                if (!next.IsPassable()) continue;
                if (disallowedTiles != null && disallowedTiles.Contains(next) && next != to) continue;

                float stepCost = next.HasRoad ? EXISTING_ROAD_TILE_COST : NEW_TILE_COST;
                float newDist = bestDist[current] + stepCost;

                if (!bestDist.TryGetValue(next, out float existingDist) || newDist < existingDist)
                {
                    bestDist[next] = newDist;
                    cameFrom[next] = current;
                    if (!frontier.Contains(next)) frontier.Add(next);
                }
            }
        }

        if (to != from && !cameFrom.ContainsKey(to)) return null;

        List<WorldMapTile> path = new List<WorldMapTile>();
        WorldMapTile step = to;
        while (step != from)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Add(from);
        path.Reverse();
        return path;
    }

    /// <summary>
    /// Returns the shortest path of passable tiles from 'from' to 'to' (inclusive of both),
    /// or null if no passable path exists. All transitions are treated as equal cost.
    /// <br/>If disallowedTiles is given, no intermediate tile may belong to it (the 'from'/'to' endpoints are exempt).
    /// </summary>
    public static List<WorldMapTile> FindPath(WorldMapTile from, WorldMapTile to, HashSet<WorldMapTile> disallowedTiles = null)
    {
        if (from == null || to == null) return null;
        if (!from.IsPassable() || !to.IsPassable()) return null;
        if (from == to) return new List<WorldMapTile> { from };

        Queue<WorldMapTile> frontier = new Queue<WorldMapTile>();
        Dictionary<WorldMapTile, WorldMapTile> cameFrom = new Dictionary<WorldMapTile, WorldMapTile>();

        frontier.Enqueue(from);
        cameFrom[from] = null;

        while (frontier.Count > 0)
        {
            WorldMapTile current = frontier.Dequeue();
            if (current == to) break; // early exit once target is reached

            foreach (WorldMapTile next in current.GetAdjacentTiles())
            {
                if (!next.IsPassable()) continue;
                if (disallowedTiles != null && disallowedTiles.Contains(next) && next != to) continue;
                if (cameFrom.ContainsKey(next)) continue; // already discovered
                cameFrom[next] = current;
                frontier.Enqueue(next);
            }
        }

        if (!cameFrom.ContainsKey(to)) return null; // no path found

        // Reconstruct path by walking parents back from the target
        List<WorldMapTile> path = new List<WorldMapTile>();
        WorldMapTile step = to;
        while (step != null)
        {
            path.Add(step);
            step = cameFrom[step];
        }
        path.Reverse();
        return path;
    }


    #endregion
}