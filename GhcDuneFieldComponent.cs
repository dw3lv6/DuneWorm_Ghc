using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GhcDuneField
{
    public class GhcDuneFieldComponent : GH_Component
    {
        public GhcDuneFieldComponent()
          : base("GhcDuneFieldComponent", 
                "DuneFieldSolver",
                "Dune Movement Simulation Based on CA",
                "DuneField", 
                "Solver")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("StartingField", "S F", "S F", GH_ParamAccess.list, null);
            pManager.AddBooleanParameter("Reset", "Reset", "Reset", GH_ParamAccess.item, false);
            //pManager.AddBooleanParameter("Simulate", "S", "S", GH_ParamAccess.item, false);
            // -- FOLLOWING UP: Random scale setting also + input height information

            // Simulation Settings
            //pManager.AddIntegerParameter("HopLength", "HopLength", "Number Input", GH_ParamAccess.item);
            pManager.AddVectorParameter("WindDir", "WindDir", "Vector Input", GH_ParamAccess.item, new Vector3d(1,0,0));
            // -- FOLLOWING UP: p Sand + pNoSand + Shadow_Slope

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("ShadowValues", "D", "D", GH_ParamAccess.tree);

            pManager.AddTextParameter("ShadowValuesDebugger", "D", "D", GH_ParamAccess.item);
            pManager.AddTextParameter("HeightValuesDebugger", "D", "D", GH_ParamAccess.item);

            pManager.AddPointParameter("DuneFieldPoints", "Points", "Points", GH_ParamAccess.list);
            //pManager.AddBrepParameter("DuneFieldSurface", "DuneField", "DuneField", GH_ParamAccess.item);

        }

        private DuneFieldSystem myDuneFieldSystem;

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // starting points
            List<Point3d> StartingField = new List<Point3d>();
            bool iReset = false;
            //bool iSimulate = false;
            Vector3d iWindDir = Vector3d.Zero;

            if (!DA.GetDataList("StartingField", StartingField)) return;
            if (!DA.GetData("Reset", ref iReset)) return;
            //if (!DA.GetData("Simulate", ref iReset)) return;
            if (!DA.GetData("WindDir", ref iWindDir)) return;

            //====================================================================================

            if (myDuneFieldSystem == null || iReset)
            {
                myDuneFieldSystem = new DuneFieldSystem(StartingField)
                {
                    Reset = iReset,
                };
            }

            myDuneFieldSystem.WindDir = iWindDir;
            myDuneFieldSystem.Update();

            
            GH_Structure<GH_Number> shadowValues = myDuneFieldSystem.ShadowValues();
            DA.SetDataTree(0, shadowValues);

            DA.SetData("ShadowValuesDebugger", myDuneFieldSystem.OutputShadowValues());
            DA.SetData("HeightValuesDebugger", myDuneFieldSystem.OutputHeightValues());

            DA.SetDataList("DuneFieldPoints", myDuneFieldSystem.OutPoints);
            //DA.SetData("DuneFieldSurface", myDuneFieldSystem.GetOutputSurface());
        }


        protected override System.Drawing.Bitmap Icon
        {
            get{
                return GhcDuneField.Properties.Resources.Icon_DuneField;
            }
        }

        public override Guid ComponentGuid => new Guid("35317F7F-9927-42E8-BDFD-407303FE95A1");
    }
}