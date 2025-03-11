namespace Project;

public class Agent
{
    private double _alpha;
    private double _gamma;
    private (int, int)[] _actions;

    // Q-словарь: (расстояние до цели, угол до цели, [течение под ним и клетки вокруг него])
    public Dictionary<int, (int, int, (int, int)[,])> _Q_states;
    public Dictionary<int, double[]> _Q_values;
    public Agent((int, int)[] actions, double alpha, double gamma)
    {
        _alpha = alpha;
        _gamma = gamma;
        _Q_states = [];
        _Q_values = [];
        _actions = actions;
    }
     //public void Update(Dictionary<(int, int, (int, int)[,]), double[]> Q_target, (int, int, (int, int)[,]) state, (int, int) action, (int, int, (int, int)[,]) new_state, int reward, bool terminated)
     public void Update(Dictionary<int, double[]> Q_target, (int, int, (int, int)[,]) state, (int, int) action, (int, int, (int, int)[,]) new_state, int reward, bool terminated)
    
        {
        // Обновление словаря Q
        //try
        //{
        //    var t = _Q[state];
        //}
        //catch (KeyNotFoundException)
        var hash_state = GetHash(state);
         if (!_Q_values.ContainsKey(hash_state))
        {
            double[] t = new double[_actions.Length];
            for (int i = 0; i < t.Length; i++)
                t[i] = 0;

            _Q_states.Add(hash_state, state);
            _Q_values.Add(hash_state, t);
        }

        //try
        //{
        //    var t = _Q[new_state];
        //}
        //catch (KeyNotFoundException)
        var hash_new_state = GetHash(new_state);
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

        int ind = Array.IndexOf(_actions, action);
        double TD = reward + _gamma * (1 - Convert.ToInt64(terminated)) * V_value - _Q_values[hash_state][ind];
        _Q_values[hash_state][ind] += _alpha * TD;
    }


    public (int, int) Action((int, int, (int, int)[,]) state, double soft = 0.1)
    {
        // Возвращает действие агента в зависимости от текущего состояния

        Random random = new Random();
        if (random.NextDouble() < soft)
        {
            int randomIndex = random.Next(_actions.Length);
            return _actions[randomIndex];
        }


        var hash_state = GetHash(state);
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



    private int GetHash((int, int, (int, int)[,]) val)
    {
        var h = HashCode.Combine(val.Item1);
        h = HashCode.Combine(val.Item2,h);
        for (int i = 0; i < val.Item3.GetLength(0); i++)
            for (int j = 0; j < val.Item3.GetLength(1); j++)
            {
                h = HashCode.Combine(val.Item3[i, j].Item1, h);
                h = HashCode.Combine(val.Item3[i, j].Item2, h);
            }
        return h;
    }
}