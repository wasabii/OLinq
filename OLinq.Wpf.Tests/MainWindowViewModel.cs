using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace OLinq.Wpf.Tests
{

    public class MainWindowViewModel :
        INotifyPropertyChanged
    {

        string newItem;
        string filter;


        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public MainWindowViewModel()
        {
            Items = new ObservableCollection<string>();
            View = Items.AsObservableQuery()
                .Where(i => Filter != null ? i.StartsWith(Filter) : true)
                .AsObservableQuery().ToObservableView().ToBuffer();
            AddCommand = new DelegateCommand(_ => Items.Add(NewItem));
        }

        public ICommand AddCommand { get; set; }

        public string NewItem
        {
            get { return newItem; }
            set { newItem = value; OnPropertyChanged("NewItem"); }
        }

        public string Filter
        {
            get { return filter; }
            set { filter = value; OnPropertyChanged("Filter"); }
        }

        public ObservableCollection<string> Items { get; set; }

        public IEnumerable<string> View { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

}
