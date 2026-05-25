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
using HabitTracker1.Views;

namespace HabitTracker1
{
    public partial class MainWindow : Window
    {
        private HabitViewModel _habitViewModel;
        private User _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            System.Diagnostics.Debug.WriteLine("ShowLoginWindow: начало");
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.CurrentUser;
                System.Diagnostics.Debug.WriteLine($"ShowLoginWindow: пользователь залогинен: {_currentUser?.Username ?? "NULL"}");
                if (_currentUser != null)
                {
                    InitializeMainWindow();
                    this.Show();
                    System.Diagnostics.Debug.WriteLine($"ShowLoginWindow: главное окно показано");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ShowLoginWindow: _currentUser is still NULL после входа!");
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("ShowLoginWindow: вход отменён");
                this.Close();
            }
        }

        private void InitializeMainWindow()
        {
            System.Diagnostics.Debug.WriteLine($"InitializeMainWindow: _currentUser = {_currentUser?.Username ?? "NULL"} (ID: {_currentUser?.Id.ToString() ?? "?"})");
            UserGreeting.Text = $"Добро пожаловать, {_currentUser.Username}!";

            _habitViewModel = new HabitViewModel();
            _habitViewModel.Initialize(_currentUser);

            HabitsButton_Click(null, null);
        }

        private void HabitsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new HabitsPage(_habitViewModel, _currentUser));
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"StatisticsButton_Click: _currentUser = {_currentUser?.Username ?? "NULL"}");
            ContentFrame.Navigate(new StatisticsPage(_currentUser));
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new CategoriesPage(_habitViewModel));
        }

        private void ChartsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new ChartsPage(_currentUser));
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new SettingsPage(_currentUser));
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            ShowLoginWindow();
        }
    }
}

