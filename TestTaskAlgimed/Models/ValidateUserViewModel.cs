using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace TestTaskAlgimed.Models
{
    class ValidateUserViewModel : INotifyPropertyChanged
    {
        private string _login;
        private string _password;
        private string _repeatpassword;
        private string _errorMessage;

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string RepeatPassword
        {
            get => _repeatpassword;
            set
            {
                _repeatpassword = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public async Task<bool> ValidateFieldsAuth()
        {
            if (string.IsNullOrEmpty(Login))
            {
                ErrorMessage = "Логин не может быть пустым";
                return false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Пароль не может быть пустым";
                return false;
            }

            await using (var db = new DatabaseContext())
            {
                var user = await db.Users.FirstOrDefaultAsync(u => u.Login == Login);
                if (user == null)
                {
                    ErrorMessage = $"Пользователя с логином {Login} не существует";
                    return false;
                }
                if (Password != user.Password)
                {
                    ErrorMessage = "Неправильный пароль";
                    return false;
                }
            }

            ErrorMessage = null;
            return true;
        }

        public async Task<bool> ValidateFieldsReg()
        {
            if (string.IsNullOrEmpty(Login))
            {
                ErrorMessage = "Логин не может быть пустым";
                return false;
            }

            if (string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Пароль не может быть пустым";
                return false;
            }

            if (!ValidationPassword(Password))
            {
                //ErrorMessage = "Пароль не может быть пустым";
                return false;
            }

            if (string.IsNullOrEmpty(RepeatPassword))
            {
                ErrorMessage = "Повтор пароля не написан";
                return false;
            }

            if (RepeatPassword != Password)
            {
                ErrorMessage = "Повтор пароля не совпадает с паролем";
                return false;
            }

            await using (var db = new DatabaseContext())
            {
                if (await db.Users.FirstOrDefaultAsync(u => u.Login == Login) != null)
                {
                    ErrorMessage = $"Пользователя с логином {Login} не существует";
                    return false;
                }
            }

            ErrorMessage = null;
            return true;
        }

        private bool ValidationPassword(string password)
        {
            if (password.Length <= 6)
            {
                ErrorMessage = "Пароль должен содержать не менее 6-ти символов";
                return false;
            }
            else if (!Regex.IsMatch(password, @"^(?=.*\p{L})(?=.*\p{N})"))
            {
                ErrorMessage = "Пароль должен содержать хотя бы одну букву или цифру";
                return false;
            }
            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
