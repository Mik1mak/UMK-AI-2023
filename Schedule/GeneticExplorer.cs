using GeneticSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schedule;

class GeneticScheduleExplorer : ISolutionExplorer<Schedule>
{
    private Stopwatch stopwatch = new Stopwatch();
    private readonly TimeSpan maxExecutionTime;

    public string Name => nameof(GeneticScheduleExplorer);

    public TimeSpan Duration => stopwatch.Elapsed;

    public Schedule InitialSolution { get; }

    public GeneticScheduleExplorer(Schedule initialSolution, TimeSpan maxExecutionTime)
    {
        InitialSolution = initialSolution;
        this.maxExecutionTime = maxExecutionTime;
    }

    public Schedule FindBestSolution()
    {
        stopwatch.Start();

        var selection = new EliteSelection(); // strategia selekcji wybierająca w większości chromosomy o największej wartości fitness (funkcji celu)
        var crossover = new OrderedCrossover(); // określa sposób mieszania się genów chromosomów - rodziców
        var mutation = new ReverseSequenceMutation(); // określa sposób mutowania genów - kolejność części sekwencji genów może zotać odwrócony
        var fitness = new ScheduleProblemFitness(InitialSolution);

        // początkowy chromosom stworzony jest z genów o kolejnych wartościach od 0 do x,
        // gdzie x jest ilością zadań od których nie zależą inne zadania
        var chromosome = new IntsChromosome(Enumerable.Range(0, InitialSolution.Count(j => !j.HasNextJob)).ToArray());
        var population = new Population(50, 70, chromosome);

        // algorytm szuka sekwencji wartości int, która pozwala na przekształcenie początkowego harmonogramu na optymalny poprzez zmianę
        // kolejności przekazywania zadań do metody układającej je w harmonogram (ScheduleGenerator.GenerateSchedule)
        var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new TimeEvolvingTermination(maxExecutionTime),
        };
        
        ga.Start();
        fitness.Evaluate(ga.BestChromosome);
        stopwatch.Stop();
        return fitness.LastEvaluated!;
    }
}

// chromosom zapisujący w każdym genie wartość int
public class IntsChromosome : ChromosomeBase
{
    private readonly Random random = new Random();
    private readonly int[] inputGenes;

    public IntsChromosome(int[] inputGenes) : base(inputGenes.Length)
    {
        this.inputGenes = inputGenes;
        CreateGenes(); // wywołanie GenetateGene dla każdego indeksu
    }

    // metoda tworząca gen o zadanym indeksie
    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(inputGenes[geneIndex]);
    }

    // metoda generująca nowy chromosom
    public override IChromosome CreateNew()
    {
        return new IntsChromosome(inputGenes.OrderBy(x => random.NextDouble()).ToArray());
    }
}

// klasa wyznaczająca wartość funkcji celu na podstawie genów chromosomu
public class ScheduleProblemFitness : IFitness
{
    private readonly Schedule initialSchdule;

    public Schedule? LastEvaluated { get; private set; }

    public ScheduleProblemFitness(Schedule initialSchdule)
    {
        this.initialSchdule = initialSchdule;
    }

    public double Evaluate(IChromosome chromosome)
    {
        // odczytywanie tablicy z genów
        int[] genes = chromosome.GetGenes().Select(g => (int)g.Value).ToArray();
        // tworzenie nowego harmonogramu na podstawie początkowego harmonogramu oraz genów
        LastEvaluated = ScheduleGenerator.GenerateSchedule(initialSchdule, genes);
        // zwrócenie wartości funkcji celu
        return -LastEvaluated.MaxTime;
    }
}
