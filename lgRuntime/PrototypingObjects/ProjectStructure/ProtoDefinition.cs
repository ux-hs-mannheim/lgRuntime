using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using lgRuntime.PrototypingObjects.ProjectStructure.Manipulations;

namespace lgRuntime.PrototypingObjects.ProjectStructure
{
    public class ProtoDefinition
    {
        // flag indicating if the object should be kept inside the boundaries (on the screen)
        public bool KeepInsideBoundaries = true;

        public string ObjectName = "DefaultName";

        public double X = 0;
        public double Y = 0;

        public double Width = 7.0;
        public double Height = 5.0;

        public String Color = "#FF0097";

        public String ImagePath = null;

        #region Constructors
        public ProtoDefinition()
        {

        }
        #endregion

        #region Manipulations

        public MoveManipulation MoveManipulation = new MoveManipulation();

        public RotateManipulation RotateManipulation = new RotateManipulation();

        public ScaleManipulation ScaleManipulation = new ScaleManipulation();

        #endregion
    }
}
