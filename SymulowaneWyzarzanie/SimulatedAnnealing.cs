namespace SymulowaneWyzarzanie
{
    public abstract class SimulatedAnnealing
    {
        protected double CurrentSolution { get; private set; }
        protected double BestSolution { get; private set; }
        protected double Temperature { get; set; }

        public int Iteration { get; private set; } = 0;
        public double InitialSolution { get; }
        public double Neighbourhood { get; }

        protected SimulatedAnnealing(double neighbourhood, double initialSolution)
        {
            Neighbourhood = neighbourhood;
            BestSolution = CurrentSolution = InitialSolution = initialSolution;
        }

        public double FindBestSolution()
        {
            while(!StoppingCriterion())
            {
                double candidateSolution = ExplorationCriterion();

                if(AcceptanceCriterion(candidateSolution))
                {
                    CurrentSolution = candidateSolution;

                    if(ImprovesOverBest(candidateSolution))
                        BestSolution = candidateSolution;
                }
                if(TemperatureLength())
                {
                    CoolingScheme();
                }
                TemperatureRestart();

                Iteration++;
            }
            return BestSolution;
        }

        protected abstract bool StoppingCriterion();
        protected abstract double ExplorationCriterion();
        protected abstract bool AcceptanceCriterion(double candidateSolution);
        protected abstract bool ImprovesOverBest(double candidateSolution);
        protected abstract bool TemperatureLength();
        protected abstract void CoolingScheme();
        protected abstract void TemperatureRestart();
    }
}
