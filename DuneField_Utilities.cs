using Grasshopper.Kernel;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhcDuneField
{
    public class DuneField_Utilities
    {
        public static Vector3d[] neighborDirections = {
        new Vector3d(0, 1, 0),    // 上
        new Vector3d(1, 1, 0),    // 右上
        new Vector3d(1, 0, 0),    // 右
        new Vector3d(1, -1, 0),   // 右下
        new Vector3d(0, -1, 0),   // 下
        new Vector3d(-1, -1, 0),  // 左下
        new Vector3d(-1, 0, 0),   // 左
        new Vector3d(-1, 1, 0)    // 左上
    };

        public static Vector3d FindClosestDirection(Vector3d vector)
        {
            Vector3d closestDirection = new Vector3d(0,0, 0);
            float smallestAngle = float.MaxValue;

            foreach (Vector3d direction in neighborDirections)
            {
                float angle = (float)Vector3d.VectorAngle(vector, direction);       //  (float)     problematic 
                if (angle < smallestAngle)
                {
                    smallestAngle = angle;
                    closestDirection = direction;
                }
            }
            return closestDirection;
        }
        public static int Upslope(int[,] Height, int Width, int Length, int a, int b, out int aSteep, out int bSteep)
        {
            //  6 2 8
            //  1   4
            //  5 3 7
            int wLeft, wRight, xUp, xDown;
            aSteep = a; bSteep = b;
            xUp = xDown = 0;
            int h = Height[a, b];  // 获取当前格子的高度
            int maxLength = Length - 1;
            int maxWidth = Width - 1;

            // 检查 Von Neumann 邻居
            if ((Height[a, xUp = (b - 1) & maxLength] - h) >= 2)
            {
                bSteep = xUp; return 2;
            }
            if ((Height[wRight = (a + 1) & maxWidth, b] - h) >= 2)  //(a != maxWidth) && 
            {
                aSteep = wRight; return 2;
            }
            if ((Height[wLeft = (a - 1) & maxWidth, b] - h) >= 2)   //(a > 0) && 
            {
                aSteep = wLeft; return 2;
            }
            if ((Height[a, xDown = (b + 1) & maxLength] - h) >= 2)
            {
                bSteep = xDown; return 2;
            }

            //对角线检查
            if ((Height[wLeft, xUp] - h) >= 2)
            {
                aSteep = wLeft; bSteep = xUp; return 2;
            }
            if ((Height[wRight, xUp] - h) >= 2)
            {
                aSteep = wRight; bSteep = xUp; return 2;
            }
            if ((Height[wLeft, xDown] - h) >= 2)
            {
                aSteep = wLeft; bSteep = xDown; return 2;
            }
            if ((Height[wRight, xDown] - h) >= 2)
            {
                aSteep = wRight; bSteep = xDown; return 2;
            }

            return 0;
        }

        public static int Downslope(int[,] Height, int Width, int Length, int a, int b, out int aSteep, out int bSteep)
        {
            //  8 2 6
            //  4   1
            //  7 3 5
            int wLeft, wRight, xUp, xDown;
            aSteep = a; bSteep = b;
            xUp = xDown = 0;
            int h = Height[a, b];  // 获取当前格子的高度
            int maxLength = Length - 1;
            int maxWidth = Width - 1;

            // 检查 Von Neumann 邻居
            if ((h - Height[a, xDown = (b + 1) & maxLength]) >= 2)  //2 up
            {
                bSteep = xDown; return 2;
            }
            if ((h - Height[wRight = (a + 1) & maxWidth, b]) >= 2)  //1 right
            {
                aSteep = wRight; return 2;
            }
            if ((h - Height[wLeft = (a - 1) & maxWidth, b]) >= 2)   //4 left
            {
                aSteep = wLeft; return 2;
            }
            if ((h - Height[a, xUp = (b - 1) & maxLength]) >= 2)    //3 down
            {
                bSteep = xUp; return 2;
            }

            // 检查对角线邻居
            if ((h - Height[wLeft, xDown]) >= 2)
            {
                aSteep = wLeft; bSteep = xDown; return 2;
            }
            if ((h - Height[wRight, xDown]) >= 2)
            {
                aSteep = wRight; bSteep = xDown; return 2;
            }

            if ((h - Height[wLeft, xUp]) >= 2)
            {
                aSteep = wLeft; bSteep = xUp; return 2;
            }
            if ((h - Height[wRight, xUp]) >= 2)
            {
                aSteep = wRight; bSteep = xUp; return 2;
            }
            return 0;
        }
    }
}