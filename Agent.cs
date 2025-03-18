using Microsoft.Extensions.Configuration;
using System;

namespace Project;

public class Agent
{
    private readonly double _alpha;
    private readonly double _gamma;
    private StrengthVector[] _actions;

    private readonly double _startRandom;
    private readonly double _deltaRandom;
    private readonly int _stepRandomChange;
    private int _stepCounter;
    private double _currentRandom;

    private readonly string configfilePath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
    private IConfiguration config;


    public Dictionary<int, State> _Q_states;
    public Dictionary<int, double[]> _Q_values;
    public Agent(StrengthVector[] actions)
    {

        config = new ConfigurationBuilder()
            .AddJsonFile(configfilePath, optional: true, reloadOnChange: true)
            .Build();

        _alpha = Convert.ToDouble(config["AlphaCoefficient"] ?? string.Empty);
        _gamma = Convert.ToDouble(config["GammaCoefficient"] ?? string.Empty);
        _Q_states = [];
        _Q_values = [];
        _actions = actions;

        _startRandom = Convert.ToDouble(config["StartRandomChance"] ?? string.Empty);
        _stepRandomChange = Convert.ToInt32(config["StepRandomChanceChange"] ?? string.Empty);
        _deltaRandom = Convert.ToDouble(config["DeltaRandomChance"] ?? string.Empty);
        _currentRandom = _startRandom;
        _stepCounter = 0;
    }
     
     public void Update(Dictionary<int, double[]> Q_target, State state, StrengthVector action, State new_state, int reward, bool terminated)
    
        {
        var hash_state = state.Hash;
         if (!_Q_values.ContainsKey(hash_state))
        {
            double[] t = new double[_actions.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = 0;

            _Q_states.Add(hash_state, state);
            _Q_values.Add(hash_state, t);
        }

        var hash_new_state = new_state.Hash;
        if (!_Q_values.ContainsKey(hash_new_state))
        {
            double[] t = new double[_actions.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = 0;

            _Q_states.Add(hash_new_state, new_state);
            _Q_values.Add(hash_new_state, t);
        }

        int argmax = _Q_values[hash_new_state].ToList().IndexOf(_Q_values[hash_new_state].Max());

        double V_value = 0;

        try
        {
            V_value = Q_target[hash_new_state][argmax];
        }
        catch (KeyNotFoundException)
        {
            V_value = 0;
        }

        int ind = VectorHelper.GetIndex(_actions, action);
        double TD = reward + _gamma * (1 - Convert.ToInt64(terminated)) * V_value - _Q_values[hash_state][ind];
        _Q_values[hash_state][ind] += _alpha * TD;
    }

    public StrengthVector Action(State state)
    {
        _stepCounter++;
        if (_stepCounter == _stepRandomChange)
        {
            _stepCounter = 0;
            _currentRandom -= _deltaRandom;
        }
        if (_currentRandom < 0)
        {
            _currentRandom = 0;
        }
        
        // Возвращает действие агента в зависимости от текущего состояния

        Random random = new Random();
        if (random.NextDouble() < _currentRandom)
        {
            int randomIndex = random.Next(_actions.Length);
            return _actions[randomIndex];
        }


        var hash_state = state.Hash;
        if (!_Q_values.ContainsKey(hash_state))
        {
            double[] t = new double[_actions.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = 0;

            _Q_states.Add(hash_state, state);
            _Q_values.Add(hash_state, t);

            return _actions[random.Next(0, _actions.Length)];
        }

        return _actions[Array.IndexOf(_Q_values[hash_state], _Q_values[hash_state].Max())];
    }


    public void WriteDictionaryToFile(string filePath)
    {
        using StreamWriter writer = new(filePath);
        foreach (var pair in _Q_values)
        {
            writer.Write($"{_Q_states[pair.Key].ToString()}: {string.Join(" ", pair.Value)}");
            writer.WriteLine();
        }
    }
}