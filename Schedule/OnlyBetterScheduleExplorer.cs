using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule;

internal class OnlyBetterScheduleExplorer : ISolutionExplorer<Schedule>
{
    private readonly Stopwatch stopwatch = new Stopwatch();
    private readonly Schedule initialSchedule;
    private readonly int maxIter;
    private readonly int neighbourhood;
    private readonly Random rng;

    public TimeSpan Duration => stopwatch.Elapsed;
    public Schedule InitialSolution => initialSchedule;

    public string Name => nameof(OnlyBetterScheduleExplorer);

    public OnlyBetterScheduleExplorer(int maxJobsDuration, Schedule initialSchedule, Random rng, int maxIter = 10_000)
    {
        this.initialSchedule = initialSchedule;
        this.maxIter = maxIter;
        this.rng = rng;
        neighbourhood = maxJobsDuration / 2;
    }

    public Schedule FindBestSolution()
    {
        stopwatch.Start();

        Schedule bestSolution = initialSchedule;
        int minMaxT = bestSolution.MaxTime;
        for (int i = 0; i < maxIter; i++)
        {
            Schedule schedule = ScheduleGenerator.GenerateSchedule(initialSchedule, rng, neighbourhood);

            if (schedule.MaxTime < minMaxT)
            {
                minMaxT = schedule.MaxTime;
                bestSolution = schedule;
            }
        }
        stopwatch.Stop();
        return bestSolution;
    }
}
