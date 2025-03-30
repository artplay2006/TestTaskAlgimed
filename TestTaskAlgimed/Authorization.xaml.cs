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
        public Authorization()
        {
            InitializeComponent();
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LoginLabel.Text))
            {
                MessageBox.Show("Логин не написан");
            }
            else if (string.IsNullOrEmpty(PasswordLabel.Text))
            {
                MessageBox.Show("Пароль не написан");
            }
            else
            {
                await using (var db = new DatabaseContext())
                {
                    var user = await db.Users.FirstOrDefaultAsync(u=>u.Login==LoginLabel.Text);
                    if(user == null)
                    {
                        MessageBox.Show($"Пользователя с логином {LoginLabel.Text} не существует");
                    }
                    else if (PasswordLabel.Text != user.Password)
                    {
                        MessageBox.Show("Неправильный пароль");
                    }
                    else
                    {
                        new Menu().Show();
                        Close();
                    }
                }
            }
        }

        private void RegPageButton_Click(object sender, RoutedEventArgs e)
        {
            new Registration().Show();
            this.Close();
        }
    }
}
