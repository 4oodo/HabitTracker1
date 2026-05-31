using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using HabitTracker1.Models;
using HabitTracker1.Services;

namespace HabitTracker1.Views
{
    public partial class ChartsPage : Page
    {
        private User _currentUser;
        private IHabitService _habitService;
        private IStatisticsService _statisticsService;

        public ChartsPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _habitService = new HabitService();
            _statisticsService = new StatisticsService();

            this.Loaded += (s, e) =>
            {
                LoadCharts();
            };
        }

        private void LoadCharts()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("LoadCharts: начало");

                if (_currentUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadCharts: ОШИБКА - _currentUser is null");
                    return;
                }

                if (_habitService == null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadCharts: ОШИБКА - _habitService is null");
                    return;
                }

                DateTime endDate = DateTime.Today;
                DateTime startDate = endDate.AddDays(-7);

                if (PeriodComboBox != null && PeriodComboBox.SelectedItem is ComboBoxItem item && item.Tag != null)
                {
                    if (int.TryParse(item.Tag.ToString(), out int days))
                    {
                        startDate = endDate.AddDays(-days);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"LoadCharts: период {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}");

                // Получаем привычки
                var habits = _habitService.GetUserHabits(_currentUser.Id);

                System.Diagnostics.Debug.WriteLine($"LoadCharts: получено {habits?.Count ?? 0} привычек");

                if (habits == null || habits.Count == 0)
                {
                    MessageBox.Show("Нет привычек для отображения статистики", "Информация");
                    return;
                }

                // Рисуем столбчатую диаграмму прогресса
                if (HabitProgressChart != null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadCharts: рисуем прогресс диаграмму");
                    DrawProgressChart(habits, startDate, endDate);
                }

                // Рисуем диаграмму по категориям
                if (CategoryChart != null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadCharts: рисуем диаграмму категорий");
                    DrawCategoryChart(habits, startDate, endDate);
                }

                // Рисуем тепловую карту
                if (HeatmapGrid != null)
                {
                    System.Diagnostics.Debug.WriteLine("LoadCharts: рисуем тепловую карту");
                    DrawHeatmap(habits, startDate, endDate);
                }

                System.Diagnostics.Debug.WriteLine("LoadCharts: завершено успешно");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"LoadCharts ОШИБКА: {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show($"Ошибка при загрузке графиков: {ex.Message}", "Ошибка");
            }
        }

        private void DrawProgressChart(List<Habit> habits, DateTime startDate, DateTime endDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DrawProgressChart: начало, привычек: {habits?.Count ?? 0}");

                HabitProgressChart.Children.Clear();

                if (habits == null || habits.Count == 0) 
                {
                    System.Diagnostics.Debug.WriteLine("DrawProgressChart: нет привычек");
                    return;
                }

                double chartWidth = HabitProgressChart.ActualWidth - 40;
                double chartHeight = HabitProgressChart.ActualHeight - 40;

                if (chartWidth <= 0 || chartHeight <= 0)
                {
                    System.Diagnostics.Debug.WriteLine($"DrawProgressChart: ОШИБКА - недопустимые размеры {chartWidth}x{chartHeight}");
                    return;
                }

                double barWidth = Math.Max(30, chartWidth / habits.Count);
                double maxHeight = chartHeight - 40;

                double startX = 20;
                double startY = chartHeight + 20;

                // Рисуем оси
                DrawAxis(HabitProgressChart, startX, startY, chartWidth);

                int colorIndex = 0;
                Color[] colors = new[] { Colors.LimeGreen, Colors.SkyBlue, Colors.Orange, Colors.Crimson, Colors.Purple, Colors.Teal };

                foreach (var habit in habits)
                {
                    try
                    {
                        if (habit == null)
                        {
                            System.Diagnostics.Debug.WriteLine("DrawProgressChart: пропускаем null привычку");
                            continue;
                        }

                        System.Diagnostics.Debug.WriteLine($"DrawProgressChart: обработка привычки {habit.Id}");

                        var stats = _statisticsService.GetHabitStatistics(habit.Id, startDate, endDate);

                        if (stats == null)
                        {
                            System.Diagnostics.Debug.WriteLine($"DrawProgressChart: stats is null для привычки {habit.Id}");
                            continue;
                        }

                        double barHeight = ((double)stats.SuccessPercentage / 100) * maxHeight;
                        double x = startX + (habits.IndexOf(habit) * barWidth) + 5;
                        double y = startY - barHeight;

                        // Рисуем столбец
                        Rectangle bar = new Rectangle
                        {
                            Width = barWidth - 10,
                            Height = barHeight,
                            Fill = new SolidColorBrush(colors[colorIndex % colors.Length]),
                            Stroke = new SolidColorBrush(Colors.DarkGray),
                            StrokeThickness = 1
                        };

                        Canvas.SetLeft(bar, x);
                        Canvas.SetTop(bar, y);
                        HabitProgressChart.Children.Add(bar);

                        // Рисуем текст с процентом
                        TextBlock label = new TextBlock
                        {
                            Text = $"{stats.SuccessPercentage:F0}%",
                            FontSize = 10,
                            Foreground = new SolidColorBrush(Colors.Black),
                            TextAlignment = TextAlignment.Center,
                            Width = barWidth - 10
                        };

                        Canvas.SetLeft(label, x);
                        Canvas.SetTop(label, y - 20);
                        HabitProgressChart.Children.Add(label);

                        // Название привычки
                        TextBlock habitName = new TextBlock
                        {
                            Text = habit.Title.Length > 8 ? habit.Title.Substring(0, 8) + "..." : habit.Title,
                            FontSize = 9,
                            Foreground = new SolidColorBrush(Colors.DarkGray),
                            TextAlignment = TextAlignment.Center,
                            Width = barWidth - 10
                        };

                        Canvas.SetLeft(habitName, x);
                        Canvas.SetTop(habitName, startY + 5);
                        HabitProgressChart.Children.Add(habitName);

                        colorIndex++;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"DrawProgressChart: ОШИБКА при обработке привычки - {ex.GetType().Name}: {ex.Message}");
                    }
                }

                System.Diagnostics.Debug.WriteLine("DrawProgressChart: завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawProgressChart: КРИТИЧЕСКАЯ ОШИБКА - {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DrawCategoryChart(List<Habit> habits, DateTime startDate, DateTime endDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DrawCategoryChart: начало, привычек: {habits?.Count ?? 0}");

                CategoryChart.Children.Clear();

                if (habits == null || habits.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("DrawCategoryChart: нет привычек");
                    return;
                }

                var categories = habits
                    .GroupBy(h => h.CategoryId ?? 0)
                    .Select(g => new
                    {
                        Category = g.First().Category?.Name ?? "Без категории",
                        Percentage = g.Average(h => (double)_statisticsService.GetHabitStatistics(h.Id, startDate, endDate).SuccessPercentage)
                    })
                    .ToList();

                if (categories.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("DrawCategoryChart: нет категорий");
                    return;
                }

                double centerX = CategoryChart.ActualWidth / 2;
                double centerY = CategoryChart.ActualHeight / 2;
                double radius = Math.Min(CategoryChart.ActualWidth, CategoryChart.ActualHeight) / 3;

                // Нормализуем проценты
                double totalPercentage = categories.Sum(c => c.Percentage);
                if (totalPercentage == 0) totalPercentage = 1;

                double currentAngle = -90;
                Color[] colors = new[] { Colors.LimeGreen, Colors.SkyBlue, Colors.Orange, Colors.Crimson, Colors.Purple, Colors.Teal };
                int colorIndex = 0;

                foreach (var category in categories)
                {
                    double percentage = category.Percentage / totalPercentage;
                    double sweepAngle = percentage * 360;

                    DrawPieSlice(CategoryChart, centerX, centerY, radius, currentAngle, sweepAngle, colors[colorIndex % colors.Length]);

                    // Добавляем подпись
                    double labelAngle = (currentAngle + sweepAngle / 2) * Math.PI / 180;
                    double labelX = centerX + (radius * 0.7) * Math.Cos(labelAngle);
                    double labelY = centerY + (radius * 0.7) * Math.Sin(labelAngle);

                    TextBlock label = new TextBlock
                    {
                        Text = $"{category.Category}\n{category.Percentage:F0}%",
                        FontSize = 10,
                        Foreground = new SolidColorBrush(Colors.White),
                        TextAlignment = TextAlignment.Center
                    };

                    Canvas.SetLeft(label, labelX - 25);
                    Canvas.SetTop(label, labelY - 15);
                    CategoryChart.Children.Add(label);

                    currentAngle += sweepAngle;
                    colorIndex++;
                }

                System.Diagnostics.Debug.WriteLine("DrawCategoryChart: завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawCategoryChart: КРИТИЧЕСКАЯ ОШИБКА - {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void DrawHeatmap(List<Habit> habits, DateTime startDate, DateTime endDate)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"DrawHeatmap: начало, привычек: {habits?.Count ?? 0}");

                var heatmapItems = new ObservableCollection<Rectangle>();

                if (habits == null || habits.Count == 0)
                {
                    System.Diagnostics.Debug.WriteLine("DrawHeatmap: нет привычек");
                    return;
                }

                // Проходим по дням
                for (DateTime date = startDate; date <= endDate; date = date.AddDays(1))
                {
                    // Подсчитываем количество завершенных привычек в этот день
                    int completedCount = 0;
                    int totalCount = habits.Count;

                    foreach (var habit in habits)
                    {
                        try
                        {
                            if (habit == null) continue;

                            var logs = _habitService.GetHabitLogsByDateRange(habit.Id, date, date);
                            if (logs != null && logs.Any(l => l.IsCompleted))
                            {
                                completedCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"DrawHeatmap: ошибка при обработке привычки {habit?.Id} - {ex.Message}");
                        }
                    }

                    // Определяем цвет в зависимости от процента
                    double percentage = totalCount > 0 ? (double)completedCount / totalCount : 0;
                    Color cellColor = GetHeatmapColor(percentage);

                    Rectangle cell = new Rectangle
                    {
                        Width = 25,
                        Height = 25,
                        Fill = new SolidColorBrush(cellColor),
                        Stroke = new SolidColorBrush(Colors.LightGray),
                        StrokeThickness = 1,
                        Margin = new Thickness(2)
                    };

                    // Добавляем подсказку
                    cell.ToolTip = $"{date:dd.MM.yyyy}: {completedCount}/{totalCount}";
                    heatmapItems.Add(cell);
                }

                if (HeatmapGrid != null)
                {
                    HeatmapGrid.ItemsSource = heatmapItems;
                }

                System.Diagnostics.Debug.WriteLine("DrawHeatmap: завершено");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"DrawHeatmap: КРИТИЧЕСКАЯ ОШИБКА - {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private Color GetHeatmapColor(double percentage)
        {
            if (percentage == 0) return Colors.WhiteSmoke;
            if (percentage <= 0.25) return Color.FromRgb(255, 200, 200);  // Красный
            if (percentage <= 0.50) return Color.FromRgb(255, 200, 0);    // Оранжевый
            if (percentage <= 0.75) return Color.FromRgb(200, 255, 100);  // Желто-зеленый
            return Colors.LimeGreen;                                        // Зеленый
        }

        private void DrawAxis(Canvas canvas, double x, double y, double width)
        {
            // X axis
            Line xAxis = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x + width,
                Y2 = y,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2
            };
            canvas.Children.Add(xAxis);

            // Y axis
            Line yAxis = new Line
            {
                X1 = x,
                Y1 = y,
                X2 = x,
                Y2 = y - 150,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 2
            };
            canvas.Children.Add(yAxis);

            // Y axis labels
            for (int i = 0; i <= 100; i += 25)
            {
                double labelY = y - (i / 100.0) * 150;
                TextBlock label = new TextBlock
                {
                    Text = i + "%",
                    FontSize = 8,
                    Foreground = new SolidColorBrush(Colors.Gray)
                };
                Canvas.SetLeft(label, x - 25);
                Canvas.SetTop(label, labelY - 6);
                canvas.Children.Add(label);
            }
        }

        private void DrawPieSlice(Canvas canvas, double centerX, double centerY, double radius, double startAngle, double sweepAngle, Color color)
        {
            // Конвертируем углы в радианы
            double startRad = startAngle * Math.PI / 180;
            double sweepRad = sweepAngle * Math.PI / 180;

            // Вычисляем точки
            double x1 = centerX + radius * Math.Cos(startRad);
            double y1 = centerY + radius * Math.Sin(startRad);

            double x2 = centerX + radius * Math.Cos(startRad + sweepRad);
            double y2 = centerY + radius * Math.Sin(startRad + sweepRad);

            // Если угол больше 180, используем arc flag
            bool isLargeArc = sweepAngle > 180;

            // Создаем Path для сегмента
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure();

            pathFigure.StartPoint = new Point(centerX, centerY);
            pathFigure.Segments.Add(new LineSegment(new Point(x1, y1), false));
            pathFigure.Segments.Add(new ArcSegment(new Point(x2, y2), new Size(radius, radius), 0, isLargeArc, SweepDirection.Clockwise, false));
            pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY), false));
            pathFigure.IsClosed = true;

            pathGeometry.Figures.Add(pathFigure);

            Path path = new Path
            {
                Data = pathGeometry,
                Fill = new SolidColorBrush(color),
                Stroke = new SolidColorBrush(Colors.White),
                StrokeThickness = 2
            };

            canvas.Children.Add(path);
        }

        private void PeriodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadCharts();
        }
    }
}
