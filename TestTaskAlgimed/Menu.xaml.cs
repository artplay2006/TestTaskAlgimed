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

namespace TestTaskAlgimed
{
    /// <summary>
    /// Логика взаимодействия для Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void ModesTableButton_Click(object sender, RoutedEventArgs e)
        {
            //new SelectedTable("Modes").Show();
            new Modes().Show();
        }

        private void StepsTableButton_Click(object sender, RoutedEventArgs e)
        {
            //new SelectedTable("Steps").Show();
            new Steps().Show();
        }
    }
}
