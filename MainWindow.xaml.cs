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
        private ListSortDirection sortDirection = ListSortDirection.Ascending;
        private PropertyDescriptor sortProperty;

        protected override bool SupportsSortingCore => true;

        protected override bool SupportsSearchingCore => true;

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            List<T> itemsList = (List<T>)this.Items;

            if (prop.PropertyType.GetInterface(nameof(IComparable)) != null)
            {
                var comparer = new PropertyComparer<T>(prop, direction);
                itemsList.Sort(comparer);
                isSorted = true;
                sortProperty = prop;
                sortDirection = direction;
            }
            else
            {
                throw new NotSupportedException("Cannot sort by this property.");
            }

            OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            isSorted = false;
            sortProperty = null;
            sortDirection = ListSortDirection.Ascending;
        }

        protected override bool IsSortedCore => isSorted;

        protected override ListSortDirection SortDirectionCore => sortDirection;

        protected override PropertyDescriptor SortPropertyCore => sortProperty;

        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            if (prop == null) throw new ArgumentNullException(nameof(prop));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (!typeof(string).IsAssignableFrom(prop.PropertyType) && !typeof(int).IsAssignableFrom(prop.PropertyType))
                throw new NotSupportedException("Searching is supported only for string and Int32 properties.");

            var property = TypeDescriptor.GetProperties(typeof(T))[prop.Name];
            for (int i = 0; i < Count; ++i)
            {
                T item = Items[i];
                var value = property.GetValue(item);
                if (value != null && value.Equals(key))
                    return i;
            }

            return -1;
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
            object xValue = property.GetValue(x);
            object yValue = property.GetValue(y);

            if (xValue == null && yValue == null)
                return 0;
            if (xValue == null)
                return direction == ListSortDirection.Ascending ? -1 : 1;
            if (yValue == null)
                return direction == ListSortDirection.Ascending ? 1 : -1;

            return direction == ListSortDirection.Ascending
                ? ((IComparable)xValue).CompareTo(yValue)
                : ((IComparable)yValue).CompareTo(xValue);
        }
    }
}