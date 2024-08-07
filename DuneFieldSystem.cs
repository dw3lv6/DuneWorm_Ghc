using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino;
using Rhino.Geometry;
using Rhino.DocObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Collections;

namespace GhcDuneField
{
    public class DuneFieldSystem
    {
        public int[,] Height;
        public float[,] Shadow;
        private int Width;
        private int Length;

        public bool Reset;
        private bool initialized = false;

        public Vector3d WindDir = new Vector3d(0, 0, 0);
        public Vector2d windDir;

        public int HopLength = 3;
        private float SHADOW_SLOPE = 0.83f;
        public float pSand = 0.6f;
        public float pNoSand = 0.4f;

        System.Random random = new System.Random();

        public List<Point3d> OutPoints = new List<Point3d>();
        public Surface OutSurface;
        private Vector2d lastWindDir = new Vector2d(); // 存储上一次的风向

        public DuneFieldSystem(List<Point3d> startingField)
        {
            InitializeStartingField(startingField);
        }

        public void InitializeStartingField(List<Point3d> startingField)
        {
            if (!initialized || Reset)
            {
                Width = (int)startingField.Max(pt => pt.X) + 1;
                Length = (int)startingField.Max(pt => pt.Y) + 1;

                Height = new int[Width + 1, Length + 1];
                Shadow = new float[Width + 1, Length + 1];

                foreach (var point in startingField)
                {
                    int x = (int)point.X;
                    int y = (int)point.Y;
                    int z = (int)point.Z;
                    Height[x, y] = z;
                    Shadow[x, y] = 0; // 初始阴影设为0
                }

                initialized = true;
            }
        }
        public void Update()
        {
            CalculateShadows();
            Tick();
            UpdatePoints();
        }
        public void CalculateShadows()
        {
            WindDir = DuneField_Utilities.FindClosestDirection(WindDir);
            windDir = new Vector2d(WindDir.X, WindDir.Y);
            //Rhino.RhinoApp.WriteLine("风向" + windDir);
            CheckWindDir(WindDir);

            float[,] newShadow = new float[Width, Length];
            int h, xs, ys;
            float hs;           //高度阴影

            #region change array process according to windDir
            // decide shadow accodring to wind direction, 匹配风向和数组遍历顺序
            int startX = windDir.X < 0 ? Width - 1 : 0;
            int endX = windDir.X < 0 ? -1 : Width;
            int stepX = windDir.X < 0 ? -1 : 1;

            int startY = windDir.Y < 0 ? Length - 1 : 0;
            int endY = windDir.Y < 0 ? -1 : Length;
            int stepY = windDir.Y < 0 ? -1 : 1;
            #endregion

            for (int i = startX; i != endX; i += stepX)
            {
                for (int j = startY; j != endY; j += stepY)
                {
                    h = Height[i, j];
                    if (h == 0) continue;
                    int previousI = (Width + i - (int)windDir.X) % Width;
                    int previousJ = (Length + j - (int)windDir.Y) % Length;
                    hs = Math.Max(h, newShadow[previousI, previousJ] - SHADOW_SLOPE);

                    xs = i; ys = j;
                    while (hs >= h)
                    {
                        newShadow[xs, ys] = hs;
                        hs -= SHADOW_SLOPE;
                        xs = (Width + xs + (int)windDir.X) % Width;
                        ys = (Length + ys + (int)windDir.Y) % Length;
                    }
                }
            }

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (newShadow[i, j] == Height[i, j])
                    {
                        newShadow[i, j] = 0;                    //修正阴影：检查新计算的阴影高度是否与当前位置的沙丘高度相等。如果是，将该位置的阴影高度设置为0。 
                    }
                }
            }

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    Shadow[i, j] = newShadow[i, j];
                }
            }
        }
        void Tick()
        {
            for (int i = Width * Length; i > 0; i--) // 根据需要设置执行次数
            {
                int a = random.Next(0, Width);
                int b = random.Next(0, Length);

                if (Height[a, b] == 0) { continue; } //开始下一次循环，提前结束当前的迭代
                if (Shadow[a, b] > 0) { continue; }
                erodeBehaviour(a, b);
                //Debug.Log("ERODE" + ":" + a + "," + b);

                // 跳跃沉积循环
                int n = HopLength; // 设定跳跃次数

                while (true)
                {
                    //int a, b;
                    a = (a + (int)windDir.X * HopLength + Width) % Width;
                    b = (b + (int)windDir.Y * HopLength + Length) % Length;

                    if (Shadow[a, b] > 0)   //模拟第一次弹跳   
                    {
                        depositBehaviour(a, b);
                        //Debug.Log("JUMP1");
                        break;
                    }

                    if (--n > 0) // ++i 是先增加后引用,让i先加1，然后在i所在的表达式中使用i的新值
                    {
                        // 沉积条件判断
                        if (random.NextDouble() < (Height[a, b] > 0 ? pSand : pNoSand))
                        {
                            depositBehaviour(a, b);
                            //Debug.Log("JUMP +++++++" + ":" + a +","+ b);
                            break;  //跳出循环，不需要进一步迭代。
                        }
                        n = HopLength;
                    }
                }
            }
        }
        void UpdatePoints()
        {
            OutPoints.Clear();
            for (int a = 0; a < Width; a++)
            {
                for (int b = 0; b < Length; b++)
                {
                    OutPoints.Add(new Point3d(a, b, Height[a, b]));
                }
            }
        }
        void erodeBehaviour(int a, int b)
        {
            int aSteep, bSteep;

            while (DuneField_Utilities.Upslope(Height, Width, Length, a, b, out aSteep, out bSteep) >= 2)
            {
                aSteep = (aSteep + Width) % Width;  // 循环处理
                bSteep = (bSteep + Length) % Length;
                a = aSteep;
                b = bSteep;
            }

            Height[aSteep, bSteep] = Math.Max(0, Height[aSteep, bSteep] - 1); 
        }
        void depositBehaviour(int a, int b)
        {
            int aSteep, bSteep;

            //找到最陡下坡方向
            while (DuneField_Utilities.Downslope(Height, Width, Length, a, b, out aSteep, out bSteep) >= 2)
            {
                aSteep = (aSteep + Width) % Width;  // 循环处理
                bSteep = (bSteep + Length) % Length;
                a = aSteep;
                b = bSteep;
            }

            Height[aSteep, bSteep] = Math.Min(30, ++Height[aSteep, bSteep]);  // 避免高度过大 randomMaxHeight
        }

        //public Brep GetOutputSurface()
        //{
        //    if (OutPoints != null)
        //    {
        //        IEnumerable<GeometryBase> geometryBases = OutPoints.Select(pt => new Rhino.Geometry.Point(pt) as GeometryBase);

        //        Brep patchSurface = Brep.CreatePatch(geometryBases, 20,20, 1.0);

        //        return patchSurface;
        //    }
        //    else
        //        return null;
        //}

        //   -------------------------------Output Values -------------------------------
        public GH_Structure<GH_Number> ShadowValues()
        {
            GH_Structure<GH_Number> shadowValues = new GH_Structure<GH_Number>();

            // 确保Shadow数组不为null
            if (Shadow == null) return shadowValues;

            GH_Path path = new GH_Path(0); // 创建一个新的路径，所有值都会被放在这个分支下

            for (int i = 0; i < Shadow.GetLength(0); i++)
            {
                for (int j = 0; j < Shadow.GetLength(1); j++)
                {
                    // 添加每个Shadow值到shadowValues结构中
                    shadowValues.Append(new GH_Number(Shadow[i, j]), path);
                }
            }

            return shadowValues;
        }

        //   -------------------------------Debugger -------------------------------


        public void CheckWindDir(Vector3d newWindDir)
        {
            Vector2d newWindDir2d = new Vector2d(newWindDir.X, newWindDir.Y);
            if (!lastWindDir.Equals(newWindDir2d))
            {
                WindDir = newWindDir;
                lastWindDir = newWindDir2d; // 更新上一次的风向值

                Rhino.RhinoApp.WriteLine("Wind Direction: " + newWindDir2d.ToString());

            }
        }

        public string OutputShadowValues()
        {
            // 确保Shadow数组不为null
            if (Shadow == null) return "";

            StringBuilder shadowValues = new StringBuilder();
            for (int i = 0; i < Shadow.GetLength(0); i++)
            {
                for (int j = 0; j < Shadow.GetLength(1); j++)
                {
                    shadowValues.AppendLine($"Shadow[{i}, {j}] = {Shadow[i, j]}");
                }
            }

            return shadowValues.ToString();
        }

        public string OutputHeightValues()
        {
            // 确保Shadow数组不为null
            if (Height == null) return "";

            StringBuilder HeightValues = new StringBuilder();
            for (int i = 0; i < Height.GetLength(0); i++)
            {
                for (int j = 0; j < Height.GetLength(1); j++)
                {
                    HeightValues.AppendLine($"Height[{i}, {j}] = {Height[i, j]}");
                }
            }

            return HeightValues.ToString();
        }


    }
}