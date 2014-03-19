using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;
using lgRuntime.PrototypingObjects.ProjectStructure;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using lgRuntime.UI;

namespace lgRuntime
{
    /// <summary>
    /// Interaction logic for SurfaceWindow1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            //Register Window in the App Context
            App.Window = this;
            App.MainGrid = this.MainGrid;
                        
            //debug log
            Debug.log("Started in DevMode? " + App.DevMode);

            //Workaround for
            //SurfaceTouchHelper.ReenableWPFTabletSupport(this);

            string loadedFile = File.ReadAllText("projects/lgStudy/project.json");
            Debug.log("file loaded 'project.json':");

            JavaScriptSerializer jsSerializer = new JavaScriptSerializer();
            Project project = jsSerializer.Deserialize<Project>(loadedFile);

            App.ProjectPath = "projects/lgStudy/";

            Debug.log("#### json file deserialized #####");
            Debug.log("project: " + project.Name);
            Debug.log("-> unit: " + project.Unit);
            Debug.log("-> dpi: " + project.DPI);

            //setting the DPI for the project, used to calculate distances (in inch) depending on the device
            App.ChangeDPI(project.DPI);

            switch (project.Unit)
            {
                case Project.UNIT_CM: App.UnitConversionFactor = Utils.CmToInchFactor; break;
                case Project.UNIT_INCH: App.UnitConversionFactor = Utils.InchToCmFactor; break;
                case Project.UNIT_PIXEL: App.UnitConversionFactor = 1; break; //TODO: figure out how to convert from inch to pixel in .net
                //if the prototype should operate in DIU, 1 DIU = 1/96 inch.
                case Project.UNIT_DIU: App.UnitConversionFactor = 1; App.ChangeDPI(1); break;
            }

            Debug.log("-> screens:");
            foreach (KeyValuePair<string, Screen> entry in project.Screens)
            {
                Debug.log("   -> " + entry.Value.Name);

                //adding a screen "switch" button to the Runtime UI
                lgButton screenButton = new lgButton()
                {
                    TextValue = entry.Value.Name,
                    Height = 50,
                    Margin = new Thickness(10,10,0,0)
                };
                screenButton.TouchUp += (sender, e) => {
                    this.ChangeScreen(entry.Value);
                };

                this.ScreenButtons.Children.Add(screenButton);
            }
            /*Debug.log("-> templates:");
            foreach (KeyValuePair<string, ProtoDefinition> entry in project.Templates)
            {
                Debug.log("   -> " + entry.Key);
            }*/
        }

        #region changing the screen

        private void ChangeScreen(Screen screen)
        {
            Debug.log("Changing screen: " + screen.Name);
            this.ScreenName.Text = screen.Name;
            this.PrototypingObjects.Children.Clear();
            foreach (ProtoDefinition instance in screen.Instances)
            {
                //add instances
                SimObject smSVI = new SimObject(instance);
                this.PrototypingObjects.Children.Add(smSVI);
            }
        }

        #endregion

        #region Window Load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IntPtr windowHandle = new WindowInteropHelper(Application.Current.MainWindow).Handle;
            Debug.log("window handle: " + windowHandle);
            Debug.log("screen height (px): " + SystemParameters.PrimaryScreenHeight);
            Debug.log("screen width  (px): " + SystemParameters.PrimaryScreenWidth);
        }
        #endregion

        #region closing the window

        private void HandleKeyboardPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                App.Current.Shutdown();
            }

            if (e.Key == Key.Space)
            {
                if (this.RuntimeUIGrid.Visibility == Visibility.Hidden)
                {
                    this.RuntimeUIGrid.Visibility = Visibility.Visible;
                }
                else
                {
                    this.RuntimeUIGrid.Visibility = Visibility.Hidden;
                }
            }
        }

        private void CloseButton_TouchDown(object sender, TouchEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            App.Current.Shutdown();
        }

        #endregion

    }
}