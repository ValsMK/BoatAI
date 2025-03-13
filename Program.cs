using Project;
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

Console.WriteLine("Begin read color map");

string[] colorMapString = config.GetSection("ColorMap").Get<string[]>() ?? [];
ColorMatch[] colorMap = MapReader.GetColorMapFromString(colorMapString);
foreach (var entry in colorMap)
    Console.WriteLine(entry);

Console.WriteLine("Begin read flow map");

//Таймер считает время выполнения процедуры
Stopwatch stopwatch = Stopwatch.StartNew();

//using используется, чтобы в конце процедуры не заморачиваться с закрытием файла и освобождением оперативной памяти, которая для него выделена
//Это возможно, если класс объекта реализует интерфейс IDisposablе. Реализация этого интерфейса предполагает, что объект такого класса знает,
//как себя правильно уничтожить
using Bitmap bmp = new(trainingMapPath);
var flowMap = MapReader.GetArrayFromImage(bmp, colorMap);

stopwatch.Stop();
Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

//После того, как заполнили массив flows, выведем его в файл для дебага
var outputFilePath = config["OutputFilePath"] ?? string.Empty;
outputFilePath = Path.Combine(workingDir, outputFilePath);
if (outputFilePath != string.Empty)
{
    Console.WriteLine("Write to file");
    MapReader.WriteArrayToFile(flowMap, outputFilePath);
}

//Зададим начальную и конечную координаты
flowMap.StartPoint = new Point(1400, 10);
flowMap.EndPoint = new Point(700, flowMap.LenX - 50);
Console.WriteLine($"StartPoint: {flowMap.GetFlow(flowMap.StartPoint)}");
Console.WriteLine($"EndPoint: {flowMap.GetFlow(flowMap.EndPoint)}");

Console.WriteLine("End read flow map");

//тестовая карта течений
//var testFlowMap = TestFlowMap.Get();
//MapReader.WriteArrayToFile(testFlowMap, outputFilePath);




//Инициализация обучения

//создадим переменную среды
var env = new Enviorment(flowMap);

//Набор действий
StrengthVector[] actions = [ new(5, 0), new(5, 45), new(5, 90), new(5, 135), new(5, 180), new(5, 225), new(5, 270), new(5, 315)];

//созадим переменную агента
var agent = new Agent(actions, 0.05, 1);

var state = env.CoordsToNewState(flowMap.StartPoint);

int count_steps = 0;

for (int i = 0; i < 100_000_000; i++)
{
    var action = agent.Action(state);

    var new_state = env.Step(action);

    agent.Update(agent._Q_values, state, action, new_state.Item1, new_state.Item2, new_state.Item4);
    //model.Update(model._Q, state, action, new_state.Item1, new_state.Item2, new_state.Item4);

    state = new_state.Item1;

    if (count_steps % 10000 == 0)
        Console.WriteLine(count_steps);
    count_steps++;

    if (new_state.Item3 || new_state.Item4)
    {
        env = new Enviorment(flowMap);
        state = env.CoordsToNewState(flowMap.StartPoint);
    }
}


//var dict = model._Q.OrderBy(pair => (pair.Key.Item1, pair.Key.Item2));
var dict_states = agent._Q_states;
var dict_values = agent._Q_values;
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


env = new Enviorment(flowMap);
state = env.CoordsToNewState(flowMap.StartPoint);
var action1 = agent.Action(state, 0);
var new_state1 = env.Step(action1);
agent.Update(agent._Q_values, state, action1, new_state1.Item1, new_state1.Item2, new_state1.Item4);
state = new_state1.Item1;
int count = 1;
while (!new_state1.Item3 && !new_state1.Item4)
{
    Console.WriteLine(action1);
    action1 = agent.Action(new_state1.Item1, 0);
    new_state1 = env.Step(action1);
    count += 1;
}
Console.WriteLine(count);
