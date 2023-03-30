using System;

namespace SymulowaneWyzarzanie
{
    public class SimpleSimulatedAnnealing : SimulatedAnnealing
    {
        private const int INITIAL_TEMPERATURE = 100;
        private const int FIXED_TEMPERATURE_LENGTH = 1;
        private const double MINIMAL_TEMPERATURE = -273.15;

        protected readonly Random random = new Random();
        protected readonly Func<double, double> objectiveFunction;
        protected readonly int fixedTemperatureLength;

        public SimpleSimulatedAnnealing(Func<double, double> objectiveFunction,
            double neighbourhood, double initialSolution) : base(neighbourhood, initialSolution)
        {
            this.objectiveFunction = objectiveFunction;

            Temperature = INITIAL_TEMPERATURE;
        }

        protected override bool StoppingCriterion()
        {
            return Iteration == 10_000 ;
        }

        protected override double ExplorationCriterion()
        {
            double temp = random.NextDouble() * Neighbourhood;

            if (random.Next(0, 2) == 1)
                return CurrentSolution + temp;
            
            return CurrentSolution - temp;
        }

        protected override bool AcceptanceCriterion(double candidateSolution)
        {
            double delta = objectiveFunction(candidateSolution) - objectiveFunction(CurrentSolution);

            if (delta <= 0)
                return true;

            return random.NextDouble() <= Math.Exp(-delta / Temperature);
        }

        protected override bool ImprovesOverBest(double candidateSolution)
        {
            return objectiveFunction(candidateSolution) < objectiveFunction(BestSolution);
        }

        private int l = 0;
        protected override bool TemperatureLength()
        {
            return l++ % FIXED_TEMPERATURE_LENGTH == 0;
        }

        protected override void CoolingScheme()
        {
            Temperature *= 0.95;
        }

        protected override void TemperatureRestart()
        {
            if(Temperature <= MINIMAL_TEMPERATURE)
                Temperature = INITIAL_TEMPERATURE;
        }
    }
}
