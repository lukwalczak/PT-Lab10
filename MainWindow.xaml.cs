using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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

namespace PT_Lab10
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BindingList<Car> carList;
        public MainWindow()
        {
            InitializeComponent();
            Debug.WriteLine("A");
            InitializeCarData();
        }

        public void InitializeCarData()
        {
            List<Car> myCars = new List<Car>()
            {
                new Car("E250", new Engine(1.8, 204, "CGI"), 2009),
                new Car("E350", new Engine(3.5, 292, "CGI"), 2009),
                new Car("A6", new Engine(2.5, 187, "FSI"), 2012),
                new Car("A6", new Engine(2.8, 220, "FSI"), 2012),
                new Car("A6", new Engine(3.0, 295, "TFSI"), 2012),
                new Car("A6", new Engine(2.0, 175, "TDI"), 2011),
                new Car("A6", new Engine(3.0, 309, "TDI"), 2011),
                new Car("S6", new Engine(4.0, 414, "TFSI"), 2012),
                new Car("S8", new Engine(4.0, 513, "TFSI"), 2012)
            };
            carList = new BindingList<Car>(myCars);
            carDataGrid.ItemsSource = carList;
        }

        private void AddCarButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            carList.Add(new Car("New car", new Engine(1.0, 100, "New engine"), 2019));
        }

        private void RemoveSelectedCarButtonClick(object sender, RoutedEventArgs e)
        {
            if (carDataGrid.SelectedItem != null)
            {
                carList.Remove((Car)carDataGrid.SelectedItem);
            }
        }

    }
}