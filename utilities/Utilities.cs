using Godot;
using System;

public partial class Utilities : Node
{
    /// <summary>
    /// DO NOT CHANGE THE ORDER
    /// This order matters if using the dir_arrows.png spritemap
    /// </summary>
    public enum Directions
    {
        DIR_NONE = 0,
        DIR_WEST = 1,
        DIR_NORTHWEST = 2,
        DIR_NORTH = 3,
        DIR_NORTHEAST = 4,
        DIR_EAST = 5,
        DIR_SOUTHEAST = 6,
        DIR_SOUTH = 7,
        DIR_SOUTHWEST = 8
    }

    /// <summary>
    /// for a given vector, returns a Directions enum for the direction vector provided
    /// 
    /// NW    N    NE
    /// W    None   E
    /// SW    S    SE
    /// 
    /// </summary>
    /// <param name="directionVector"></param>
    /// <returns></returns>
    public static Directions GetDirection_9WAY(Vector2 directionVector)
    {
        // normalize as a unit vector, just in case...
        var directionUnitVector = directionVector.Normalized();

        // set our default direction is DIR_NONE        
        Directions direction = Directions.DIR_NONE;

        // now use the DirectionUnitVector to discretize into 8 directions
        if ((directionUnitVector.X == 0) && (directionUnitVector.Y == 0))
        {
            direction = Directions.DIR_NONE;
        }
        else if ((directionUnitVector.X > 0.924) && (directionUnitVector.Y <= 0.383) && (directionUnitVector.Y >= -.383))
        {
            direction = Directions.DIR_EAST;
        }
        else if ((directionUnitVector.X < -0.924) && (directionUnitVector.Y <= 0.383) && (directionUnitVector.Y >= -.383))
        {
            direction = Directions.DIR_WEST;
        }
        else if ((directionUnitVector.Y > 0.924) && (directionUnitVector.X <= 0.383) && (directionUnitVector.X >= -.383))
        {
            direction = Directions.DIR_SOUTH;
        }
        else if ((directionUnitVector.Y < -0.924) && (directionUnitVector.X <= 0.383) && (directionUnitVector.X >= -.383))
        {
            direction = Directions.DIR_NORTH;
        }

        else if (directionUnitVector.X >= 0.383 && (directionUnitVector.X <= 0.924))
        {
            if (directionUnitVector.Y > 0)
            {
                direction = Directions.DIR_SOUTHEAST;
            }
            else
            {
                direction = Directions.DIR_NORTHEAST;
            }
        }
        else if (directionUnitVector.X <= -0.383 && (directionUnitVector.X >= -0.924))
        {
            if (directionUnitVector.Y > 0)
            {
                direction = Directions.DIR_SOUTHWEST;
            }
            else
            {
                direction = Directions.DIR_NORTHWEST;
            }
        }
        else
        {
            direction = Directions.DIR_NONE;
        }

        return direction;

    }

    /// <summary>
    /// A functon to roll the dice between the range of min and max
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static int GetRandomNumber(int min, int max)
    {
        // swap the values if our min max values are reversed
        if (min > max)
        {
            int temp = min;
            min = max;
            max = temp;
        }

        RandomNumberGenerator rng = new RandomNumberGenerator();
        return rng.RandiRange(min, max);
    }
}
