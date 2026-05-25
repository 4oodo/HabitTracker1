using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace HabitTracker1
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Создаём главное окно вручную
            // Оно НЕ будет показано до успешного входа пользователя
            MainWindow mainWindow = new MainWindow();
            // MainWindow.Show() будет вызван из MainWindow.xaml.cs после успешного входа
        }
    }
}

