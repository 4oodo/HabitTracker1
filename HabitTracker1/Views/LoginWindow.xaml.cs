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
using System.Windows.Shapes;
using HabitTracker1.Models;
using HabitTracker1.Services;

namespace HabitTracker1.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IAuthenticationService _authService;
        public User CurrentUser { get; private set; }

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthenticationService();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Пожалуйста, заполните все поля.";
                return;
            }

            try
            {
                CurrentUser = _authService.Login(username, password);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            if (registerWindow.ShowDialog() == true)
            {
                CurrentUser = registerWindow.CurrentUser;
                this.DialogResult = true;
                this.Close();
            }
        }
    }
}
