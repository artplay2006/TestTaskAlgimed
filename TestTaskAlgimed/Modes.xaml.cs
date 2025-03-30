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

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для Modes.xaml
    /// </summary>
    public partial class Modes : Window
    {
        public Modes()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("Artem");
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
                        e.Cancel = true; // Отменяем создание столбца
                    }
                    // 2. Делаем столбец "ID" нередактируемым
                    if (e.PropertyName == "ID" && e.Column is DataGridBoundColumn column)
                    {
                        column.IsReadOnly = true; // Запрещаем редактирование
                        //column.Header = "ID";     // Можно задать кастомный заголовок
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
                            MessageBox.Show("Лист 'Modes' пуст или не содержит данных.");
                            return;
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

            if (row != null)
            {
                selectedMode = row.DataContext as Mode;
                NameLabel.Text = selectedMode.Name;
                MaxBottleNumberLabel.Text = selectedMode.MaxBottleNumber.ToString();
                MaxUsedTipsLabel.Text = selectedMode.MaxUsedTips.ToString();
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(NameLabel.Text)) { MessageBox.Show("Name не написано"); return; }
                if (string.IsNullOrEmpty(MaxBottleNumberLabel.Text)) { MessageBox.Show("MaxBottleNumber не написано"); return; }
                if (!int.TryParse(MaxBottleNumberLabel.Text, out int MaxBottleNumber)) { MessageBox.Show("MaxBottleNumber не число"); return; }
                if (string.IsNullOrEmpty(MaxUsedTipsLabel.Text)) { MessageBox.Show("MaxUsedTips не написано"); return; }
                if (!int.TryParse(MaxUsedTipsLabel.Text, out int MaxUsedTips)) { MessageBox.Show("MaxUsedTips не число"); return; }

                await using (var db = new DatabaseContext())
                {
                    await db.Modes.AddAsync(new Mode
                    {
                        Name = NameLabel.Text,
                        MaxBottleNumber = MaxBottleNumber,
                        MaxUsedTips = MaxUsedTips
                    });
                    await db.SaveChangesAsync();
                }
                MessageBox.Show("Запись добавлена");
                await LoadTable();
                ClearTextBoxs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMode == null) { MessageBox.Show("Mode не выбран"); return; }
                if (string.IsNullOrEmpty(NameLabel.Text)) { MessageBox.Show("Name не написано"); return; }
                if (string.IsNullOrEmpty(MaxBottleNumberLabel.Text)) { MessageBox.Show("MaxBottleNumber не написано"); return; }
                if (!int.TryParse(MaxBottleNumberLabel.Text, out int MaxBottleNumber)) { MessageBox.Show("MaxBottleNumber не число"); return; }
                if (string.IsNullOrEmpty(MaxUsedTipsLabel.Text)) { MessageBox.Show("MaxUsedTips не написано"); return; }
                if (!int.TryParse(MaxUsedTipsLabel.Text, out int MaxUsedTips)) { MessageBox.Show("MaxUsedTips не число"); return; }

                await using (var db = new DatabaseContext())
                {
                    selectedMode.Name = NameLabel.Text;
                    selectedMode.MaxBottleNumber = MaxBottleNumber;
                    selectedMode.MaxUsedTips = MaxUsedTips;
                    db.Update(selectedMode);
                    await db.SaveChangesAsync();
                }
                MessageBox.Show("Запись изменена");
                await LoadTable();
                ClearTextBoxs();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMode == null) { MessageBox.Show("Mode не выбран"); return; }

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
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
            }
        }
        private void ClearTextBoxs()
        {
            NameLabel.Text = "";
            MaxBottleNumberLabel.Text = "";
            MaxUsedTipsLabel.Text = "";
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            selectedMode = null;
            ClearTextBoxs();
        }
    }
}
