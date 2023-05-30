namespace Schedule
{

    internal class Program
    {
        static void Main(string[] args)
        {
        #if DEBUG
           args = new[] { "4", "1000", "150", "100"};
           // args = new[] { "4", "20", "input.txt", "150", "100"};
        #endif

            var scoreTable = new Dictionary<string, int>()
            {
                [nameof(ScheduleSimulatedAnnealing)] = 0,
                [nameof(OnlyBetterScheduleExplorer)] = 0,
                [nameof(GeneticScheduleExplorer)] = 0,
            };

            for (int i = 0; i < 10; i++)
               TestSolutions(args, scoreTable);
            
            Console.WriteLine();

            foreach (var kpv in scoreTable)
                Console.WriteLine($"{kpv.Key} was best {kpv.Value} times");

            Console.WriteLine("");
            if (Console.ReadKey().Key == ConsoleKey.Tab)
                Main(args);
        }

        static void TestSolutions(string[] args, IDictionary<string, int> scores)
        {
            // odczytywanie parametrów
            IEnumerable<Job> inputJobs;
            int numberOfJobs, maxJobDuration, seed;
            int numberOfProcessors = int.Parse(args.ElementAtOrDefault(0) ?? "4");
            TimeSpan maxTime = TimeSpan.FromMilliseconds(int.Parse(args.ElementAtOrDefault(1) ?? "2"));

            if (File.Exists(args.ElementAtOrDefault(2)))
            {
                // odczytywanie z danych wejściowych z pliku
                List<Job> jobs = ScheduleGenerator.ReadJobsFromFile(args[2]);
                inputJobs = jobs;
                numberOfJobs = jobs.Count;
                maxJobDuration = jobs.Max(job => job.Duration);
                seed = Guid.NewGuid().GetHashCode();
            }
            else
            {
                numberOfJobs = int.Parse(args.ElementAtOrDefault(2) ?? "150");
                maxJobDuration = int.Parse(args.ElementAtOrDefault(3) ?? "100");
                seed = int.Parse(args.ElementAtOrDefault(4) ?? Guid.NewGuid().GetHashCode().ToString());
                Random rng = new Random(seed);

                // generowanie danych wejściowych
                inputJobs = ScheduleGenerator
                    .GenerateInputData(numberOfJobs, rng, maxJobDuration)
                    .OrderBy(x => rng.NextDouble());
            }

            Console.WriteLine("Parameters:");
            Console.WriteLine($"\tnumber of jobs: {numberOfJobs}");
            Console.WriteLine($"\tnumber of processors: {numberOfProcessors}");
            Console.WriteLine($"\tmax single job duration: {maxJobDuration}");
            Console.WriteLine($"\tmax time per exploration: {maxTime.TotalMilliseconds}ms");
            Console.WriteLine($"\tseed: {seed}\n");

            // układanie początkowego harmonogramu
            Schedule initialSchedule = ScheduleGenerator.GenerateSchedule(inputJobs, numberOfProcessors);
            Console.WriteLine($"Initial schedule has {initialSchedule.MaxTime} execution time units");
            Console.WriteLine();

            // definicja obiektów klas poszukujących rozwiązania
            var explorers = new List<ISolutionExplorer<Schedule>>()
            {
                new ScheduleSimulatedAnnealing(maxJobDuration, initialSchedule, maxTime, new Random(seed)),
                new OnlyBetterScheduleExplorer(maxJobDuration, initialSchedule, maxTime, new Random(seed)),
                new GeneticScheduleExplorer(initialSchedule, maxTime),
            };

            // wyznaczanie najlepszych rozwiązań
            var bestSolutions = new Dictionary<string, Schedule>();
            Console.WriteLine();
            foreach (ISolutionExplorer<Schedule> explorer in explorers)
            {
                Schedule bestSolution = explorer.FindBestSolution();
                Console.WriteLine($"Best found schedule by {explorer.Name}" +
                    $" has {bestSolution.MaxTime} execution time units." +
                    $" Elapsed time: {explorer.Duration.TotalMilliseconds}ms");
                bestSolutions.Add(explorer.Name, bestSolution);

            #if DEBUG
                if (!bestSolution.Validate(initialSchedule.SumOfDurations))
                    throw new Exception($"Explorer {explorer} works wrong.");
            #endif
            }

            // inkrementacja "punktów" dla najlepszych rozwiązań
            foreach (var kpv in bestSolutions.Where(kpv => kpv.Value.MaxTime == bestSolutions.Values.Min(sch => sch.MaxTime)))
                scores[kpv.Key]++;
        }
    }
}