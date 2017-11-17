using Gecko;
using PaPortable;
using PaPortable.Views;
using SIL.IO;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

namespace WindowsFormsApp1
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Xpcom.Initialize("Firefox");
            var geckoWebBrowser = new GeckoWebBrowser { Dock = DockStyle.Fill };
            Form f = new Form { Size = new Size(750, 725) };
            f.Controls.Add(geckoWebBrowser);

            var model = new Lip3Data().MyRecs;
            var template = new DataCorpus() { Model = model };
            var page = template.GenerateString();
            using (var tempfile = TempFile.WithExtension("html"))
            {
                File.WriteAllText(tempfile.Path, page);
                var uri = new Uri(tempfile.Path);
                var folder = Path.GetDirectoryName(tempfile.Path);
                WriteResource(folder, "Content", "bootstrap.css");
                WriteResource(folder, "Scripts", "bootstrap.js");
                WriteResource(folder, "Scripts", "jquery-1.9.1.js");
                geckoWebBrowser.Navigate(uri.AbsoluteUri);
                Application.Run(f);
            }
        }

        private static void WriteResource(string folder, string projectLocation, string name)
        {
            using (var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("WindowsFormsApp1." + projectLocation + "." + name)))
            {
                File.WriteAllText(Path.Combine(folder, name), str.ReadToEnd());
            }
        }
    }
}
