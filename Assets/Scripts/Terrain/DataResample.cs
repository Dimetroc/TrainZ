using UnityEngine;
using System.Collections;

public class DataResample
{
    public static float[,] DoubleResample(float[,] data)
    {
        var newData = AddBordersAndDouble(data);
        var size = newData.GetLength(0);
        for (int x = 2; x < size - 4; x+=2)
        {
            for (int z = 2; z < size - 2; z++)
            {
                newData[x + 1, z] = CubicPolate(newData[x -2,z], newData[x, z], newData[x + 2, z], newData[x + 4, z]);
            }
        }
        for (int x = 2; x < size - 2; x ++)
        {
            for (int z = 2; z < size - 4; z+=2)
            {
                newData[x + 1, z] = (CubicPolate(newData[x, z - 2], newData[x, z], newData[x, z + 2], newData[x, z + 4]) + newData[x + 1, z])/2.0f;
            }
        }

        var finalData = new float[size - 4, size - 4];

        for (int x = 0; x < size - 4; x++)
        {
            for (int z = 0; z < size - 4; z++)
            {
                finalData[x, z] = newData[x + 2, z + 2];
            }
        }

        return finalData;
    }

    private static float[,] AddBordersAndDouble(float[,] data)
    {
        var size = data.GetLength(0) ;
        var newSize = (size + 1) * 2 + 1;
        var borderedData = new float[newSize, newSize];
        

        for (int x =  0; x < newSize; x++)
        {
            for (int z = 0; z < newSize; z++)
            {
                borderedData[x,z] = data[Mathf.Clamp(Mathf.CeilToInt(x/2.0f) - 1, 0, size - 1), Mathf.Clamp(Mathf.CeilToInt(z / 2.0f) - 1, 0, size - 1)];
            }
        }

        return borderedData;
    }

    private static  float CubicPolate(float p1, float p2, float p3, float p4)
    {
        var A = (p4 - p3) - (p1 - p2);
        var B = (p1 - p2) - A;
        var C = p3 - p1;
        var D = p2;

        return A * 0.125f + B * 0.25f + C * 0.5f + D;
    }

}
