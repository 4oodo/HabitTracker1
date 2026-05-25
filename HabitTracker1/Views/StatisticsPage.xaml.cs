using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class HabitStatisticsDisplay
    {
        public string Name { get; set; }
        public decimal SuccessPercentage { get; set; }
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int TotalCompleted { get; set; }
        public int TotalExpected { get; set; }
    }

    public partial class StatisticsPage : Page
    {
        private User _currentUser;
        private IStatisticsService _statisticsService;
        private ObservableCollection<HabitStatisticsDisplay> _statistics;
        private bool _isInitialized = false;

        public StatisticsPage(User currentUser)
        {
            System.Diagnostics.Debug.WriteLine($"StatisticsPage constructor (начало): currentUser = {currentUser?.Username ?? "NULL"}");

            // Инициализируем поля ПЕРЕД InitializeComponent
            _currentUser = currentUser;
            _statisticsService = new StatisticsService();
            _statistics = new ObservableCollection<HabitStatisticsDisplay>();

            System.Diagnostics.Debug.WriteLine($"StatisticsPage constructor: поля инициализированы");

            // Теперь вызываем InitializeComponent (он может вызвать Loaded)
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine($"StatisticsPage constructor: после InitializeComponent()");

            // Добавляем обработчик Loaded для повторной загрузки
            this.Loaded += (s, e) => 
            {
                System.Diagnostics.Debug.WriteLine($"StatisticsPage.Loaded event: _currentUser = {_currentUser?.Username ?? "NULL"}");
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    LoadStatistics();
                }
            };

            System.Diagnostics.Debug.WriteLine($"StatisticsPage constructor: инициализация завершена");
        }

        private void LoadStatistics()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("=== Начало загрузки статистики ===");
                System.Diagnostics.Debug.WriteLine($"LoadStatistics: _currentUser is {(_currentUser == null ? "NULL" : $"{_currentUser.Username} (ID: {_currentUser.Id})")}");

                // Защита: проверяем инициализацию
                if (_statistics == null)
                {
                    System.Diagnostics.Debug.WriteLine("КРИТИЧЕСКАЯ ОШИБКА: _statistics is null! Конструктор еще не завершен.");
                    return;
                }

                if (_statisticsService == null)
                {
                    System.Diagnostics.Debug.WriteLine("КРИТИЧЕСКАЯ ОШИБКА: _statisticsService is null!");
                    return;
                }

                // Проверяем текущего пользователя
                if (_currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("КРИТИЧЕСКАЯ ОШИБКА: _currentUser is null!");
                    System.Diagnostics.Debug.WriteLine($"  this = {this?.ToString() ?? "NULL"}");
                    System.Diagnostics.Debug.WriteLine($"  _statisticsService = {_statisticsService?.ToString() ?? "NULL"}");
                    System.Diagnostics.Debug.WriteLine($"  _statistics = {_statistics?.ToString() ?? "NULL"}");
                    MessageBox.Show("Ошибка: текущий пользователь не определен. Пожалуйста, перезагрузитесь в приложение.", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Пользователь: {_currentUser.Username} (ID: {_currentUser.Id})");

                // Определяем период на основе выбора в ComboBox
                DateTime endDate = DateTime.Today;
                DateTime startDate = endDate.AddDays(-7); // По умолчанию неделя

                // Безопасная проверка PeriodComboBox
                if (PeriodComboBox != null && PeriodComboBox.SelectedItem != null)
                {
                    try
                    {
                        ComboBoxItem selectedItem = (ComboBoxItem)PeriodComboBox.SelectedItem;
                        if (selectedItem != null && selectedItem.Tag != null)
                        {
                            if (int.TryParse(selectedItem.Tag.ToString(), out int days))
                            {
                                startDate = endDate.AddDays(-days);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Ошибка при обработке периода: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Период: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}");

                // Получаем все привычки пользователя
                var habitService = new HabitService();
                List<Habit> allHabits = null;

                try
                {
                    System.Diagnostics.Debug.WriteLine($"Вызов GetUserHabits для пользователя {_currentUser.Id}...");
                    allHabits = habitService.GetUserHabits(_currentUser.Id);
                    System.Diagnostics.Debug.WriteLine($"Получено привычек из сервиса: {allHabits?.Count ?? 0}");
                }
                catch (NullReferenceException nrex)
                {
                    System.Diagnostics.Debug.WriteLine($"NullReferenceException при получении привычек: {nrex.Message}\n{nrex.StackTrace}");
                    MessageBox.Show($"Null reference при получении привычек: {nrex.Message}\n\n{nrex.StackTrace}", "Критическая ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"ОШИБКА при получении привычек: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"Ошибка при получении привычек: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _statistics.Clear();

                if (allHabits == null || allHabits.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("Привычек не найдено");

                    // Показываем пустой список
                    if (StatisticsListBox != null)
                    {
                        StatisticsListBox.ItemsSource = _statistics;
                    }
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"Обработка {allHabits.Count} привычек...");

                // Для каждой привычки получаем статистику
                int successCount = 0;
                foreach (var habit in allHabits)
                {
                    if (habit == null)
                    {
                        System.Diagnostics.Debug.WriteLine("Предупреждение: habit is null, пропускаем");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(habit.Title))
                    {
                        System.Diagnostics.Debug.WriteLine($"Предупреждение: привычка {habit.Id} без названия, пропускаем");
                        continue;
                    }

                    try
                    {
                        System.Diagnostics.Debug.WriteLine($"Обработка привычки: {habit.Title} (ID: {habit.Id})");

                        var stat = _statisticsService.GetHabitStatistics(habit.Id, startDate, endDate);
                        System.Diagnostics.Debug.WriteLine($"  ✓ Получена статистика для привычки {habit.Id}");

                        if (stat == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"  → Ошибка: stat is null для привычки {habit.Id}");
                            continue;
                        }

                        var displayItem = new HabitStatisticsDisplay
                        {
                            Name = habit.Title ?? "Без названия",
                            SuccessPercentage = stat.SuccessPercentage,
                            CurrentStreak = stat.CurrentStreak,
                            LongestStreak = stat.LongestStreak,
                            TotalCompleted = stat.TotalCompleted,
                            TotalExpected = stat.TotalExpected
                        };

                        _statistics.Add(displayItem);
                        successCount++;

                        System.Diagnostics.Debug.WriteLine($"  ✓ Успешность: {stat.SuccessPercentage:F1}%, Серия: {stat.CurrentStreak} дней, Завершено: {stat.TotalCompleted}/{stat.TotalExpected}");
                    }
                    catch (NullReferenceException nrex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ NullReferenceException для привычки {habit?.Id}: {nrex.Message}\n{nrex.StackTrace}");
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"  ✗ {ex.GetType().Name} для привычки {habit?.Id}: {ex.Message}\n{ex.StackTrace}");
                    }
                }

                System.Diagnostics.Debug.WriteLine($"Успешно обработано {successCount} из {allHabits.Count} привычек");

                // Привязываем данные к ListBox
                if (StatisticsListBox != null)
                {
                    StatisticsListBox.ItemsSource = _statistics;
                    System.Diagnostics.Debug.WriteLine($"ListBox обновлен с {_statistics.Count} элементами");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ОШИБКА: StatisticsListBox is null");
                }

                System.Diagnostics.Debug.WriteLine("=== Загрузка статистики завершена ===");
            }
            catch (Exception ex)
            {
                string errorMsg = $"Ошибка при загрузке статистики:\n\n{ex.GetType().Name}: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMsg += $"\n\nВнутренняя ошибка: {ex.InnerException.Message}";
                }

                System.Diagnostics.Debug.WriteLine($"=== КРИТИЧЕСКАЯ ОШИБКА при загрузке статистики ===");
                System.Diagnostics.Debug.WriteLine($"Тип: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Сообщение: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Стек вызовов:\n{ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}");
                }

                MessageBox.Show(errorMsg, "Ошибка при загрузке статистики", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadStatistics();
        }
    }
}
