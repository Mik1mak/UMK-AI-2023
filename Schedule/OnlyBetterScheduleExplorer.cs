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
    private readonly TimeSpan maxTime;
    private readonly int neighbourhood;
    private readonly Random rng;

    public TimeSpan Duration => stopwatch.Elapsed;
    public Schedule InitialSolution => initialSchedule;

    public string Name => nameof(OnlyBetterScheduleExplorer);

    public OnlyBetterScheduleExplorer(int maxJobsDuration, Schedule initialSchedule, TimeSpan maxTime, Random rng)
    {
        this.initialSchedule = initialSchedule;
        this.maxTime = maxTime;
        this.rng = rng;
        neighbourhood = Math.Max(maxJobsDuration / 50, 2);
    }

    public Schedule FindBestSolution()
    {
        stopwatch.Start();

        Schedule bestSolution = initialSchedule;
        int minMaxT = bestSolution.MaxTime;
        while(Duration < maxTime)
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
