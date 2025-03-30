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
using Microsoft.Data.SqlClient;

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для Steps.xaml
    /// </summary>
    public partial class Steps : Window
    {
        private ValidateStepViewModel _viewModel = new ValidateStepViewModel();
        private Step? selectedStep;

        public Steps()
        {
            InitializeComponent();
            DataContext = _viewModel;
            ExcelPackage.License.SetNonCommercialPersonal("Artem");
            LoadTable();
        }
        private async Task LoadTable()
        {
            await using (var db = new DatabaseContext())
            {
                DataGridTable.ItemsSource = await db.Steps.ToListAsync();
                DataGridTable.AutoGeneratingColumn += (sender, e) =>
                {
                    if (e.PropertyName == "Model")
                    {
                        e.Cancel = true;
                    }

                    if (e.PropertyName == "Id" && e.Column is DataGridBoundColumn column)
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
                when (ex.InnerException is SqlException { Number: 547 })
                {
                    MessageBox.Show("Ошибка: связанный элемент не существует!", "Ошибка FK");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при чтении файла: " + ex.Message);
                }
            }
        }

        private void DataGridTable_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataGridTable.SelectedItem is Step step)
            {
                selectedStep = step;
                _viewModel.LoadFromStep(step);
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _viewModel.ValidateForAdd();

                await using (var db = new DatabaseContext())
                {
                    await db.Steps.AddAsync(_viewModel.ToStep());
                    await db.SaveChangesAsync();
                }

                MessageBox.Show("Запись добавлена");
                await LoadTable();
                _viewModel.Clear();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                _viewModel.ErrorMessage = "Ошибка: связанный элемент не существует!";
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
                    selectedStep.ModeId = int.Parse(_viewModel.ModeId);
                    selectedStep.Timer = int.Parse(_viewModel.Timer);
                    selectedStep.Destionation = _viewModel.Destination;
                    selectedStep.Speed = int.Parse(_viewModel.Speed);
                    selectedStep.Type = _viewModel.Type;
                    selectedStep.Volume = int.Parse(_viewModel.Volume);

                    db.Update(selectedStep);
                    await db.SaveChangesAsync();
                }

                MessageBox.Show("Запись изменена");
                await LoadTable();
                _viewModel.Clear();
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
            {
                _viewModel.ErrorMessage = "Ошибка: связанный элемент не существует!";
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

                MessageBoxResult result = MessageBox.Show(
                    $"Вы действительно хотите удалить Step с ID {selectedStep.ID}?",
                    "Подтверждение",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    await using (var db = new DatabaseContext())
                    {
                        db.Steps.Remove(selectedStep);
                        await db.SaveChangesAsync();
                    }
                    await LoadTable();
                    _viewModel.Clear();
                }
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = ex.Message;
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            selectedStep = null;
            _viewModel.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Menu().Show();
            Close();
        }
    }
}
