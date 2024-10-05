using Godot;
using System;

public partial class AreaProceduralGeneration : Node
{
    TileMapLayer floors;
    TileMapLayer walls;

    string[] room_map;
    int tile_size = 16;
    int fringe = 5;  // number of dead cells beyond room walls -- includes 1 for impenetrable width
    int impenetrable_border = 2;  // impenetrable width -- included in the fringe value
    int width;
    int height;
    int total_width;
    int total_height;

    int numRooms = 4;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        floors = GetNode<TileMapLayer>("TML_Floors");
        walls = GetNode<TileMapLayer>("TML_Walls");

        ImportData data = new ImportData();



        ProceduralMapGenerate();
    }

    public override void _Process(double delta)
    {

    }

    public void ProceduralMapGenerate()
    {
        var rng = new RandomNumberGenerator();

        int[] w_data = new int[numRooms];
        int[] h_data = new int[numRooms];
        int[] posx_data = new int[numRooms];
        int[] posy_data = new int[numRooms];

        // create floor areas borders
        for(int i = 0; i < numRooms; i++)
        {
            int width = rng.RandiRange(6, 20);
            int height = rng.RandiRange(6, 20);
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
        for(int i = 0; i < numRooms; i++)
        {
            if(posx_data[i] < temp_min_x)
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

        room_map = new string[total_width * total_height];

        // Create the rooms
        for (int i = 0; i < numRooms; i++)
        {
            CreateFloorAreas(w_data[i], h_data[i], posx_data[i], posy_data[i]);
        }

        // Add the impenetrable border around the outside of the map
        for (int j = 0; j < total_height; j++)
        {
            string str = "";
            for (int i = 0; i < total_width; i++)
            {
                if(i < impenetrable_border || i >= (total_width - impenetrable_border) || j < impenetrable_border || j >= (total_height - impenetrable_border))
                {
                    room_map[j * total_width + i] = "*";
                }
            }
        }

                // create vertical walls

                // create horizontal walls

                // determine corners

                // determine no spawn perimeter (2 tiles)

                GD.Print("Map Generated");
        for (int j = 0; j < total_height; j++)
        {
            string str = "";
            for (int i = 0; i < total_width; i++)
            {
                if(room_map[j * total_width + i] == "X")
                {
                    str += "X";
                } 
                else if(room_map[j * total_width + i] == "*")
                {
                    str += "*";
                }
                else
                {
                    str += ".";
                }
            }
            GD.Print(str);
        }
    }

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

                //// horizontal fringe pieces
                //if (j < impenetrable_border || j >= 2 * fringe + height - impenetrable_border)
                //{
                //    room_map[j * (width + 2 * fringe) + i] = "O";
                //    color_rect.Color = new Color(0, 0, 0, 1);
                //}
                //else if (i < impenetrable_border || i >= 2 * fringe + width - impenetrable_border)
                //{
                //    room_map[j * (width + 2 * fringe) + i] = "O";
                //    color_rect.Color = new Color(0, 0, 0, 1);
                //}

                //else if (j < fringe || j >= fringe + height)
                //{
                //    room_map[j * (width + 2 * fringe) + i] = "-";
                //    color_rect.Color = new Color(1, 1, 1, 1);
                //}

                //// horizontal fringe pieces
                //else if (i < fringe || i >= fringe + width)
                //{
                //    room_map[j * (width + 2 * fringe) + i] = "-";
                //    color_rect.Color = new Color(1, 1, 1, 1);
                //}


                // the origin of the room
                if(i==0 && j == 0)
                {
                    //room_map[j * total_width + i] = "*";
                    //color_rect.Color = new Color(0, 0, 0, 0.1f);
                } 
                // outside the room area
                else if(j < pos_y || j >= pos_y + height || i < pos_x || i >= pos_x + width)
                {
                    //room_map[j * total_width + i] = ".";
                    //color_rect.Color = new Color(1, 1, 1, 1.0f);
                }
                else
                // We've found a floor tile of a room
                //else
                {
                    room_map[j * total_width + i] = "X";
                    color_rect.Color = new Color(1, 0, 0, 0.1f);
                    color_rect.Size = new Vector2(tile_size, tile_size);
                    color_rect.Position = new Vector2(i * tile_size, j * tile_size);
                    floors.AddChild(color_rect);
                }
            }
        }
    }

    public void FindWalls()
    {
        

    }
}
