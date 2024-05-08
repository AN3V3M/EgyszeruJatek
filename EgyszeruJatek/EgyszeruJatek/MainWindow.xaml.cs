using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace AmobaGame
{
    public partial class MainWindow : Window
    {
        private AmobaViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new AmobaViewModel();
            DataContext = _viewModel;
        }
    }

    public class AmobaViewModel : INotifyPropertyChanged
    {
        private const int BoardSize = 3;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<CellViewModel> Board { get; private set; }

        public ICommand CellClickCommand { get; private set; }
        public ICommand NewGameCommand { get; private set; }

        private bool _player1Turn;

        public AmobaViewModel()
        {
            Board = new ObservableCollection<CellViewModel>();
            CellClickCommand = new RelayCommand<CellViewModel>(CellClick);
            NewGameCommand = new RelayCommand<object>(NewGame);

            NewGame(null);
        }

        private void NewGame(object obj)
        {
            Board.Clear();
            _player1Turn = true;

            for (int i = 0; i < BoardSize * BoardSize; i++)
            {
                Board.Add(new CellViewModel());
            }
        }

        private void CellClick(CellViewModel cell)
        {
            if (!string.IsNullOrEmpty(cell.Content))
                return;

            cell.Content = _player1Turn ? "X" : "O";
            _player1Turn = !_player1Turn;

            CheckWin();
        }

        private void CheckWin()
        {
            for (int i = 0; i < BoardSize; i++)
            {
                if (CheckLine(BoardSize * i, 1))
                    return;
            }

            for (int i = 0; i < BoardSize; i++)
            {
                if (CheckLine(i, BoardSize))
                    return;
            }
            if (CheckLine(0, BoardSize + 1) || CheckLine(BoardSize - 1, BoardSize - 1))
                return;

            if (Board.All(cell => !string.IsNullOrEmpty(cell.Content)))
            {
                MessageBox.Show("Döntetlen!");
                NewGame(null);
            }
        }

        private bool CheckLine(int start, int step)
        {
            for (int i = start; i < start + BoardSize * step; i += step)
            {
                if (string.IsNullOrEmpty(Board[i].Content) || Board[i].Content != Board[start].Content)
                    return false;
            }

            MessageBox.Show($"Győzött a(z) '{Board[start].Content}' játékos!");
            NewGame(null);
            return true;
        }
    }

    public class CellViewModel : INotifyPropertyChanged
    {
        private string _content;

        public string Content
        {
            get { return _content; }
            set
            {
                _content = value;
                OnPropertyChanged("Content");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
