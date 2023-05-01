using System;

namespace SymulowaneWyzarzanie
{
    class Program
    {
        static void Main(string[] args)
        {
            SimulatedAnnealingBase<double> problem = new SimpleSimulatedAnnealing(x => x * x, 10, 5);
            Console.WriteLine($"Najlepsze rozwiązanie f(x)=x^2: {problem.FindBestSolution()} znalezione w {problem.Iteration} iteracji");
        }
    }
}
