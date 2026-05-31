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
    public partial class CategoriesPage : Page
    {
        private HabitViewModel _viewModel;

        public CategoriesPage(HabitViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            System.Diagnostics.Debug.WriteLine($"[CategoriesPage] Конструктор: категорий = {_viewModel.Categories?.Count ?? 0}");

            // Загружаем категории при инициализации
            _viewModel.LoadCategories();
            CategoriesListBox.ItemsSource = _viewModel.Categories;

            System.Diagnostics.Debug.WriteLine($"[CategoriesPage] После LoadCategories: категорий = {_viewModel.Categories?.Count ?? 0}");
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Простой диалог для ввода названия категории
                var inputWindow = new InputDialogWindow("Новая категория", "Введите название категории:");
                if (inputWindow.ShowDialog() == true)
                {
                    string categoryName = inputWindow.InputValue;
                    System.Diagnostics.Debug.WriteLine($"[CategoriesPage] AddCategoryButton_Click: categoryName='{categoryName}'");

                    if (!string.IsNullOrWhiteSpace(categoryName))
                    {
                        System.Diagnostics.Debug.WriteLine($"[CategoriesPage] AddCategoryButton_Click: вызываем CreateNewCategory");
                        _viewModel.CreateNewCategory(categoryName);
                        System.Diagnostics.Debug.WriteLine($"[CategoriesPage] AddCategoryButton_Click: завершено, категорий: {_viewModel.Categories.Count}");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CategoriesPage] AddCategoryButton_Click: Exception - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[CategoriesPage] AddCategoryButton_Click: StackTrace - {ex.StackTrace}");
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var category = button?.DataContext as Category;

                if (category == null)
                    return;

                var inputWindow = new InputDialogWindow("Изменить категорию", "Введите новое название:");
                inputWindow.InputTextBox.Text = category.Name;

                if (inputWindow.ShowDialog() == true)
                {
                    string newName = inputWindow.InputValue;
                    if (!string.IsNullOrWhiteSpace(newName))
                    {
                        _viewModel.UpdateCategory(category.Id, newName);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var button = sender as Button;
                var category = button?.DataContext as Category;

                if (category == null)
                    return;

                var result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить категорию '{category.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    _viewModel.DeleteCategory(category.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении категории: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

