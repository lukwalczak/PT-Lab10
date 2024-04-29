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