using Godot;
using Godot.Collections;
using System;

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

    int numRooms = 5;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        floors = GetNode<TileMapLayer>("TML_Floors");
        walls = GetNode<TileMapLayer>("TML_Walls");

        ImportData data = new ImportData();


        // Create room area layout and fringe areas and impenetrable border
        GD.Print("Generating room areas...");
        ProceduralMapGenerate();
        PrintMap();

        GD.Print("Adding map border...");
        AddMapBorder();
        PrintMap();

        // Find the walls and reentrant corners
        GD.Print("Finding walls...");
        FindWalls();
        PrintMap();

        // Find the exterior corners
        FindExteriorCorners();

        GD.Print("Map Generated");

        PrintMap();

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
            int width = rng.RandiRange(5, 15);
            int height = rng.RandiRange(5, 5);
            int pos_x = rng.RandiRange(0, 15);
            int pos_y = rng.RandiRange(0, 15);

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
                // create a colorrect tile
                ColorRect color_rect = new ColorRect();

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
                    color_rect.Color = new Color(1, 0, 0, 0.1f);
                    color_rect.Color = new Color(1, 0, 0, 0.1f);
                    color_rect.Size = new Vector2(tile_size, tile_size);
                    color_rect.Position = new Vector2(i * tile_size, j * tile_size);
                    floors.AddChild(color_rect);
                }
            }
        }
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
    /// Logic to locate walls around the outside of the room area we previously created.
    /// </summary>
    public void FindWalls()
    {
        for (int j = 0; j < total_height; j++)
        {
            for (int i = 0; i < total_width; i++)
            {
                // Is this cell one of the border cells?
                // -- this will blow up the CheckNeighbors function otherwise with an index
                //    out of bounds error
                if(room_map[j * total_width + i] == TileTypes.TITLETYPE_MAP_BORDER)
                {
                    continue;
                }

                // if our  current tile is a floor tile, continue
                if (room_map[j * total_width + i] == TileTypes.TITLETYPE_FLOOR)
                {
                    continue;
                }

                bool[] floor_neighbors = CheckFloorNeighbors(i, j);
                int floor_count = countTrueNeighbors(floor_neighbors);

                bool north = floor_neighbors[(int)Dirs.NORTH];
                bool south = floor_neighbors[(int)Dirs.SOUTH];
                bool east = floor_neighbors[(int)Dirs.EAST];
                bool west = floor_neighbors[(int)Dirs.WEST];

                // No floor neighbors
                if(floor_count == 0)
                {
                    room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;
                    continue;
                }

                // A single floor neighbor
                if(floor_count == 1)
                {
                    if (floor_neighbors[(int)Dirs.EAST] is true)
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_RIGHT;
                        continue;
                    }
                    else if (floor_neighbors[(int)Dirs.SOUTH] is true)
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_TOP;
                        continue;
                    }
                    else if (floor_neighbors[(int)Dirs.WEST] is true)
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_VERT_LEFT;
                        continue;
                    }
                    else if (floor_neighbors[(int)Dirs.NORTH] is true)
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_STRAIGHT_HORIZ_BOTTOM;
                        continue;
                    }

                }

                if(floor_count == 2)
                {

                    //double sided wall north-south
                    if(north && south)
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_VERT;
                        continue;
                    } 
                    // double sided wall east-west
                    else if(east && west)
                    {
                        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_DOUBLESIDE_STRAIGHT_HORIZ;
                        continue;
                    }

                    // otherwise we have two floor neighbors adjacent
                    // -- need to check if this is a reentrant corner with a floor tile diagonal to this cell
                    else
                    {
                        TileTypes type;
                        if (north && east)
                        {
                            if (room_map[(j - 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMLEFT;
                            }
                            else
                            {
                                //room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;
                                room_map[(j - 1) * total_width + (i + 1)] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPRIGHT;
                            }
                        }
                        //else if (south && east)
                        //{
                        //    if (room_map[(j + 1) * total_width + (i + 1)] == TileTypes.TITLETYPE_FLOOR)
                        //    {
                        //        room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPLEFT;
                        //    }
                        //    else
                        //    {
                        //        room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;
                        //        room_map[(j + 1) * total_width + (i + 1)] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMRIGHT;
                        //    }
                        //}
                        else if (north && west)
                        {
                            if (room_map[(j - 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_BOTTOMRIGHT;
                            }
                            else
                            {
                                room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;
                                room_map[(j - 1) * total_width + (i - 1)] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_TOPLEFT;
                            }
                        }
                        //    else if (south && west)
                        //    {
                        //        if (room_map[(j + 1) * total_width + (i - 1)] == TileTypes.TITLETYPE_FLOOR)
                        //        {
                        //            room_map[j * total_width + i] = TileTypes.TITLETYPE_WALL_SINGLE_CORNER_TOPRIGHT;
                        //        }
                        //        else
                        //        {
                        //            room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;
                        //            room_map[(j + 1) * total_width + (i - 1)] = TileTypes.TITLETYPE_WALL_DOUBLEWALL_REENTRANT_CORNER_BOTTOMLEFT;
                        //        }
                        //    } else
                        //    {
                        //        room_map[j * total_width + i] = TileTypes.TITLETYPE_UNDEFINED;

                    }
                }
            }
        }
    }

    public void FindExteriorCorners()
    {
        for (int j = 0; j < total_height; j++)
        {
            for (int i = 0; i < total_width; i++)
            {
                //// ignore the impenetrable border tiles, currentwall tiles, and floor tiles
                //if ((room_map[j * total_width + i] == "*") ||   // impenetrable
                //    (room_map[j * total_width + i] == "X") ||   // floor tile
                //    (room_map[j * total_width + i] == "W") ||   // single wall next to X
                //    (room_map[j * total_width + i] == "1") ||   // reentrant corner 2 W's
                //    (room_map[j * total_width + i] == "2") ||   // reentrant corner 2 W's
                //    (room_map[j * total_width + i] == "3") ||   // reentrant corner 2 W's
                //    (room_map[j * total_width + i] == "4") ||   // reentrant corner 2 W's
                //    (room_map[j * total_width + i] == "5") ||   // reentrant corner 2 W's
                //    (room_map[j * total_width + i] == "6"))     // reentrant corner 2 W's
                //{
                //    continue;
                //}

                //bool north = false;
                //bool east = false;
                //bool south = false;
                //bool west = false;

                //// Scan the room for adjacent floor tiles
                //// -- must not be a floor tile
                //int count_adjacent_walls = 0;
                //if (room_map[j * total_width + i] != "W")
                //{
                //    if ((room_map[j * total_width + i - 1] == "W") || 
                //        (room_map[j * total_width + i - 1] == "1") ||
                //        (room_map[j * total_width + i - 1] == "2") ||
                //        (room_map[j * total_width + i - 1] == "3") ||
                //        (room_map[j * total_width + i - 1] == "4"))
                //    {
                //        west = true;
                //        count_adjacent_walls++;
                //    }
                //    if ((room_map[j * total_width + i + 1] == "W") ||
                //        (room_map[j * total_width + i + 1] == "1") ||
                //        (room_map[j * total_width + i + 1] == "2") ||
                //        (room_map[j * total_width + i + 1] == "3") ||
                //        (room_map[j * total_width + i + 1] == "4"))
                //    {
                //        east = true;
                //        count_adjacent_walls++;
                //    }
                //    if ((room_map[(j - 1) * total_width + i] == "W") || 
                //       (room_map[(j - 1) * total_width + i] == "1") ||
                //       (room_map[(j - 1) * total_width + i] == "2") ||
                //       (room_map[(j - 1) * total_width + i] == "3") ||
                //       (room_map[(j - 1) * total_width + i] == "4"))
                //    {
                //        north = true;
                //        count_adjacent_walls++;
                //    }
                //    if ((room_map[(j + 1) * total_width + i] == "W") ||
                //        (room_map[(j + 1) * total_width + i] == "1") ||
                //        (room_map[(j + 1) * total_width + i] == "2") ||
                //        (room_map[(j + 1) * total_width + i] == "3") ||
                //        (room_map[(j + 1) * total_width + i] == "4"))
                //    {
                //        south = true;
                //        count_adjacent_walls++;
                //    }

                //    if (count_adjacent_walls == 2)
                //    {
                //        // do we have a reentrant corner?
                //        //    A     B      C     D    E     F
                //        //   WX     XW     WC    DW   W
                //        //   AW     WB     XW    WX         W W
                //        //                            W
                //        if (north && east)
                //        {

                //            if((room_map[(j-1) * total_width + i + 1] == "X") )
                //            {
                //                room_map[j * total_width + i] = "A";
                //                continue;
                //            }
                //        }

                //        else if (west && north)
                //        {
                //            if ((room_map[(j - 1) * total_width + i - 1] == "X"))
                //            {
                //                room_map[j * total_width + i] = "B";
                //                continue;
                //            }
                //        }

                //        else if (west && south)
                //        {
                //            if ((room_map[(j + 1) * total_width + i - 1] == "X"))
                //            {
                //                room_map[j * total_width + i] = "C";
                //                continue;
                //            }
                //        }

                //        else if (east && south)
                //        {
                //            if ((room_map[(j + 1) * total_width + i + 1] == "X") )
                //            {
                //                room_map[j * total_width + i] = "D";
                //                continue;
                //            }
                //        }

                //        else
                //        {
                //            // do nothing
                //        }

                //        // or is it a single wide wall passing through?

                //    }

                //}
                //else
                //{
                //    continue;
                //}
            }
        }
    }
}
