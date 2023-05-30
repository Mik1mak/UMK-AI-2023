using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule;

public static class Extensions
{
    public static bool Validate(this Schedule schedule, int expectedSumOfDuration)
    {
        if (schedule.SumOfDurations != expectedSumOfDuration)
            return false;

        foreach (Job job in schedule)
        {
            if (job.PreviousJob != null)
                if (job.Start < job.PreviousJob.End)
                    return false;

            if (schedule.Any(j => CheckColiding(j, job)))
                return false;
        }

        return true;
    }
    private static bool CheckColiding(Job a, Job b, bool firstCheck = true)
    {
        if (a == b)
            return false;

        if (a.ProcessorIndex!.Value != b.ProcessorIndex!.Value)
            return false;

        if (a.Start!.Value == b.Start!.Value)
            return true;

        if (a.Start!.Value < b.Start!.Value && b.End < a.End)
            return true;

        if (firstCheck)
            return CheckColiding(b, a, false);

        return false;
    }
}
