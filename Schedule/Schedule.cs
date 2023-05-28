using System.Collections;

namespace Schedule
{
    public class Schedule : IEnumerable<Job>
    {
        // zbior zadan w harmonogramie, nie zawiera duplikatow
        private readonly HashSet<Job> jobs;

        // liczba zadań w harmonogramie
        public int NumberOfJobs => jobs.Count;

        // dlugosc tablicy odpowiada ilosci procesorow - k, a wartosc zapisana pod konkrentym indeksem reprezentuje koniec ostatniego z przypisanych zadan
        public int[] Processors { get; }

        // maxT
        public int MaxTime => Processors.Max();

        // suma wszystkich dlugosci zadan w harmonogramie
        public int SumOfDurations { get; private set; }

        public Schedule(int numberOfProcessors)
        {
            Processors = new int[numberOfProcessors];
            this.jobs = new();
        }

        public void AddJob(Job job)
        {
        #if DEBUG
            if (job.ProcessorIndex == null || job.Start == null)
                throw new ArgumentException(nameof(job));

            if (Processors[job.ProcessorIndex.Value] > job.Start.Value)
                throw new InvalidOperationException();
        #endif
            Processors[job.ProcessorIndex.Value] = job.End;

            jobs.Add(job);
            SumOfDurations += job.Duration;
        }

        public IEnumerator<Job> GetEnumerator() => jobs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}