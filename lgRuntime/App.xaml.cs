using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace lgRuntime
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //used for special app-logic
        //some functions don't work on development hardware like touch monitors or trackpads
        public static bool DevMode = true;

        //General UI Elements, which are always there and visible
        public static MainWindow Window;
        public static Grid MainGrid;
        public static Grid ContentGrid;
        public static Canvas ToolObjects;
        public static Canvas UIObjects;

        //global randomizer
        public static Random Rnd = new Random();

        //The Tool/Play-Mode
        public int CurrentMode = 0;
        public static int MODE_TOOL = 0;
        public static int MODE_PLAY = 1;

        #region Set global DPI-Factor

        //the dpi-factor for distance/inertia calculation (device-independent)
        public static double DpiFactor = (1.0 / 96.0);

        public static void ChangeDPI(double dpi)
        {
            App.DpiFactor = (1.0 / dpi);
        }

        #endregion Set global DPI-Factor

        //Project Unit Conversion Factor
        public static double UnitConversionFactor = Utils.CmToInchFactor;

        //the Project path on the file system
        public static string ProjectPath = "/";
    }
}