using Project;
using System.Collections.Generic;
using LoadFlowMap;
using Microsoft.Extensions.Configuration;
using System.Diagnostics;
using System.Drawing;



//созадим объект конфигурации
string configfilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
var config = new ConfigurationBuilder()
    .AddJsonFile(configfilePath, optional: true, reloadOnChange: true)
    .Build();

//загрузим из конфигурации путь, где лежит карта
var workingDir = config["WorkingDir"] ?? string.Empty;
var trainingMapPath = config["TrainingMapPath"] ?? string.Empty;
trainingMapPath = Path.Combine(workingDir, trainingMapPath);
if (trainingMapPath == string.Empty)
    throw new Exception("Section TrainingMapPath not found");

string[] colorMapString = config.GetSection("ColorMap").Get<string[]>() ?? [];
ColorMatch[] colorMap = MapReader.GetColorMapFromString(colorMapString);
foreach (var entry in colorMap)
    Console.WriteLine(entry);


Console.WriteLine("Begin read");

//Таймер считает время выполнения процедуры
Stopwatch stopwatch = Stopwatch.StartNew();

//using используется, чтобы в конце процедуры не заморачиваться с закрытием файла и освобождением оперативной памяти, которая для него выделена
//Это возможно, если класс объекта реализует интерфейс IDisposablе. Реализация этого интерфейса предполагает, что объект такого класса знает,
//как себя правильно уничтожить
using Bitmap bmp = new(trainingMapPath);
var flows = MapReader.GetArrayFromImage(bmp, colorMap);

stopwatch.Stop();
Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

//После того, как заполнили массив flows, выведем его в файл для дебага
var outputFilePath = config["OutputFilePath"] ?? string.Empty;
outputFilePath = Path.Combine(workingDir, outputFilePath);
if (outputFilePath != string.Empty)
{
    Console.WriteLine("Write to file");
    MapReader.WriteArrayToFile(flows, outputFilePath);
}

Console.WriteLine("End read");


flows[50, 700] = (10, 10);

(int, int)[,] _flows1 = new (int, int)[15, 7] { { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (10, 10) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (-1, -1), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 45), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2, 315), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 45), (2 , 90), (1, 135) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 315),(2 , 90), (1, 215) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) },
                                                { (-1, -1), (0 , 90), (0 , 90), (0 , 90), (1 , 90), (2 , 90), (1 , 90) } };


(int, int) _start_position = (2400, 1400);

Console.WriteLine(flows[_start_position.Item1, _start_position.Item2]);

var env = new Enviorment(flows, _start_position);

(int, int)[] actions = [(5, 0), (5, 45), (5, 90), (5, 135), (5, 180), (5, 225), (5, 270), (5, 315)];

var model = new Agent(actions, 0.05, 1);

var state = env.CoordsToNewState(_start_position);

int count_steps = 0;

for (int i = 0; i < 100_000_000; i++)
{
    var action = model.Action(state);

    var new_state = env.Step(action);

    model.Update(model._Q_values, state, action, new_state.Item1, new_state.Item2, new_state.Item4);
    //model.Update(model._Q, state, action, new_state.Item1, new_state.Item2, new_state.Item4);

    state = new_state.Item1;

    if (count_steps % 10000 == 0)
        Console.WriteLine(count_steps);
    count_steps++;

    if (new_state.Item3 || new_state.Item4)
    {
        env = new Enviorment(flows, _start_position);
        state = env.CoordsToNewState(_start_position);
    }
}


//var dict = model._Q.OrderBy(pair => (pair.Key.Item1, pair.Key.Item2));
var dict_states = model._Q_states;
var dict_values = model._Q_values;
foreach (var pair in dict_states)
{
    //Console.WriteLine($"{pair.Key.Item1}, {pair.Key.Item2}, {Format2DArray(pair.Key.Item3)}  :  {string.Join(", ", pair.Value)}");
    Console.WriteLine($"{pair.Key}  :  {string.Join(", ", pair.Value)}  :  {string.Join(" ; ", dict_values[pair.Key])}");
}

//static string Format2DArray((int, int)[,] array)
//{
//    List<string> elements = new List<string>();
//    int rows = array.GetLength(0);
//    int cols = array.GetLength(1);

//    for (int i = 0; i < rows; i++)
//    {
//        for (int j = 0; j < cols; j++)
//        {
//            elements.Add($"({array[i, j].Item1}, {array[i, j].Item2})");
//        }
//    }

//    return $"[{string.Join(", ", elements)}]";
//}

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();


env = new Enviorment(flows, _start_position);
state = env.CoordsToNewState(_start_position);
var action1 = model.Action(state, 0);
var new_state1 = env.Step(action1);
model.Update(model._Q_values, state, action1, new_state1.Item1, new_state1.Item2, new_state1.Item4);
state = new_state1.Item1;
int count = 1;
while (!new_state1.Item3 && !new_state1.Item4)
{
    Console.WriteLine(action1);
    action1 = model.Action(new_state1.Item1, 0);
    new_state1 = env.Step(action1);
    count += 1;
}
Console.WriteLine(count);
