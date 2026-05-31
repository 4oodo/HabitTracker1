using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HabitTracker1.Models;
using HabitTracker1.Services;

namespace HabitTracker1.ViewModels
{
    public class HabitViewModel : ViewModelBase
    {
        private ObservableCollection<Habit> _habits;
        private ObservableCollection<Category> _categories;
        private Habit _selectedHabit;
        private Category _selectedCategory;
        private string _habitName;
        private FrequencyType _selectedFrequency;
        private string _selectedUnit;
        private decimal? _targetValue;
        private TimeSpan? _reminderTime;
        private bool _isLoading;
        private User _currentUser;
        private readonly IHabitService _habitService;
        private readonly ICategoryService _categoryService;

        public ObservableCollection<Habit> Habits
        {
            get => _habits;
            set => SetProperty(ref _habits, value);
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public Habit SelectedHabit
        {
            get => _selectedHabit;
            set => SetProperty(ref _selectedHabit, value);
        }

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public string HabitName
        {
            get => _habitName;
            set => SetProperty(ref _habitName, value);
        }

        public FrequencyType SelectedFrequency
        {
            get => _selectedFrequency;
            set => SetProperty(ref _selectedFrequency, value);
        }

        public string SelectedUnit
        {
            get => _selectedUnit;
            set => SetProperty(ref _selectedUnit, value);
        }

        public decimal? TargetValue
        {
            get => _targetValue;
            set => SetProperty(ref _targetValue, value);
        }

        public TimeSpan? ReminderTime
        {
            get => _reminderTime;
            set => SetProperty(ref _reminderTime, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand AddHabitCommand { get; }
        public ICommand EditHabitCommand { get; }
        public ICommand DeleteHabitCommand { get; }
        public ICommand AddCategoryCommand { get; }
        public ICommand DeleteCategoryCommand { get; }
        public ICommand LogCompletionCommand { get; }
        public ICommand RefreshCommand { get; }

        public event Action ShowAddHabitDialog;
        public event Action ShowAddCategoryDialog;

        public HabitViewModel()
        {
            _habitService = new HabitService();
            _categoryService = new CategoryService();
            _habits = new ObservableCollection<Habit>();
            _categories = new ObservableCollection<Category>();

            AddHabitCommand = new RelayCommand(_ => ExecuteAddHabit());
            EditHabitCommand = new RelayCommand(_ => ExecuteEditHabit(), _ => SelectedHabit != null);
            DeleteHabitCommand = new RelayCommand(_ => ExecuteDeleteHabit(), _ => SelectedHabit != null);
            AddCategoryCommand = new RelayCommand(_ => ExecuteAddCategory());
            DeleteCategoryCommand = new RelayCommand(_ => ExecuteDeleteCategory(), _ => SelectedCategory != null);
            LogCompletionCommand = new RelayCommand(_ => ExecuteLogCompletion(), _ => SelectedHabit != null);
            RefreshCommand = new RelayCommand(_ => LoadHabits());
        }

        public void Initialize(User currentUser)
        {
            _currentUser = currentUser;
            LoadHabits();
            LoadCategories();
        }

        public void LoadHabits()
        {
            if (_currentUser == null)
                return;

            try
            {
                IsLoading = true;
                var habits = _habitService.GetUserHabits(_currentUser.Id);
                Habits = new ObservableCollection<Habit>(habits);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке привычек: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public void LoadCategories()
        {
            if (_currentUser == null)
                return;

            try
            {
                var categories = _categoryService.GetUserCategories(_currentUser.Id);
                Categories = new ObservableCollection<Category>(categories);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при загрузке категорий: {ex.Message}");
            }
        }

        private void ExecuteAddHabit()
        {
            ShowAddHabitDialog?.Invoke();
        }

        private void ExecuteEditHabit()
        {
            if (SelectedHabit == null)
                return;

            // Здесь будет логика редактирования
            ShowAddHabitDialog?.Invoke();
        }

        private void ExecuteDeleteHabit()
        {
            if (SelectedHabit == null)
                return;

            try
            {
                _habitService.DeleteHabit(SelectedHabit.Id);
                LoadHabits();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении привычки: {ex.Message}");
            }
        }

        private void ExecuteAddCategory()
        {
            ShowAddCategoryDialog?.Invoke();
        }

        private void ExecuteDeleteCategory()
        {
            if (SelectedCategory == null)
                return;

            try
            {
                _categoryService.DeleteCategory(SelectedCategory.Id);
                LoadCategories();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении категории: {ex.Message}");
            }
        }

        private void ExecuteLogCompletion()
        {
            if (SelectedHabit == null)
                return;

            try
            {
                _habitService.LogHabitCompletion(SelectedHabit.Id, DateTime.Today, true);
                LoadHabits();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при логировании привычки: {ex.Message}");
            }
        }

        public void CreateNewHabit(string name, int? categoryId, FrequencyType frequency, string unit = null, decimal? targetValue = null, TimeSpan? reminderTime = null, int frequencyDays = 1)
        {
            if (_currentUser == null || string.IsNullOrWhiteSpace(name))
                return;

            try
            {
                _habitService.CreateHabit(_currentUser.Id, name, categoryId, frequency, frequencyDays, unit: unit, targetValue: targetValue, reminderTime: reminderTime);
                LoadHabits();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при создании привычки: {ex.Message}");
            }
        }

        public void CreateNewCategory(string name)
        {
            if (_currentUser == null || string.IsNullOrWhiteSpace(name))
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] CreateNewCategory: _currentUser={_currentUser}, name='{name}'");
                return;
            }

            try
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] CreateNewCategory: начало для пользователя {_currentUser.Id}");
                _categoryService.CreateCategory(_currentUser.Id, name);
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] CreateNewCategory: категория создана, загружаем список");
                LoadCategories();
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] CreateNewCategory: список загружен, всего категорий: {Categories?.Count}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] CreateNewCategory: Ошибка - {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] CreateNewCategory: StackTrace - {ex.StackTrace}");
            }
        }

        public void UpdateCategory(int categoryId, string name)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] UpdateCategory: categoryId={categoryId}, name='{name}'");
                _categoryService.UpdateCategory(categoryId, name);
                LoadCategories();
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] UpdateCategory: завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] UpdateCategory: Ошибка - {ex.Message}");
                throw;
            }
        }

        public void DeleteCategory(int categoryId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] DeleteCategory: categoryId={categoryId}");
                _categoryService.DeleteCategory(categoryId);
                LoadHabits();
                LoadCategories();
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] DeleteCategory: завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[HabitViewModel] DeleteCategory: Ошибка - {ex.Message}");
                throw;
            }
        }

        public void ExecuteLogCompletion(int habitId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"ExecuteLogCompletion: habitId={habitId}, currentUser={_currentUser?.Id}");
                _habitService.LogHabitCompletion(habitId, DateTime.Today, true);
                System.Diagnostics.Debug.WriteLine($"ExecuteLogCompletion: успешно для привычки {habitId}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при логировании привычки: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        public void ExecuteDeleteHabit(int habitId)
        {
            try
            {
                _habitService.DeleteHabit(habitId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка при удалении привычки: {ex.Message}");
                throw;
            }
        }
    }
}
