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
    private readonly TimeSpan maxTime;

    public string Name => nameof(GeneticScheduleExplorer);

    public TimeSpan Duration => stopwatch.Elapsed;

    public Schedule InitialSolution { get; }

    public GeneticScheduleExplorer(Schedule initialSolution, TimeSpan maxTime)
    {
        InitialSolution = initialSolution;
        this.maxTime = maxTime;
    }

    public Schedule FindBestSolution()
    {
        stopwatch.Start();

        var selection = new EliteSelection();
        var crossover = new OrderedCrossover();
        var mutation = new ReverseSequenceMutation();
        var fitness = new ScheduleProblemFitness(InitialSolution);
        var chromosome = new IntsChromosome(Enumerable.Range(0, InitialSolution.Count(j => !j.HasNextJob)).ToArray());
        var population = new Population(50, 70, chromosome);

        var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation)
        {
            Termination = new TimeEvolvingTermination(maxTime),
        };

        ga.Start();
        fitness.Evaluate(ga.BestChromosome);
        stopwatch.Stop();
        return fitness.LastEvaluated!;
    }
}

public class IntsChromosome : ChromosomeBase
{
    private readonly Random random = new Random();
    private readonly int[] inputGenes;

    public IntsChromosome(int[] inputGenes) : base(inputGenes.Length)
    {
        this.inputGenes = inputGenes;
        CreateGenes();
    }

    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(inputGenes[geneIndex]);
    }

    public override IChromosome CreateNew()
    {
        return new IntsChromosome(inputGenes.OrderBy(x => random.NextDouble()).ToArray());
    }
}

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
        int[] genes = chromosome.GetGenes().Select(g => (int)g.Value).ToArray();
        LastEvaluated = ScheduleGenerator.GenerateSchedule(initialSchdule, genes);
        return -LastEvaluated.MaxTime;
    }
}
