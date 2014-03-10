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

namespace lgRuntime
{
    /// <summary>
    /// Interaktionslogik für SelfmadeSVI.xaml
    /// </summary>
    public partial class SelfmadeSVI : UserControl
    {
        // empty ProtoDefinition for debugging
        // this Object will be replaced by a specific definition at runtime
        public ProtoDefinition ProtoDefinition = new ProtoDefinition();

        #region discrete rotation
        
        // internal value for counting the rotation degrees
        private double DiscreteRotationAngleCounter = 0;
        
        #endregion

        #region discrete X and Y movement

        //internal value for counting the discrete X movement
        private double DiscreteMovementCounter_X = 0;
        //internal value for counting the discrete Y movement
        private double DiscreteMovementCounter_Y = 0;

        #endregion

        #region discrete scaling

        private double DiscreteScalingCounter_X = 0;
        private double DiscreteScalingCounter_Y = 0;

        #endregion

        public SelfmadeSVI(ProtoDefinition protoDefinition = null)
        {
            InitializeComponent();

            if (protoDefinition != null)
            {
                this.ProtoDefinition = protoDefinition;
            }

            //apply width and height
            this.MainCanvas.Width = this.ProtoDefinition.Width;
            this.MainCanvas.Height = this.ProtoDefinition.Height;

            // applying the color
            BrushConverter cConverter = new BrushConverter();
            this.OwnColoredRectangle.Fill = cConverter.ConvertFromString(this.ProtoDefinition.Color) as SolidColorBrush;

            //TODO: apply the given image

            // initial position
            MatrixTransform matrixTransform = this.RenderTransform as MatrixTransform;
            Matrix rectsMatrix = matrixTransform.Matrix;
            rectsMatrix.Translate(this.ProtoDefinition.X, this.ProtoDefinition.Y);
            this.RenderTransform = new MatrixTransform(rectsMatrix);
        }

        public SelfmadeSVI() : this(null)
        {

        }

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

            e.Handled = true;
        }

        private void Canvas_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            Matrix rectsMatrix = ((MatrixTransform)this.RenderTransform).Matrix;

            //get the current center point of the object
            // 1. relative to itself
            // 2. translated to the global window object --> point is then relative to the screen size
            Point centerRelative = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
            Point centerTranslatedToWindow = this.TranslatePoint(centerRelative, App.Window);

            //count how many fingers are currently manipulating the object
            int numberOfFingers = e.Manipulators.Count();

            //One or Two Finger Rotation
            if (this.ProtoDefinition.RotateManipulation.IsEnabled)
            {
                //standard 2+ Finger-Rotation center point
                Point rotationCenter = new Point(e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

                //special rotation if the object is fixed in one axis (or both)
                //either CanMove is false, or the translation modifier is reduced to 0 in its X or Y component
                if (!this.ProtoDefinition.MoveManipulation.IsEnabled || this.ProtoDefinition.MoveManipulation.XModifier == 0 || this.ProtoDefinition.MoveManipulation.YModifier == 0)
                {
                    //the object needs to be rotate around its own center for a stabilized rotation
                    rotationCenter.X = centerTranslatedToWindow.X;
                    rotationCenter.Y = centerTranslatedToWindow.Y;
                }

                //rotation by a discrete amount, no smooth analogue movement!
                if (this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle != 0)
                {
                    //if the (absolute) sum of the rotation deltas is bigger than the fixed rotation angle --> rotate
                    if (Math.Abs(this.DiscreteRotationAngleCounter) >= this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle)
                    {
                        rectsMatrix.RotateAt(Math.Sign(e.DeltaManipulation.Rotation) * this.ProtoDefinition.RotateManipulation.DiscreteRotationAngle, rotationCenter.X, rotationCenter.Y);
                        this.DiscreteRotationAngleCounter = 0;
                    }
                    else if (e.DeltaManipulation.Rotation != 0)
                    {
                        //sum up all rotation deltas
                        this.DiscreteRotationAngleCounter += e.DeltaManipulation.Rotation;
                    }

                }
                else
                {
                    //standard smooth analogue rotation
                    rectsMatrix.RotateAt(e.DeltaManipulation.Rotation * this.ProtoDefinition.RotateManipulation.Modifier, rotationCenter.X, rotationCenter.Y);
                }

            }

            //scaling with 2 fingers
            if (this.ProtoDefinition.ScaleManipulation.IsEnabled) // && e.DeltaManipulation.Scale.X != 1)
            {
                double xScale = 1 + (e.DeltaManipulation.Scale.X - 1) * this.ProtoDefinition.ScaleManipulation.XModifier;
                double yScale = 1 + (e.DeltaManipulation.Scale.Y - 1) * this.ProtoDefinition.ScaleManipulation.YModifier;

                //discrete X Scaling
                if (this.ProtoDefinition.ScaleManipulation.DiscreteScalingX != 0)
                {
                    if (Math.Abs(this.DiscreteScalingCounter_X) >= 0.75)
                    {
                        xScale = 1 - (Math.Sign(1 - e.DeltaManipulation.Scale.X) * this.ProtoDefinition.ScaleManipulation.DiscreteScalingX);
                        this.DiscreteScalingCounter_X = 0;
                    }
                    else if(e.DeltaManipulation.Scale.X != 1.0)
                    {
                        this.DiscreteScalingCounter_X += ((1 - e.DeltaManipulation.Scale.X) * 10);
                        xScale = 1;
                    }
                }

                if (this.ProtoDefinition.ScaleManipulation.DiscreteScalingY != 0)
                {
                    if (Math.Abs(this.DiscreteScalingCounter_Y) >= 0.75)
                    {
                        yScale = 1 - (Math.Sign(1 - e.DeltaManipulation.Scale.Y) * this.ProtoDefinition.ScaleManipulation.DiscreteScalingY);
                        this.DiscreteScalingCounter_Y = 0;
                    }
                    else if (e.DeltaManipulation.Scale.Y != 1.0)
                    {
                        this.DiscreteScalingCounter_Y += ((1 - e.DeltaManipulation.Scale.Y) * 10);
                        yScale = 1;
                    }
                }

                rectsMatrix.ScaleAt(xScale, yScale, e.ManipulationOrigin.X, e.ManipulationOrigin.Y);

            }

            //translation with multiple fingers
            if (this.ProtoDefinition.MoveManipulation.IsEnabled)
            {
                //discrete movement, like on a chess board for example

                // standard movement formula
                double xDif = e.DeltaManipulation.Translation.X * this.ProtoDefinition.MoveManipulation.XModifier;
                double yDif = e.DeltaManipulation.Translation.Y * this.ProtoDefinition.MoveManipulation.YModifier;

                // overriding the standard movement if the ProtoDefinition contains a "discrete X movement distance"
                if (this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceX != 0)
                {
                    if (Math.Abs(this.DiscreteMovementCounter_X) >= this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceX)
                    {
                        xDif = Math.Sign(e.DeltaManipulation.Translation.X) * this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceX * this.ProtoDefinition.MoveManipulation.XModifier;
                        this.DiscreteMovementCounter_X = 0;
                    }
                    else if(e.DeltaManipulation.Translation.X != 0)
                    {
                        this.DiscreteMovementCounter_X += e.DeltaManipulation.Translation.X;
                        xDif = 0;
                    }
                }

                // overriding the standard movement if the ProtoDefinition contains a "discrete Y movement distance"
                if(this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceY != 0)
                {
                    if (Math.Abs(this.DiscreteMovementCounter_Y) >= this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceY)
                    {
                        yDif = Math.Sign(e.DeltaManipulation.Translation.Y) * this.ProtoDefinition.MoveManipulation.DiscreteMovementDistanceY * this.ProtoDefinition.MoveManipulation.YModifier;
                        this.DiscreteMovementCounter_Y = 0;
                    }
                    else if (e.DeltaManipulation.Translation.Y != 0)
                    {
                        this.DiscreteMovementCounter_Y += e.DeltaManipulation.Translation.Y;
                        yDif = 0;
                    }
                }

                //apply the translation values, either analogue or discrete, or a mixture with one axis analogue and the other discrete
                rectsMatrix.Translate(xDif, yDif);
                
            }

            //apply the matrix transformation to the render transform of the object
            this.RenderTransform = new MatrixTransform(rectsMatrix);

            //check if the object should be kept inside the given boundaries after inertia behaviour has started
            if (this.ProtoDefinition.KeepInsideBoundaries)
            {
                //window boundaries
                Rect boundaries = new Rect(((FrameworkElement)e.ManipulationContainer).RenderSize);
                //the objects bounds
                Rect thisBounds = this.RenderTransform.TransformBounds(new Rect(this.RenderSize));

                //stop movement inside boundaries
                if (e.IsInertial && !boundaries.Contains(thisBounds))
                {
                    e.Complete();
                }
            }
            e.Handled = true;

            //Console.WriteLine("Selfmade SVI, rendertransform: " + rectsMatrix.M11);
        }

        /// <summary>
        /// Delegate the inertia implementation to the Manipulation objects
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Canvas_ManipulationInertiaStarting(object sender, ManipulationInertiaStartingEventArgs e)
        {
            //this.ProtoDefinition.MoveManipulation.InertiaImplementation(sender, e);

            //this.ProtoDefinition.ScaleManipulation.InertiaImplementation(sender, e);

            //this.ProtoDefinition.RotateManipulation.InertiaImplementation(sender, e);

            e.Handled = true;
        }
    }
}
