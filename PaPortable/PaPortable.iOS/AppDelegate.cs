using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;
using System.IO;

namespace PaPortable
{
    [Register("AppDelegate")]
    public partial class AppDelegate : UIApplicationDelegate
    {
        UIWindow window;

        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {

            // create a new window instance based on the screen size
            window = new UIWindow(UIScreen.MainScreen.Bounds);

            window.RootViewController = new RazorViewController();

            // make the window visible
            window.MakeKeyAndVisible();

            return true;
        }
    }
}

