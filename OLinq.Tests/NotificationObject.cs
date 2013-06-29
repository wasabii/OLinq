using System.ComponentModel;

namespace OLinq.Tests
{
    /// <summary>
    /// Simple notification object for use with tests.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class NotificationObject<T> : INotifyPropertyChanged
    {

        T value1;
        T value2;
        T value3;
        T value4;

        public T Value1
        {
            get { return value1; }
            set { value1 = value; OnPropertyChanged("Value1"); }
        }

        public T Value2
        {
            get { return value2; }
            set { value2 = value; OnPropertyChanged("Value2"); }
        }

        public T Value3
        {
            get { return value3; }
            set { value3 = value; OnPropertyChanged("Value3"); }
        }

        public T Value4
        {
            get { return value4; }
            set { value4 = value; OnPropertyChanged("Value4"); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
