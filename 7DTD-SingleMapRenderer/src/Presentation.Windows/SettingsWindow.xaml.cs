using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using _7DTD_SingleMapRenderer.Settings;
using Microsoft.Win32;

namespace _7DTD_SingleMapRenderer.Presentation.Windows
{
    /// <summary>
    /// Interaktionslogik für SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window, INotifyPropertyChanged
    {
        private AppSettings m_settings;
        public AppSettings Settings { get { return this.m_settings; } }

        public bool UseDataStore
        {
            get { return m_settings.UseDataStore; }
            set
            {
                if (m_settings.UseDataStore != value)
                {
                    m_settings.UseDataStore = value;
                    RaisePropertyChanged("UseDataStore");
                }
            }
        }

        public string GameRootPath
        {
            get { return m_settings.GameRootPath; }
            set
            {
                if (m_settings.GameRootPath != value)
                {
                    m_settings.GameRootPath = value;
                    RaisePropertyChanged("GameRootPath");
                }
            }
        }

        public string SaveFolderPath
        {
            get { return m_settings.SaveFolderPath; }
            set
            {
                if (m_settings.SaveFolderPath != value)
                {
                    m_settings.SaveFolderPath = value;
                    RaisePropertyChanged("SaveFolderPath");
                }
            }
        }


        public string GeneratedWorldsFolderPath
        {
            get { return m_settings.GeneratedWorldsFolderPath; }
            set
            {
                if (m_settings.GeneratedWorldsFolderPath != value)
                {
                    m_settings.GeneratedWorldsFolderPath = value;
                    RaisePropertyChanged("GeneratedWorldsFolderPath");
                }
            }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        public SettingsWindow(AppSettings settings)
            : this()
        {
            this.m_settings = new AppSettings();
            settings.CopyTo(this.m_settings);
        }

        #region ********** INotifyPropertyChanged **********
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private void RaisePropertyChanged(string prop)
        {
            this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
        #endregion

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void ButtonGameRoot_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "7DaysToDie.exe|7DaysToDie.exe|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = GameRootPath;
            var result = openFileDialog.ShowDialog();
            if (result == true)
            {
                string filepath = openFileDialog.FileName;
                string directory = System.IO.Path.GetDirectoryName(filepath);
                this.GameRootPath = directory;
            }
        }

        private void ButtonSaveFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = SaveFolderPath;
            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.SaveFolderPath = folderBrowserDialog.SelectedPath;
            }
        }

        private void ButtonWorldsFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = GeneratedWorldsFolderPath;
            var result = folderBrowserDialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                this.GeneratedWorldsFolderPath = folderBrowserDialog.SelectedPath;
            }
        }
    }
}
