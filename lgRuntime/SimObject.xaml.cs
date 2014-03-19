using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using lgRuntime.PrototypingObjects.ProjectStructure;
using lgRuntime.PrototypingObjects.ProjectStructure.Manipulations;
using System.Dynamic;

namespace lgRuntime
{
    /// <summary>
    /// Interaktionslogik für SelfmadeSVI.xaml
    /// </summary>
    public partial class SimObject : UserControl
    {
        #region setup variables
        // empty ProtoDefinition for debugging
        // this Object will be replaced by a specific definition at runtime
        public ProtoDefinition ProtoDefinition = new ProtoDefinition();
        #endregion

        #region Constructors

        public SimObject(ProtoDefinition protoDefinition = null)
        {
            InitializeComponent();

            if (protoDefinition != null)
            {
                this.ProtoDefinition = protoDefinition;
            }

            //apply width and height
            this.Width = (this.ProtoDefinition.Width * App.UnitConversionFactor) / App.DpiFactor;
            this.Height = (this.ProtoDefinition.Height * App.UnitConversionFactor) / App.DpiFactor;

            // applying the color
            BrushConverter cConverter = new BrushConverter();
            this.MainGrid.Background = cConverter.ConvertFromString(this.ProtoDefinition.Color) as SolidColorBrush;

            //apply the given image
            if (this.ProtoDefinition.ImagePath != null && this.ProtoDefinition.ImagePath != "")
            {
                this.MainGrid.Background = Brushes.Transparent;
                this.ObjectNameTextBlock.Visibility = Visibility.Hidden;

                //see if we can load an image
                try
                {
                    BitmapImage src = new BitmapImage();
                    src.BeginInit();
                    src.UriSource = new Uri(App.ProjectPath + this.ProtoDefinition.ImagePath, UriKind.Relative);
                    src.CacheOption = BitmapCacheOption.OnLoad;
                    src.EndInit();
                    this.OwnImage.Source = src;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                this.OwnImage.Visibility = Visibility.Visible;

            }

            // initial position
            MatrixTransform matrixTransform = this.RenderTransform as MatrixTransform;
            Matrix rectsMatrix = matrixTransform.Matrix;
            rectsMatrix.Translate((this.ProtoDefinition.X * App.UnitConversionFactor) / App.DpiFactor, (this.ProtoDefinition.Y * App.UnitConversionFactor) / App.DpiFactor);
            this.RenderTransform = new MatrixTransform(rectsMatrix);

            //applying the name for debugging if in devmode
            if (App.DevMode)
            {
                this.ObjectNameTextBlock.Text = this.ProtoDefinition.ObjectName;
            }

            //Rendering Event for selfmade inertia
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        public SimObject()
            : this(null)
        {

        }

        #endregion

        #region Manipulation Starting

        private void Canvas_ManipulationStarting(object sender, ManipulationStartingEventArgs e)
        {
            e.ManipulationContainer = App.Window;

            //setup single finger rotation
            if (this.ProtoDefinition.RotateManipulation.OneFingerRotationEnabled)
            {
                Point center = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
                center = this.TranslatePoint(center, App.Window);
                e.Pivot = new ManipulationPivot(center, 48);
            }

            this.IsInertialTranslation = false;
            this.IsInertialRotation = false;
            this.IsInertialScaling = false;

            e.Handled = true;
        }

        #endregion

        #region Manipulation Delta

        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            //get the current center point of the object
            // 1. relative to itself
            // 2. translated to the global window object --> point is then relative to the screen size
            Point centerRelative = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
            Point centerTranslatedToWindow = this.TranslatePoint(centerRelative, App.Window);

            Point manipulationOrigin = new Point()
            {
                X = e.ManipulationOrigin.X,
                Y = e.ManipulationOrigin.Y
            };

            Vector scaleVector = new Vector()
            {
                X = e.DeltaManipulation.Expansion.X,
                Y = e.DeltaManipulation.Expansion.Y
            };

            Vector translationVector = new Vector()
            {
                X = e.DeltaManipulation.Translation.X,
                Y = e.DeltaManipulation.Translation.Y
            };

            //count how many fingers are currently manipulating the object
            int numberOfFingers = e.Manipulators.Count();

            //apply the transformations
            this.Translate(translationVector) //translation in INCHES
                .Scale(scaleVector, manipulationOrigin) //scaling with a manipulation delta in INCHES
                .Rotate(centerTranslatedToWindow, manipulationOrigin, e.DeltaManipulation.Rotation); //rotation in DEGREES

            e.Handled = true;

        }

        #endregion

        #region Rotate Code

        // internal value for counting the rotation degrees
        private double DiscreteRotationAngleCounter = 0;

        //the current rotation angle of the object in degrees
        public double CurrentAngle = 0;

        private SimObject Rotate(Point centerPoint, Point ManipulationOrigin, double angle)
        {

            //One or Two Finger Rotation
            if (this.ProtoDefinition.RotateManipulation.IsEnabled)
            {

                //get the transform matrix
                Matrix transformMatrix = ((MatrixTransform)this.RenderTransform).Matrix;

                //standard 2+ Finger-Rotation center point
                Point rotationCenter = new Point(ManipulationOrigin.X, ManipulationOrigin.Y);

                //special rotation if the object is fixed in one axis (or both)
                //either the MoveManipulation is disabled or the translation modifier is reduced to 0 in either X or Y component
                if (!this.ProtoDefinition.MoveManipulation.IsEnabled || this.ProtoDefinition.MoveManipulation.XModifier == 0 || this.ProtoDefinition.MoveManipulation.YModifier == 0)
                {
                    //the object needs to be rotate around its own center for a stabilized rotation
                    rotationCenter.X = centerPoint.X;
                    rotationCenter.Y = centerPoint.Y;
                }

                //rotation by a discrete amount, no smooth analogue movement!
                if (this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle != 0)
                {
                    //if the (absolute) sum of the rotation deltas is bigger than the fixed rotation angle --> rotate
                    while (Math.Abs(this.DiscreteRotationAngleCounter) >= this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle)
                    {
                        transformMatrix.RotateAt(Math.Sign(angle) * this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle * this.ProtoDefinition.RotateManipulation.Modifier, rotationCenter.X, rotationCenter.Y);
                        this.DiscreteRotationAngleCounter -= Math.Sign(this.DiscreteRotationAngleCounter) * this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle;
                    }
                    this.DiscreteRotationAngleCounter += angle;

                }
                else
                {
                    //standard smooth analogue rotation
                    transformMatrix.RotateAt(angle * this.ProtoDefinition.RotateManipulation.Modifier, rotationCenter.X, rotationCenter.Y);
                }

                //apply the transformation
                this.RenderTransform = new MatrixTransform(transformMatrix);

                //calculate the current angle of the object
                //a random vector (magnitude > 0) is rotated through the matrix transfom
                //the rotated and the original vector are then compared
                var v = new Vector(1, 1);
                Vector vRotated = Vector.Multiply(v, transformMatrix);
                double angleAfterTransform = Vector.AngleBetween(v, vRotated);
                //the final angle ranges from 0 to 180 in the right half of the full circle, and from -1 to -180 in the left half,
                //to correct this to a full 360 deg. rotational circle the values smaller than 0 are offset against 360 deg.
                this.CurrentAngle = Math.Sign(angleAfterTransform) >= 0 ? angleAfterTransform : (360 + angleAfterTransform); ;

                //perform boundary check
                this.KeepInBoundaries();
            }

            return this;
        }

        #endregion

        #region Scaling Code

        //internal values for discrete scaling
        private double DiscreteScalingCounter_X = 0;
        private double DiscreteScalingCounter_Y = 0;

        private SimObject Scale(Vector scaleVector, dynamic manipulationOrigin)
        {
            //scaling with 2 fingers
            if (this.ProtoDefinition.ScaleManipulation.IsEnabled)
            {
                //Applying the X and Y Modifier (slowing down the scaling)
                double widthDif = (scaleVector.X * this.ProtoDefinition.ScaleManipulation.XModifier);
                double heightDif = (scaleVector.Y * this.ProtoDefinition.ScaleManipulation.YModifier);

                // The limit, against which the cummulated manipulation deltas (aka the sum of the scaleVector values) are compared,
                // is converted from the given project-wide measuring unit (e.g. cm) to diu (.net standard -> 1/96 inch), considering
                // the DPI Factor. The DPI Factor can vary between device screens and has to be set once for each project.
                if (this.ProtoDefinition.ScaleManipulation.DiscreteScalingX != 0)
                {
                    widthDif = 0;
                    double xLimit = (this.ProtoDefinition.ScaleManipulation.DiscreteScalingX * App.UnitConversionFactor) / App.DpiFactor;
                    while (Math.Abs(this.DiscreteScalingCounter_X) >= xLimit)
                    {
                        widthDif += Math.Sign(scaleVector.X) * xLimit * this.ProtoDefinition.ScaleManipulation.XModifier;
                        this.DiscreteScalingCounter_X -= Math.Sign(this.DiscreteScalingCounter_X) * xLimit;
                    }
                    this.DiscreteScalingCounter_X += scaleVector.X;
                }
                if (this.ProtoDefinition.ScaleManipulation.DiscreteScalingY != 0)
                {
                    heightDif = 0;
                    double yLimit = (this.ProtoDefinition.ScaleManipulation.DiscreteScalingY * App.UnitConversionFactor) / App.DpiFactor;
                    while (Math.Abs(this.DiscreteScalingCounter_Y) >= yLimit)
                    {
                        heightDif += Math.Sign(scaleVector.Y) * yLimit * this.ProtoDefinition.ScaleManipulation.YModifier;
                        this.DiscreteScalingCounter_Y -= Math.Sign(this.DiscreteScalingCounter_Y) * yLimit;
                    }
                    this.DiscreteScalingCounter_Y += scaleVector.Y;
                }

                //calculating the new width and height of the object
                double oldWidth = this.Width;
                double oldHeight = this.Height;
                double tempWidth = Math.Max(this.Width + widthDif, 0); //width and height < 0 are not allowed in wpf/c#
                double tempHeight = Math.Max(this.Height + heightDif, 0); 

                //convert the min/max values to DIU
                double minWidth = (this.ProtoDefinition.ScaleManipulation.MinWidth * App.UnitConversionFactor) / App.DpiFactor;
                double minHeight = (this.ProtoDefinition.ScaleManipulation.MinHeight * App.UnitConversionFactor) / App.DpiFactor;
                double maxWidth = (this.ProtoDefinition.ScaleManipulation.MaxWidth * App.UnitConversionFactor) / App.DpiFactor;
                double maxHeight = (this.ProtoDefinition.ScaleManipulation.MaxHeight * App.UnitConversionFactor) / App.DpiFactor;
                
                //only clamp the object's width/height if a value is not 0.
                //mininum size
                if (minHeight > 0)
                {
                    tempHeight = Math.Max(tempHeight, minHeight);
                }
                if (minWidth > 0)
                {
                    tempWidth = Math.Max(tempWidth, minWidth);
                }
                //maximum size
                if (maxHeight > 0)
                {
                    tempHeight = Math.Min(tempHeight, maxHeight);
                }
                if (maxWidth > 0)
                {
                    tempWidth = Math.Min(tempWidth, maxWidth);
                }

                //apply the new size
                this.Width = tempWidth;
                this.Height = tempHeight;
                
                //calculate final width/height difference for translation correction
                widthDif = this.Width - oldWidth;
                heightDif = this.Height - oldHeight;

                // Since the changing of the internal width and height properties occures only into one direction,
                // yet the scaling effect should look (and feel) like it is expanding into all directions, the translation
                // has to be corrected by half the delta scaling amount into the reversed direction of the widths and heights expansion.

                // To complicate it even more, the rotation of the object has to be considered, which means the
                // vector describing the translation correction too has to be rotated: trigonometry ftw!
                double alpha = Utils.DegToRad(this.CurrentAngle);

                Vector correctionVector = new Vector((widthDif / 2.0) * -1, (heightDif / 2.0) * -1);
                Vector rotatedCorrectionVector = 
                    new Vector(correctionVector.X * Math.Cos(alpha) - correctionVector.Y * Math.Sin(alpha),
                               correctionVector.X * Math.Sin(alpha) + correctionVector.Y * Math.Cos(alpha));

                //apply the rotated correction vector to the object
                MatrixTransform matrixTransform = this.RenderTransform as MatrixTransform;
                Matrix rectsMatrix = matrixTransform.Matrix;
                rectsMatrix.Translate(rotatedCorrectionVector.X, rotatedCorrectionVector.Y);
                this.RenderTransform = new MatrixTransform(rectsMatrix);

                this.KeepInBoundaries();

            }

            return this;
        }

        #endregion

        #region Translation Code

        //internal value for counting the discrete X movement
        private double DiscreteMovementCounter_X = 0;
        //internal value for counting the discrete Y movement
        private double DiscreteMovementCounter_Y = 0;

        private SimObject Translate(Vector translationVector, bool smooth = false)
        {

            //translation with multiple fingers
            if (this.ProtoDefinition.MoveManipulation.IsEnabled)
            {
                //get the transform matrix
                Matrix transformMatrix = ((MatrixTransform)this.RenderTransform).Matrix;

                // standard movement formula
                double xDif = translationVector.X * this.ProtoDefinition.MoveManipulation.XModifier;
                double yDif = translationVector.Y * this.ProtoDefinition.MoveManipulation.YModifier;
                
                // overriding the standard X movement value (see above) if the ProtoDefinition contains a "discrete X movement distance"
                if (this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceX != 0 && !smooth)
                {
                    // everytime the user moved his finger further than the adjusted discrete movement distance step
                    // since the finger is usually moved further than this movement step, the while loop adds multiple steps to the overall xDif/yDif
                    xDif = 0;
                    double xLimit = (this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceX * App.UnitConversionFactor) / App.DpiFactor;
                    while (Math.Abs(this.DiscreteMovementCounter_X) >= xLimit)
                    {
                        xDif += Math.Sign(DiscreteMovementCounter_X) * xLimit * this.ProtoDefinition.MoveManipulation.XModifier;
                        this.DiscreteMovementCounter_X -= Math.Sign(this.DiscreteMovementCounter_X) * xLimit;
                    }
                    this.DiscreteMovementCounter_X += translationVector.X;
                }

                // same as above, only for Y this time
                if (this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceY != 0 && !smooth)
                {
                    yDif = 0;
                    double yLimit = (this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceY * App.UnitConversionFactor) / App.DpiFactor;
                    while (Math.Abs(this.DiscreteMovementCounter_Y) >= yLimit)
                    {
                        yDif += Math.Sign(DiscreteMovementCounter_Y) * yLimit * this.ProtoDefinition.MoveManipulation.YModifier;
                        this.DiscreteMovementCounter_Y -= Math.Sign(this.DiscreteMovementCounter_Y) * yLimit;
                        
                    }
                    this.DiscreteMovementCounter_Y += translationVector.Y;
                }
                
                //apply the translation values, either analogue or discrete, or a mixture with one axis analogue and the other discrete
                transformMatrix.Translate(xDif, yDif);

                this.RenderTransform = new MatrixTransform(transformMatrix);

                //perform boundary check
                this.KeepInBoundaries();
            }

            return this;
        }

        #endregion

        #region Check Boundaries

        private void KeepInBoundaries()
        {
            //check if the object should be kept inside the given boundaries after inertia behaviour has started
            if ((this.IsInertialTranslation || this.IsInertialScaling || this.IsInertialRotation) && this.ProtoDefinition.KeepInsideBoundaries)
            {
                //window boundaries
                Rect boundaries = new Rect(((FrameworkElement)App.Window).RenderSize);
                //the objects bounds
                Rect thisBounds = this.RenderTransform.TransformBounds(new Rect(this.RenderSize));

                //stop inertial movement before the object leaves the boundaries
                if ((this.IsInertialTranslation || this.IsInertialScaling || this.IsInertialRotation) && !boundaries.Contains(thisBounds))
                {
                    this.IsInertialTranslation = false;
                    this.IsInertialScaling = false;
                    this.IsInertialRotation = false;
                }
            }
        }

        #endregion

        #region Inertia Setup

        //if set to true this flag kickstarts the inertia processing during the rendering event
        private bool IsInertialTranslation = false;
        private bool IsInertialRotation = false;
        private bool IsInertialScaling = false;

        //this timespan is a timestamp since the last rendering event call, used to calculate framerate
        //important for a smooth motion calculation
        private TimeSpan lastRenderTime;

        //the manipulation origin during the inertia movement
        //this point changes over time because of translation inertia etc.
        private Point inertiaManipulationOrigin = new Point(0, 0);

        /// <summary>
        /// Delegate the inertia implementation to the Manipulation objects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            //getting the original Manipulation Origin Point
            this.inertiaManipulationOrigin = new Point()
            {
                X = e.ManipulationOrigin.X,
                Y = e.ManipulationOrigin.Y
            };

            //if inertia is activated setup the necessary variables
            if (this.ProtoDefinition.MoveManipulation.FlickingBehavior != MoveManipulation.FLICK_NONE)
            {
                this.startInertiaTranslation(e);
            }

            if (this.ProtoDefinition.RotateManipulation.FlickingBehavior != RotateManipulation.FLICK_NONE)
            {
                this.startInertiaRotation(e);
            }

            if (this.ProtoDefinition.ScaleManipulation.FlickingBehavior != ScaleManipulation.FLICK_NONE)
            {
                this.startInertiaScaling(e);
            }

            e.Handled = true;
        }

        //the deceleration delta for the translation inertia behavior
        private double inertiaTranslationDecelDelta = (10.0 / (1.0 / 96.0)) / (1000.0 * 1000.0);
        private Vector inertiaTranslationVector = new Vector(0, 0);

        private void startInertiaTranslation(ManipulationInertiaStartingEventArgs e)
        {
            //TRANSLATION: calculate the overall speedTranslation, depended on the final velocity of the fingers after lifting from the sim object
            double speedTranslation = e.InitialVelocities.LinearVelocity.Length * App.DpiFactor; //* 10.416; //the initial speedTranslation (depending on the dpi of the screen --> Magic Number for dpi of 96: 10.416 inch)
            double movementTimeInMS = this.ProtoDefinition.MoveManipulation.MovementTime * 1000.0;

            double msTillDistanceIsReached = (2 * this.ProtoDefinition.MoveManipulation.TravelDistance * App.UnitConversionFactor) / speedTranslation; //fixed distance
            //double distanceTraveledOverTime = (speedTranslation * movementTimeInMS) / 2; //fixed time

            //TRANSLATION: Calculate the deceleration delta
            switch (this.ProtoDefinition.MoveManipulation.FlickingBehavior)
            {
                case MoveManipulation.FLICK_MOVE_OVER_TIME: this.inertiaTranslationDecelDelta = (speedTranslation / movementTimeInMS) / App.DpiFactor; break; //equivalent formula: ((2 * distanceTraveledOverTime) / Math.Pow(movementTimeInMS, 2.0)) / App.DpiFactor; break;
                case MoveManipulation.FLICK_MOVE_OVER_DISTANCE: this.inertiaTranslationDecelDelta = (speedTranslation / msTillDistanceIsReached) / App.DpiFactor; break; //(((speedTranslation * speedTranslation) / (2 * this.ProtoDefinition.MoveManipulation.TravelDistance)) / App.DpiFactor) / (1000.0 * 1000.0); break;
                case MoveManipulation.FLICK_MOVE_CONSTANT:
                default: this.inertiaTranslationDecelDelta = ((this.ProtoDefinition.MoveManipulation.ConstantDeceleration * App.UnitConversionFactor) / App.DpiFactor) / (1000.0 * 1000.0); break;
            }
            this.inertiaTranslationVector = new Vector(e.InitialVelocities.LinearVelocity.X, e.InitialVelocities.LinearVelocity.Y);

            //Debug.log("Translation Inertia started; v_t = " + this.inertiaTranslationVector + "; decel = " + this.inertiaTranslationDecelDelta);

            //go go power rangers!
            this.IsInertialTranslation = true;
        }

        //the deceleration delta for the translation inertia behavior
        private double inertiaScalingDecelDelta = 0.1;
        private Vector inertiaScalingVector = new Vector(0, 0);

        private void startInertiaScaling(ManipulationInertiaStartingEventArgs e)
        {
            Debug.log("scale: " + e.InitialVelocities.ExpansionVelocity + ", l: " + e.InitialVelocities.ExpansionVelocity.Length);
            double speedScaling = e.InitialVelocities.ExpansionVelocity.Length * App.DpiFactor; //the initial scaling speed
            double scalingTimeInMS = this.ProtoDefinition.ScaleManipulation.ScalingTime * 1000.0;

            double msTillFactorIsReached = (2 * this.ProtoDefinition.ScaleManipulation.ScaleDistance * App.UnitConversionFactor) / speedScaling; //fixed distance
            //double distanceTraveledOverTime = (speedTranslation * movementTimeInMS) / 2; //fixed time

            //TRANSLATION: Calculate the deceleration delta
            switch (this.ProtoDefinition.ScaleManipulation.FlickingBehavior)
            {
                case ScaleManipulation.FLICK_SCALE_OVER_TIME: this.inertiaScalingDecelDelta = (speedScaling / scalingTimeInMS) / App.DpiFactor; break; //equivalent formula: ((2 * distanceTraveledOverTime) / Math.Pow(movementTimeInMS, 2.0)) / App.DpiFactor; break;
                case ScaleManipulation.FLICK_SCALE_DISTANCE: this.inertiaScalingDecelDelta = (speedScaling / msTillFactorIsReached) / App.DpiFactor; break; //(((speedTranslation * speedTranslation) / (2 * this.ProtoDefinition.ScaleManipulation.ScaleDistance)) / App.DpiFactor) / (1000.0 * 1000.0); break;
                case ScaleManipulation.FLICK_SCALE_CONSTANT:
                default: this.inertiaScalingDecelDelta = ((this.ProtoDefinition.ScaleManipulation.ConstantDeceleration * App.UnitConversionFactor) / App.DpiFactor) / (1000.0 * 1000.0); break;
            }
            this.inertiaScalingVector = new Vector(e.InitialVelocities.ExpansionVelocity.X, e.InitialVelocities.ExpansionVelocity.Y);

            //Debug.log("Scaling speed length: " + speedScaling);
            //Debug.log("Scaling Inertia Started; v_s = " + this.inertiaScalingVector + "; decel = " + this.inertiaScalingDecelDelta + "; duration = " + msTillFactorIsReached + "ms");

            this.IsInertialScaling = true;
        }

        //deceleration for the rotation inertia behavior
        private double inertiaRotationDecelDelta = 360 * 2;
        private double inertiaRotationAngle = 30; //the angular velocity during the inertia movement
        private int inertiaRotationDirection = 1; //1 means to the right, -1 means to the left, same as Math.Sign(inertiaRotationAngle)

        private void startInertiaRotation(ManipulationInertiaStartingEventArgs e)
        {
            //ROTATION: speed and rotation centers, initial angulr velocity is measured in deg/s (NOT in rad!)
            double speedRotation = Math.Abs(e.InitialVelocities.AngularVelocity);
            double rotationTimeInMS = this.ProtoDefinition.RotateManipulation.RotationTime * 1000.0;

            double msTillAngleIsReached = (2 * this.ProtoDefinition.RotateManipulation.RotationToAngle) / speedRotation; //fixed distance
            //double angleTraveledOverTime = (speedRotation * rotationTimeInMS) / 2; //fixed angular distance

            //ROTATION: deceleration delta
            switch (this.ProtoDefinition.RotateManipulation.FlickingBehavior)
            {
                case RotateManipulation.FLICK_ROTATE_OVER_TIME: this.inertiaRotationDecelDelta = speedRotation / rotationTimeInMS; break; //equivalent formula: (2 * angleTraveledOverTime) / Math.Pow(rotationTimeInMS, 2.0); break;
                case RotateManipulation.FLICK_ROTATE_TO_ANGLE: this.inertiaRotationDecelDelta = speedRotation / msTillAngleIsReached; break; //fixed angular distance
                case RotateManipulation.FLICK_ROTATE_CONSTANT:
                default: this.inertiaRotationDecelDelta = this.ProtoDefinition.RotateManipulation.ConstantDeceleration / (1000.0 * 1000.0); break; //fixed deceleration in deg per seconds square
            }
            this.inertiaRotationAngle = Math.Abs(e.InitialVelocities.AngularVelocity);
            this.inertiaRotationDirection = Math.Sign(e.InitialVelocities.AngularVelocity);

            //and start..
            this.IsInertialRotation = true;
        }

        #endregion

        #region Inertia Implementation

        /**
         * This method performs the necessary inertia calculations, such as slowing down the movement.
         * The composite rendering event handler is called with about every 16ms, that's ~60fps.
         */
        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            //only perform independent movement if the object is inertial
            if (this.IsInertialTranslation || this.IsInertialRotation || this.IsInertialScaling)
            {
                //initial timestamp, when the rendering loop is first called
                if (lastRenderTime == null)
                {
                    lastRenderTime = ((RenderingEventArgs)e).RenderingTime;
                }
                //get the time in miliseconds
                double lastMS = lastRenderTime.Milliseconds;
                //return if the rendering event came "too fast" (as in no time difference can be distinguished)
                if (lastRenderTime == ((RenderingEventArgs)e).RenderingTime)
                {
                    return;
                }
                lastRenderTime = ((RenderingEventArgs)e).RenderingTime;

                double nowMS = lastRenderTime.Milliseconds;

                //the time difference (in milliseconds) between two event calls determines the overall framerate
                double timeDifference = nowMS - lastMS;

                //HACK!
                //I have no clue why this is a problem, but this "if-statement" fixes it...
                //filter out some timing differences, the average difference in milliseconds should be around 15-17ms, this correlates to ~60fps
                //i don't know how some differences of -900ms or +748ms come to be... wierd
                if (timeDifference < 0 || timeDifference > 60)
                {
                    if (App.DevMode)
                    {
                        //Console.WriteLine("wierd time: " + timeDifference);
                    }
                    return;
                }

                /**
                 * General Position of the object during inertia
                 */
                Point centerRelative = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
                Point centerTranslatedToWindow = this.TranslatePoint(centerRelative, App.Window);

                /**
                 * Translation, shortening the initial translation vector step by step
                 */
                if (this.IsInertialTranslation)
                {
                    // a velocity-vector consists of a lenght and a direction
                    Vector translationVelocity = inertiaTranslationVector;
                    double translationVelocityLength = translationVelocity.Length;
                    Vector translationVelocityDirection = translationVelocity;

                    //normalizing the translation vector for later multiplication with the shortend new length (due to consecutive inertia)
                    translationVelocityDirection.Normalize();

                    // Determine new velocity magnitude and velocity vector
                    // by reducing the length of the velocity vector and keeping the direction the same as before
                    translationVelocityLength = Math.Max(0, translationVelocityLength - (this.inertiaTranslationDecelDelta * timeDifference));
                    inertiaTranslationVector = translationVelocityLength * translationVelocityDirection;

                    //check if the translation vector still has some magnitude
                    if (translationVelocityLength != 0)
                    {
                        //Apply the inertiaTranslationVector to the SimObject 'n'-times, where 'n' is the number of milliseconds since the last Rendering Event call
                        for (int i = 0; i < timeDifference; i++)
                        {
                            //IMPORTANT: the original manipulation origin point must be updated during translation, otherwise the rotation will occur around the wrong point!
                            this.inertiaManipulationOrigin.X += inertiaTranslationVector.X;
                            this.inertiaManipulationOrigin.Y += inertiaTranslationVector.Y;

                            this.Translate(inertiaTranslationVector);
                        }
                    }
                    else
                    {
                        //Inertia stopped after the magnitude of the translation vector reaches 0
                        this.IsInertialTranslation = false;
                        //reseting the framerate timer
                        //lastRenderTime = TimeSpan.Zero;
                        Debug.log("Translation Inertia ended.");
                    }
                }

                /**
                 * Rotation
                 */
                if (this.IsInertialRotation)
                {

                    this.inertiaRotationAngle = Math.Max(0, this.inertiaRotationAngle - (this.inertiaRotationDecelDelta * timeDifference));

                    if (this.inertiaRotationAngle > 0)
                    {
                        for (int i = 0; i < timeDifference; i++)
                        {
                            //Debug.log("rotated: " + this.inertiaRotationAngle);
                            this.Rotate(centerTranslatedToWindow, this.inertiaManipulationOrigin, this.inertiaRotationDirection * this.inertiaRotationAngle);
                        }
                    }
                    else
                    {
                        this.IsInertialRotation = false;
                        this.inertiaRotationAngle = 0;
                        //lastRenderTime = TimeSpan.Zero;
                        Debug.log("Rotation Inertia ended.");
                    }
                }

                /**
                 * Scaling
                 */
                if (this.IsInertialScaling)
                {
                    // a velocity-vector consists of a lenght and a direction
                    Vector scalingVelocity = this.inertiaScalingVector;
                    double scalingVelocityLength = scalingVelocity.Length;
                    Vector scalingVelocityDirection = scalingVelocity;

                    //normalizing the scaling vector, same as translation
                    scalingVelocityDirection.Normalize();

                    // Determine new velocity magnitude and velocity vector
                    // by reducing the length of the velocity vector and keeping the direction the same as before
                    scalingVelocityLength = Math.Max(0, scalingVelocityLength - (this.inertiaScalingDecelDelta * timeDifference));
                    this.inertiaScalingVector = scalingVelocityLength * scalingVelocityDirection;

                    //check if the translation vector still has some magnitude
                    if (scalingVelocityLength != 0)
                    {
                        //Apply the new scaling vector to the SimObject for each millisecond passed since the last rendering event call
                        for (int i = 0; i < timeDifference; i++)
                        {
                            this.Scale(this.inertiaScalingVector, this.inertiaManipulationOrigin);
                        }
                    }
                    else
                    {
                        //Inertia stopped after the magnitude of the translation vector reaches 0
                        this.IsInertialScaling = false;
                        //reseting the framerate timer
                        //lastRenderTime = TimeSpan.Zero;
                        Debug.log("Scaling Inertia ended.");
                    }

                }
            }
        }

        #endregion

        #region Debug

        private void MainViewbox_PreviewTouchUp(object sender, TouchEventArgs e)
        {
            if (this.TouchesOver.Count() > 1)
            {
                return;
            }
            Matrix transformMatrix = ((MatrixTransform)this.RenderTransform).Matrix;

            Debug.log("Object Info (" + this.ProtoDefinition.ObjectName + "):");
            Debug.log("--> (x, y) = (" + transformMatrix.OffsetX + ", " + transformMatrix.OffsetY + ")");
            Debug.log("--> [w, h] = [" + this.Width + ", " + this.Height + "]");
            Debug.log("--> angle_deg = " + this.CurrentAngle + ", angle_rad = " + Utils.DegToRad(this.CurrentAngle));
        }

        #endregion

    }
}
