namespace Project;

public class Flow
{
    public Flow(int strength, int angle)
    {
        Strength = strength;
        Angle = angle;
    }

    public int Strength { get; }

    public int Angle { get; } 
}

public class Flows
{
    private readonly Flow[,] _flows;

    public Flows(int lenX, int lenY) { 
        _flows = new Flow[lenY, lenY]; 
    }

    public int LenX => _flows.GetLength(0);
    public int LenY => _flows.GetLength(1);
    public Flow GetFlow(int x, int y) => _flows[x,y];

    public void SetFLow(int x, int y, int strength, int angle) 
    {
        _flows[x, y] = new Flow(strength, angle);
    }

}
