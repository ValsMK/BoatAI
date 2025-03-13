using System;
using System.Drawing;

namespace Project;

/// <summary>
///     Класс описывает течение в точке карты
/// </summary>
public class StrengthVector: IEquatable<StrengthVector>
{
    //TODO Задавать направление не целым числом, а перечислением 

    public StrengthVector(int strength, int angle)
    {
        Strength = strength;
        Angle = angle;
    }

    public int Strength { get; }

    public int Angle { get; }


    public override string ToString()
    {
        return $"({Strength}, {Angle})";
    }
    public bool Equals(StrengthVector? other)
    {
        if (other is null)
            return false;

        return Strength == other.Strength && Angle == other.Angle;
    }
}

/// <summary>
/// Класс описывает карту течений и препятствий, по которой плывет лодка
/// ВАЖНО! Карта задается по стандартной системе координат
/// Начало координат в левом нижнем углу
/// </summary>
public class FlowMap
{
    private Point _endPoint;
    private readonly StrengthVector[,] _flows;

    private void SetEndPoint(Point point)
    {
        _endPoint = point;
        _flows[point.X, point.Y] = new StrengthVector(10, 10);
    }

    public FlowMap(int lenX, int lenY) { 
        _flows = new StrengthVector[lenX, lenY]; 
    }

    /// <summary>
    ///     Размер карты по горизонтали
    /// </summary>
    public int LenX => _flows.GetLength(0);

    /// <summary>
    ///     Размер карты по вретикали
    /// </summary>
    public int LenY => _flows.GetLength(1);

    /// <summary>
    ///     Начальная точка
    /// </summary>
    public Point StartPoint { get; set; }

    /// <summary>
    ///     Конечная точка
    /// </summary>
    public Point EndPoint { get => _endPoint; set => SetEndPoint(value); }


    /// <summary>
    ///     Течение в точке (x,y)
    /// </summary>
    public StrengthVector GetFlow(int x, int y) => _flows[x,y];

    /// <summary>
    ///     Течение в точке (x,y)
    /// </summary>
    public StrengthVector GetFlow(Point point) => _flows[point.X, point.Y];

    /// <summary>
    ///     Метод задает течение в точке (x, y)
    /// </summary>
    /// <param name="strength">Сила течения</param>
    /// <param name="angle">Направление течения</param>
    public void SetFLow(int x, int y, int strength, int angle) 
    {
        _flows[x, y] = new StrengthVector(strength, angle);
    }

    /// <summary>
    ///     Метод задает течение в точке (x, y)
    /// </summary>
    /// <param name="flowTuple">Пара (сила, направление)</param>
    public void SetFLow(int x, int y, (int,int) flowTuple)
    {
        _flows[x, y] = new StrengthVector(flowTuple.Item1, flowTuple.Item2);
    }
}

public static class TestFlowMap 
{
    public static FlowMap Get() 
    {
        FlowMap flows = new(7, 15);
        //Заполним всю карту 
        for (var x = 0; x < flows.LenX; x++)
            for (var y = 0; y < flows.LenY; y++)
                flows.SetFLow(x, y, 0, 90);

        for (var y = 0; y < flows.LenY; y++)
        {
            flows.SetFLow(0, y, -1, -1);
        }

        for (var y = 0; y < flows.LenY; y++)
        {
            flows.SetFLow(4, y, 1, 90);
            flows.SetFLow(6, y, 1, 90);
        }

        for (var y = 0; y < flows.LenY; y++)
        {
            flows.SetFLow(5, y, 2, 90);
        }

        flows.SetFLow(5, 11, -1, -1);

        flows.StartPoint = new Point(2, 0); 
        flows.EndPoint = new Point(flows.LenX - 1, flows.LenY - 1);

        return flows;

        //Должна быть такая карта
        //{ { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (10, 10) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (-1, -1), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 45), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2, 315), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 45), (2 , 90), (1, 135) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 315),(2 , 90), (1, 215) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
        //{ (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) }
    }

}
