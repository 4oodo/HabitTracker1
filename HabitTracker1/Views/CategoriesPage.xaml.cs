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
            CategoriesListBox.ItemsSource = _viewModel.Categories;
        }

        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Простой диалог для ввода названия категории
            var inputWindow = new InputDialogWindow("Новая категория", "Введите название категории:");
            if (inputWindow.ShowDialog() == true)
            {
                string categoryName = inputWindow.InputValue;
                if (!string.IsNullOrWhiteSpace(categoryName))
                {
                    _viewModel.CreateNewCategory(categoryName);
                }
            }
        }
    }
}

