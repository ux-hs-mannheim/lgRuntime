using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace lgRuntime
{
    static class Utils
    {

        #region Metro Colors

        public static SolidColorBrush COLOR_PURPLE = new SolidColorBrush(Color.FromRgb(162, 0, 255));
        public static SolidColorBrush COLOR_ORANGE = new SolidColorBrush(Color.FromRgb(240, 150, 9));
        public static SolidColorBrush COLOR_BLUE = new SolidColorBrush(Color.FromRgb(27, 161, 226));
        public static SolidColorBrush COLOR_LIME = new SolidColorBrush(Color.FromRgb(140, 191, 38));
        public static SolidColorBrush COLOR_MAGENTA = new SolidColorBrush(Color.FromRgb(255, 0, 151));
        public static SolidColorBrush COLOR_TEAL = new SolidColorBrush(Color.FromRgb(0, 171, 169));
        public static SolidColorBrush COLOR_BROWN = new SolidColorBrush(Color.FromRgb(160, 80, 0));
        public static SolidColorBrush COLOR_RED = new SolidColorBrush(Color.FromRgb(229, 20, 0));
        public static SolidColorBrush COLOR_GREEN = new SolidColorBrush(Color.FromRgb(51, 153, 51));
        public static SolidColorBrush COLOR_PINK = new SolidColorBrush(Color.FromRgb(230, 113, 184));

        #endregion

        #region Fixed Measuring Unit Conversion Factors

        public static double CmToInchFactor = (1.0 / 2.54);
        public static double InchToCmFactor = 1; //1 since the default .net measuring unit is already inch!
        public static double DIUFactor = 1; //standard factor is 1

        #endregion

        #region Math Helpers, Deg->Rad, Rad->Deg Conversion

        private static double _degreeToRadianFactor = (Math.PI / 180.0);
        private static double _radianToDegreeFactor = (180.0 / Math.PI);
        public static double DegToRad(double angle)
        {
            return angle * Utils._degreeToRadianFactor;
        }
        public static double RadToDeg(double angle)
        {
            return angle * Utils._radianToDegreeFactor;
        }

        #endregion
    }
}
