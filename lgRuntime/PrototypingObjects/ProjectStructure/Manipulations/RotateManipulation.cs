using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace lgRuntime.PrototypingObjects.ProjectStructure.Manipulations
{
    public class RotateManipulation
    {
        // flag indicating if the rotate manipulation is enabled
        public bool IsEnabled = true;
        // flag indicating if the single finger rotation is enabled
        public bool OneFingerRotationEnabled = true;

        // the rotation factor, 1 = 100% aligned to fingers
        // below 1 means slower rotation
        // above 1 means faster rotation
        public double Modifier = 1;

        //discrete rotation variables
        public double DiscreteRotationAngle = 0; //0 means no discrete rotation

        // there are three different flicking bahaviours available
        public const string FLICK_NONE = "none";
        public const string FLICK_ROTATE_OVER_TIME = "time";
        public const string FLICK_ROTATE_TO_ANGLE = "angle";
        public const string FLICK_ROTATE_CONSTANT = "constant";

        public string FlickingBehavior = RotateManipulation.FLICK_NONE;

        //the maximum time the object will rotate after a flick
        public double RotationTime = 3.0;
        public double RotationToAngle = 180;
        public double ConstantDeceleration = 360.0 * 2;

    }

}
