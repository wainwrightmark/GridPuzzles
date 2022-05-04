namespace GridPuzzles.Session;

public class SessionSettings
{

    public int BifurcateDepth { get; set; } = 0;
    public bool GoToFinalStateOnKeyPress { get; set; } = false;
    public bool SingleStep { get; set; } = false;
    public int MinimumValuesToShow { get; set; } = 8;
    public int MaxFinalIntervalMS { get; set; } = 100;

    public TimeSpan MaxFinalInterval =>  TimeSpan.FromMilliseconds(MaxFinalIntervalMS);
        
}