namespace Schedule
{
    public class Job
    {
        // poprzednie zadanie, które musi spełniać: this.Start.Value >= previousJob.End
        public Job? PreviousJob { get; set; }

        // określa czy dane zadanie jest wymagane, aby zacząć jedno z następnych
        public bool HasNextJob { get; set; } = false;

        // czas wymagany na wykonanie zadania - W(J)
        public int Duration { get; set; } = 1;

        // jednostka czasu, w której zadanie zostanie ukończone - f(J)+W(J)
        public int End => Start.HasValue ? Start.Value + Duration : -1;

        // jednostka czasu w której zadanie zostanie rozpoczęte - f(J)
        public int? Start { get; set; }

        // indeks procesora do którego przydzielone jest zadanie
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