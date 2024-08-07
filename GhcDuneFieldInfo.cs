using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace GhcDuneField
{
    public class GhcDuneFieldInfo : GH_AssemblyInfo
    {
        public override string Name => "GhcDuneField";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("CED0E277-FE55-4586-AA67-0F7B479D4DB3");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}