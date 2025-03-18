using System.Drawing;

namespace Project;

public class Enviorment
{   
    // Двумерный массив течений; каждый элемент - пара чисел; первое число - сила течения (на сколько клеток сносит), второе число - направление течения (угол от 0 до 315, кратный 45 градусам)
    private FlowMap _flowMap;
    // Количество шагов в этом эпизоде; после 100 шагов эпизод обрывается
    private int _step_counter;
    // Награда за шаг
    private int _reward;
    // Текущее положение агента
    private Point _currentPosition;
    // Начальное положение агента
    private Point _startPosition;

    private Point _endPosition;

    private readonly int _obstacleLen = 3;


    // Создание экземпляра класса; принимает на вход массив течений и начальное положение, обнуляет кол-во шагов
    public Enviorment( FlowMap flows )
    {
        _flowMap = flows;
        _currentPosition = flows.StartPoint;
        _startPosition = flows.StartPoint;
        _endPosition = flows.EndPoint;

        _step_counter = 0;
        _reward = -1;
    }

    
    // Функция "шага"; принимает на вход текущее положение и действие; возвращает новое состояние и награду
    public (State, int, bool, bool) Step( StrengthVector action)
    {
        // Проверяем, что шагов не больше 100; если больше, "обрезаем" эпизод
        bool truncated = (_step_counter >= 700);
        // Прибавляем 1 к счётчику шагов
        _step_counter++;


        // Складываем векторы сноса течением и собственного движения лодки
        StrengthVector movement = VectorHelper.VectorSum( action, _flowMap.GetFlow(_currentPosition) );

        // Записываем новое состояние, применяя общий вектор перемещения к текущему положению
        Point newPosition =  VectorHelper.OffsetPolar( _currentPosition, movement );


        // Если лодка вышла за пределы "карты", возвращаем её в ближайшую клетку в пределах "карты"
        bool up = ( newPosition.Y >= _flowMap.LenY );
        bool down = ( newPosition.Y < 0 );
        bool left = ( newPosition.X < 0 );
        bool right = ( newPosition.X >= _flowMap.LenX );

        if (up)
            newPosition.Y = _flowMap.LenY - 1;
        if (down)
            newPosition.Y = 0;
        if (left)
             newPosition.X = 0;
        if (right)
            newPosition.X = _flowMap.LenX - 1;


        // Проверяем, не столкнуась ли лодка с островом во время перемещения
        if (ColissionСheck(_currentPosition, newPosition))
        {
            _currentPosition = _startPosition;
            // Если столкнулась, возвращаем её в начальное положение
            return (CoordsToNewState(_startPosition), _reward, truncated, false);
        }


        // Проверяем, является ли текущее состояние терминальным
        bool terminated = (_flowMap.GetFlow(newPosition.X, newPosition.Y).Strength == new StrengthVector(10, 10).Strength  && _flowMap.GetFlow(newPosition.X, newPosition.Y).Angle == new StrengthVector(10, 10).Angle);

        _currentPosition = newPosition;

        return (CoordsToNewState(newPosition), _reward, truncated, terminated);
    }

    private bool ColissionСheck( Point currentPosition, Point newPosition)
    {
        var cells = new List<Point>();

        int x0 = currentPosition.X;
        int y0 = currentPosition.Y;
        int x1 = newPosition.X;
        int y1 = newPosition.Y;

        int dx = x1 - x0;
        int dy = y1 - y0;
        int nx = Math.Abs(dx);
        int ny = Math.Abs(dy);

        int signX = dx > 0 ? 1 : (dx < 0 ? -1 : 0);
        int signY = dy > 0 ? 1 : (dy < 0 ? -1 : 0);

        int x = x0, y = y0;
        cells.Add(new Point(x, y));

        // Обработка вертикальной линии
        if (nx == 0)
        {
            while (y != y1)
            {
                y += signY;
                cells.Add(new Point(x, y));
            }
        }
        // Обработка горизонтальной линии
        else if (ny == 0)
        {
            while (x != x1)
            {
                x += signX;
                cells.Add(new Point(x, y));
            }
        }
        else
        {
            int ix = 0, iy = 0;

            while (ix < nx || iy < ny)
            {
                float tMaxX = (ix + 0.5f) / nx;
                float tMaxY = (iy + 0.5f) / ny;

                if (tMaxX < tMaxY)
                {
                    x += signX;
                    ix++;
                }
                else if (tMaxY < tMaxX)
                {
                    y += signY;
                    iy++;
                }
                else // Линия проходит точно через угол клетки – добавляем смежную клетку
                {
                    x += signX;
                    y += signY;
                    ix++;
                    iy++;
                    cells.Add(new Point(x - signX, y));
                }
                cells.Add(new Point(x, y));
            }
        }
        


        bool collision = false;

        for (int i = 0; i < cells.Count(); i++)
        {
            if (_flowMap.GetFlow( cells[i].X, cells[i].Y ).Strength == new StrengthVector(-1, -1).Strength)
            {
                collision = true;
                return collision;
            }

        }

        return collision;
    }

    public State CoordsToNewState(Point coords)
    {
        // Заполнение данных о территории вокруг агента
        Obstacles around = new(_obstacleLen);

        int halfLength = ((around.Len - 1) / 2);

        for (int x = -1 * halfLength; x <= halfLength; x++)
        {
            for (int y = -1 * halfLength; y <= halfLength; y++)
            {
                if (IndexOutOfBounds( new Point(coords.X + x, coords.Y + y) )) {
                    around.SetValue( new Point(x + halfLength, y + halfLength), ObstaclesEnum.Obstacle);
                }
                else
                {
                    if (_flowMap.GetFlow(coords.X + x, coords.Y + y).Strength == new StrengthVector(-1, -1).Strength && _flowMap.GetFlow(coords.X + x, coords.Y + y).Angle == new StrengthVector(-1, -1).Angle)
                        around.SetValue(new Point(x + halfLength, y + halfLength), ObstaclesEnum.Obstacle);
                }
            }
        }


        // Расстояние и угол до цели
        double distance_x = _endPosition.X - coords.X;
        double distance_y = _endPosition.Y - coords.Y;

        int distance = (int)Math.Round(Math.Sqrt(distance_x * distance_x + distance_y * distance_y));
        double degree1 = Math.Acos(distance_x / distance) * 180 / Math.PI ;

        // Округление расстояния и угла до цели

        int degree = (int)Math.Round(degree1 / 10) * 10;

        int lower = distance - (distance % 3);
        int upper = lower + 3;
        distance = (distance - lower < upper - distance) ? lower : upper;


        StrengthVector distanceToEndPosition = new(distance, degree);
        StrengthVector currentFlow = _flowMap.GetFlow(coords);

        State state = new()
        {
            DistanceToEndPosition = distanceToEndPosition,
            CurrentFlow = currentFlow,
            Obstacles = around
        };

        return state;
    }



    
    private bool IndexOutOfBounds(Point coords)
    {
        bool up = (coords.Y >= _flowMap.LenY);
        bool down = (coords.Y < 0);
        bool left = (coords.X < 0);
        bool right = (coords.X >= _flowMap.LenX);

        return (up || down || left || right);
    }
}