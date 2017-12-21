using System;
using System.Windows.Forms;
using Gecko;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SIL.IO;
using PaPortable.Views;
using System.Drawing;
using System.Threading;
using System.ComponentModel;

namespace PaPortable.Windows
{
    static class Program
    {
        static List<String> _supportFile = new List<string>();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Xpcom.Initialize("Firefox");
            CreateResources();
            DisplaySplash();
            DisplayData();
            foreach (var fullPath in _supportFile)
            {
                File.Delete(fullPath);
            }
        }

        private static void DisplaySplash()
        {
            var f = new Form { Size = new Size(550, 200) };
            Gecko.GeckoWebBrowser browser = new Gecko.GeckoWebBrowser { Dock = DockStyle.Fill };
            f.Controls.Add(browser);
            var template = new Splash() { Model = 0 };
            var page = template.GenerateString();
            using (var tempFile = TempFile.WithExtension("html"))
            {
                File.WriteAllText(tempFile.Path, page);
                var uri = new Uri(tempFile.Path);
                browser.Navigate(uri.AbsoluteUri);
                f.Show();
                var worker = new BackgroundWorker();
                worker.DoWork += _DoWork;
                worker.RunWorkerAsync();
                while (worker.IsBusy) Application.DoEvents();
            }
            f.Close();
        }

        private static void _DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
        }

        private static void DisplayData()
        {
            var f = new Form { Size = new Size(750, 725) };
            Gecko.GeckoWebBrowser browser = new Gecko.GeckoWebBrowser { Dock = DockStyle.Fill };
            f.Controls.Add(browser);
            var model = new Lip3Data().MyRecs;
            var template = new DataCorpus() { Model = model };
            var page = template.GenerateString();
            using (var tempFile = TempFile.WithExtension("html"))
            {
                File.WriteAllText(tempFile.Path, page);
                var uri = new Uri(tempFile.Path);
                browser.Navigate(uri.AbsoluteUri);
                Application.Run(f);
            }
        }

        private static void CreateResources()
        {
            var folder = Path.GetTempPath();
            WriteResource(folder, "Content", "bootstrap.css");
            WriteResource(folder, "Content", "PA-64x64.png");
            WriteResource(folder, "Scripts", "bootstrap.js");
            WriteResource(folder, "Scripts", "jquery-1.9.1.js");
        }

        private static void WriteResource(string folder, string projectLocation, string name)
        {
            using (var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("PaPortable." + projectLocation + "." + name)))
            {
                var fullPath = Path.Combine(folder, name);
                var buffer = new byte[1000];
                using (var os = new FileStream(fullPath, FileMode.Create, FileAccess.Write))
                {
                    int count;
                    do
                    {
                        count = str.BaseStream.Read(buffer, 0, 1000);
                        os.Write(buffer, 0, count);
                    } while (count > 0);
                }
                _supportFile.Add(fullPath);
            }
        }

    }
}
