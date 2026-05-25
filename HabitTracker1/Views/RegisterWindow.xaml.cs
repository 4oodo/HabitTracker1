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
    public partial class RegisterWindow : Window
    {
        private readonly IAuthenticationService _authService;
        public User CurrentUser { get; private set; }

        public RegisterWindow()
        {
            InitializeComponent();
            _authService = new AuthenticationService();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Пожалуйста, заполните обязательные поля.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessage.Text = "Пароли не совпадают.";
                return;
            }

            if (password.Length < 6)
            {
                ErrorMessage.Text = "Пароль должен быть не менее 6 символов.";
                return;
            }

            try
            {
                CurrentUser = _authService.Register(username, password, email);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = ex.Message;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
