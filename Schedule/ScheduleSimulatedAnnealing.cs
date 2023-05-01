using SymulowaneWyzarzanie;

namespace Schedule
{
    class ScheduleSimulatedAnnealing : SimulatedAnnealingBase<Schedule>
    {
        private const int INITIAL_TEMPERATURE = 1000;
        private const int FIXED_TEMPERATURE_LENGTH = 1;
        private const double MINIMAL_TEMPERATURE = 0;

        protected readonly Random random;

        protected double Temperature { get; set; }
        protected int Neighbourhood { get; set; }

        public ScheduleSimulatedAnnealing(int neighbourhood,
            Schedule initialSolution, Random? rng = null) : base(initialSolution)
        {
            if (rng == null)
                random = new Random();
            else
                random = rng;
            
            Neighbourhood = neighbourhood;
            Temperature = INITIAL_TEMPERATURE;
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
            Temperature *= 0.95;
        }

        protected override Schedule ExplorationCriterion()
        {
            return ScheduleGenerator.GenerateSchedule(CurrentSolution, random, Neighbourhood);
        }

        protected override bool ImprovesOverBest(Schedule candidateSolution)
        {
            return ObjectiveFunction(candidateSolution) < ObjectiveFunction(BestSolution);
        }

        protected override bool StoppingCriterion()
        {
            return Iteration == 100_000; // || CurrentSolution.MaxTime <= CurrentSolution.SumOfDurations / CurrentSolution.Processors.Length + 10;
        }

        private int l = 0;
        protected override bool TemperatureLength()
        {
            return l++ % FIXED_TEMPERATURE_LENGTH == 0;
        }

        protected override void TemperatureRestart()
        {
            if (Temperature <= MINIMAL_TEMPERATURE)
                Temperature = INITIAL_TEMPERATURE;
        }
    }
}