using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace _7DTD_SingleMapRenderer.Tools.RegionViewer
{
    /// <summary>
    /// Interaktionslogik für RegionViewer.xaml
    /// </summary>
    public partial class RegionViewer : Window
    {
        RegionViewerViewModel m_ViewModel;

        public RegionViewer()
        {
            InitializeComponent();
        }

        public RegionViewer(RegionViewerViewModel viewModel)
            : this()
        {
            this.DataContext = m_ViewModel = viewModel;
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.m_ViewModel == null)
                return;

            var btn = sender as System.Windows.Controls.Primitives.ToggleButton;
            var vm = btn.DataContext as ChunkViewModel;

            if (btn.IsChecked.Value)
                m_ViewModel.StatusText = vm.Name;
            else
                m_ViewModel.StatusText = string.Empty;
        }

    }
}
