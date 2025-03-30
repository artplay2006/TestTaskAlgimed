using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Win32;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xml.Linq;
using TestTaskAlgimed.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для Modes.xaml
    /// </summary>
    public partial class Modes : Window
    {
        private ValidateModeViewModel _viewModel = new ValidateModeViewModel();
        public Modes()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("Artem");
            DataContext = _viewModel;
            LoadTable();
        }
        Mode? selectedMode;
        private async Task LoadTable()
        {
            await using (var db = new DatabaseContext())
            {
                DataGridTable.ItemsSource = await db.Modes.ToListAsync();
                DataGridTable.AutoGeneratingColumn += (sender, e) =>
                {
                    if (e.PropertyName == "Steps")
                    {
                        e.Cancel = true;
                    }

                    if (e.PropertyName == "ID" && e.Column is DataGridBoundColumn column)
                    {
                        column.IsReadOnly = true;
                    }
                };
            }
        }
        private async void FromExcelButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx|All Files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var package = new ExcelPackage(new FileInfo(openFileDialog.FileName)))
                    {
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sh => sh.Name == "Modes");
                        if (worksheet == null)
                        {
                            throw new Exception("Нет листа в Excel документе с именем Modes");
                        }

                        if (worksheet.Dimension == null || worksheet.Dimension.End.Row <= 1)
                        {
                            throw new Exception("Лист 'Modes' пуст или не содержит данных");
                        }

                        List<Mode> modes = new List<Mode>();

                        if (DataGridTable.ItemsSource != null)
                        {
                            modes = DataGridTable.ItemsSource.Cast<Mode>().ToList();
                        }

                        List<Mode> newModes = new List<Mode>();
                        List<Mode> existingModesToUpdate = new List<Mode>();

                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;

                        for (int rowNum = 2; rowNum <= rowCount; rowNum++)
                        {
                            string idStr = worksheet.Cells[rowNum, 1].Text;
                            string name = worksheet.Cells[rowNum, 2].Text;
                            string maxBottleNumberStr = worksheet.Cells[rowNum, 3].Text;
                            string maxUsedTipsStr = worksheet.Cells[rowNum, 4].Text;

                            if (int.TryParse(idStr, out int id) &&
                                int.TryParse(maxBottleNumberStr, out int maxBottleNumber) &&
                                int.TryParse(maxUsedTipsStr, out int maxUsedTips))
                            {
                                Mode mode = new Mode
                                {
                                    ID = id,
                                    Name = name,
                                    MaxBottleNumber = maxBottleNumber,
                                    MaxUsedTips = maxUsedTips
                                };

                                await using (var db = new DatabaseContext())
                                {
                                    var existingMode = await db.Modes.FirstOrDefaultAsync(m => m.ID == id);
                                    if (existingMode != null)
                                    {
                                        MessageBoxResult result = MessageBox.Show(
                                            $"Запись с ID {id} уже существует. Хотите обновить её?",
                                            "Подтверждение обновления",
                                            MessageBoxButton.YesNoCancel,
                                            MessageBoxImage.Question);

                                        switch (result)
                                        {
                                            case MessageBoxResult.Yes:
                                                existingMode.Name = mode.Name;
                                                existingMode.MaxBottleNumber = mode.MaxBottleNumber;
                                                existingMode.MaxUsedTips = mode.MaxUsedTips;
                                                existingModesToUpdate.Add(existingMode);
                                                break;
                                            case MessageBoxResult.No:
                                                break;
                                            case MessageBoxResult.Cancel:
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        newModes.Add(mode);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Неверные данные в строке {rowNum}. Проверьте формат данных.");
                            }
                        }

                        modes.AddRange(newModes);
                        modes.AddRange(existingModesToUpdate);

                        await using (var db = new DatabaseContext())
                        {
                            db.Modes.UpdateRange(existingModesToUpdate);
                            await db.Modes.AddRangeAsync(newModes);
                            await db.SaveChangesAsync();
                        }

                        DataGridTable.ItemsSource = modes;

                        await LoadTable();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
                }
            }
        }

        private void DataGridTable_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DataGridRow row = ItemsControl.ContainerFromElement((DataGrid)sender, e.OriginalSource as DependencyObject) as DataGridRow;
            _viewModel.ErrorMessage = "";
            if (row != null)
            {
                selectedMode = row.DataContext as Mode;
                NameLabel.Text = selectedMode.Name;
                MaxBottleNumberLabel.Text = selectedMode.MaxBottleNumber.ToString();
                MaxUsedTipsLabel.Text = selectedMode.MaxUsedTips.ToString();
                _viewModel.IsModeSelected = true;
                _viewModel.Name = selectedMode.Name;
                _viewModel.MaxBottleNumber = selectedMode.MaxBottleNumber.ToString();
                _viewModel.MaxUsedTips = selectedMode.MaxUsedTips.ToString();
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ValidateForAdd();

                await using (var db = new DatabaseContext())
                {
                    await db.Modes.AddAsync(new Mode
                    {
                        Name = NameLabel.Text,
                        MaxBottleNumber = int.Parse(_viewModel.MaxBottleNumber),
                        MaxUsedTips = int.Parse(_viewModel.MaxUsedTips)
                    });
                    await db.SaveChangesAsync();
                }
                MessageBox.Show("Запись добавлена");
                await LoadTable();
                ClearTextBoxs();
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ValidateForSave();

                await using (var db = new DatabaseContext())
                {
                    selectedMode.Name = _viewModel.Name;
                    selectedMode.MaxBottleNumber = int.Parse(_viewModel.MaxBottleNumber);
                    selectedMode.MaxUsedTips = int.Parse(_viewModel.MaxUsedTips);

                    db.Update(selectedMode);
                    await db.SaveChangesAsync();
                }

                MessageBox.Show("Запись изменена");
                await LoadTable();
                ClearTextBoxs();
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ValidateForDelete();

                MessageBoxResult result = MessageBox.Show($"Вы действительно хотите удалить Mode с именем {selectedMode.Name}?\n" +
                    $"Все связанные записи из таблицы Steps удалятся", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await using (var db = new DatabaseContext())
                    {
                        db.Modes.Remove(selectedMode);
                        await db.SaveChangesAsync();
                    }
                    await LoadTable();
                    ClearTextBoxs();
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
        }
        private void ClearTextBoxs()
        {
            NameLabel.Text = "";
            MaxBottleNumberLabel.Text = "";
            MaxUsedTipsLabel.Text = "";
            _viewModel.IsModeSelected = false;
            _viewModel.Name = "";
            _viewModel.MaxBottleNumber = "";
            _viewModel.MaxUsedTips = "";
            _viewModel.ErrorMessage = "";
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            selectedMode = null;
            ClearTextBoxs();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Menu().Show();
            Close();
        }
    }
}
