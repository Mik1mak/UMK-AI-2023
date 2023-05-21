using SymulowaneWyzarzanie;

namespace Schedule
{
    class ScheduleSimulatedAnnealing : SimulatedAnnealingBase<Schedule>, ISolutionExplorer<Schedule>
    {
        private const double INITIAL_TEMPERATURE_MULTIPLIER = 4.5;
        private const int FIXED_TEMPERATURE_LENGTH = 30;
        private const double MINIMAL_TEMPERATURE = 0.000000001;

        protected readonly Random random;

        private readonly int maxIterations;
        private readonly double initialTemperature;

        protected double Temperature { get; set; }
        protected int Neighbourhood { get; set; }

        public string Name => nameof(ScheduleSimulatedAnnealing);

        public ScheduleSimulatedAnnealing(int maxJobsDuration,
            Schedule initialSolution, Random? rng = null, int maxIterations = 10_000) : base(initialSolution)
        {
            if (rng == null)
                random = new Random();
            else
                random = rng;
            
            Neighbourhood = maxJobsDuration / 2;
            this.maxIterations = maxIterations;
            Temperature = initialTemperature = INITIAL_TEMPERATURE_MULTIPLIER * maxJobsDuration;
        }

        private static int ObjectiveFunction(Schedule schedule) => schedule.MaxTime;

        protected override bool AcceptanceCriterion(Schedule candidateSolution)
        {
            int delta = ObjectiveFunction(candidateSolution) - ObjectiveFunction(CurrentSolution);

            if (delta <= 0)
                return true;

            return random.NextDouble() <= Math.Exp(-delta / Temperature);
        }

        protected override void CoolingScheme()
        {
            Temperature *= 0.97;
        }

        protected override Schedule ExplorationCriterion()
        {
            return ScheduleGenerator.GenerateSchedule(CurrentSolution, random, Neighbourhood);
        }

        private int lastImproveIteration = 0;
        protected override bool ImprovesOverBest(Schedule candidateSolution)
        {
            bool result = ObjectiveFunction(candidateSolution) < ObjectiveFunction(BestSolution);

            if (result)
                lastImproveIteration = Iteration;

            return result;
        }

        protected override bool StoppingCriterion()
        {
            if (Iteration % (maxIterations / 80) == 0)
                Console.WriteLine($"Iteration: {Iteration};" +
                    $" current solution: {ObjectiveFunction(CurrentSolution)} time units;" +
                    $" elapsed time: {Duration.TotalMilliseconds}ms");

            return Iteration == maxIterations;
        }

        private int l = 0;
        protected override bool TemperatureLength()
        {
            return l++ % FIXED_TEMPERATURE_LENGTH == 0;
        }

        protected override void TemperatureRestart()
        {
            if (Iteration - lastImproveIteration >= maxIterations * 5 / 6)
            {
                lastImproveIteration = Iteration;
                Temperature = initialTemperature / 5;
                Console.WriteLine("Reset Temperature");
            }

            //if (Temperature <= MINIMAL_TEMPERATURE)
            //{
            //    Temperature = initialTemperature;
            //    Console.WriteLine("Reset Temperature");
            //}
        }
    }
}