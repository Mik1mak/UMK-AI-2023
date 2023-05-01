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
        public static Schedule GenerateSchedule(IEnumerable<Job> jobs, int numberOfProcessors, Random rng)
        {
            Schedule result = new(numberOfProcessors);

            foreach (Job job in jobs)
                AddAllDependendJobs(result, job, rng);

            return result;
        }

        // tworzy nowy harmonogram na podstawie istniejacego podajac zadania w losowej kolejnosci ktora uwzglednia aktualne polozenie i sasiedztwo
        public static Schedule GenerateSchedule(Schedule schedule, Random rng, int neighbourhood)
            => GenerateSchedule(
                schedule.Where(j => !j.HasNextJob).OrderBy(j => j.Start + rng.Next(-neighbourhood, neighbourhood)).Select(j => Job.NewUnasignedJob(j)),
                schedule.Processors.Length, rng);


        // dodaje podane zadanie oraz wszystkie zalezne do harmonogramu
        private static void AddAllDependendJobs(Schedule schedule, Job job, Random random)
        {
            // rekurncyjne wywolanie dla przypadku w ktorym zadanie posiada poprzednika
            if (job.PreviousJob != null)
                AddAllDependendJobs(schedule, job.PreviousJob, random);

            // jezeli podane zadanie juz posiada przypisany procesor to konczymy funkcje
            if (job.ProcessorIndex.HasValue)
                return;

            // przypisuje zadanie do procesora, ktory jest najmniej obciazony
            job.ProcessorIndex = IndexOfMin(schedule.Processors);
            job.Start = schedule.Processors[job.ProcessorIndex.Value];
            
            // upewnia sie ze spelniony jest warunek f(J) + W(J) <= f(J'), w przeciwnym przypadku probuje przypisac zadanie do innego procesora
            while (job.PreviousJob?.End > job.Start.Value)
            {
                job.ProcessorIndex = (job.ProcessorIndex + random.Next(1, schedule.Processors.Length)) % schedule.Processors.Length;
                job.Start = schedule.Processors[job.ProcessorIndex.Value];
            }

            schedule.AddJob(job);
        }

    }
}
