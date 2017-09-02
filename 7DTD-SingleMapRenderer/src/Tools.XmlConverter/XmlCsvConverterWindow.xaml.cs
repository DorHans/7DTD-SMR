using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace _7DTD_SingleMapRenderer.Tools.XmlConverter
{
    /// <summary>
    /// Interaktionslogik für XmlCsvConverterWindow.xaml
    /// </summary>
    public partial class XmlCsvConverterWindow : Window
    {
        private string gameDirectory;
        private string configDirectory;

        public XmlCsvConverterWindow()
        {
            InitializeComponent();
            this.gameDirectory = getGameDir();
            if (gameDirectory != null)
                this.configDirectory = Path.Combine(gameDirectory, "Data", "Config");
            else
                this.txtStatus.Text = "Game not found. Conversion impossible!";
        }

        private void btnItems_Click(object sender, RoutedEventArgs e)
        {
            this.txtStatus.Text = String.Empty;
            try
            {
                string filename = "items.csv";
                if (saveDialog(ref filename))
                {
                    ConvertItems(Path.Combine(this.configDirectory, "items.xml"), filename);
                }
                this.txtStatus.Text = "done";
            }
            catch (Exception ex)
            {
                this.txtStatus.Text = ex.Message;
            }
        }

        private void btnBlocks_Click(object sender, RoutedEventArgs e)
        {
            this.txtStatus.Text = String.Empty;
            try
            {
                string filename = "blocks.csv";
                if (saveDialog(ref filename))
                {
                    ConvertBlocks(Path.Combine(this.configDirectory, "blocks.xml"), filename);
                }
                this.txtStatus.Text = "done";
            }
            catch (Exception ex)
            {
                this.txtStatus.Text = ex.Message;
            }
        }

        private void btnMaterials_Click(object sender, RoutedEventArgs e)
        {
            this.txtStatus.Text = String.Empty;
            try
            {
                string filename = "materials.csv";
                if (saveDialog(ref filename))
                {
                    ConvertMaterials(Path.Combine(this.configDirectory, "materials.xml"), filename);
                }
                this.txtStatus.Text = "done";
            }
            catch (Exception ex)
            {
                this.txtStatus.Text = ex.Message;
            }
        }


        string getGameDir()
        {
            string gamedir = null;
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 251570"))
            {
                if (key != null)
                    gamedir = (string)key.GetValue("InstallLocation");
            }
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\Steam App 251570"))
            {
                if (key != null)
                    gamedir = (string)key.GetValue("InstallLocation");
            }
            return gamedir;
        }

        private bool saveDialog(ref string filename)
        {

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
            saveFileDialog.InitialDirectory = AppDomain.CurrentDomain.BaseDirectory;
            saveFileDialog.FileName = filename;
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "CSV files|*.csv";

            var dlgresult = saveFileDialog.ShowDialog();
            if (dlgresult.Value == true)
            {
                filename = saveFileDialog.FileName;
            }
            return dlgresult.Value;
        }

        public void ConvertMaterials(string xmlfilename, string csvfilename)
        {
            materials mats;
            using (var fs = File.OpenRead(xmlfilename))
            {
                var serial = new XmlSerializer(typeof(materials));
                mats = serial.Deserialize(fs) as materials;
            }

            var table = new DataTable("Materials");
            table.Columns.Add("id", typeof(String));
            foreach (var material in mats.Items)
            {
                foreach (var property in material.property)
                {
                    if (!table.Columns.Contains(property.name))
                        table.Columns.Add(property.name, typeof(string));
                }

                var row = table.NewRow();
                row["id"] = material.id;

                foreach (var property in material.property)
                {
                    row[property.name] = property.value;
                }
                table.Rows.Add(row);
            }
            WriteDataTableToCsv(table, csvfilename);
        }

        public void ConvertBlocks(string xmlfilename, string csvfilename)
        {
            blocks blocks;
            using (var fs = File.OpenRead(xmlfilename))
            {
                var serial = new XmlSerializer(typeof(blocks));
                blocks = serial.Deserialize(fs) as blocks;
            }

            var table = new DataTable("Blocks");
            table.Columns.Add("id", typeof(String));
            table.Columns.Add("name", typeof(String));
            foreach (var block in blocks.Items)
            {
                foreach (var property in block.property)
                {
                    if (property.@class != null)
                    {
                        foreach (var item in property.property1)
                        {
                            if (!table.Columns.Contains(property.@class + "." + item.name))
                                table.Columns.Add(property.@class + "." + item.name, typeof(string));
                        }
                    }
                    if (property.name == null)
                        continue;
                    if (!table.Columns.Contains(property.name))
                        table.Columns.Add(property.name, typeof(string));
                }

                var row = table.NewRow();
                row["id"] = block.id;
                row["name"] = block.name;

                foreach (var property in block.property)
                {
                    if (property.@class != null)
                    {
                        foreach (var item in property.property1)
                        {
                            row[property.@class + "." + item.name] = item.value;
                        }
                    }
                    if (property.name == null)
                        continue;
                    row[property.name] = property.value;
                }
                table.Rows.Add(row);
            }
            WriteDataTableToCsv(table, csvfilename);
        }

        public void ConvertItems(string xmlfilename, string csvfilename)
        {
            items itemsInstance;
            using (var fs = File.OpenRead(xmlfilename))
            {
                var serial = new XmlSerializer(typeof(items));
                itemsInstance = serial.Deserialize(fs) as items;
            }

            var table = new DataTable("Items");
            table.Columns.Add("id", typeof(String));
            table.Columns.Add("name", typeof(String));

            HashSet<string> columnNames = new HashSet<string>();
            foreach (var item in itemsInstance.Items)
            {
                foreach (var property in item.property1)
                {
                    if (property.@class != null)
                    {
                        if (property.property1 != null)
                            foreach (var item2 in property.property1)
                            {
                                columnNames.Add(property.@class + "." + item2.name);
                                //if (!table.Columns.Contains(property.@class + "." + item2.name))
                                //    table.Columns.Add(property.@class + "." + item2.name, typeof(string));
                            }
                    }
                    if (property.name == null)
                        continue;
                    columnNames.Add(property.name);
                    //if (!table.Columns.Contains(property.name))
                    //    table.Columns.Add(property.name, typeof(string));
                }
            }
            var liste = columnNames.OrderBy(s => s).Distinct().ToList();
            foreach (var columnName in liste)
            {
                table.Columns.Add(columnName, typeof(string));
            }
            foreach (var item in itemsInstance.Items)
            {
                var row = table.NewRow();
                row["id"] = item.id;
                row["name"] = item.name;

                foreach (var property in item.property1)
                {
                    if (property.@class != null)
                    {
                        if (property.property1 != null)
                            foreach (var item2 in property.property1)
                            {
                                row[property.@class + "." + item2.name] = item2.value;
                            }
                    }
                    if (property.name == null)
                        continue;
                    row[property.name] = property.value;
                }
                table.Rows.Add(row);
            }
            WriteDataTableToCsv(table, csvfilename);
        }

        private static void WriteDataTableToCsv(DataTable table, string filename)
        {
            StringBuilder sb = new StringBuilder();

            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(";", columnNames));

            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(";", fields));
            }

            File.WriteAllText(filename, sb.ToString());
        }

    }
}
