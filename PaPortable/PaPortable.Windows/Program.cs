using System;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using Gecko;
using SIL.IO;
using PaPortable.Views;
using System.Reflection;

namespace PaPortable
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
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
            using (var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("PaPortable." + projectLocation + "." + name)))
            {
                File.WriteAllText(Path.Combine(folder, name), str.ReadToEnd());
            }
        }

    }
}
