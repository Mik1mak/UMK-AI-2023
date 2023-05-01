using SymulowaneWyzarzanie;

namespace Schedule
{

    internal class Program
    {
        static void Main(string[] args)
        {
            Random rng = new();
#if DEBUG
            args = new string[] { "150", "4", "100" };
#endif

            #region odczytywanie_parametrow
                int numberOfJobs = int.Parse(args.ElementAtOrDefault(0) ?? "150");
                int numberOfProcessors = int.Parse(args.ElementAtOrDefault(1) ?? "4");
                int maxJobsDuration = int.Parse(args.ElementAtOrDefault(2) ?? "100");
                int seed = int.Parse(args.ElementAtOrDefault(3) ?? rng.Next(int.MinValue, int.MaxValue).ToString());
                rng = new Random(seed);

                Console.WriteLine("Parameters:");
                Console.WriteLine($"\tnumber of jobs: {numberOfJobs}");
                Console.WriteLine($"\tnumber of processors: {numberOfProcessors}");
                Console.WriteLine($"\tmax jobs duration: {maxJobsDuration}");
                Console.WriteLine($"\tseed: {seed}\n");
            #endregion

            // generowanie danych wejściowych
            List<Job> inputJobs = ScheduleGenerator.GenerateInputData(numberOfJobs, rng, maxJobsDuration);

            // układanie początkowego harmonogramu
            Schedule schedule = ScheduleGenerator.GenerateSchedule(inputJobs, numberOfProcessors, rng);
            Console.WriteLine($"Initial schedule has {schedule.MaxTime} execution time units");

            #region wyznaczanie_najlepszego_harmonogramu_losujac_je_z_sasiedztwa
                Schedule bestSolution = schedule;
                rng = new Random(seed);
                int minMaxT = bestSolution.MaxTime;
                for (int i = 0; i < 100_000; i++)
                {
                    schedule = ScheduleGenerator.GenerateSchedule(schedule, rng, 1000);

                    if (schedule.MaxTime < minMaxT)
                    {
                        minMaxT = schedule.MaxTime;
                        bestSolution = schedule;
                    }
                }
                Console.WriteLine($"Best found schedule by random exploration has {bestSolution.MaxTime} execution time units");
            #endregion
            #region wyznaczanie_najlepszego_harmonogramu_korzystajac_z_symulowanego_wyzarzania
                rng = new Random(seed);
                SimulatedAnnealingBase<Schedule> simulatedAnnealing = new ScheduleSimulatedAnnealing(1000, schedule, rng);
                bestSolution = simulatedAnnealing.FindBestSolution();
                Console.WriteLine($"Best found schedule by simulated annealing has {bestSolution.MaxTime} execution time units");
            #endregion
        }
    }
}