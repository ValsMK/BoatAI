using System.Drawing;

namespace Project;

/// <summary>
///     Класс описывает течение в точке карты
/// </summary>
public class Flow
{
    //TODO Задавать направление не целым числом, а перечислением 

    public Flow(int strength, int angle)
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
}

/// <summary>
/// Класс описывает карту течений и препятствий, по которой плывет лодка
/// ВАЖНО! Карта задается по стандартной системе координат
/// Начало координат в левом нижнем углу
/// </summary>
public class FlowMap
{
    private Point _endPoint;
    private readonly Flow[,] _flows;

    private void SetEndPoint(Point point)
    {
        _endPoint = point;
        _flows[point.X, point.Y] = new Flow(10, 10);
    }

    public FlowMap(int lenX, int lenY) { 
        _flows = new Flow[lenY, lenY]; 
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
    public Flow GetFlow(int x, int y) => _flows[x,y];

    /// <summary>
    ///     Течение в точке (x,y)
    /// </summary>
    public Flow GetFlow(Point point) => _flows[point.X, point.Н];

    /// <summary>
    ///     Метод зажает течение в точке (x, y)
    /// </summary>
    /// <param name="strength">Сила течения</param>
    /// <param name="angle">Направление течения</param>
    public void SetFLow(int x, int y, int strength, int angle) 
    {
        _flows[x, y] = new Flow(strength, angle);
    }

    /// <summary>
    ///     Метод зажает течение в точке (x, y)
    /// </summary>
    /// <param name="flowTuple">Пара (сила, направление)</param>
    public void SetFLow(int x, int y, (int,int) flowTuple)
    {
        _flows[x, y] = new Flow(flowTuple.Item1, flowTuple.Item2);
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
            flows.SetFLow(flows.LenX-1, y, -1, -1);
        }

        flows.StartPoint = new Point(5, 0); 
        flows.EndPoint = new Point(flows.LenX, flows.LenY);

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
