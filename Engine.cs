using System;
using System.Xml.Serialization;

namespace PT_Lab10;

public class Engine : IComparable
{
    public double Displacement { get; set; }

    public double HorsePower { get; set; }

    [XmlAttribute("Model")]
    public string Model { get; set; }

    public Engine()
    {
    }

    public Engine(double displacement = default, double horsePower = default, string model = null)
    {
        this.Displacement = displacement;
        this.HorsePower = horsePower;
        this.Model = model;
    }

    public int CompareTo(object? obj)
    {
        if (obj == null) return 1;

        Engine? otherEngine = obj as Engine;
        if (otherEngine != null)
        {
            if (this.Displacement == otherEngine.Displacement)
            {
                return this.HorsePower.CompareTo(otherEngine.HorsePower);
            }
            else
            {
                return this.Displacement.CompareTo(otherEngine.Displacement);
            }
        }
        else
        {
            throw new ArgumentException("Object is not an Engine");
        }
    }
}