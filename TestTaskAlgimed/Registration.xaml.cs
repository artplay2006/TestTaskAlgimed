using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для Registration.xaml
    /// </summary>
    public partial class Registration : Window
    {
        private ValidateUserViewModel _viewModel = new ValidateUserViewModel();
        public Registration()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Authorization().Show();
            this.Close();
        }

        private async void RegButton_Click(object sender, RoutedEventArgs e)
        {
            if (!await _viewModel.ValidateFieldsReg())
            {
                return;
            }
            await using (var db = new DatabaseContext())
            {
                await db.Users.AddAsync(new User { Login = LoginLabel.Text, Password = PasswordLabel.Text });
                await db.SaveChangesAsync();
                MessageBox.Show("Аккаунт создан");
                new Authorization().Show();
                Close();
            }

        }
    }
}
