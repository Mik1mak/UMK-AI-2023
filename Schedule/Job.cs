namespace Schedule
{
    public class Job
    {
        // poprzednie zadanie, ktore musi spelniac: this.Start.Value >= previousJob.End
        public Job? PreviousJob { get; set; }

        // okresla czy dane zadanie jest wymagane, aby zaczac jedno z nastepnych
        public bool HasNextJob { get; set; } = false;

        // czas wymagany na wykonanie zadania - W(J)
        public int Duration { get; set; } = 1;

        // jednostka czasu w ktorej zadanie zostanie ukonczone - f(J)+W(J)
        public int End => Start.HasValue ? Start.Value + Duration : -1;

        // jednostka czasu w ktorej zadanie zostanie rozpoczete - f(J)
        public int? Start { get; set; }

        // index procesora do ktorego przydzielone jest zadanie
        public int? ProcessorIndex { get; set; }

        public Job() { }

        // tworzenie nowych obiektow na podstawie istniejacego zadania
        public static Job NewUnasignedJob(Job job) => new Job(job);
        private Job(Job job) 
        {
            Duration = job.Duration;
            HasNextJob = job.HasNextJob;

            if (job.PreviousJob != null)
                PreviousJob = new Job(job.PreviousJob);
        }
    }
}