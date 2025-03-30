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
        public Registration()
        {
            InitializeComponent();
        }

        private static bool ValidationPassword(string password)
        {
            if (password.Length <= 6)
            {
                MessageBox.Show("Пароль должен содержать не менее 6-ти символов");
                return false;
            }
            else if(!Regex.IsMatch(password, @"^(?=.*\p{L})(?=.*\p{N})"))
            {
                MessageBox.Show("Пароль должен содержать хотя бы одну букву или цифру");
                return false;
            }
            return true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new Authorization().Show();
            this.Close();
        }

        private async void RegButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginLabel.Text))
            {
                MessageBox.Show("Логин не написан");
            }
            else if (string.IsNullOrEmpty(PasswordLabel.Text))
            {
                MessageBox.Show("Пароль не написан");
            }
            else if (!ValidationPassword(PasswordLabel.Text))
            {
                return;
            }
            else if (string.IsNullOrEmpty(RepeatPasswordLabel.Text))
            {
                MessageBox.Show("Повтор пароля не написан");
            }
            else if (RepeatPasswordLabel.Text != PasswordLabel.Text)
            {
                MessageBox.Show("Повтор пароля не совпадает с паролем");
            }
            else
            {
                await using (var db = new DatabaseContext())
                {
                    if (await db.Users.FirstOrDefaultAsync(u=>u.Login==LoginLabel.Text)!=null)
                    {
                        MessageBox.Show("Такой логин уже существует");
                    }
                    else
                    {
                        await db.Users.AddAsync(new User { Login = LoginLabel.Text, Password = PasswordLabel.Text});
                        MessageBox.Show("Аккаунт создан");
                        await db.SaveChangesAsync();
                        new Authorization().Show();
                        Close();
                    }
                }
            }
        }
    }
}
