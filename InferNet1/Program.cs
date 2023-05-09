using Microsoft.ML.Probabilistic.Models;

namespace InferNet1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // losowanie danych doświadczalnych
            Random random = new Random();
            bool[] data = new bool[100];
            for (int i = 0; i < data.Length; i++)
                data[i] = random.Next(0, 2) == 1;
            //data[i] = random.Next(0, 5) == 1;

            // gęstość prawdopodobieństwa a priori
            Variable<double> beta = Variable.Beta(1, 1);

            // wiązanie danych
            for (int i = 0; i < data.Length; i++)
            {
                Variable<bool> x = Variable.Bernoulli(beta);
                x.ObservedValue = data[i];
            }

            // budowanie silnika Infer.NET
            InferenceEngine engine = new InferenceEngine();

            // gęstość pradopodobieństwa a posteriori
            Console.WriteLine("beta=" + engine.Infer(beta));
        }
    }
}