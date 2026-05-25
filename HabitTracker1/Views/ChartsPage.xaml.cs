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

namespace HabitTracker1.Views
{
    public partial class ChartsPage : Page
    {
        private User _currentUser;

        public ChartsPage(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }
    }
}
