using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule
{
    static class ScheduleGenerator
    {

        // metoda generująca dane wejsciowe
        public static List<Job> GenerateInputData(int count, Random rng, int maxJobDuration = 10)
        {
            List<Job> result = new();

            // zadanie od ktorych wykonania nie zależą inne zadania
            List<Job> independentJobs = new();

            // losowanie wartosci zadan i powiazan miedzy nimi
            count++;
            for (int i = 0; i < count - 1; i++)
            {
                var newJob = new Job
                {
                    Duration = rng.Next(1, maxJobDuration),
                    PreviousJob = rng.Next(2) == 1 ? null : independentJobs.ElementAtOrDefault(rng.Next(independentJobs.Count)),
                };

                if (newJob.PreviousJob != null)
                {
                    newJob.PreviousJob.HasNextJob = true;
                    independentJobs.Remove(newJob.PreviousJob);
                }

                result.Add(newJob);
                independentJobs.Add(newJob);
            }

            return result;
        }


        // wyznaczanie indeksu wskazujacego na najmniejsza wartosc w tablicy
        private static int IndexOfMin(int[] array)
        {

            int min = array[0];
            int minIndex = 0;

            for (int i = 1; i < array.Length; ++i)
            {
                if (array[i] < min)
                {
                    min = array[i];
                    minIndex = i;
                }
            }

            return minIndex;
        }

        // tworzy nowy harmonogram dla wskazanej ilosci procesorow - k
        public static Schedule GenerateSchedule(IEnumerable<Job> jobs, int numberOfProcessors)
        {
            Schedule result = new(numberOfProcessors);

            foreach (Job job in jobs)
                AddAllDependendJobs(result, job);

            return result;
        }

        // tworzy nowy harmonogram na podstawie istniejacego podajac zadania w losowej kolejnosci ktora uwzglednia aktualne polozenie i sasiedztwo
        public static Schedule GenerateSchedule(Schedule schedule, Random rng, int neighbourhood)
        {
            int orderIndex = 0;

            var orderedJobs = schedule
                .Where(j => !j.HasNextJob)
                .OrderBy(j => orderIndex++ + rng.Next(-neighbourhood, neighbourhood+1))
                .Select(j => Job.NewUnasignedJob(j));

            return GenerateSchedule(orderedJobs, schedule.Processors.Length);
        }

        //=> GenerateSchedule(
        //        schedule.Where(j => !j.HasNextJob)
        //            .OrderBy(j => j.Start + rng.Next(-neighbourhood, neighbourhood))
        //            .Select(j => Job.NewUnasignedJob(j)),
        //        schedule.Processors.Length);

        public static Schedule GenerateSchedule(Schedule baseSchedule, int[] orders)
        {
            int orderIndex = 0;

            var orderedJobs = baseSchedule
                .Where(j => !j.HasNextJob)
                .OrderBy(j => orders[orderIndex++])
                .Select(j => Job.NewUnasignedJob(j));

            return GenerateSchedule(orderedJobs, baseSchedule.Processors.Length);
        }

        // dodaje podane zadanie oraz wszystkie zalezne do harmonogramu
        private static void AddAllDependendJobs(Schedule schedule, Job job)
        {
            // rekurncyjne wywolanie dla przypadku w ktorym zadanie posiada poprzednika
            if (job.PreviousJob != null)
                AddAllDependendJobs(schedule, job.PreviousJob);

            // jezeli podane zadanie juz posiada przypisany procesor to konczymy funkcje
            if (job.ProcessorIndex.HasValue)
                return;

            // przypisuje zadanie do procesora, ktory jest najmniej obciazony
            job.ProcessorIndex = IndexOfMin(schedule.Processors);
            job.Start = schedule.Processors[job.ProcessorIndex.Value];
            
            // upewnia sie ze spelniony jest warunek f(J) + W(J) <= f(J'), w przeciwnym przypadku probuje przypisac zadanie do innego procesora
            while (job.PreviousJob?.End > job.Start.Value)
            {
                job.ProcessorIndex = (job.ProcessorIndex + 1) % schedule.Processors.Length;
                job.Start = schedule.Processors[job.ProcessorIndex.Value];
            }

            schedule.AddJob(job);
        }


        public static List<Job> ReadJobsFromFile(string path)
        {
            string[] lines = File.ReadAllLines(path);
            var jobs = new List<Job>();

            foreach (string durationOfJob in lines[0].Split(','))
                jobs.Add(new Job()
                {
                    Duration = int.Parse(durationOfJob),
                });

            foreach (string dependency in lines.Skip(1))
            {
                string[] parts = dependency.Split(',');
                int previousJobIndex = int.Parse(parts[0]);
                int nextJobIndex = int.Parse(parts[1]);

                jobs[previousJobIndex].HasNextJob = true;
                jobs[nextJobIndex].PreviousJob = jobs[previousJobIndex];
            }

            return jobs;
        }
    }
}
