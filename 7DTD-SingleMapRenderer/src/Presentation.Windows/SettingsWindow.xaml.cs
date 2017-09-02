using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using _7DTD_SingleMapRenderer.Settings;

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

    }
}
