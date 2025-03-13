using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project;

public static class VectorHelper
{
    public static StrengthVector VectorSum( StrengthVector vector1, StrengthVector vector2 )
    {
        // Перевод углов в радианы
        double angle1Rad = vector1.Angle * Math.PI / 180.0;
        double angle2Rad = vector2.Angle * Math.PI / 180.0;

        // Разложение векторов на компоненты
        double x1 = vector1.Strength * Math.Cos(angle1Rad);
        double y1 = vector1.Strength * Math.Sin(angle1Rad);
        double x2 = vector2.Strength * Math.Cos(angle2Rad);
        double y2 = vector2.Strength * Math.Sin(angle2Rad);

        // Сложение компонент
        double xSum = x1 + x2;
        double ySum = y1 + y2;

        // Преобразование обратно в полярные координаты
        int strengthSum = (int)Math.Round(Math.Sqrt(xSum * xSum + ySum * ySum));
        int angleSum = (int)Math.Round( Math.Atan2(ySum, xSum) * 180.0 / Math.PI );;

        return new StrengthVector(strengthSum, angleSum);
    }


    public static Point OffsetPolar( Point currentPosition, StrengthVector move)
    {

        int dx = (int) Math.Round( Math.Cos(move.Angle * Math.PI / 180) * move.Strength );
        int dy = (int) Math.Round( Math.Sin(move.Angle * Math.PI / 180) * move.Strength );

        Point newPosition = new( currentPosition.X + dx , currentPosition.Y + dy );

        return newPosition;
    }
}
