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
using HabitTracker1.ViewModels;

namespace HabitTracker1.Views
{
    public partial class AddHabitWindow : Window
    {
        private HabitViewModel _viewModel;

        public AddHabitWindow(HabitViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            LoadCategories();
        }

        private void LoadCategories()
        {
            CategoryComboBox.ItemsSource = _viewModel.Categories;
            CategoryComboBox.DisplayMemberPath = "Name";
            CategoryComboBox.SelectedValuePath = "Id";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            ErrorMessage.Text = string.Empty;

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                ErrorMessage.Text = "Пожалуйста, введите название привычки.";
                return;
            }

            try
            {
                string name = NameTextBox.Text;
                int? categoryId = CategoryComboBox.SelectedValue as int?;

                FrequencyType frequency = FrequencyType.Daily;
                var selectedFrequency = FrequencyComboBox.SelectedItem as ComboBoxItem;
                if (selectedFrequency != null)
                {
                    Enum.TryParse(selectedFrequency.Tag.ToString(), out frequency);
                }

                string unit = string.IsNullOrWhiteSpace(UnitTextBox.Text) ? null : UnitTextBox.Text;
                decimal? targetValue = null;
                if (decimal.TryParse(TargetValueTextBox.Text, out decimal value))
                {
                    targetValue = value;
                }

                TimeSpan? reminderTime = null;
                if (TimeSpan.TryParse(ReminderTimeTextBox.Text, out TimeSpan timespan))
                {
                    reminderTime = timespan;
                }

                _viewModel.CreateNewHabit(name, categoryId, frequency, unit, targetValue, reminderTime);
                this.Close();
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Ошибка: {ex.Message}";
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
