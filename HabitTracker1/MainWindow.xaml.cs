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
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.CurrentUser;
                if (_currentUser != null)
                {
                    InitializeMainWindow();
                    this.Show();
                }
            }
            else
            {
                this.Close();
            }
        }

        private void InitializeMainWindow()
        {
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

