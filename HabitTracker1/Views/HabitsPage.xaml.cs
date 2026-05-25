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
using System.Windows.Navigation;
using System.Windows.Shapes;
using HabitTracker1.Models;
using HabitTracker1.ViewModels;

namespace HabitTracker1.Views
{
    public partial class HabitsPage : Page
    {
        private HabitViewModel _viewModel;
        private User _currentUser;

        public HabitsPage(HabitViewModel viewModel, User currentUser)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _currentUser = currentUser;
            this.DataContext = _viewModel;
        }

        private void AddHabitButton_Click(object sender, RoutedEventArgs e)
        {
            AddHabitWindow addHabitWindow = new AddHabitWindow(_viewModel);
            addHabitWindow.ShowDialog();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _viewModel.LoadHabits();
        }
    }
}
