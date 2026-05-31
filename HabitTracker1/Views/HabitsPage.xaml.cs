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

        private void MarkHabitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button?.DataContext is Habit habit)
                {
                    System.Diagnostics.Debug.WriteLine($"Попытка отметить привычку: {habit.Title} (ID: {habit.Id})");
                    _viewModel.ExecuteLogCompletion(habit.Id);
                    _viewModel.LoadHabits();
                    MessageBox.Show($"Привычка '{habit.Title}' отмечена успешно!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Не удалось получить данные привычки", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при отметке: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Ошибка при отметке привычки: {ex.Message}\n\n{ex.InnerException?.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteHabitButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button button = sender as Button;
                if (button?.DataContext is Habit habit)
                {
                    var result = MessageBox.Show("Вы уверены, что хотите удалить эту привычку?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        _viewModel.ExecuteDeleteHabit(habit.Id);
                        _viewModel.LoadHabits();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении привычки: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
