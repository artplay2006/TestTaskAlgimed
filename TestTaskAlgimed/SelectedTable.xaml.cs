using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
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
using TestTaskAlgimed.Models;
using OfficeOpenXml;

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для SelectedTable.xaml
    /// </summary>
    public partial class SelectedTable : Window
    {
        string tableName;
        public SelectedTable(string tableName)
        {
            InitializeComponent();
            // Устанавливаем контекст лицензии для EPPlus
            ExcelPackage.License.SetNonCommercialPersonal("Artem");
            this.tableName = tableName;
            using (var db = new DatabaseContext())
            {
                if (tableName == "Modes")
                {
                    DataGridTable.ItemsSource = db.Modes.ToList();
                    DataGridTable.AutoGeneratingColumn += (sender, e) =>
                    {
                        if (e.PropertyName == "Steps")
                        {
                            e.Cancel = true; // Отменяем создание столбца
                        }
                    };
                    //DataGridTable.ItemsSource = db.Modes.Select(c => c.Name).ToList();
                }
                else if (tableName == "Steps")
                {
                    DataGridTable.ItemsSource = db.Steps.ToList();
                    DataGridTable.AutoGeneratingColumn += (sender, e) =>
                    {
                        if (e.PropertyName == "Model")
                        {
                            e.Cancel = true; // Отменяем создание столбца
                        }
                    };
                    //DataGridTable.ItemsSource = db.Steps.Select(c => c.Name).ToList();
                }
            }
        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Если нужно только целые числа:
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void FromExcelButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sh=>sh.Name==tableName); // Предполагаем, что данные на первом листе
                        if (worksheet == null)
                        {
                            //MessageBox.Show($"Нет листа в Excel документе с именем {tableName}");
                            //return;
                            throw new Exception($"Нет листа в Excel документе с именем {tableName}");
                        }
                        DataTable dataTable = new DataTable();

                        // Добавляем столбцы в DataTable
                        foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
                        {
                            dataTable.Columns.Add(firstRowCell.Text);
                        }

                        // Добавляем строки в DataTable
                        for (int rowNum = 2; rowNum <= worksheet.Dimension.End.Row; rowNum++)
                        {
                            var row = worksheet.Cells[rowNum, 1, rowNum, worksheet.Dimension.End.Column];
                            DataRow dataRow = dataTable.NewRow();
                            foreach (var cell in row)
                            {
                                dataRow[cell.Start.Column - 1] = cell.Text;
                            }
                            dataTable.Rows.Add(dataRow);
                        }

                        // Устанавливаем DataSource для DataGrid
                        DataGridTable.ItemsSource = dataTable.DefaultView;
                    }
            }
                catch (Exception ex)
                {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }
        }
    }
}
