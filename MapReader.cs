using Project;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace LoadFlowMap;

/// <summary>
///     Класс описывает соотвествие цвета и значения для матрицы течений
/// </summary>
public class ColorMatch
{
    public Color Color {get;set;}

    /// <summary>
    ///     Значение, которое попадет в матрицу течений
    /// </summary>
    public (int,int) Tuple {get;set;}

    /// <summary>
    ///     Коммент (название цвета) для удобства
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    ///     Строковое представление пары (для вывода в консоль)
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"Color (R,G,B): ({Color.R}, {Color.G}, {Color.B}) = ({Tuple.Item1}, {Tuple.Item2}). Comment: {Comment}";
    }
}

/// <summary>
///     Статический класс (это означает, что не надо создавать экзепляер этого класса. к процедурам обращаемся через имя класса)
///     Содержит процедуры для работы с картой течений
/// </summary>
public static class MapReader
{
    //Разделитель строк по умолчанию. Вынесен сюда, чтобы не объявлять каждый раз
    private const string Separator = " ";

    /// <summary>
    ///     Процедура ищет цвет в массиве соответсвий, если находит возвращает значение соответствия
    /// </summary>
    /// <param name="col">Цвет, который ищем</param>
    /// <param name="colorMap">Массив, в котором ищем</param>
    private static (int, int) ColorToTuple(Color col, ColorMatch[] colorMap)
    {
        foreach (var entry in colorMap)
            if (col.ToArgb() == entry.Color.ToArgb())
                return entry.Tuple;

        // Если ничего не нашли
        return (-1, -1);
    }

    /// <summary>
    ///     Процедура создает массив течений из битмапа используя массив соответсвий
    /// </summary>
    /// <param name="bmp">исходная картинка</param>
    /// <param name="colorMap">массив соответсуий цветов</param>
    /// <returns></returns>
    public static FlowMap GetArrayFromImage(Bitmap bmp, ColorMatch[] colorMap)
    {
        var flows = new FlowMap(bmp.Width, bmp.Height);

        //Циклом пройдем по картинке и переделаем каждый пискель в элемент массива
        for (int y = 0; y < bmp.Height; y++)
            for (int x = 0; x < bmp.Width; x++)
            {
                Color pixelColor = bmp.GetPixel(x, y);
                var flowTuple = ColorToTuple(pixelColor, colorMap);
                var yNew = bmp.Height - 1 - y;
                flows.SetFLow(x, yNew, flowTuple);
            }

        return flows;
    }

    /// <summary>
    ///     Процедура создает массив соответствий из массива строк вида "(R,G,B) (int, int)" 
    /// </summary>
    public static ColorMatch[] GetColorMapFromString(string[] strings)
    {
        //Регулярное выражение, которое обозначает 2 проблеа и более
        Regex regex = new("[ ]{2,}");

        ColorMatch[] colorMap = new ColorMatch[strings.Length];
        int i = 0;
        foreach (var entry in strings)
        {
            //Заменим все подстроки где 2 пробела и больше на разделитель 
            var str = regex.Replace(entry, Separator).Trim(); 
            //Разделим строку на массив строк по разделителю
            var parts = str.Split(Separator);
            var colorParts = parts[0].Trim('(', ')').Split(',');
            var coordParts = parts[1].Trim('(', ')').Split(',');
            var commentPart = (parts.Length>2)?parts[2].Trim():string.Empty;

            var color = Color.FromArgb(int.Parse(colorParts[0]), int.Parse(colorParts[1]), int.Parse(colorParts[2]));
            var coord = (int.Parse(coordParts[0]), int.Parse(coordParts[1]));

            colorMap[i] = new ColorMatch() { Color = color, Tuple = coord, Comment = commentPart };
            i++;
        }
        return colorMap;
    }

    /// <summary>
    ///     Процедура записывает массив течений в файл
    /// </summary>
    /// <param name="array">массив течений</param>
    /// <param name="filePath">пусть к файлу</param>
    public static void WriteArrayToFile(FlowMap flows, string filePath) 
    {
        //using используется, чтобы в конце процедуры не заморачиваться с закрытием файла и освобождением оперативной памяти, которая для него выделена
        //Это возможно, если класс объекта реализует интерфейс IDisposablе. Реализация этого интерфейса предполагает, что объект такого класса знает,
        //как себя правильно уничтожить
        using StreamWriter writer = new(filePath);
        for (int y = flows.LenY-1; y >= 0 ; y--)
        {
            for (int x = 0; x < flows.LenX; x++)
            {
                writer.Write(flows.GetFlow(x,y));
                if (x < flows.LenX - 1)
                    writer.Write(Separator);
            }
            writer.WriteLine();
        }
    }
}
