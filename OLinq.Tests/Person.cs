using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OLinq.Tests.Annotations;

namespace OLinq.Tests
{
    public class Person : INotifyPropertyChanged
    {
        private string _lastName;
        private string _firstName;
        private DateTimeOffset _dob;
        private ObservableValue<int> _yearOfBirth;

        public Person(string firstName, string lastName, DateTimeOffset dob)
        {
            FirstName = firstName;
            LastName = lastName;
            Dob = dob;
            _yearOfBirth = new ObservableValue<int>(() => Dob.Year).Raise(() => this.YearOfBirth, OnPropertyChanged);
        }

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (value == _firstName) return;
                _firstName = value;
                OnPropertyChanged();
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (value == _lastName) return;
                _lastName = value;
                OnPropertyChanged();
            }
        }

        public int YearOfBirth
        {
            get { return _yearOfBirth.Value; }
        }

        public DateTimeOffset Dob
        {
            get { return _dob; }
            set
            {
                if (value.Equals(_dob)) return;
                _dob = value;
                OnPropertyChanged();
            }
        }

       

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}