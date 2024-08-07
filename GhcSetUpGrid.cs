using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GhcDuneField
{
    public class GhcSetUpGrid : GH_Component
    {
        public GhcSetUpGrid()
          : base("GhcSetUpField", 
                "SetUpGrid",
              "SetUpGrid into 2dArray",
              "DuneField",
              "SetUp")
        {
        }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddIntegerParameter("Width", "FieldWidth", "Number Input", GH_ParamAccess.item, 32);
            pManager.AddIntegerParameter("Length", "FieldLength", "Number Input", GH_ParamAccess.item, 32);
            // Initial Setup
            pManager.AddIntegerParameter("InitialHeight", "SandHeight", "Number Input", GH_ParamAccess.item, 4);
            pManager.AddBooleanParameter("RandomHeight", "RandomTerrain", "Boolean Input", GH_ParamAccess.item, true);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            //pManager.AddGenericParameter("Dune Field Data", "DFD", "Data including Height and Shadow arrays", GH_ParamAccess.item);
            pManager.AddPointParameter("OutPoints", "pt", "debug grid", GH_ParamAccess.list);
        }

        private DuneFieldData myDuneFieldData;
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            int iWidth = 64;
            int iLength = 64;
            int iInitialHeight =3;
            bool iRandomHeight = false;

            if (!DA.GetData("Width", ref iWidth)) return;
            if (!DA.GetData("Length", ref iLength)) return;
            if (!DA.GetData("InitialHeight", ref iInitialHeight)) return;
            if (!DA.GetData("RandomHeight", ref iRandomHeight)) return;

            myDuneFieldData = new DuneFieldData(iWidth, iLength)
            {
                Width = iWidth,
                Length = iLength,
                InitialHeight = iInitialHeight,
                RandomHeight = iRandomHeight,
            };

            myDuneFieldData.InitializeGrid();

            //DA.SetData("Dune Field Data", myDuneFieldData.ToString());    //
            DA.SetDataList("OutPoints", myDuneFieldData.OutPoints);
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return GhcDuneField.Properties.Resources.Icon_SetUpField;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("81927041-DB4B-4769-8A96-39E1F1A769F9"); }
        }
    }

    public class DuneFieldData
    {
        public int[,] Height;
        public float[,] Shadow;

        public int Width,Length;
        public int InitialHeight;
        public bool RandomHeight;

        public List<Point3d> OutPoints = new List<Point3d>();
        Random random = new Random(2256);

        public DuneFieldData(int width, int length) //, int initialHeight, bool randomHeight
        {
            Height = new int[width, length];
            Shadow = new float[width, length];
        }

        //public override string ToString()
        //{
        //    return $"DuneFieldData with dimensions {Height.GetLength(0)}x{Height.GetLength(1)}";
        //}


        public void InitializeGrid()
        {
            Height = new int[Width, Length];
            Shadow = new float[Width, Length];

            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Length; j++)
                {
                    if (RandomHeight == true) Height[i, j] = random.Next(0, 10); // 20 MaxHeight
                    else if (RandomHeight == false) Height[i, j] = InitialHeight;

                    Point3d OPoint = new Point3d(i, j, Height[i, j]);
                    OutPoints.Add(OPoint);
                }
            }
        }


    }
}