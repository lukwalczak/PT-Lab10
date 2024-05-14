using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
    ///
    public partial class MainWindow : Window
    {
        private BindingList<Car> carList;
        private Dictionary<string, ListSortDirection?> columnSortDirections;

        public MainWindow()
        {
            InitializeComponent();
            Debug.WriteLine("A");
            InitializeCarData();
            LoadSearchableProperties();
            columnSortDirections = new Dictionary<string, ListSortDirection?>();
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

            //zad1 query expression syntax
            var elementsQueryExpression = from car in carList
                where car.Model == "A6"
                group car by car.Engine.Model.Contains("TDI") ? "diesel" : "petrol"
                into carGroup
                let avgHPPL = carGroup.Average(c => c.Engine.HorsePower / c.Engine.Displacement)
                orderby avgHPPL descending
                select new
                {
                    engineType = carGroup.Key,
                    avgHPPL = avgHPPL
                };

            foreach (var e in elementsQueryExpression)
            {
                Console.WriteLine(e.engineType + ": " + e.avgHPPL);
            }

            //zad1 Method-Based Query Syntax
            var elementsMethodBased = carList
                .Where(car => car.Model == "A6")
                .GroupBy(car => car.Engine.Model.Contains("TDI") ? "diesel" : "petrol")
                .Select(group => new
                {
                    engineType = group.Key,
                    avgHPPL = group.Average(car => car.Engine.HorsePower / car.Engine.Displacement)
                })
                .OrderByDescending(result => result.avgHPPL);

            foreach (var e in elementsMethodBased)
            {
                Console.WriteLine(e.engineType + ": " + e.avgHPPL);
            }


            // zad2
            CarComparer arg1 = new CarComparer();
            CarPredicate arg2 = new CarPredicate();
            CarAction arg3 = new CarAction();

            myCars.Sort(new Comparison<Car>(arg1.Compare));

            foreach (var car in carList)
            {
                if (arg2.IsTDI(car))
                {
                    arg3.ShowMessageBox(car);
                }
            }

            // zad3
            var newcarList = new SortableBindingList<Car>(myCars);
            newcarList.Sort(nameof(Car.Engine), ListSortDirection.Descending);

            Console.WriteLine("Sorted by Engine Descending:");
            foreach (var car in carList)
            {
                Console.WriteLine(
                    $"Model: {car.Model}, Displacement: {car.Engine.Displacement}, Engine Type: {car.Engine.Model}");
            }

            // Demonstrating search
            string searchModel = "A6";
            int index = newcarList.Find(nameof(Car.Model), searchModel);
            if (index != -1)
            {
                Console.WriteLine($"\nFound car {searchModel} at position {index}");
            }
            else
            {
                Console.WriteLine($"\nCar {searchModel} not found");
            }
        }

        private void LoadSearchableProperties()
        {
            var properties = typeof(Car).GetProperties()
                .Where(p => p.PropertyType == typeof(string) || p.PropertyType == typeof(int))
                .Select(p => p.Name).ToList();

            properties.AddRange(typeof(Engine).GetProperties()
                .Where(p => p.PropertyType == typeof(string) || p.PropertyType == typeof(int) || p.PropertyType == typeof(double))
                .Select(p => "Engine." + p.Name));

            propertyComboBox.ItemsSource = properties;
        }

        private void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            string selectedProperty = propertyComboBox.SelectedItem?.ToString();
            string searchText = searchTextBox.Text;

            if (string.IsNullOrEmpty(selectedProperty) || string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Please select a property and enter a search value.");
                return;
            }

            PropertyInfo propertyInfo = typeof(Car).GetProperty(selectedProperty.Split('.')[0]);
            if (propertyInfo == null && selectedProperty.Contains("."))
            {
                propertyInfo = typeof(Engine).GetProperty(selectedProperty.Split('.')[1]);
            }

            if (propertyInfo != null)
            {
                var foundItem = carList.FirstOrDefault(car =>
                {
                    object value = selectedProperty.Contains(".")
                        ? typeof(Engine).GetProperty(selectedProperty.Split('.')[1]).GetValue(car.Engine)
                        : propertyInfo.GetValue(car);

                    return value?.ToString().Equals(searchText, StringComparison.OrdinalIgnoreCase) == true;
                });

                if (foundItem != null)
                {
                    carDataGrid.SelectedItem = foundItem;
                    carDataGrid.ScrollIntoView(foundItem);
                }
                else
                {
                    MessageBox.Show("No items found.");
                }
            }
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

        private void CarDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
        {
            e.Handled = true;
            var column = e.Column;
            string columnName = column.SortMemberPath;

            if (!columnSortDirections.ContainsKey(columnName))
            {
                columnSortDirections[columnName] = null;
            }

            ListSortDirection? sortDirection = columnSortDirections[columnName];
            if (sortDirection == null || sortDirection == ListSortDirection.Descending)
            {
                column.SortDirection = ListSortDirection.Ascending;
                columnSortDirections[columnName] = ListSortDirection.Ascending;
            }
            else
            {
                column.SortDirection = ListSortDirection.Descending;
                columnSortDirections[columnName] = ListSortDirection.Descending;
            }

            Sort(columnName, column.SortDirection ?? ListSortDirection.Ascending);
        }


        private void Sort(string columnName, ListSortDirection direction)
        {
            Func<Car, object> keySelector = x =>
            {
                var propertyPath = columnName.Split('.');
                object value = x;
                foreach (var property in propertyPath)
                {
                    if (value == null) break;
                    var propInfo = value.GetType().GetProperty(property);
                    value = propInfo?.GetValue(value);
                }

                return value;
            };

            if (direction == ListSortDirection.Ascending)
            {
                carList = new BindingList<Car>(carList.OrderBy(keySelector).ToList());
            }
            else
            {
                carList = new BindingList<Car>(carList.OrderByDescending(keySelector).ToList());
            }

            carDataGrid.ItemsSource = carList;
        }
    }

    public class SortableBindingList<T> : BindingList<T>
    {
        private bool isSorted;
        private PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection;

        public SortableBindingList(List<T> list) : base(list)
        {
        }

        protected override bool SupportsSortingCore => true;

        protected override bool IsSortedCore => isSorted;

        protected override PropertyDescriptor SortPropertyCore => sortProperty;

        protected override ListSortDirection SortDirectionCore => sortDirection;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            if (prop.PropertyType.GetInterface(nameof(IComparable)) == null)
            {
                throw new ArgumentException("Property does not implement IComparable");
            }

            List<T> items = Items as List<T>;
            items.Sort(new PropertyComparer<T>(prop, direction));

            isSorted = true;
            sortProperty = prop;
            sortDirection = direction;

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override bool SupportsSearchingCore => true;

        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            for (int i = 0; i < Count; i++)
            {
                if (prop.GetValue(this[i]).Equals(key))
                {
                    return i;
                }
            }

            return -1;
        }

        public int Find(string propertyName, object key)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, true);
            if (prop == null)
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            return FindCore(prop, key);
        }

        public void Sort(string propertyName, ListSortDirection direction)
        {
            PropertyDescriptor prop = TypeDescriptor.GetProperties(typeof(T)).Find(propertyName, true);
            if (prop == null)
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }

            ApplySortCore(prop, direction);
        }
    }

    public class PropertyComparer<T> : IComparer<T>
    {
        private readonly PropertyDescriptor property;
        private readonly ListSortDirection direction;

        public PropertyComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            this.property = property;
            this.direction = direction;
        }

        public int Compare(T x, T y)
        {
            var xValue = property.GetValue(x) as IComparable;
            var yValue = property.GetValue(y) as IComparable;

            if (direction == ListSortDirection.Ascending)
            {
                return xValue.CompareTo(yValue);
            }
            else
            {
                return yValue.CompareTo(xValue);
            }
        }
    }
}