using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace lgRuntime.PrototypingObjects.ProjectStructure
{
    public class Project
    {
        public string Name = "Empty-Project";

        public double DPI = 96.0;

        public Dictionary<string, Screen> Screens = new Dictionary<string, Screen>();
        public Dictionary<string, ProtoDefinition> Templates = new Dictionary<string, ProtoDefinition>();

        //Measuring units for movements
        public const string UNIT_CM = "cm";
        public const string UNIT_INCH = "inch";
        public const string UNIT_PIXEL = "pixel";
        public const string UNIT_DIU = "diu";

        //default unit is cm
        public string Unit = Project.UNIT_CM; 
    }
}
