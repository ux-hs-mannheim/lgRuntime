using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace lgRuntime.PrototypingObjects.ProjectStructure.Manipulations
{
    public class MoveManipulation
    {
        // flag indicating if the move manipulation is enabled or not
        public bool IsEnabled = true;

        // the X and Y movement factors, 
        // 1 = 100% aligned with the finger, 
        // below 1 means slower movement, 
        // over 1 means faster movement
        public double XModifier = 1;
        public double YModifier = 1;

        public double DiscreteMovementDistanceX = 0;
        public double DiscreteMovementDistanceY = 0;

        // there are three different flicking bahaviours available
        public const string FLICK_NONE = "none";
        public const string FLICK_MOVE_OVER_TIME = "time";
        public const string FLICK_MOVE_OVER_DISTANCE = "distance";
        public const string FLICK_MOVE_CONSTANT = "constant";

        public string FlickingBehavior = MoveManipulation.FLICK_NONE;

        // move for X seconds
        public double MovementTime = 4.0; //seconds!
        // move X inch
        public double TravelDistance = 4.0; //inch!
        // the constant deceleration factor
        // (10 inches / second) / second on a 96dpi Screen --> deceleration
        //  1 inch = 2.54 centimeter
        // 10.0 * 96.0 / (1000.0 * 1000.0)
        public double ConstantDeceleration = 10.0;

    }
}
