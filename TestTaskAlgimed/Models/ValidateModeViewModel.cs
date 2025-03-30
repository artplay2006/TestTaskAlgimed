using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace TestTaskAlgimed.Models
{
    public class ValidateModeViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _maxBottleNumber;
        private string _maxUsedTips;
        private string _errorMessage;
        private bool _isModeSelected;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        public string MaxBottleNumber
        {
            get => _maxBottleNumber;
            set
            {
                _maxBottleNumber = value;
                OnPropertyChanged();
            }
        }

        public string MaxUsedTips
        {
            get => _maxUsedTips;
            set
            {
                _maxUsedTips = value;
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

        public bool IsModeSelected
        {
            get => _isModeSelected;
            set
            {
                _isModeSelected = value;
                OnPropertyChanged();
            }
        }

        public void ValidateForSave()
        {
            if (!IsModeSelected)
                throw new ArgumentException("Mode не выбран");

            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException("Name не написано");

            if (string.IsNullOrEmpty(MaxBottleNumber))
                throw new ArgumentException("MaxBottleNumber не написано");

            if (!int.TryParse(MaxBottleNumber, out _))
                throw new ArgumentException("MaxBottleNumber не число");

            if (string.IsNullOrEmpty(MaxUsedTips))
                throw new ArgumentException("MaxUsedTips не написано");

            if (!int.TryParse(MaxUsedTips, out _))
                throw new ArgumentException("MaxUsedTips не число");
        }

        public void ValidateForAdd()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ArgumentException("Name не написано");

            if (string.IsNullOrEmpty(MaxBottleNumber))
                throw new ArgumentException("MaxBottleNumber не написано");

            if (!int.TryParse(MaxBottleNumber, out _))
                throw new ArgumentException("MaxBottleNumber не число");

            if (string.IsNullOrEmpty(MaxUsedTips))
                throw new ArgumentException("MaxUsedTips не написано");

            if (!int.TryParse(MaxUsedTips, out _))
                throw new ArgumentException("MaxUsedTips не число");
        }

        public void ValidateForDelete()
        {
            if (!IsModeSelected)
                throw new ArgumentException("Mode не выбран");
        }

        public void Clear()
        {
            Name = string.Empty;
            MaxBottleNumber = string.Empty;
            MaxUsedTips = string.Empty;
            ErrorMessage = string.Empty;
            IsModeSelected = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}