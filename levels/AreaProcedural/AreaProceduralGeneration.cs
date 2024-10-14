using Godot;
using Godot.Collections;
using System;
using static Godot.Time;

public partial class AreaProceduralGeneration : Node
{
    // Constants for generating rooms in our generator
    private int numRooms = 40;
    private int minRoomWidth = 5;
    private int maxRoomWidth = 30;
    private int minRoomHeight = 5;
    private int maxRoomHeight = 30;


    public enum Dirs
    {
        EAST = 0,
        SOUTH = 1,
        WEST = 2,
        NORTH = 3
    }

    public enum TileTypes
    {
        // UNDEFINED
        TITLETYPE_UNDEFINED = -3,

        // Impenetriable border
        TITLETYPE_MAP_BORDER = -2,

        // FLOOR TILE
        TITLETYPE_FLOOR = -1,

        // SINGLESIDED STRAIGHT WALLS
        TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT = 0,
        TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT = 1,
        TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP = 2,
        TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM = 3,

        // DOUBLEWALL NEIGHBORS
        // ONESIDED CORNERS -- connecting single walls
        TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT = 4,  // walls to bottom and right
        TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT = 5, // walls to bottom and left
        TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT = 6, // walls to top and right
        TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT = 7, // walls to top and left

        // DOUBLEWALL STRAIGHT -- OPPOSITE SIDES ARE FLOORS
        TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT = 8, // floors to left and right
        TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ = 9,// floors to top and bottom

        // DOUBLESIDED CORNERS -- connecting double walls
        TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPLEFT = 10, // walls to bottom and right
        TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPRIGHT = 11, // walls to bottom and left
        TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMLEFT = 12, // walls to top and right
        TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMRIGHT = 13, // walls to top and left

        // TRIPLE WALL NEIGHBORS -- connecting double walls
        TITLETYPE_WALL_DOUBLESIDE_TEE_LEFT = 14,
        TITLETYPE_WALL_DOUBLESIDE_TEE_RIGHT = 15,
        TITLETYPE_WALL_DOUBLESIDE_TEE_TOP = 16,
        TITLETYPE_WALL_DOUBLESIDE_TEE_BOTTOM = 17,

        // DOUBLEWALL DEAD ENDS -- connecting a single double wall -- three floor neighbors
        TITLETYPE_WALL_DOUBLESIDE_DEADEND_LEFT = 18,
        TITLETYPE_WALL_DOUBLESIDE_DEADEND_RIGHT = 19,
        TITLETYPE_WALL_DOUBLESIDE_DEADEND_TOP = 20,
        TITLETYPE_WALL_DOUBLESIDE_DEADEND_BOTTOM = 21,

        // DOUBLEWALL REENTRANT CORNERS
        TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPLEFT = 22,  // walls to bottom and right
        TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT = 23, // walls to bottom and left
        TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMLEFT = 24, // walls to top and right
        TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT = 25, // walls to top and left

        // QUADRUPLE FLOOR NEIGHBORS -- double wall
        TITLETYPE_WALL_DOUBLESIDE_CROSS = 26,

        // GENERIC INTERSECTON
        TITLETYPE_WALL_GENERIC_INTERSECTION = 27,
        TILETYPE_WALL_CORNER = 28

    }

    // Dictionary of map symbols
    Dictionary<TileTypes, string> MapSymbols = new Dictionary<TileTypes, string>()
    {
        { TileTypes.TITLETYPE_UNDEFINED, "."},

        { TileTypes.TITLETYPE_MAP_BORDER, "*"},

        { TileTypes.TITLETYPE_FLOOR, "x"},

        { TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT, "W"},
        { TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT, "W"},
        { TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP, "W"},
        { TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM, "W"},

        { TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT, "W"},
        { TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT, "W"},
        { TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT, "W"},
        { TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT, "W"},

        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT, "|"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ, "="},

        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPLEFT, "1"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPRIGHT, "2"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMLEFT, "3"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMRIGHT, "4"},

        // CONNECTING THREE DOUBLE SIDE WALLS
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_LEFT, "T"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_RIGHT, "T"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_TOP, "T"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_BOTTOM, "T"},

        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_LEFT, "o"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_RIGHT, "o"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_TOP, "o"},
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_BOTTOM, "o"},

        // TRIPLE FLOOR NEIGHBORS
        { TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPLEFT, "A"},
        { TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT, "B"},
        { TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMLEFT, "C"},
        { TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT, "D"},

        // QUADRUPLE FLOOR NEIGHBORS
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_CROSS, "+"},

        // GENERIC INERSECTION
        { TileTypes.TITLETYPE_WALL_GENERIC_INTERSECTION, "w"},

        { TileTypes.TILETYPE_WALL_CORNER, "c"}
    };

    TileMapLayer floors;
    TileMapLayer walls;
    PlayerSpawn playerSpawn;

    TileTypes[] room_map;

    // Remember to change the TML layer TileSize parameter in the GODOT inspector
    //int tile_size = 16; // for the purple dungeon tileset
    int tile_size = 32; // for the custon Duhamel ungeon tileset

    // number of dead cells beyond room walls -- includes impenetrable width amount
    // need to make sure it's at least 1 more than impenetrable border so we can fit the wall tiles
    int fringe = 14;
    int impenetrable_border = 1;  // impenetrable width -- included in the fringe value
    int width;
    int height;
    int total_width;
    int total_height;


    int wall_loop_counter = 100;  // to help us get out of our exit while loop in FindWalls if we get stuck;
    int wall_loop_counter_max = 100;
    int wall_corner_loop_counter = 100;  // to help us get out of our exit while loop in FindWallCorners if we get stuck;
    int wall_corner_loop_counter_max = 100;
    int fill_extra_loop_counter = 100;  // to help us get out of our exit while loop in FillExtraRoomHoles if we get stuck;
    int fill_extra_loop_counter_max = 100;
    int reentrant_corner_loop_counter = 100;  // to help us get out of our exit while loop in FindReentrantCorners if we get stuck;
    int reentrant_corner_loop_counter_max = 100;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        floors = GetNode<TileMapLayer>("TML_Floors");
        walls = GetNode<TileMapLayer>("TML_Walls");
        playerSpawn = GetNode<PlayerSpawn>("PlayerSpawn");

        ImportData data = new ImportData();


        // Create room area layout and fringe areas and impenetrable border
        GD.Print("Generating room areas...");
        ProceduralMapGenerate();
        PrintMap();

        GD.Print("Adding map border...");
        AddMapBorder();

        // Find the walls and reentrant corners
        GD.Print("Finding walls...");
        // Run the wall command multiple times just in case
        for (int i = 0; i < 2; i++)
        {
            FindWalls();  // this function will iterate until all single walls are found.
            FindWallCorners();

            // reset the loop counters
            wall_loop_counter = wall_loop_counter_max;
            reentrant_corner_loop_counter = reentrant_corner_loop_counter_max;
            wall_corner_loop_counter = wall_corner_loop_counter_max;
            fill_extra_loop_counter = fill_extra_loop_counter_max;

            //            PrintMap();

        }

        GD.Print("Map Generated");

        PrintMap();

        // Now Render the map to the scene
        RenderMap();


        // Set the player spawn location -- find a random tile designated as a floor
        var rng = new RandomNumberGenerator();
        while (true)
        {
            var rand_x = rng.RandiRange(0, total_width - 1);
            var rand_y = rng.RandiRange(0, total_height - 1);
            if (room_map[rand_y * total_width + rand_x] == TileTypes.TITLETYPE_FLOOR)
            {
                // set the player
                GlobalPlayerManager.Instance.player.GlobalPosition = new Vector2(rand_x * tile_size, rand_y * tile_size);

                // set the player spawn -- for future reloads
                playerSpawn.Position = new Vector2(rand_x * tile_size, rand_y * tile_size);
                break;
            }
        }




    }

    public override void _Process(double delta)
    {

    }

    /// <summary>
    /// Procedurally creates the specified room floor areas
    /// </summary>
    public void ProceduralMapGenerate()
    {
        var rng = new RandomNumberGenerator();

        int[] w_data = new int[numRooms];
        int[] h_data = new int[numRooms];
        int[] posx_data = new int[numRooms];
        int[] posy_data = new int[numRooms];

        // create floor areas borders
        for (int i = 0; i < numRooms; i++)
        {
            int width = rng.RandiRange(minRoomWidth, maxRoomHeight);
            int height = rng.RandiRange(minRoomHeight, maxRoomHeight);
            int pos_x = rng.RandiRange(0, (int)2*maxRoomWidth);
            int pos_y = rng.RandiRange(0, (int)2*maxRoomHeight);

            // store the randomizd parameters
            w_data[i] = width;
            h_data[i] = height;
            posx_data[i] = pos_x;
            posy_data[i] = pos_y;

            //GD.Print("Room (" + i + "):  w: " + width + "  h: " + height + " x: " + pos_x + " y: " + pos_y);
        }

        // find the leftmost point of the areas
        int min_x = 0;
        int temp_min_x = 100000;
        for (int i = 0; i < numRooms; i++)
        {
            if (posx_data[i] < temp_min_x)
            {
                temp_min_x = posx_data[i];
            }
        }
        min_x = temp_min_x;

        var offset_x = fringe - min_x; // ensure we have a fringe value on the left side

        for (int i = 0; i < numRooms; i++)
        {
            posx_data[i] += offset_x;
        }


        // find the rightmost point of the areas
        int max_x = 0;
        int temp_max_x = -100000;
        for (int i = 0; i < numRooms; i++)
        {
            if (posx_data[i] + w_data[i] > temp_max_x)
            {
                temp_max_x = posx_data[i] + w_data[i];
            }
        }
        max_x = temp_max_x;


        // find the topmost point of the areas
        int min_y = 0;
        int temp_min_y = 100000;
        for (int i = 0; i < numRooms; i++)
        {
            if (posy_data[i] < temp_min_y)
            {
                temp_min_y = posy_data[i];
            }
        }
        min_y = temp_min_y;

        var offset_y = fringe - min_y; // ensure we have a fringe value

        for (int i = 0; i < numRooms; i++)
        {
            posy_data[i] += offset_y;
        }

        // find the bottommost point of the areas
        int max_y = 0;
        int temp_max_y = -100000;
        for (int i = 0; i < numRooms; i++)
        {
            if (posy_data[i] + h_data[i] > temp_max_y)
            {
                temp_max_y = posy_data[i] + h_data[i];
            }
        }
        max_y = temp_max_y;


        //GD.Print("min_x: " + min_x);
        //GD.Print("max_x: " + max_x);
        //GD.Print("min_y: " + min_y);
        //GD.Print("max_y: " + max_y);

        total_width = max_x + fringe;  // add fringe value to the right
        total_height = max_y + fringe; // add fringe value to the bottom
        //GD.Print("total_map_width: " + total_width);
        //GD.Print("total_map_height: " + total_height);

        // Set all cells to undefined type
        room_map = new TileTypes[total_width * total_height];
        for (int i = 0; i < total_width * total_height; i++)
        {
            room_map[i] = TileTypes.TITLETYPE_UNDEFINED;
        }

        // Create the room areas
        for (int i = 0; i < numRooms; i++)
        {
            CreateFloorAreas(w_data[i], h_data[i], posx_data[i], posy_data[i]);
        }
    }

    /// <summary>
    /// Adds the map border (makes searching room by room much simpler)
    /// </summary>
    private void AddMapBorder()
    {
        // Add the impenetrable border around the outside of the map
        for (int j = 0; j < total_height; j++)
        {
            for (int i = 0; i < total_width; i++)
            {
                if (i < impenetrable_border || i >= (total_width - impenetrable_border) || j < impenetrable_border || j >= (total_height - impenetrable_border))
                {
                    room_map[j * total_width + i] = TileTypes.TITLETYPE_MAP_BORDER;
                }
            }
        }
    }


    /// <summary>
    /// Prints an ascii representation of the map based on the map symbol dictionary
    /// </summary>
    private void PrintMap()
    {
        for (int j = 0; j < total_height; j++)
        {
            string str = "";
            for (int i = 0; i < total_width; i++)
            {
                var val = room_map[j * total_width + i];
                str += MapSymbols[val].ToString();
            }
            GD.Print(str);
        }
    }

    /// <summary>
    /// Creates the floor area regions and adds them to the room_map with
    /// appropriate tiletype.
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="pos_x"></param>
    /// <param name="pos_y"></param>
    private void CreateFloorAreas(int width, int height, int pos_x, int pos_y)
    {
        GD.Print("Creating room:  w: " + width + "  h: " + height + " x: " + pos_x + " y: " + pos_y);
        // create room 1
        for (int j = 0; j < total_height; j++)
        {
            for (int i = 0; i < total_width; i++)
            {
                // the origin of the room
                if (i == 0 && j == 0)
                {

                }
                // outside the room area
                else if (j < pos_y || j >= pos_y + height || i < pos_x || i >= pos_x + width)
                {

                }
                else
                // We've found a floor tile of a room
                //else
                {
                    room_map[j * total_width + i] = TileTypes.TITLETYPE_FLOOR;
                }
            }
        }
    }

    /// <summary>
    /// Logic to locate walls around the outside of the room area we previously created.
    /// </summary>
    public void FindWalls()
    {
        bool change_made = true;
        wall_loop_counter--;
        // Loop until there are no more changes
        while (change_made)
        {
            if (wall_loop_counter < 0)
            {
                GD.Print("Looping too many times.  Breaking out of level creation loop.");
                break;
            }

            change_made = false;
            for (int j = 0; j < total_height; j++)
            {
                for (int i = 0; i < total_width; i++)
                {
                    // if our current tile is not undefined, then it's already been set and we can continue on.
                    if (room_map[j * total_width + i] != TileTypes.TITLETYPE_UNDEFINED)
                    {
                        continue;
                    }

                    // If there is not a floor tile in any of the eight directions, then this cannot be a wall so we continue on.
                    if(HasFloorNeighbor(i, j) is false)
                    {
                        continue;
                    }

                    bool[] floor_neighbors = CheckFloorNeighbors(i, j);
                    int floor_count = countTrueNeighbors(floor_neighbors);

                    bool[] wall_neighbors = CheckWallNeighbors(i, j);
                    int wall_count = countTrueNeighbors(wall_neighbors);

                    bool north_floor = floor_neighbors[(int)Dirs.NORTH];
                    bool south_floor = floor_neighbors[(int)Dirs.SOUTH];
                    bool east_floor = floor_neighbors[(int)Dirs.EAST];
                    bool west_floor = floor_neighbors[(int)Dirs.WEST];

                    bool north_wall = wall_neighbors[(int)Dirs.NORTH];
                    bool south_wall = wall_neighbors[(int)Dirs.SOUTH];
                    bool east_wall = wall_neighbors[(int)Dirs.EAST];
                    bool west_wall = wall_neighbors[(int)Dirs.WEST];

                    // A single floor neighbor -- this must be a wall
                    if (floor_count == 1)
                    {
                        if (floor_neighbors[(int)Dirs.EAST] is true)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT;
                            change_made = true;
                            continue;
                        }
                        else if (floor_neighbors[(int)Dirs.SOUTH] is true)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP;
                            change_made = true;
                            continue;
                        }
                        else if (floor_neighbors[(int)Dirs.WEST] is true)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT;
                            change_made = true;
                            continue;
                        }
                        else if (floor_neighbors[(int)Dirs.NORTH] is true)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM;
                            change_made = true;
                            continue;
                        }
                    }

                    if (floor_count == 2)
                    {
                        // it's a double side wall segment
                        //double sided wall north-south
                        if (north_floor && south_floor)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ;
                            change_made = true;
                            continue;
                        }
                        // double sided wall east-west
                        else if (east_floor && west_floor)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT;
                            change_made = true;
                            continue;
                        }

                        // otherwise we have two floor neighbors adjacent
                        // -- need to check if this is a reentrant corner with a floor tile diagonal to this cell
                        else
                        {
                            room_map[j * total_width + i] = TileTypes.TILETYPE_WALL_CORNER;
                            change_made = true;
                            continue;
                            // otherwise its a corner so we'll work with that after all the single walls are in place.
                        }
                    }

                    // otherwise we found a singular wall segment -- will need to keep checking until we find the end
                    if (floor_count == 3)
                    {
                        // this is a dead end wall.
                        if (north_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_BOTTOM;
                        }
                        if (east_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_LEFT;
                        }
                        if (south_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_TOP;
                        }
                        if (west_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_RIGHT;
                        }

                        // if we still have an undefined tile, then we need to recheck
                        if(floor_count + wall_count != 4)
                        {
                            change_made = true;
                        }
                        continue;
                    }

                    // if it's a single whole element, lets make this a floor tile too.
                    if (floor_count == 4)
                    {
                        // make this one a floor tile too
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_FLOOR;
                        change_made = true;
                        continue;
                    }
                }
            }

            if (change_made is true)
            {
                // run through this again
                FindWalls();
                FindWallCorners();
            }



            change_made = false;
        }
    }

    private void FindWallCorners()
    {
        bool change_made = true;
        wall_corner_loop_counter--;
        // Loop until there are no more changes
        while (change_made)
        {
            change_made = false;
            if (wall_corner_loop_counter < 0)
            {
                GD.Print("Looping too many times.  Breaking out of level creation for wall corners loop.");
                break;
            }
            for (int j = 0; j < total_height; j++)
            {
                for (int i = 0; i < total_width; i++)
                {
                    // if our current tile is not undefined, then it's already been set and we can continue on.
                    if (room_map[j * total_width + i] != TileTypes.TITLETYPE_UNDEFINED)
                    {
                        continue;
                    }

                    if (IsWallCornerTile(i, j))
                    {
                        room_map[j * total_width + i] = TileTypes.TILETYPE_WALL_CORNER;
                        change_made = true;
                        continue;
                    }
                }
            }

            if (change_made is true)
            {
                // run through this again
                FindWallCorners();
            }
        }
    }

    /// <summary>
    /// Determines if he neighbors of a cell are floortypes
    /// 0:  east
    /// 1:  south
    /// 2:  west
    /// 3:  north
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool[] CheckFloorNeighbors(int x, int y)
    {
        //GD.Print("x: " + x + " y: " + y);

        bool[] neighbors = new bool[4];

        //east
        neighbors[(int)Dirs.EAST] = room_map[y * total_width + (x + 1)] == TileTypes.TITLETYPE_FLOOR;
        //south
        neighbors[(int)Dirs.SOUTH] = room_map[(y + 1) * total_width + x] == TileTypes.TITLETYPE_FLOOR;
        //west
        neighbors[(int)Dirs.WEST] = room_map[y * total_width + (x - 1)] == TileTypes.TITLETYPE_FLOOR;
        //north
        neighbors[(int)Dirs.NORTH] = room_map[(y - 1) * total_width + x] == TileTypes.TITLETYPE_FLOOR;

        return neighbors;
    }

    /// <summary>
    /// Helper function to determine which neighbor cells are wall tiles
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool[] CheckWallNeighbors(int x, int y)
    {
        bool[] neighbors = new bool[4];

        //east
        neighbors[(int)Dirs.EAST] = IsWallTile(x + 1, y);
        //south
        neighbors[(int)Dirs.SOUTH] = IsWallTile(x, y + 1);
        //west
        neighbors[(int)Dirs.WEST] = IsWallTile(x - 1, y);
        //north
        neighbors[(int)Dirs.NORTH] = IsWallTile(x, y - 1);

        return neighbors;
    }

    /// <summary>
    /// Helper function to count the number of "TRUE" in a boolean array
    /// </summary>
    /// <param name="neighbors"></param>
    private int countTrueNeighbors(bool[] arr)
    {
        int count = 0;
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] == true)
            {
                count++;
            }
        }
        return count;

    }

    /// <summary>
    /// Helper function to determine if a tile is a wall tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsWallTile(int x, int y)
    {
        TileTypes tile_type = room_map[y * total_width + x];

        bool is_wall = false;

        is_wall =
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT ||
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP ||
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM ||

            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT ||
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT ||

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ ||

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMLEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMRIGHT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPLEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPRIGHT ||

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMLEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPLEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT ||

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_LEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_RIGHT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_TOP ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_BOTTOM ||

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_LEFT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_RIGHT ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_TOP ||
            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_BOTTOM ||

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_CROSS ||

            tile_type == TileTypes.TITLETYPE_WALL_GENERIC_INTERSECTION ||
            tile_type == TileTypes.TILETYPE_WALL_CORNER;

        return is_wall;
    }

    /// <summary>
    /// Helper function to determine if a tile is a floor tile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsFloorTile(int x, int y)
    {
        TileTypes tile_type = room_map[y * total_width + x];

        bool is_floor = false;

        is_floor = (tile_type == TileTypes.TITLETYPE_FLOOR);

        return is_floor;

    }

    /// <summary>
    /// Helper function to determine if the tile below a current one is a wall tile (for detemining which wall section
    /// to draw -- walls vs. black region)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool HasWallBelow(int x, int y)
    {
        return IsWallTile(x, y + 1);
    }

    /// <summary>
    /// Helper function to determine if the current tile is corner tile
    /// to draw -- walls vs. black region)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsWallCornerTile(int x, int y)
    {
        bool north = IsWallTile(x, y - 1);
        bool east = IsWallTile(x + 1, y);
        bool south = IsWallTile(x, y + 1);
        bool west = IsWallTile(x - 1, y);
        bool ne = IsWallTile(x + 1, y - 1);
        bool se = IsWallTile(x + 1, y + 1);
        bool sw = IsWallTile(x - 1, y + 1);
        bool nw = IsWallTile(x - 1, y - 1);

        if ((north && east) || (east && south) || (south && west) || (west && north))
        {
            // must have a floor tile immediately adjacent (one of the eight directions must be a floor tile)
            if (HasFloorNeighbor(x, y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Helper function to determine if the tile below a current one is a floor tile (for detemining which wall section
    /// to draw -- walls vs. black region)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool HasFloorNeighbor(int x, int y)
    {
        bool floor_ne = IsFloorTile(x + 1, y - 1);
        bool floor_se = IsFloorTile(x + 1, y + 1);
        bool floor_sw = IsFloorTile(x - 1, y + 1);
        bool floor_nw = IsFloorTile(x - 1, y - 1);
        bool floor_n = IsFloorTile(x, y - 1);
        bool floor_e = IsFloorTile(x + 1, y);
        bool floor_s = IsFloorTile(x, y + 1);
        bool floor_w = IsFloorTile(x - 1, y);

        return floor_ne || floor_se || floor_sw || floor_nw || floor_n || floor_e || floor_s || floor_w;
    }

    /// <summary>
    /// Helper function to determine if the tile below a current one is a floor tile (for detemining which wall section
    /// to draw -- walls vs. black region)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool HasFloorBelow(int x, int y)
    {
        TileTypes tile_type = room_map[(y + 1) * total_width + x];

        bool is_floor_below = false;

        is_floor_below = (tile_type == TileTypes.TITLETYPE_FLOOR);

        return is_floor_below;
    }

    /// <summary>
    /// Helper function to determine if the tile above a current one is a floor tile (for detemining which wall section
    /// to draw -- walls vs. black region)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool HasFloorAbove(int x, int y)
    {
        TileTypes tile_type = room_map[(y - 1) * total_width + x];

        bool is_floor_above = false;

        is_floor_above = (tile_type == TileTypes.TITLETYPE_FLOOR);

        return is_floor_above;
    }










    private void RenderMap()
    {
        // Comment out the contents to not sure this purple dungeon tile set
        #region TileSetVectors for 32x80 custom Duhamel stone tile sets  -- tilesets already created in Godot
        // tileset source IDs for the layers -- should be zero if only one tileset on the tilemaplayer
        int floor_tileset_source_id = 2;   // this is for the 32x32 Duhamel tileset graphics
        int wall_tileset_source_id = 1;   // this is for the 32x80 Duhamel tileset graphics


        // indices (atlas coords) for the upper wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_upper =
        {
            new Vector2I(1, 0),
            new Vector2I(2, 0),
            new Vector2I(3, 0),
            new Vector2I(4, 0),
        };

        // indices (atlas coords) for the lower wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_lower =
        {
            new Vector2I(1, 4),
            new Vector2I(2, 4),
            new Vector2I(3, 4),
            new Vector2I(4, 4),
        };

        // indices (atlas coords) for the left wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_left =
        {
            new Vector2I(0, 1),
            new Vector2I(0, 2),
            new Vector2I(0, 3),
        };

        // indices (atlas coords) for the right wall tiles tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_right =
        {
            new Vector2I(5, 1),
            new Vector2I(5, 2),
            new Vector2I(5, 3),
        };

        // indices for the wall corner graphics in the tile set
        // this is graphic specific
        Vector2I[] wall_tiles_upper_left_corner = { new Vector2I(0, 0) };
        Vector2I[] wall_tiles_upper_right_corner = { new Vector2I(5, 0) };
        Vector2I[] wall_tiles_lower_left_corner = { new Vector2I(0, 4) };
        Vector2I[] wall_tiles_lower_right_corner = { new Vector2I(5, 4) };

        #endregion

        // Our loop for rendering the tiles
        for (int j = 0; j < total_height; j++)
        {
            for (int i = 0; i < total_width; i++)
            {
                // if this is the impenetrable map border, we draw nothing
                if(room_map[j * total_width + i] == TileTypes.TITLETYPE_MAP_BORDER)
                {
                    continue;
                }

                if (IsFloorTile(i, j))
                {
                    //// create a colorrect tile
                    //ColorRect color_rect = new ColorRect();

                    //color_rect.Color = new Color(1, 0, 0, 0.1f);
                    //color_rect.Color = new Color(1, 0, 0, 0.1f);
                    //color_rect.Size = new Vector2(tile_size, tile_size);
                    //color_rect.Position = new Vector2(i * tile_size, j * tile_size);
                    //floors.AddChild(color_rect);

                    RenderFloor(i, j, floors, floor_tileset_source_id);
                }
                else if (IsWallTile(i, j))
                {
                    RenderWall(i, j, walls, wall_tileset_source_id);
                }
                else
                {
                    //TODO: render other tiles here
                    RenderBlackSpace(i, j, walls, wall_tileset_source_id);
                }

            }
        }
    }

    /// <summary>
    /// Renders the blackspace around our procedurally generated area
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="walls"></param>
    /// <param name="wall_tileset_source_id"></param>
    private void RenderBlackSpace(int i, int j, TileMapLayer walls, int wall_tileset_source_id)
    {
        var rng = new RandomNumberGenerator();
        // set a random floor type

        Vector2I atlas_coord = new Vector2I(4, 2);

        Vector2I tile_pos = new Vector2I(i, j);
        walls.SetCell(tile_pos, wall_tileset_source_id, atlas_coord);
    }

    /// <summary>
    /// Function to render the floor tilesto the floor tilemap layer
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="tilemap_layer"></param>
    private void RenderFloor(int i, int j, TileMapLayer tilemap_layer, int floor_tileset_source_id = 0)
    {

        #region TileSetVectors for 16x16 Purple dungeon set floor tiles
        //// indices (atlas coords) for the floor tiles in the tileset
        //// this is graphic specific
        //Vector2I[] floor_tiles =
        //{
        //    new Vector2I(6, 0),
        //    new Vector2I(6, 1),
        //    new Vector2I(6, 2),
        //    new Vector2I(7, 0),
        //    new Vector2I(7, 1),
        //    new Vector2I(7, 2),
        //    new Vector2I(8, 0),
        //    new Vector2I(8, 1),
        //    new Vector2I(8, 2),
        //    new Vector2I(9, 0),
        //    new Vector2I(9, 1),
        //    new Vector2I(9, 2)
        //};
        #endregion

        #region TileSetVectors for 32x32 Duhamel Dungeon stone tile set
        // indices (atlas coords) for the floor tiles in the tileset
        // this is graphic specific
        Vector2I[] floor_tiles =
        {
            new Vector2I(0, 0),
            new Vector2I(0, 1),
            new Vector2I(0, 2),
            new Vector2I(0, 3),
            new Vector2I(1, 0),
            new Vector2I(1, 1),
            new Vector2I(1, 2),
            new Vector2I(1, 3),
            new Vector2I(2, 0),
            new Vector2I(2, 1),
            new Vector2I(2, 2),
            new Vector2I(2, 3),
            new Vector2I(3, 0),
            new Vector2I(3, 1),
            new Vector2I(3, 2),
            new Vector2I(3, 3)
        };
        #endregion

        // setup random number generator
        var rng = new RandomNumberGenerator();

        // set a random floor type
        var rand_number = rng.RandiRange(0, floor_tiles.Length - 1);
        Vector2I atlas_coord = floor_tiles[rand_number];

        Vector2I tile_pos = new Vector2I(i, j);
        tilemap_layer.SetCell(tile_pos, floor_tileset_source_id, atlas_coord);
    }

    /// <summary>
    /// Function to render the wall tiles to the wall tilemap layer
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="tilemap_layer"></param>
    private void RenderWall(int i, int j, TileMapLayer tilemap_layer, int wall_tileset_source_id = 0)
    {
        // Comment out the contents to not sure this purple dungeon tile set
        #region TileSetVectors for 16x16 Purple dungeon set
        //// tileset source IDs for the layers -- should be zero if only one tileset on the tilemaplayer
        //int floor_tileset_source_id = 0;   // this is for the 16x16 tileset graphics
        //int wall_tileset_source_id = 0;   // this is for the 16x16 tileset graphics

        // indices (atlas coords) for the lower wall tiles in the tileset
        // this is graphic specific
        //Vector2I[] wall_tiles_top_floorbelow =
        //{

        //};

        //Vector2I[] wall_tiles_top =
        //{
        //    new Vector2I(1, 0),
        //    new Vector2I(2, 0),
        //    new Vector2I(3, 0),
        //    new Vector2I(4, 0),
        //};

        //// indices (atlas coords) for the lower wall tiles in the tileset
        //// this is graphic specific
        ///        //Vector2I[] wall_tiles_bottom_floorbelow =
        //{

        //};
        //Vector2I[] wall_tiles_bottom =
        //{
        //    new Vector2I(1, 4),
        //    new Vector2I(2, 4),
        //    new Vector2I(3, 4),
        //    new Vector2I(4, 4),
        //};

        //// indices (atlas coords) for the left wall tiles in the tileset
        //// this is graphic specific
        //Vector2I[] wall_tiles_left_floorbelow =
        //{

        //};

        //Vector2I[] wall_tiles_left =
        //{
        //    new Vector2I(0, 1),
        //    new Vector2I(0, 2),
        //    new Vector2I(0, 3),
        //};

        //// indices (atlas coords) for the right wall tiles tiles in the tileset
        //// this is graphic specific
        //Vector2I[] wall_tiles_right_floorbelow =
        //{

        //};

        //Vector2I[] wall_tiles_right =
        //{
        //    new Vector2I(5, 1),
        //    new Vector2I(5, 2),
        //    new Vector2I(5, 3),
        //};

        //// indices for the wall corner graphics in the tile set
        //// this is graphic specific
        //Vector2I[] wall_tiles_upper_left_corner = { new Vector2I(0, 0) };
        //Vector2I[] wall_tiles_upper_right_corner = { new Vector2I(5, 0) };
        //Vector2I[] wall_tiles_lower_left_corner = { new Vector2I(0, 4) };
        //Vector2I[] wall_tiles_lower_right_corner = { new Vector2I(5, 4) };

        #endregion

        #region TileSetVectors for 32x32 Duhamel Dungeon stone tile set
        // indices (atlas coords) for the upper wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_top_floorbelow =
        {
            new Vector2I(1,2)
        };

        Vector2I[] wall_tiles_top =
        {
            new Vector2I(5,3)
        };

        // indices (atlas coords) for the lower wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_bottom_floorbelow =
        {
            new Vector2I(1,2)
        };

        Vector2I[] wall_tiles_bottom =
        {
            new Vector2I(5,3)
        };

        // indices (atlas coords) for the left wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_right_floorbelow =
        {
            new Vector2I(8, 2)
        };

        Vector2I[] wall_tiles_right =
        {
            new Vector2I(8, 2)
        };

        // indices (atlas coords) for the right wall tiles tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_left_floorbelow =
        {
            new Vector2I(8,2)
        };

        Vector2I[] wall_tiles_left =
        {
            new Vector2I(8,2)
        };

        Vector2I[] wall_tiles_undefined =
        {
            new Vector2I(4,2)
        };


        // indices for the wall corner graphics in the tile set
        // this is graphic specific
        Vector2I[] wall_tiles_corner =
        {
            new Vector2I(8, 2)
        };

        Vector2I[] wall_tiles_corner_floorbelow =
        {
            new Vector2I(8, 2)
        };

        //// indices for the wall corner graphics in the tile set
        //// this is graphic specific
        //Vector2I[] wall_tiles_upper_left_corner = { new Vector2I(8, 2) };
        //Vector2I[] wall_tiles_upper_right_corner = { new Vector2I(8, 2) };
        //Vector2I[] wall_tiles_lower_left_corner = { new Vector2I(8, 2) };
        //Vector2I[] wall_tiles_lower_right_corner = { new Vector2I(8, 2) };

        //Vector2I[] wall_tiles_reentrant_upper_left_corner = { new Vector2I(8, 2) };
        //Vector2I[] wall_tiles_reentrant_upper_right_corner = { new Vector2I(8, 2) };
        //Vector2I[] wall_tiles_reentrant_lower_left_corner = { new Vector2I(8, 2) };
        //Vector2I[] wall_tiles_reentrant_lower_right_corner = { new Vector2I(8, 2) };

        Vector2I[] wall_tiles_other = { new Vector2I(8, 2) };

        #endregion

        Vector2I[] tile_array = null;



        switch (room_map[j * total_width + i])
        {
            // 0
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT:
                if (HasFloorBelow(i, j))
                {
                    tile_array = wall_tiles_left_floorbelow;
                }
                else
                {
                    tile_array = wall_tiles_left;
                }
                break;

            // 1
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT:
                if (HasFloorBelow(i, j))
                {
                    tile_array = wall_tiles_right_floorbelow;
                }
                else
                {
                    tile_array = wall_tiles_right;
                }
                break;
            // 2
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP:
                if (HasFloorBelow(i, j))
                {
                    tile_array = wall_tiles_top_floorbelow;
                }
                else
                {
                    tile_array = wall_tiles_top;
                }
                break;
            // 3
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM:
                if (HasFloorBelow(i, j))
                {
                    tile_array = wall_tiles_bottom_floorbelow;
                }
                else
                {
                    tile_array = wall_tiles_bottom;
                }
                break;

            ////4
            //case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT:
            //    tile_array = wall_tiles_upper_left_corner; break;
            ////5
            //case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT:
            //    tile_array = wall_tiles_upper_right_corner; break;
            ////6
            //case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT:
            //    tile_array = wall_tiles_lower_left_corner; break;
            ////7
            //case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT:
            //    tile_array = wall_tiles_lower_right_corner; break;

            //8
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT:
                tile_array = wall_tiles_other; break;
            //9
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ:
                tile_array = wall_tiles_other; break;

            ////10
            //case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPLEFT:
            //    tile_array = wall_tiles_upper_left_corner; break;
            ////11
            //case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPRIGHT:
            //    tile_array = wall_tiles_upper_right_corner; break;
            ////12
            //case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMLEFT:
            //    tile_array = wall_tiles_lower_left_corner; break;
            ////13
            //case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMRIGHT:
            //    tile_array = wall_tiles_lower_right_corner; break;

            //14
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_LEFT:
                tile_array = wall_tiles_other; break;
            //15
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_RIGHT:
                tile_array = wall_tiles_other; break;
            //16
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_TOP:
                tile_array = wall_tiles_other; break;
            //17
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_BOTTOM:
                tile_array = wall_tiles_other; break;


            //18
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_LEFT:
                tile_array = wall_tiles_other; break;
            //19
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_RIGHT:
                tile_array = wall_tiles_other; break;
            //20
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_TOP:
                tile_array = wall_tiles_other; break;
            //21
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_BOTTOM:
                tile_array = wall_tiles_other; break;

            //22
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPLEFT:
                tile_array = wall_tiles_other; break;
            //23
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT:
                tile_array = wall_tiles_other; break;
            //24
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMLEFT:
                tile_array = wall_tiles_other; break;
            //25
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT:
                tile_array = wall_tiles_other; break;

            //26
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CROSS:
                tile_array = wall_tiles_other; break;

            //27
            case TileTypes.TITLETYPE_WALL_GENERIC_INTERSECTION:
                tile_array = wall_tiles_other; break;

            //28
            case TileTypes.TILETYPE_WALL_CORNER:
                if(HasFloorBelow(i, j))
                {
                    tile_array = wall_tiles_corner_floorbelow; break;
                } else
                {
                    tile_array = wall_tiles_corner; break;
                }
                break;

            default:
                //tile_array = wall_tiles_undefined; 
                break;
        }

        if (tile_array == null)
        {
            // draw nothing
            return;
        }

        // setup random number generator
        var rng = new RandomNumberGenerator();

        // set a random floor type
        var rand_number = rng.RandiRange(0, tile_array.Length - 1);
        Vector2I atlas_coord = tile_array[rand_number];

        Vector2I tile_pos = new Vector2I(i, j);
        tilemap_layer.SetCell(tile_pos, wall_tileset_source_id, atlas_coord);
    }
}
