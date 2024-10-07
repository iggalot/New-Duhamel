using Godot;
using Godot.Collections;
using System;
using static Godot.Time;

public partial class AreaProceduralGeneration : Node
{
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
        { TileTypes.TITLETYPE_WALL_DOUBLESIDE_CROSS, "+"}
    };

    TileMapLayer floors;
    TileMapLayer walls;
    PlayerSpawn playerSpawn;

    TileTypes[] room_map;
    int tile_size = 16;

    // number of dead cells beyond room walls -- includes impenetrable width amount
    // need to make sure it's at least 1 more than impenetrable border so we can fit the wall tiles
    int fringe = 4;  
    int impenetrable_border = 1;  // impenetrable width -- included in the fringe value
    int width;
    int height;
    int total_width;
    int total_height;

    int numRooms = 15;

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
            FindReentrantCorners();
            FindWallCorners();
            //        FillExtraRoomHoles();  // fill in any undefined nodes in the room map that are neighbors of a floor tile

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
            int width = rng.RandiRange(5, 30);
            int height = rng.RandiRange(5, 30);
            int pos_x = rng.RandiRange(0, 50);
            int pos_y = rng.RandiRange(0, 50);

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
                if(i==0 && j == 0)
                {

                } 
                // outside the room area
                else if(j < pos_y || j >= pos_y + height || i < pos_x || i >= pos_x + width)
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
                    // if our current tile is undefined, then it's already been set and we can continue on.
                    if (room_map[j * total_width + i] != TileTypes.TITLETYPE_UNDEFINED)
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

                    // No floor neighbors
                    if (floor_count == 0)
                    {
                        if (wall_count == 3)
                        {
                            // this is a T junction -- stem up
                            if (south_wall is false)
                            {
                                // are both upper left diagonal and upper right diagonal a floor tiles?
                                // this is a single stem T
                                if (room_map[(j - 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR &&
                                    room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_TOP;
                                    change_made = true;
                                }
                                // is upper left diagonal a floor tile?
                                else if (room_map[(j - 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT;
                                    change_made = true;
                                }
                                // is upper right diagonal a floor tile?
                                else if (room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT;
                                    change_made = true;
                                }
                                else
                                {
                                    // do nothing since we don't know for sure what it is
                                }
                            }
                            // This is a T junction -- stem down
                            else if (north_wall is false)
                            {
                                // are both lower left diagonal and lower right diagonal a floor tiles?
                                // this is a single stem T
                                if (room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR &&
                                    room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_BOTTOM;
                                    change_made = true;
                                }
                                // is lower left diagonal a floor tile?
                                else if (room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                                    change_made = true;
                                }
                                // is lower right diagonal a floor tile?
                                else if (room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT;
                                    change_made = true;
                                }
                                else
                                {
                                    // do nothing since we don't know for sure what it is
                                }
                            }
                            // Is it a T to the right?
                            else if (west_wall is false)
                            {
                                // are both right side diagonals a floor tiles?
                                // this is a single stem T
                                if (room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR &&
                                    room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_RIGHT;
                                    change_made = true;
                                }
                                else if (room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT;
                                    change_made = true;
                                }
                                else if (room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT;
                                    change_made = true;
                                }
                                else
                                {
                                    // do nothing since we don't know for sure what it is
                                }
                            }
                            // Is it a T to the left?
                            else if (east_wall is false)
                            {
                                // are both left side diagonals a floor tiles?
                                // this is a single stem T
                                if (room_map[(j - 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR &&
                                    room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_LEFT;
                                    change_made = true;
                                    continue;

                                }
                                else if (room_map[(j - 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT;
                                    change_made = true;
                                    continue;
                                }
                                else if (room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                                    change_made = true;
                                    continue;
                                }
                                else
                                {
                                    // do nothing since we don't know for sure what it is
                                }
                            }
                        }
                    }

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
                            // otherwise its a corner so we'll work with that after all the single walls are in place.
                        }
                    }

                    if (floor_count == 3)
                    {
                        // this is a dead end wall.
                        if (north_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_BOTTOM;
                            change_made = true;

                            continue;
                        }
                        if (east_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_LEFT;
                            change_made = true;

                            continue;
                        }
                        if (south_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_TOP;
                            change_made = true;

                            continue;
                        }
                        if (west_floor is false)
                        {
                            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_DEADEND_RIGHT;
                            change_made = true;

                            continue;
                        }
                    }

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
            if (wall_corner_loop_counter < 0)
            {
                GD.Print("Looping too many times.  Breaking out of level creation loop.");
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

                    // count the number of known walls around this tile -- it's possible that there are more than that
                    bool[] wall_neighbors = CheckWallNeighbors(i, j);
                    int wall_count = countTrueNeighbors(wall_neighbors);

                    bool north_wall = wall_neighbors[(int)Dirs.NORTH];
                    bool south_wall = wall_neighbors[(int)Dirs.SOUTH];
                    bool east_wall = wall_neighbors[(int)Dirs.EAST];
                    bool west_wall = wall_neighbors[(int)Dirs.WEST];

                    // count the number of known floors around this tile -- this will always be an accurate number since
                    // floors are always known from the creation process
                    bool[] floor_neighbors = CheckFloorNeighbors(i, j);
                    int floor_count = countTrueNeighbors(floor_neighbors);

                    bool north_floor = floor_neighbors[(int)Dirs.NORTH];
                    bool south_floor = floor_neighbors[(int)Dirs.SOUTH];
                    bool east_floor = floor_neighbors[(int)Dirs.EAST];
                    bool west_floor = floor_neighbors[(int)Dirs.WEST];

                    if (wall_count == 0)
                    {
                        //// if floor count = 3, this is a dead end wall section
                        //if()
                        //// no walls adjacent -- guess would could make a column type here -- but for now dont specify that
                        //room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;
                        //continue;
                    }
                    else if (wall_count == 1)
                    {
                        // could be a dead end if the other three are floor tiles -- but I believe these have already been set.

                    }
                    else if (wall_count == 2)
                    {
                        if (north_wall && east_wall)
                        {
                            // is north east corner a floor tile?
                            if (room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT;
                                change_made = true;
                                continue;
                            }
                            else
                            {
                                if(west_floor && south_floor)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT;
                                    change_made = true;
                                    continue;
                                } else
                                {
                                    room_map[(j - 1) * total_width + (i + 1)] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                                    change_made = true;
                                    continue;
                                }
                            }
                        }

                        if (south_wall && east_wall)
                        {
                            // is south east corner a floor tile?
                            if (room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT;
                                change_made = true;
                                continue;
                            }
                            else
                            {
                                if (west_floor && north_floor)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPLEFT;
                                    change_made = true;
                                    continue;
                                }
                                else
                                {
                                    room_map[(j + 1) * total_width + (i + 1)] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT;
                                    change_made = true;
                                    continue;
                                }
                            }
                        }

                        if (north_wall && west_wall)
                        {
                            // is north west corner a floor tile?
                            if (room_map[(j - 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT;
                            }
                            else
                            {
                                if (east_floor && south_floor)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT;
                                    change_made = true;
                                    continue;
                                }
                                else
                                {
                                    room_map[(j - 1) * total_width + (i - 1)] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT;
                                    change_made = true;
                                    continue;
                                }

                            }
                        }

                        if (south_wall && west_wall)
                        {
                            // is south west corner a floor tile?
                            if (room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                            }
                            else
                            {
                                if (east_floor && south_floor)
                                {
                                    room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT;
                                    change_made = true;
                                    continue;
                                }
                                else
                                {
                                    room_map[(j + 1) * total_width + (i - 1)] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                                    change_made = true;
                                    continue;
                                }
                            }
                        }

                        // could be a straight wall section


                        // or could be a corner

                        // check renentrant corner
                        // -- opposite corner is not a floor tile

                        // check externior corern
                        // -- opposite corner is a floor tile
                    }
                    else if (wall_count == 3)
                    {
                        // could be a tee
                        // -- three are wall tiles and one is not a floor tile

                    }
                    else if (wall_count == 4)
                    {
                        // if all four are wall tiles, then its a cross
                    }

                }
            }

            if (change_made is true)
            {
                // run through this again
                FindWallCorners();
            }

            change_made = false;
        }
    }

    private void FillExtraRoomHoles()
    {
        bool change_made = true;
        fill_extra_loop_counter--;
        // Loop until there are no more changes
        while (change_made)
        {
            if (fill_extra_loop_counter < 0)
            {
                GD.Print("Looping too many times.  Breaking out of filling extra room holes loop");
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

                    bool[] floor_neighbors = CheckFloorNeighbors(i, j);
                    int floor_count = countTrueNeighbors(floor_neighbors);

                    if(floor_count > 0)
                    {
                        // set the current tile to a FLOOR tile if it is touching another floor tile.
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_FLOOR;
                        change_made = true;
                    }

                }
            }

            if (change_made is true)
            {
                // run through this again looking for any new holes.
                FillExtraRoomHoles();
            }

            change_made = false;
        }
    }

    private void FindReentrantCorners()
    {
        bool change_made = true;
        reentrant_corner_loop_counter--;
        // Loop until there are no more changes
        while (change_made)
        {
            if (reentrant_corner_loop_counter < 0)
            {
                GD.Print("Looping too many times.  Breaking out of reeentrant corners loop");
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
                    
                    // check for L corners of floor tiles.
                    // -upper left
                    //     XX     where X = floor tile
                    //     X*     * = undefined tile
                    if((room_map[(j) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j - 1) * total_width + (i)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j-1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR))
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT;
                        change_made = true;
                    }

                    // Check for L corners of floor tiles.
                    // -upper right
                    //     XX     where X = floor tile
                    //     *X     * = undefined tile
                    if ((room_map[(j) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j - 1) * total_width + (i)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR))
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                        change_made = true;
                    }

                    // Check for L corners of floor tiles.
                    // -lower left
                    //     X*     where X = floor tile
                    //     XX     * = undefined tile
                    if ((room_map[(j) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j + 1) * total_width + (i)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR))
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT;
                        change_made = true;
                    }

                    // Check for L corners of floor tiles.
                    // -lower right
                    //     *X     where X = floor tile
                    //     XX     * = undefined tile
                    if ((room_map[(j) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j + 1) * total_width + (i)] == TileTypes.TITLETYPE_FLOOR) &&
                        (room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR))
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT;
                        change_made = true;
                    }
                }
            }

            if (change_made is true)
            {
                // run through this again looking for any new holes.
                FindReentrantCorners();
            }

            change_made = false;
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
        neighbors[(int)Dirs.NORTH] = IsWallTile(x, y-1);

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

            tile_type == TileTypes.TITLETYPE_WALL_DOUBLESIDE_CROSS;


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

    private void RenderMap()
    {
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



        // Our loop for rendering the tiles
        for (int j = 0; j < total_height; j++)
        {
            for (int i = 0; i < total_width; i++)
            {
                if (IsFloorTile(i, j))
                { 
                    //// create a colorrect tile
                    //ColorRect color_rect = new ColorRect();

                    //color_rect.Color = new Color(1, 0, 0, 0.1f);
                    //color_rect.Color = new Color(1, 0, 0, 0.1f);
                    //color_rect.Size = new Vector2(tile_size, tile_size);
                    //color_rect.Position = new Vector2(i * tile_size, j * tile_size);
                    //floors.AddChild(color_rect);

                    RenderFloor(i, j, floors);
                }
                else if (IsWallTile(i, j))
                {
                    RenderWall(i, j, walls);
                }
                else
                {
                    //TODO: render other tiles here
                }

            }
        }
    }

    /// <summary>
    /// Function to render the floor tilesto the floor tilemap layer
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="tilemap_layer"></param>
    private void RenderFloor(int i, int j, TileMapLayer tilemap_layer)
    {
        // tileset source IDs for the layers -- should be zero if only one tileset on the tilemaplayer
        int floor_tileset_source_id = 0;

        // indices (atlas coords) for the floor tiles in the tileset
        // this is graphic specific
        Vector2I[] floor_tiles =
        {
            new Vector2I(6, 0),
            new Vector2I(6, 1),
            new Vector2I(6, 2),
            new Vector2I(7, 0),
            new Vector2I(7, 1),
            new Vector2I(7, 2),
            new Vector2I(8, 0),
            new Vector2I(8, 1),
            new Vector2I(8, 2),
            new Vector2I(9, 0),
            new Vector2I(9, 1),
            new Vector2I(9, 2)
        };

        // setup random number generator
        var rng = new RandomNumberGenerator();

        // set a random floor type
        var rand_number = rng.RandiRange(0, floor_tiles.Length - 1);
        Vector2I atlas_coord = floor_tiles[rand_number];

        Vector2I tile_pos = new Vector2I(i, j);
        tilemap_layer.SetCell(tile_pos, floor_tileset_source_id, atlas_coord);
    }

    /// <summary>
    /// Function to render the wall tilesto the wall tilemap layer
    /// </summary>
    /// <param name="i"></param>
    /// <param name="j"></param>
    /// <param name="tilemap_layer"></param>
    private void RenderWall(int i, int j, TileMapLayer tilemap_layer)
    {
        // tileset source IDs for the layers -- should be zero if only one tileset on the tilemaplayer
        int wall_tileset_source_id = 0;

        // indices (atlas coords) for the upper wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_top =
        {
            new Vector2I(1, 0),
            new Vector2I(2, 0),
            new Vector2I(3, 0),
            new Vector2I(4, 0),
        };

        // indices (atlas coords) for the lower wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_bottom =
        {
            new Vector2I(1, 4),
            new Vector2I(2, 4),
            new Vector2I(3, 4),
            new Vector2I(4, 4),
        };

        // indices (atlas coords) for the left wall tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_right =
        {
            new Vector2I(0, 1),
            new Vector2I(0, 2),
            new Vector2I(0, 3),
        };

        // indices (atlas coords) for the right wall tiles tiles in the tileset
        // this is graphic specific
        Vector2I[] wall_tiles_left =
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

        Vector2I[] wall_tiles_reentrant_upper_left_corner = { new Vector2I(0, 5) };
        Vector2I[] wall_tiles_reentrant_upper_right_corner = { new Vector2I(3, 5) };
        Vector2I[] wall_tiles_reentrant_lower_left_corner = { new Vector2I(0, 5) };
        Vector2I[] wall_tiles_reentrant_lower_right_corner = { new Vector2I(0, 5) };




        Vector2I[] wall_tiles_other = { new Vector2I(8, 7) };

        Vector2I[] tile_array;

        switch (room_map[j * total_width + i])
        {
            // 0
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT:
                tile_array = wall_tiles_left; break;
            // 1
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT:
                tile_array = wall_tiles_right; break;
            // 2
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP:
                tile_array = wall_tiles_top; break;
            // 3
            case TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM:
                tile_array = wall_tiles_bottom; break;

            //4
            case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT:
                tile_array = wall_tiles_upper_left_corner; break;
            //5
            case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT:
                tile_array = wall_tiles_upper_right_corner; break;
            //6
            case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT:
                tile_array = wall_tiles_lower_left_corner; break;
            //7
            case TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT:
                tile_array = wall_tiles_lower_right_corner; break;

            //8
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT:
                tile_array = wall_tiles_left; break;
            //9
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ:
                tile_array = wall_tiles_top; break;

            //10
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPLEFT:
                tile_array = wall_tiles_upper_left_corner; break;
            //11
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_TOPRIGHT:
                tile_array = wall_tiles_upper_right_corner; break;
            //12
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMLEFT:
                tile_array = wall_tiles_lower_left_corner; break;
            //13
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CORNER_BOTTOMRIGHT:
                tile_array = wall_tiles_lower_right_corner; break;

            //14
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_LEFT:
                tile_array = wall_tiles_upper_left_corner; break;
            //15
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_RIGHT:
                tile_array = wall_tiles_upper_right_corner; break;
            //16
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_TOP:
                tile_array = wall_tiles_lower_left_corner; break;
            //17
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_TEE_BOTTOM:
                tile_array = wall_tiles_lower_right_corner; break;
            

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
                tile_array = wall_tiles_reentrant_upper_left_corner; break;
            //23
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT:
                tile_array = wall_tiles_reentrant_upper_right_corner; break;
            //24
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMLEFT:
                tile_array = wall_tiles_reentrant_lower_left_corner; break;
            //25
            case TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT:
                tile_array = wall_tiles_reentrant_lower_right_corner; break;

            //26
            case TileTypes.TITLETYPE_WALL_DOUBLESIDE_CROSS:
                tile_array = wall_tiles_other; break;

            default:
                tile_array = wall_tiles_other; break;
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
