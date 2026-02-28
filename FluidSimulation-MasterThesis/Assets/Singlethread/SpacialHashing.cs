using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Grid
{
    public static class SpatialHashing
    {
        public static float CellSize = 1f;
      //  public static int Dimensions = 100;
        public static int DimensionX = 100;
        public static int DimensionY = 100;
        public static int DimensionZ = 100;


        public static Vector3Int GetCell(Vector3 position)
        {
            //Debug.Assert(position.x >= 0f, "Position x must be greater than 0! Are you missing an offset?" + position.x.ToString());
            // Debug.Assert(position.y >= 0f, "Position y must be greater than 0! Are you missing an offset?" + position.y.ToString());
            // Debug.Assert(position.z >= 0f, "Position z must be greater than 0! Are you missing an offset?" + position.z.ToString());
            return new Vector3Int((int)(position.x / CellSize), (int)(position.y / CellSize), (int)(position.z / CellSize));
        }

        public static int Hash(Vector3Int cell)
        {
            return cell.x + DimensionX/*Dimensions*/ * (cell.y + DimensionY/*Dimensions*/ * cell.z);
        }
    }
}