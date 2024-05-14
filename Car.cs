using System.Collections.Generic;
using System.Windows;
using System.Xml.Serialization;

namespace PT_Lab10;

public class Car
{
    public string Model { get; set; }

    public int Year { get; set; }

    [XmlElement("Engine")]

    public Engine? Engine { get; set; }

    public Car()
    {
    }

    public Car(string model, int year)
    {
        this.Model = model;
        this.Year = year;
    }

    public Car(string model, Engine? engine, int year) : this(model, year)
    {
        this.Engine = engine;
    }
}

class CarComparer : IComparer<Car>
{
    public int Compare(Car car1, Car car2)
    {
        return car2.Engine.HorsePower.CompareTo(car1.Engine.HorsePower);
    }
}

class CarPredicate
{
    public bool IsTDI(Car car)
    {
        return car.Engine.Model == "TDI";
    }
}

class CarAction
{
    public void ShowMessageBox(Car car)
    {
        MessageBox.Show($"Model: {car.Model}, Engine Type: {car.Engine.Model}, Horse Power: {car.Engine.HorsePower}");
    }
}