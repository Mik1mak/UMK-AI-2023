namespace Schedule;

internal interface ISolutionExplorer<T>
{
    public string Name { get; }
    public TimeSpan Duration { get; }
    public T InitialSolution { get; }
    public T FindBestSolution();
}
