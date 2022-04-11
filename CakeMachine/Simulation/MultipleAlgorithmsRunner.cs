using System.Reflection;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Simulation.Algorithmes;

namespace CakeMachine.Simulation
{
    internal class MultipleAlgorithmsRunner
    {
        private readonly IEnumerable<SingleAlgorithmRunner> _runners;

        public MultipleAlgorithmsRunner()
        {
            _runners = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.BaseType == typeof(Algorithme))
                .Select(algorithm => new SingleAlgorithmRunner(algorithm));
        }

        public async Task ProduirePendant(TimeSpan timeSpan)
        {
            var résultats = new List<RésultatSimulation>();

            foreach (var runner in _runners)
            {
                var sync = await runner.ProduirePendantAsync(timeSpan, true);
                var async = await runner.ProduirePendantAsync(timeSpan, false);

                if (async is not null) résultats.Add(async);
                if (sync is not null) résultats.Add(sync);
            }

            foreach (var résultat in résultats.OrderByDescending(
                         r => r.DestinationPlats[DestinationPlat.LivréConforme] - r.DestinationPlats[DestinationPlat.Perdu])) 
                Console.WriteLine(résultat);
        }

        public async Task ProduireNGâteaux(uint nombreGâteaux)
        {
            var résultats = new List<RésultatSimulation>();

            foreach (var runner in _runners)
            {
                var sync = await runner.ProduireNGâteauxAsync(nombreGâteaux, true);
                var async = await runner.ProduireNGâteauxAsync(nombreGâteaux, false);

                if (async is not null) résultats.Add(async);
                if (sync is not null) résultats.Add(sync);
            }

            foreach (var résultat in résultats.OrderBy(r => r.Temps)) Console.WriteLine(résultat);
        }
    }
}
