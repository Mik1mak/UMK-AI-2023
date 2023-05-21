namespace Schedule
{

    internal class Program
    {
        static void Main(string[] args)
        {
        #if DEBUG
            args = new[] { "4", "80", "input.txt", "150", "100"};
        #endif

            var scoreTable = new Dictionary<string, int>()
            {
                [nameof(ScheduleSimulatedAnnealing)] = 0,
                [nameof(OnlyBetterScheduleExplorer)] = 0,
            };

            for (int i = 0; i < 5; i++)
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
            Random rng;
            int numberOfJobs, maxJobsDuration, maxIter, seed;
            int numberOfProcessors = int.Parse(args.ElementAtOrDefault(0) ?? "4");
            maxIter = int.Parse(args.ElementAtOrDefault(1) ?? "30000");

            if (File.Exists(args.ElementAtOrDefault(2)))
            {
                // odczytywanie z danych wejściowych z pliku
                List<Job> jobs = ScheduleGenerator.ReadJobsFromFile(args[2]);
                inputJobs = jobs;
                numberOfJobs = jobs.Count;
                maxJobsDuration = jobs.Max(job => job.Duration);
                seed = Guid.NewGuid().GetHashCode();
                rng = new Random(seed);
            }
            else
            {
                numberOfJobs = int.Parse(args.ElementAtOrDefault(2) ?? "150");
                maxJobsDuration = int.Parse(args.ElementAtOrDefault(3) ?? "100");
                seed = int.Parse(args.ElementAtOrDefault(4) ?? Guid.NewGuid().GetHashCode().ToString());
                rng = new Random(seed);

                // generowanie danych wejściowych
                inputJobs = ScheduleGenerator
                    .GenerateInputData(numberOfJobs, rng, maxJobsDuration)
                    .OrderBy(x => rng.NextDouble());
            }

            Console.WriteLine("Parameters:");
            Console.WriteLine($"\tnumber of jobs: {numberOfJobs}");
            Console.WriteLine($"\tnumber of processors: {numberOfProcessors}");
            Console.WriteLine($"\tmax jobs duration: {maxJobsDuration}");
            Console.WriteLine($"\tmax iterations: {maxIter}");
            Console.WriteLine($"\tseed: {seed}\n");

            // układanie początkowego harmonogramu
            Schedule initialSchedule = ScheduleGenerator.GenerateSchedule(inputJobs, numberOfProcessors, rng);
            Console.WriteLine($"Initial schedule has {initialSchedule.MaxTime} execution time units");
            Console.WriteLine();

            // definicja obiektów klas poszukujących rozwiązania
            var explorers = new List<ISolutionExplorer<Schedule>>()
            {
                new ScheduleSimulatedAnnealing(maxJobsDuration, initialSchedule, new Random(seed), maxIter),
                new OnlyBetterScheduleExplorer(maxJobsDuration, initialSchedule, new Random(seed), maxIter),
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
            }

            // inkrementacja "punktów" dla najlepszych rozwiązań
            foreach (var kpv in bestSolutions.Where(kpv => kpv.Value.MaxTime == bestSolutions.Values.Min(sch => sch.MaxTime)))
                scores[kpv.Key]++;
        }
    }
}