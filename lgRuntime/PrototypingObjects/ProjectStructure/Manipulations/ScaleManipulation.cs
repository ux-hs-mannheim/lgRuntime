using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;

namespace lgRuntime.PrototypingObjects.ProjectStructure.Manipulations
{
    public class ScaleManipulation
    {
        // flag indicating if the scale manipulation is enabled or not
        public bool IsEnabled = true;

        //the scaling factor
        public double XModifier = 1;
        public double YModifier = 1;

        public double MinWidth = 0;
		public double MinHeight = 0;
	    public double MaxWidth = 0;
        public double MaxHeight = 0;

        //discrete scaling
        public double DiscreteScalingX = 0;
        public double DiscreteScalingY = 0;

        // there are three different flicking bahaviours available + "none"
        public const string FLICK_NONE = "none";
        public const string FLICK_SCALE_OVER_TIME = "time";
        public const string FLICK_SCALE_DISTANCE = "distance";
        public const string FLICK_SCALE_CONSTANT = "constant";

        public string FlickingBehavior = ScaleManipulation.FLICK_NONE;

        //these variables are for the inertia system
        public double ScalingTime = 4.0;
        public double ScaleDistance = 2.0;
        public double ConstantDeceleration = 0.1;

    }
}
