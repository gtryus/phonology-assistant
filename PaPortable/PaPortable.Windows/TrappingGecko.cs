using Gecko;

namespace PaPortable.Windows
{
    public class TrappingGecko : GeckoWebBrowser
    {
        protected override void OnDomClick(DomMouseEventArgs e)
        {
            var elem = Document.ActiveElement;
            var uri = elem.HasAttribute("Href") ? elem.GetAttribute("Href") :
                elem.Parent.HasAttribute("Href") ? elem.Parent.GetAttribute("Href") :
                "";
            const string scheme = "hybrid:";
            if (!uri.StartsWith(scheme)) base.OnDomClick(e);
            e.Handled = true;
            var resources = uri.Substring(scheme.Length).Split('?');
            var method = resources[0];
            var parameters = resources.Length > 1? System.Web.HttpUtility.ParseQueryString(resources[1]): null;
            switch (method)
            {
                case "VowelChart": break;
                case "ConstChart": break;
                case "DataCorpus": Program.DisplayData(); break;
                case "Search": break;
                case "Project": break;
                case "Settings": break;
                case "DistChart": break;
            }
        }
    }
}
