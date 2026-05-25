using System;
using System.Windows.Input;
using HabitTracker1.Models;
using HabitTracker1.Services;

namespace HabitTracker1.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _email;
        private string _errorMessage;
        private bool _isLoading;
        private readonly IAuthenticationService _authService;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand CancelCommand { get; }

        public event Action<User> RegisterSuccess;
        public event Action Cancelled;

        public RegisterViewModel()
        {
            _authService = new AuthenticationService();
            RegisterCommand = new RelayCommand(_ => ExecuteRegister());
            CancelCommand = new RelayCommand(_ => ExecuteCancel());
        }

        private void ExecuteRegister()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Пожалуйста, заполните обязательные поля.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Пароли не совпадают.";
                return;
            }

            if (Password.Length < 6)
            {
                ErrorMessage = "Пароль должен быть не менее 6 символов.";
                return;
            }

            try
            {
                IsLoading = true;
                var user = _authService.Register(Username, Password, Email);
                RegisterSuccess?.Invoke(user);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteCancel()
        {
            Cancelled?.Invoke();
        }
    }
}
