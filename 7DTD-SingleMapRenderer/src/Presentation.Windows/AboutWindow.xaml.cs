using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace _7DTD_SingleMapRenderer.Presentation.Windows
{
    /// <summary>
    /// Interaktionslogik für AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public class AssemblyInformation
        {
            public string FileName { get; set; }
            public string Version { get; set; }
        }

        public List<AssemblyInformation> VersionInformations { get; set; }

        public AboutWindow()
        {
            InitializeComponent();
            //this.SizeToContent = System.Windows.SizeToContent.WidthAndHeight;

            this.VersionInformations = new List<AssemblyInformation>();
            GetAssemblyInformation();
            this.dgVersionInfo.ItemsSource = this.VersionInformations;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void GetAssemblyInformation()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (assembly == null) return;

            var filename = System.IO.Path.GetFileName(assembly.Location);
            //var version = assembly.GetName().Version.ToString(); //AssemblyVersion
            var version = (assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).FirstOrDefault() as AssemblyFileVersionAttribute).Version;

            this.VersionInformations.Add(new AssemblyInformation() { FileName = filename, Version = version });


            var dirname = new System.IO.FileInfo(assembly.Location).DirectoryName;
            var dllfiles = System.IO.Directory.GetFiles(dirname, "*.dll");

            foreach (var dll in dllfiles)
            {
                try
                {
                    filename = System.IO.Path.GetFileName(dll);
                    version = System.Diagnostics.FileVersionInfo.GetVersionInfo(dll).FileVersion;
                    this.VersionInformations.Add(new AssemblyInformation() { FileName = filename, Version = version });
                }
                catch // gotta catch'em all
                {
                }
            }

        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
}
