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
using HabitTracker1.Services;

namespace HabitTracker1.Views
{
    public partial class SettingsPage : Page
    {
        private User _currentUser;
        private IAuthenticationService _authService;

        public SettingsPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _authService = new AuthenticationService();
            AccountInfoTextBlock.Text = $"Пользователь: {currentUser.Username}";
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBox.Show("Функция изменения пароля находится в разработке", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteAccountButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var result = MessageBox.Show(
                    "Вы уверены, что хотите удалить свой аккаунт?\nЭто действие невозможно отменить и удалит все ваши привычки и данные!",
                    "Подтверждение удаления аккаунта",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    // Дополнительное подтверждение
                    var confirmResult = MessageBox.Show(
                        $"Введите имя пользователя '{_currentUser.Username}' для подтверждения удаления аккаунта.",
                        "Дополнительное подтверждение",
                        MessageBoxButton.OKCancel,
                        MessageBoxImage.Question
                    );

                    if (confirmResult == MessageBoxResult.OK)
                    {
                        // Удаляем аккаунт
                        _authService.DeleteUser(_currentUser.Id);
                        MessageBox.Show("Аккаунт успешно удален. Приложение будет закрыто.", "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Закрываем приложение
                        Application.Current.Shutdown();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении аккаунта: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

