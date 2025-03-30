using Microsoft.EntityFrameworkCore;
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
using TestTaskAlgimed.Models;

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private ValidateUserViewModel _viewModel = new ValidateUserViewModel();
        public Authorization()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await _viewModel.ValidateFieldsAuth())
            {
                return;
            }

            try
            {
                new Menu().Show();
                Close();
            }
            catch (Exception ex)
            {
                _viewModel.ErrorMessage = "Ошибка при авторизации: " + ex.Message;
            }
        }

        private void RegPageButton_Click(object sender, RoutedEventArgs e)
        {
            new Registration().Show();
            this.Close();
        }
    }
}
