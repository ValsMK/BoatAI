using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Project;
public enum ObstaclesEnum { Free = 0, Obstacle = -1};

public class State
{
    public required StrengthVector DistanceToEndPosition { get; set; }

    public required StrengthVector CurrentFlow {  get; set; }

    public required Obstacles Obstacles { get; set; }

    public int Hash => GetHash();



    /// <summary>
    ///     Метод считает hash из состояния среды
    /// </summary>
    /// <param name="val"></param>
    /// <returns></returns>
    private int GetHash()
    {
        var hash = HashCode.Combine(DistanceToEndPosition.Strength);
        hash = HashCode.Combine(DistanceToEndPosition.Angle, hash);
        hash = HashCode.Combine(CurrentFlow.Strength, hash);
        hash = HashCode.Combine(CurrentFlow.Angle, hash);

        for (int x = 0; x < Obstacles.Len; x++)
            for (int y = 0; y < Obstacles.Len; y++)
            {
                hash = HashCode.Combine(Obstacles.GetValue(x, y), hash);
            }
        return hash;
    }
}


public class Obstacles
{
    private ObstaclesEnum[,] _obstacles;
    public Obstacles(int size)
    {
        _obstacles = new ObstaclesEnum[size, size];
    }

    public void SetValue( Point point, ObstaclesEnum value)
    {
        _obstacles[point.X, point.Y] = value;
    }

    public ObstaclesEnum GetValue( Point point )
    {
        return _obstacles[point.X, point.Y];
    }

    public ObstaclesEnum GetValue(int x, int y) => GetValue( new Point( x, y ) );

    public int Len => _obstacles.GetLength(0);
}