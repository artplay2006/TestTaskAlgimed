using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestTaskAlgimed.Models
{
    public class ValidateStepViewModel : INotifyPropertyChanged
    {
        private string _modeId;
        private string _timer;
        private string _destination;
        private string _speed;
        private string _type;
        private string _volume;
        private string _errorMessage;
        private bool _isStepSelected;

        public string ModeId
        {
            get => _modeId;
            set
            {
                _modeId = value;
                OnPropertyChanged();
            }
        }

        public string Timer
        {
            get => _timer;
            set
            {
                _timer = value;
                OnPropertyChanged();
            }
        }

        public string Destination
        {
            get => _destination;
            set
            {
                _destination = value;
                OnPropertyChanged();
            }
        }

        public string Speed
        {
            get => _speed;
            set
            {
                _speed = value;
                OnPropertyChanged();
            }
        }

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                OnPropertyChanged();
            }
        }

        public string Volume
        {
            get => _volume;
            set
            {
                _volume = value;
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

        public bool IsStepSelected
        {
            get => _isStepSelected;
            set
            {
                _isStepSelected = value;
                OnPropertyChanged();
            }
        }

        public void ValidateForAdd()
        {
            ErrorMessage = null;

            if (string.IsNullOrEmpty(ModeId))
                throw new ArgumentException("ModeId не указан");

            if (!int.TryParse(ModeId, out _))
                throw new ArgumentException("ModeId должен быть числом");

            if (string.IsNullOrEmpty(Timer))
                throw new ArgumentException("Timer не указан");

            if (!int.TryParse(Timer, out _))
                throw new ArgumentException("Timer должен быть числом");

            if (string.IsNullOrEmpty(Speed))
                throw new ArgumentException("Speed не указан");

            if (!int.TryParse(Speed, out _))
                throw new ArgumentException("Speed должен быть числом");

            if (string.IsNullOrEmpty(Type))
                throw new ArgumentException("Type не указан");

            if (string.IsNullOrEmpty(Volume))
                throw new ArgumentException("Volume не указан");

            if (!int.TryParse(Volume, out _))
                throw new ArgumentException("Volume должен быть числом");
        }

        public void ValidateForSave()
        {
            if (!IsStepSelected)
                throw new ArgumentException("Step не выбран");

            ValidateForAdd();
        }

        public void ValidateForDelete()
        {
            if (!IsStepSelected)
                throw new ArgumentException("Step не выбран");
        }

        public void Clear()
        {
            ModeId = string.Empty;
            Timer = string.Empty;
            Destination = string.Empty;
            Speed = string.Empty;
            Type = string.Empty;
            Volume = string.Empty;
            ErrorMessage = string.Empty;
            IsStepSelected = false;
        }

        public void LoadFromStep(Step step)
        {
            ModeId = step.ModeId.ToString();
            Timer = step.Timer.ToString();
            Destination = step.Destionation ?? string.Empty;
            Speed = step.Speed.ToString();
            Type = step.Type;
            Volume = step.Volume.ToString();
            IsStepSelected = true;
        }

        public Step ToStep()
        {
            return new Step
            {
                ModeId = int.Parse(ModeId),
                Timer = int.Parse(Timer),
                Destionation = Destination,
                Speed = int.Parse(Speed),
                Type = Type,
                Volume = int.Parse(Volume)
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}