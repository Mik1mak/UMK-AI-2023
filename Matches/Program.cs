using System;
using System.Linq;
using Microsoft.ML.Probabilistic;
using Microsoft.ML.Probabilistic.Distributions;
using Microsoft.ML.Probabilistic.Models;

namespace myApp
{
    using Range = Microsoft.ML.Probabilistic.Models.Range;

    class Program
    {
        // https://dotnet.github.io/infer/userguide/Running%20inference.html
        // https://dotnet.github.io/infer/userguide/Learning%20a%20Gaussian%20tutorial.html

        static void Main(string[] args)
        {
            // The winner and loser in each of 6 samples games
            var winnerData = new[] { 0, 0, 0, 1, 3, 4 };
            var loserData = new[] { 1, 3, 4, 2, 1, 2 };

            // Define the statistical model as a probabilistic program
            var game = new Range(winnerData.Length).Named("gameRange");
            var player = new Range(winnerData.Concat(loserData).Max() + 1).Named("PlayerRange");
            var playerSkills = Variable.Array<double>(player).Named("playerSkills");

            const double APRIORI_MEAN = 6;
            const double APRIORI_VARIANCE = 15;
            playerSkills[player] = Variable.GaussianFromMeanAndVariance(APRIORI_MEAN, APRIORI_VARIANCE).ForEach(player);

            var winners = Variable.Array<int>(game).Named("winners");
            var losers = Variable.Array<int>(game).Named("losers");

            using (Variable.ForEach(game))
            {
                // The player performance is a noisy version of their skill
                var winnerPerformance = Variable.GaussianFromMeanAndVariance(playerSkills[winners[game]], 1.0).Named("winnerPerformance");
                var loserPerformance = Variable.GaussianFromMeanAndVariance(playerSkills[losers[game]], 1.0).Named("loserPerformance");

                // The winner performed better in this game
                Variable.ConstrainTrue(winnerPerformance > loserPerformance);
            }

            // Attach the data to the model
            winners.ObservedValue = winnerData;
            losers.ObservedValue = loserData;

            // Run inference
            var inferenceEngine = new InferenceEngine();
            //inferenceEngine.ShowFactorGraph = true;
            var inferredSkills = inferenceEngine.Infer<Gaussian[]>(playerSkills);

            // The inferred skills are uncertain, which is captured in their variance
            var orderedPlayerSkills = inferredSkills
                .Select((s, i) => new { Player = i, Skill = s })
                .OrderByDescending(ps => ps.Skill.GetMean());;


            foreach (var playerSkill in orderedPlayerSkills)
            {
                Console.WriteLine($"Player {playerSkill.Player} a priori skill: {new Gaussian(APRIORI_MEAN, APRIORI_VARIANCE)}");

                for (int i = 0; i < winnerData.Length; i++)
                {
                    if (winnerData[i] == playerSkill.Player)
                        Console.WriteLine($"Player {playerSkill.Player} won match against Player {loserData[i]}");
                    else if (loserData[i] == playerSkill.Player)
                        Console.WriteLine($"Player {playerSkill.Player} lost match against {winnerData[i]} ");
                }
                Console.WriteLine($"Player {playerSkill.Player} a posteriori skill: {playerSkill.Skill}");
                Console.WriteLine();
            }
        }
    }
}
