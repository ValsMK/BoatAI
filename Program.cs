using LoadFlowMap;
using Microsoft.Extensions.Configuration;
using Project;
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
//тестовая карта течений
//flowMap = TestFlowMap.Get();

stopwatch.Stop();
Console.WriteLine($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");

// Запись flows в файл .txt
var outputFilePath = config["OutputFilePath"] ?? string.Empty;
outputFilePath = Path.Combine(workingDir, outputFilePath);
if (outputFilePath != string.Empty)
{
    Console.WriteLine("Write to file");
    MapReader.WriteArrayToFile(flowMap, outputFilePath);
}

//Зададим начальную и конечную координаты
//Point startPoint = new(5, 1);
//Point endPoint = new(45, 45);
//if (flowMap.GetFlow(startPoint) == new StrengthVector(-1, -1))
//    Console.WriteLine("В начальной точке препятствие!");
//if (flowMap.GetFlow(endPoint) == new StrengthVector(-1, -1))
//    Console.WriteLine("В конечной точке препятствие!");
//flowMap.StartPoint = startPoint;
//flowMap.EndPoint = endPoint;

Console.WriteLine("End read flow map");
Console.WriteLine();



//Инициализация обучения

//создадим переменную среды

//Набор действий
//StrengthVector[] actions = [ new(5, 0), new(5, 45), new(5, 90), new(5, 135), new(5, 180), new(5, 225), new(5, 270), new(5, 315)];
StrengthVector[] actions = [new(2, 0), new(2, 45), new(2, 90), new(2, 135), new(2, 180), new(2, 225), new(2, 270), new(2, 315)];

//созадим переменную агента
var agent = new Agent(actions);

//Засекаем время на обучение
Stopwatch stopwatchEducation = Stopwatch.StartNew();


//Point[] coordsVariants = { new (10, 10), new (250, 250), new (10, 10), new (490, 490), new (10, 10), new (10, 490), new (10, 10), new (490, 10),
//                           new (10, 490), new (250, 250), new (10, 490), new (490, 490), new (10, 490), new (490, 10), new (10, 490), new (10, 10),
//                           new (490, 490), new (250, 250), new (490, 490), new (10, 10), new (490, 490), new (10, 490), new (490, 490), new (490, 10), 
//                           new (490, 10), new (10, 490), new (490, 10), new (10, 10), new (490, 10), new (250, 250), new (490, 10), new (490, 490), 
//                           new (250, 250), new (10, 10), new (250, 250), new (490, 490), new (250, 250), new (10, 490), new (250, 250), new (490, 10), };
Point[] coordsVariants = { new(2, 2), new(98, 2) };

for (int variant =  0; variant < 1; variant++)
{
    Point _startPoint = coordsVariants[variant * 2];
    Point _endPoint = coordsVariants[(variant * 2) + 1];

    Console.WriteLine($"Current episode: start position - {_startPoint.X}, {_startPoint.Y}; end position - {_endPoint.X}, {_endPoint.Y}.");

    flowMap.StartPoint = _startPoint;
    flowMap.EndPoint = _endPoint;

    agent._currentRandom = 1.0;

    var env = new Enviorment(flowMap);

    var state = env.CoordsToNewState(flowMap.StartPoint);

    int count_steps = 1;

    long num_epochs = Convert.ToInt64(config["NumberOfEpochs"]);

    for (long i = 0; i < num_epochs; i++)
    {
        var action = agent.Action(state);

        var new_state = env.Step(action);

        agent.Update(agent._Q_values, state, action, new_state.Item1, new_state.Item2, new_state.Item4);

        state = new_state.Item1;

        if (count_steps % 1_000_000 == 0)
        {
            Console.WriteLine(count_steps);
        }

        count_steps++;

        if (new_state.Item3 || new_state.Item4)
        {
            env = new Enviorment(flowMap);
            state = env.CoordsToNewState(flowMap.StartPoint);
        }
    }

    env = new Enviorment(flowMap);
    state = env.CoordsToNewState(flowMap.StartPoint);
    var action1 = agent.Action(state);
    var new_state1 = env.Step(action1);
    state = new_state1.Item1;
    int count = 1;
    while (!new_state1.Item3 && !new_state1.Item4)
    {
        action1 = agent.Action(new_state1.Item1);
        new_state1 = env.Step(action1);
        count += 1;
    }
    if (count < 2000)
        Console.WriteLine($"Succsesfully found a way on this episode ({count} steps)");
    else
        Console.WriteLine($"Have not found a way on this episode");


    Console.WriteLine();

    var outputQArrayPath1 = config["OutputQArrayPath"] ?? string.Empty;
    outputQArrayPath1 = Path.Combine(workingDir, outputQArrayPath1);
    if (outputQArrayPath1 != string.Empty)
    {
        agent.WriteDictionaryToFile(outputQArrayPath1);
    }
}



stopwatchEducation.Stop();

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();

Console.WriteLine($"Education Time: {stopwatchEducation.Elapsed.Minutes} minutes.");

Console.WriteLine();
Console.WriteLine();
Console.WriteLine();


//env = new Enviorment(flowMap);
//state = env.CoordsToNewState(flowMap.StartPoint);
//var action1 = agent.Action(state);
//var new_state1 = env.Step(action1);
//state = new_state1.Item1;
//int count = 1;
//while (!new_state1.Item3 && !new_state1.Item4)
//{
//    Console.WriteLine($"{new_state1.Item1.DistanceToEndPosition}  :  {action1}");
//    action1 = agent.Action(new_state1.Item1);
//    new_state1 = env.Step(action1);
//    count += 1;
//}
//Console.WriteLine(count);




// Запись Q-массива в файл
var outputQArrayPath = config["OutputQArrayPath"] ?? string.Empty;
outputQArrayPath = Path.Combine(workingDir, outputQArrayPath);
if (outputQArrayPath != string.Empty)
{
    Console.WriteLine();
    Console.WriteLine();
    Console.WriteLine("Began writing Q-Dictionary to file");
    agent.WriteDictionaryToFile(outputQArrayPath);
    Console.WriteLine("Ended writing Q-Dictionary to file");
}