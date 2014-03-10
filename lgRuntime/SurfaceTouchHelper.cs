using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace HSMA
{
    /// <summary>
    /// Author: Thorsten Hochreuter
    /// </summary>
    class SurfaceTouchHelper
    {
        /// <summary>
        /// Code partially from http://msdn.microsoft.com/en-us/library/vstudio/dd901337(v=vs.90).aspx
        /// </summary>
        /// <param name="window"></param>
        public static void ReenableWPFTabletSupport(Window window)
        {
            //getting window handle
            WindowInteropHelper helper = new WindowInteropHelper(window);
            helper.EnsureHandle();
            IntPtr source = helper.Handle;

            // Get a collection of the tablet devices for this window.  
            TabletDeviceCollection devices = Tablet.TabletDevices;
            Console.WriteLine("Tablet Devices before workaround: " + devices.Count);

            //if there are none -> create a new one
            if (devices.Count == 0)
            {
                // Get the Type of InputManager.
                Type inputManagerType = typeof(System.Windows.Input.InputManager);

                // Call the StylusLogic method on the InputManager.Current instance.
                object stylusLogic = inputManagerType.InvokeMember("StylusLogic",
                            BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                            null, InputManager.Current, null);

                if (stylusLogic != null)
                {
                    //  Get the type of the stylusLogic returned from the call to StylusLogic.
                    Type stylusLogicType = stylusLogic.GetType();

                    // Loop until there are no more devices to remove.
                    if (devices.Count == 0)
                    {
                        // Remove the first tablet device in the devices collection.

                        stylusLogicType.InvokeMember("OnTabletAdded",
                                BindingFlags.InvokeMethod | BindingFlags.Instance | BindingFlags.NonPublic,
                                null, stylusLogic, new object[] { (uint)source });
                    }
                }

            }
            Console.WriteLine("Tablet Devices after workaround: " + devices.Count);
        }
    }
}
