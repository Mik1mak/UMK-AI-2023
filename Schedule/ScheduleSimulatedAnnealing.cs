using SymulowaneWyzarzanie;

namespace Schedule
{
    class ScheduleSimulatedAnnealing : SimulatedAnnealingBase<Schedule>, ISolutionExplorer<Schedule>
    {
        private const double INITIAL_TEMPERATURE_MULTIPLIER = 4.5;
        private const int FIXED_TEMPERATURE_LENGTH = 30;
        private const double MINIMAL_TEMPERATURE = 0.000000001;

        protected readonly Random random;

        private readonly double initialTemperature;
        private readonly TimeSpan maxExecutionTime;

        protected double Temperature { get; set; }
        protected int Neighbourhood { get; set; }

        public string Name => nameof(ScheduleSimulatedAnnealing);

        public ScheduleSimulatedAnnealing(int maxJobsDuration,
            Schedule initialSolution, TimeSpan maxExecutionTime, Random? rng = null) : base(initialSolution)
        {
            if (rng == null)
                random = new Random();
            else
                random = rng;
            
            // sąsiedztwo determinuje na ile nowo wygenerowany harmonogram będzie mógł się różnić od poprzedniego
            Neighbourhood = 3;

            // temperatura początkowa zależy od długości maksymalnej długości zadania, gdyż wpływa to na możliwą wartość delta
            Temperature = initialTemperature = INITIAL_TEMPERATURE_MULTIPLIER * maxJobsDuration;
            this.maxExecutionTime = maxExecutionTime;
        }

        // funkcja celu - zminimalizowanie czasu wykonania ostatniego zadania w harmonogramie
        private static int ObjectiveFunction(Schedule schedule) => schedule.MaxTime;

        // metoda określająca kiedy proponowane rozwiązanie zostanie zaakceptowane
        protected override bool AcceptanceCriterion(Schedule candidateSolution)
        {
            int delta = ObjectiveFunction(candidateSolution) - ObjectiveFunction(CurrentSolution);

            if (delta <= 0)
                return true;

            return random.NextDouble() <= Math.Exp(-delta / Temperature);
        }

        // metoda określająca jak szybko temepratura będzie obniżana
        protected override void CoolingScheme()
        {
            Temperature *= 0.97;
        }

        // generowanie nowego harmonogramu na podstawi aktualnego rozwiązania
        protected override Schedule ExplorationCriterion()
        {
            return ScheduleGenerator.GenerateSchedule(CurrentSolution, random, Neighbourhood);
        }


        // metoda określająca czy nowo proponowane rozwiązanie jest lepsze od dotychczas najlepszego
        // ponadto zapisuje czas kiedy ostatnio nastąpiło znalezienie najlepszego rozwiązania
        private DateTime lastImprove = DateTime.Now;
        protected override bool ImprovesOverBest(Schedule candidateSolution)
        {
            bool result = ObjectiveFunction(candidateSolution) < ObjectiveFunction(BestSolution);

            if (result)
                lastImprove = DateTime.Now;

            return result;
        }

        protected override bool StoppingCriterion()
        {
            // co 1000 iterację wypisywana jest informacja o postępie
            if (Iteration % 1000 == 0)
                Console.WriteLine($"Iteration: {Iteration};" +
                    $" current solution: {ObjectiveFunction(CurrentSolution)} time units;" +
                    $" elapsed time: {Duration.TotalMilliseconds}ms");

            // sprawdzane jest czy czas wykonywania przekroczył dozwoloną wartość
            return Duration > maxExecutionTime;
        }


        // metoda określająca ile razu ma zostać wygenerowane nowe rozwiązanie dla danej temperatury 
        private int l = 0;
        protected override bool TemperatureLength()
        {
            return l++ % FIXED_TEMPERATURE_LENGTH == 0;
        }

        // metoda określająca kiedy temperatura ma być przywrócona do początkowego stanu
        protected override void TemperatureRestart()
        {
            DateTime now = DateTime.Now;

            if (now - lastImprove >= (maxExecutionTime * 5 / 6))
            {
                lastImprove = now;
                Temperature = initialTemperature;
                Console.WriteLine("Reset Temperature");
            }
        }
    }
}