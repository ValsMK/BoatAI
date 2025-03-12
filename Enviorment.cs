namespace Project;

public class Enviorment
{   
    // Двумерный массив течений; каждый элемент - пара чисел; первое число - сила течения (на сколько клеток сносит), второе число - направление течения (угол от 0 до 315, кратный 45 градусам)
    private (int, int)[,] _flows;
    // Количество шагов в этом эпизоде; после 100 шагов эпизод обрывается
    private int _step_counter;
    // Награда за шаг
    private int _reward;
    // Текущее положение агента
    private (int, int) _current_position;
    // Начальное положение агента
    private (int, int) _start_position;

    private (int, int) goal_coords;

    // Создание экземпляра класса; принимает на вход массив течений и начальное положение, обнуляет кол-во шагов
    public Enviorment( (int, int)[,] flows, (int, int) current_position)
    {
        _flows = flows;
        _step_counter = 0;
        _reward = -1;
        _current_position = current_position;
        _start_position = current_position;

        goal_coords = FindIndex(_flows, (10, 10));
    }

    
    // Функция "шага"; принимает на вход текущее положение и действие; возвращает новое состояние и награду
    public ((int, int, (int, int)[,]), int, bool, bool) Step(( int, double ) action)
    {
        // Проверяем, что шагов не больше 100; если больше, "обрезаем" эпизод
        bool truncated = (_step_counter >= 700);
        // Прибавляем 1 к счётчику шагов
        _step_counter++;


        // Складываем векторы сноса течением и собственного движения лодки
        ( int, double ) movement = vector_sum(action, _flows[_current_position.Item1, _current_position.Item2]);

        // Записываем новое состояние, применяя общий вектор перемещения к текущему положению
        (int, int) new_position = Offset(_current_position, movement);


        // Если лодка вышла за пределы "карты", возвращаем её в ближайшую клетку в пределах "карты"
        bool up = (new_position.Item1 < 0);
        bool down = (new_position.Item1 >= _flows.GetLength(0));
        bool left = (new_position.Item2 < 0);
        bool right = (new_position.Item2 >= _flows.GetLength(1));

        if (up)
            new_position.Item1 = 0;
        else if (down)
            new_position.Item1 = _flows.GetLength(0) - 1;
        if (left)
             new_position.Item2 = 0;
        else if (right)
            new_position.Item2 = _flows.GetLength(1) - 1;


        // Проверяем, не столкнуась ли лодка с островом во время перемещения
        if (ColissionСheck(_current_position, new_position))
        {
            _current_position = _start_position;
            // Если столкнулась, возвращаем её в начальное положение
            return (CoordsToNewState(_start_position), _reward, false, truncated);
        }


        // Проверяем, является ли текущее состояние терминальным
        bool terminated = (_flows[new_position.Item1, new_position.Item2] == (10, 10));

        _current_position = new_position;

        return (CoordsToNewState(new_position), _reward, truncated, terminated);
    }

    private (int, int) Offset((int, int) current_position, (int, double) move)
    {
        return ( current_position.Item1 - (int)Math.Round(Math.Sin(move.Item2 * Math.PI / 180) * move.Item1 ),
                 current_position.Item2 + (int)Math.Round(Math.Cos(move.Item2 * Math.PI / 180) * move.Item1 ) );
    }

    static (int r, double theta) vector_sum((int, double) vector1, (int, double) vector2)
    {
        // Перевод углов в радианы
        double degree1Rad = vector1.Item2 * Math.PI / 180.0;
        double degree2Rad = vector2.Item2 * Math.PI / 180.0;

        // Разложение векторов на компоненты
        double x1 = vector1.Item1 * Math.Cos(degree1Rad);
        double y1 = vector1.Item1 * Math.Sin(degree1Rad);
        double x2 = vector2.Item1 * Math.Cos(degree2Rad);
        double y2 = vector2.Item1 * Math.Sin(degree2Rad);

        // Сложение компонент
        double xSum = x1 + x2;
        double ySum = y1 + y2;

        // Преобразование обратно в полярные координаты
        int rSum = (int)Math.Round( Math.Sqrt(xSum * xSum + ySum * ySum) );
        double degreeSum = Math.Atan2(ySum, xSum) * 180.0 / Math.PI;

        return (rSum, degreeSum);
    }

    private bool ColissionСheck((int, int) current_position, (int, int) new_position)
    {
        var cells = new List<(int, int)>();

        int x0 = current_position.Item1;
        int y0 = current_position.Item2;
        int x1 = new_position.Item1;
        int y1 = new_position.Item2;

        int dx = x1 - x0;
        int dy = y1 - y0;
        int nx = Math.Abs(dx);
        int ny = Math.Abs(dy);

        int signX = dx > 0 ? 1 : (dx < 0 ? -1 : 0);
        int signY = dy > 0 ? 1 : (dy < 0 ? -1 : 0);

        int x = x0, y = y0;
        cells.Add((x, y));

        // Обработка вертикальной линии
        if (nx == 0)
        {
            while (y != y1)
            {
                y += signY;
                cells.Add((x, y));
            }
        }
        // Обработка горизонтальной линии
        else if (ny == 0)
        {
            while (x != x1)
            {
                x += signX;
                cells.Add((x, y));
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
                    cells.Add((x - signX, y));
                }
                cells.Add((x, y));
            }
        }
        


        bool collision = false;

        for (int i = 0; i < cells.Count(); i++)
        {
            if (_flows[cells[i].Item1, cells[i].Item2] == (-1, -1))
            {
                collision = true;
                return collision;
            }

        }

        return collision;
    }

    public (int, int, (int, int)[,]) CoordsToNewState((int, int) coords)
    {
        // Заполнение данных о территории вокруг агента
        var around = new (int, int)[3, 3];

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (IndexOutOfBounds( (coords.Item1 + i, coords.Item2 + j) )) {
                    around[i + 1, j + 1] = (-1, -1);
                }
                else
                {
                    if (_flows[coords.Item1 + i, coords.Item2 + j] == (-1, -1))
                        around[i + 1, j + 1] = (-1, -1);
                }
            }
        }
        around[1, 1] = _flows[coords.Item1, coords.Item2];


        //if (_flows[coords.Item1 - 1, coords.Item2 - 1] == (-1, -1))
        //    around[0, 0] = _flows[coords.Item1 - 1, coords.Item2 - 1];
        //if (_flows[coords.Item1 - 1, coords.Item2] == (-1, -1))
        //    around[0, 1] = _flows[coords.Item1 - 1, coords.Item2];
        //if (_flows[coords.Item1 - 1, coords.Item2 + 1] == (-1, -1))
        //    around[0, 2] = _flows[coords.Item1 - 1, coords.Item2 + 1];
        //if (_flows[coords.Item1, coords.Item2 - 1] == (-1, -1))
        //    around[1, 0] = _flows[coords.Item1, coords.Item2 - 1];
        //around[1, 1] = _flows[coords.Item1, coords.Item2];
        //if (_flows[coords.Item1, coords.Item2 + 1] == (-1, -1))
        //    around[1, 2] = _flows[coords.Item1, coords.Item2 + 1];
        //if (_flows[coords.Item1 + 1, coords.Item2 - 1] == (-1, -1))
        //    around[2, 0] = _flows[coords.Item1 + 1, coords.Item2 - 1];
        //if (_flows[coords.Item1 + 1, coords.Item2] == (-1, -1))
        //    around[2, 1] = _flows[coords.Item1 + 1, coords.Item2];
        //if (_flows[coords.Item1 + 1, coords.Item2 + 1] == (-1, -1))
        //    around[2, 2] = _flows[coords.Item1 + 1, coords.Item2 + 1];


        // Расстояние и угол до цели
        double distance_x = goal_coords.Item2 - coords.Item2;
        double distance_y = goal_coords.Item1 + coords.Item1;

        int distance = (int)Math.Round(Math.Sqrt(distance_x * distance_x + distance_y * distance_y));
        double degree1 = Math.Acos(distance_x / distance) * 180 / Math.PI ;

        // Округление расстояния и угла до цели

        //int degree = (int)Math.Round(degree1 / 10) * 10;

        //int lower = distance - (distance % 3);
        //int upper = lower + 3;
        //distance = (distance - lower < upper - distance) ? lower : upper;


        return (distance, (int)Math.Round(degree1), around);
    }


    private (int, int) FindIndex((int, int)[,] array, (int, int) target)
    {
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                if (array[i, j] == target)
                    return (i, j);
            }
        }
        return (-1, -1);
    }

    
    private bool IndexOutOfBounds((int, int) coords)
    {
        bool up = (coords.Item1 < 0);
        bool down = (coords.Item1 >= _flows.GetLength(0));
        bool left = (coords.Item2 < 0);
        bool right = (coords.Item2 >= _flows.GetLength(1));

        return (up || down || left || right);
    }
}