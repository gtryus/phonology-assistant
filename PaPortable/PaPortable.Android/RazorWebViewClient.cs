using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Webkit;
using PaPortable.Views;

namespace PaPortable
{
	public class RazorWebViewClient : WebViewClient {

		Context context;
		public RazorWebViewClient (Context context){
			this.context = context;
		}
		public override bool ShouldOverrideUrlLoading (WebView webView, string url) {
			var scheme = "hybrid:";
			// If the URL is not our own custom scheme, just let the webView load the URL as usual
			if (!url.StartsWith (scheme)) 
				return false;

			// This handler will treat everything between the protocol and "?"
			// as the method name.  The querystring has all of the parameters.
			var resources = url.Substring(scheme.Length).Split('?');
			var method = resources [0];
			var parameters = System.Web.HttpUtility.ParseQueryString(resources[1]);


			if (method == "ListAll") {
                var model = new Lip3Data().MyRecs;
                var template = new DataCorpus () { Model = model };
				var page = template.GenerateString ();
				webView.LoadDataWithBaseURL("file:///android_asset/", page, "text/html", "UTF-8", null);
			}

			return true;
		}
	}
}

