namespace SymulowaneWyzarzanie
{
    public abstract class SimulatedAnnealingBase<T>
    {
        protected T CurrentSolution { get; private set; }
        protected T BestSolution { get; private set; }

        public int Iteration { get; private set; } = 0;
        public T InitialSolution { get; }

        protected SimulatedAnnealingBase(T initialSolution)
        {
            BestSolution = CurrentSolution = InitialSolution = initialSolution;
        }

        public T FindBestSolution()
        {
            while(!StoppingCriterion())
            {
                T candidateSolution = ExplorationCriterion();

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
        protected abstract T ExplorationCriterion();
        protected abstract bool AcceptanceCriterion(T candidateSolution);
        protected abstract bool ImprovesOverBest(T candidateSolution);
        protected abstract bool TemperatureLength();
        protected abstract void CoolingScheme();
        protected abstract void TemperatureRestart();
    }
}
