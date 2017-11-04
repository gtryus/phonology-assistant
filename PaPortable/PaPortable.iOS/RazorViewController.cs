using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using System.Linq;
using Twitter;
using MessageUI;
using PaPortable.Views;

namespace PaPortable
{
	public class RazorViewController : UIViewController
	{
		public RazorViewController ()
		{
		}
		UIWebView webView;
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			webView = new UIWebView (UIScreen.MainScreen.Bounds);
			View.Add (webView);

			// Intercept URL loading to handle native calls from browser
			webView.ShouldStartLoad += HandleShouldStartLoad;

            // Render the view from the type generated from RazorView.cshtml

            var model = new Lip3Data().MyRecs;
            var template = new DataCorpus () { Model = model };
			var page = template.GenerateString ();
			webView.LoadHtmlString (page, NSBundle.MainBundle.BundleUrl);
		}

		bool HandleShouldStartLoad (UIWebView webView, NSUrlRequest request, UIWebViewNavigationType navigationType) {
			var scheme = "hybrid:";
			// If the URL is not our own custom scheme, just let the webView load the URL as usual
			if (request.Url.Scheme != scheme.Replace(":", ""))
				return true;

			// This handler will treat everything between the protocol and "?"
			// as the method name.  The querystring has all of the parameters.
			var resources = request.Url.ResourceSpecifier.Split('?');
			var method = resources [0];
			var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]); // breaks if ? not present (ie no params)

			if (method == "ListAll") {
                var model = new Lip3Data().MyRecs;
                var template = new DataCorpus () { Model = model };
				var page = template.GenerateString ();
				webView.LoadHtmlString (page, NSBundle.MainBundle.BundleUrl);
			}
			return false;
		}
	}
}

