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
using System.Windows.Shapes;
using System.Xml;
using TestTaskAlgimed.Models;

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для Steps.xaml
    /// </summary>
    public partial class Steps : Window
    {
        public Steps()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("Artem");
            LoadTable();
        }
        Step? selectedStep;
        private async Task LoadTable()
        {
            await using (var db = new DatabaseContext())
            {
                DataGridTable.ItemsSource = await db.Steps.ToListAsync();
                DataGridTable.AutoGeneratingColumn += (sender, e) =>
                {
                    if (e.PropertyName == "Model")
                    {
                        e.Cancel = true; // Отменяем создание столбца
                    }
                    // 2. Делаем столбец "ID" нередактируемым
                    if (e.PropertyName == "Id" && e.Column is DataGridBoundColumn column)
                    {
                        column.IsReadOnly = true; // Запрещаем редактирование
                                                  //column.Header = "ID";     // Можно задать кастомный заголовок
                    }
                };
                // сделай поле ID не редактируемым
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
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.FirstOrDefault(sh => sh.Name == "Steps");
                        if (worksheet == null)
                        {
                            throw new Exception("Нет листа в Excel документе с именем Steps");
                        }

                        if (worksheet.Dimension == null || worksheet.Dimension.End.Row <= 1)
                        {
                            MessageBox.Show("Лист 'Steps' пуст или не содержит данных.");
                            return;
                        }

                        List<Step> steps = new List<Step>();

                        if (DataGridTable.ItemsSource != null)
                        {
                            steps = DataGridTable.ItemsSource.Cast<Step>().ToList();
                        }

                        List<Step> newsteps = new List<Step>();
                        List<Step> existingstepsToUpdate = new List<Step>();

                        int rowCount = worksheet.Dimension.End.Row;
                        int colCount = worksheet.Dimension.End.Column;

                        for (int rowNum = 2; rowNum <= rowCount; rowNum++)
                        {
                            string idStr = worksheet.Cells[rowNum, 1].Text;
                            string ModeIdStr = worksheet.Cells[rowNum, 2].Text;
                            string TimerStr = worksheet.Cells[rowNum, 3].Text;
                            string DestionationStr = worksheet.Cells[rowNum, 4].Text;
                            string SpeedStr = worksheet.Cells[rowNum, 5].Text;
                            string TypeStr = worksheet.Cells[rowNum, 6].Text;
                            string VolumeStr = worksheet.Cells[rowNum, 7].Text;
                            
                            if (int.TryParse(idStr, out int id) &&
                                int.TryParse(ModeIdStr, out int ModeId) &&
                                int.TryParse(TimerStr, out int Timer) &&
                                int.TryParse(SpeedStr, out int Speed) &&
                                int.TryParse(VolumeStr, out int Volume))
                            {
                                Step step = new Step
                                {
                                    ID = id,
                                    ModeId = ModeId,
                                    Timer = Timer,
                                    Destionation = DestionationStr,
                                    Speed = Speed,
                                    Type = TypeStr,
                                    Volume = Volume
                                };

                                await using (var db = new DatabaseContext())
                                {
                                    var existingStep = await db.Steps.FirstOrDefaultAsync(m => m.ID == id);
                                    if (existingStep != null)
                                    {
                                        MessageBoxResult result = MessageBox.Show(
                                            $"Запись с ID {id} уже существует. Хотите обновить её?",
                                            "Подтверждение обновления",
                                            MessageBoxButton.YesNoCancel,
                                            MessageBoxImage.Question);

                                        switch (result)
                                        {
                                            case MessageBoxResult.Yes:
                                                existingStep.ModeId = step.ModeId;
                                                existingStep.Timer = step.Timer;
                                                existingStep.Destionation = step.Destionation;
                                                existingStep.Speed = step.Speed;
                                                existingStep.Type = step.Type;
                                                existingStep.Volume = step.Volume;
                                                existingstepsToUpdate.Add(existingStep);
                                                break;
                                            case MessageBoxResult.No:
                                                break;
                                            case MessageBoxResult.Cancel:
                                                return;
                                        }
                                    }
                                    else
                                    {
                                        newsteps.Add(step);
                                    }
                                }
                            }
                            else
                            {
                                MessageBox.Show($"Неверные данные в строке {rowNum}. Проверьте формат данных.");
                            }
                        }

                        steps.AddRange(newsteps);
                        steps.AddRange(existingstepsToUpdate);

                        await using (var db = new DatabaseContext())
                        {
                            db.Steps.UpdateRange(existingstepsToUpdate);
                            await db.Steps.AddRangeAsync(newsteps);
                            await db.SaveChangesAsync();
                        }

                        DataGridTable.ItemsSource = steps;

                        await LoadTable();
                    }
                }
                catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
                {
                    MessageBox.Show("Ошибка при чтении файла: " + ex.Message + 
                        "\nЗаписей с такими ModeId в таблице Modes нету!!!");
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
                selectedStep = row.DataContext as Step;
                ModeIdLabel.Text = selectedStep.ModeId.ToString();
                TimerLabel.Text = selectedStep.Timer.ToString();
                DestioantionLabel.Text = selectedStep.Destionation.ToString();
                SpeedLabel.Text = selectedStep.Speed.ToString();
                TypeLabel.Text = selectedStep.Type.ToString();
                VolumeLabel.Text = selectedStep.Volume.ToString();
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(ModeIdLabel.Text)) { MessageBox.Show("ModeId не написано"); return; }
                if (!int.TryParse(ModeIdLabel.Text, out int ModeId)) { MessageBox.Show("ModeId не число"); return; }

                if (string.IsNullOrEmpty(TimerLabel.Text)) { MessageBox.Show("Timer не написано"); return; }
                if (!int.TryParse(TimerLabel.Text, out int Timer)) { MessageBox.Show("Timer не число"); return; }

                if (string.IsNullOrEmpty(SpeedLabel.Text)) { MessageBox.Show("Speed не написано"); return; }
                if (!int.TryParse(SpeedLabel.Text, out int Speed)) { MessageBox.Show("Speed не число"); return; }

                if (string.IsNullOrEmpty(TypeLabel.Text)) { MessageBox.Show("Type не написано"); return; }

                if (string.IsNullOrEmpty(VolumeLabel.Text)) { MessageBox.Show("Volume не написано"); return; }
                if (!int.TryParse(VolumeLabel.Text, out int Volume)) { MessageBox.Show("Volume не число"); return; }

                await using (var db = new DatabaseContext())
                {
                    await db.Steps.AddAsync(new Step
                    {
                        ModeId = ModeId,
                        Timer = Timer,
                        Destionation = DestioantionLabel.Text,
                        Speed = Speed,
                        Type = TypeLabel.Text,
                        Volume = Volume
                    });
                    await db.SaveChangesAsync();
                }
                MessageBox.Show("Запись добавлена");
                await LoadTable();
                ClearTextBoxs();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message +
                    "\nЗаписей с такими ModeId в таблице Modes нету!!!");
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
                if (selectedStep == null) { MessageBox.Show("Step не выбран"); return; }

                if (string.IsNullOrEmpty(ModeIdLabel.Text)) { MessageBox.Show("ModeId не написано"); return; }
                if (!int.TryParse(ModeIdLabel.Text, out int ModeId)) { MessageBox.Show("ModeId не число"); return; }

                if (string.IsNullOrEmpty(TimerLabel.Text)) { MessageBox.Show("Timer не написано"); return; }
                if (!int.TryParse(TimerLabel.Text, out int Timer)) { MessageBox.Show("Timer не число"); return; }

                if (string.IsNullOrEmpty(SpeedLabel.Text)) { MessageBox.Show("Speed не написано"); return; }
                if (!int.TryParse(SpeedLabel.Text, out int Speed)) { MessageBox.Show("Speed не число"); return; }

                if (string.IsNullOrEmpty(TypeLabel.Text)) { MessageBox.Show("Type не написано"); return; }

                if (string.IsNullOrEmpty(VolumeLabel.Text)) { MessageBox.Show("Volume не написано"); return; }
                if (!int.TryParse(VolumeLabel.Text, out int Volume)) { MessageBox.Show("Volume не число"); return; }

                await using (var db = new DatabaseContext())
                {
                    selectedStep.ModeId = ModeId;
                    selectedStep.Timer = Timer;
                    selectedStep.Destionation = DestioantionLabel.Text;
                    selectedStep.Speed = Speed;
                    selectedStep.Type = TypeLabel.Text;
                    selectedStep.Volume = Volume;
                    db.Update(selectedStep);
                    await db.SaveChangesAsync();
                }
                MessageBox.Show("Запись изменена");
                await LoadTable();
                ClearTextBoxs();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                MessageBox.Show("Ошибка при чтении файла: " + ex.Message +
                    "\nЗаписей с такими ModeId в таблице Modes нету!!!");
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
                if (selectedStep == null) { MessageBox.Show("Step не выбран"); return; }

                MessageBoxResult result = MessageBox.Show($"Вы действительно хотите удалить Step с ID {selectedStep.ID}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    await using (var db = new DatabaseContext())
                    {
                        db.Steps.Remove(selectedStep);
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
            ModeIdLabel.Text = "";
            TimerLabel.Text = "";
            DestioantionLabel.Text = "";
            SpeedLabel.Text = "";
            TypeLabel.Text = "";
            VolumeLabel.Text = "";
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStep = null;
            ClearTextBoxs();
        }
    }
}
