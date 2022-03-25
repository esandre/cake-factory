using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation.Algorithmes
{
    internal class DeuxParDeux : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsSync => false;

        /// <inheritdoc />
        public override bool SupportsAsync => false;

        /// <inheritdoc />
        public override IEnumerable<GâteauEmballé> Produire(Usine usine, CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var plats = usine.StockInfiniPlats.Take(2).ToArray();

                var gâteauxCrus = plats
                    .Select(postePréparation.Préparer)
                    .AsParallel()
                    .ToArray();

                var gâteauxCuits = posteCuisson.Cuire(gâteauxCrus);

                var gâteauxEmballés = gâteauxCuits
                    .Select(posteEmballage.Emballer)
                    .AsParallel();

                foreach (var gâteauEmballé in gâteauxEmballés)
                    yield return gâteauEmballé;
            }
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(
            Usine usine,
            [EnumeratorCancellation] CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var plats = usine.StockInfiniPlats.Take(2);
                var gâteauxCrus = await Task.WhenAll(plats.Select(postePréparation.PréparerAsync));
                
                var gâteauxCuits = await posteCuisson.CuireAsync(gâteauxCrus);

                var gâteauEmballé1Task = posteEmballage.EmballerAsync(gâteauxCuits.First());
                var gâteauEmballé2Task = posteEmballage.EmballerAsync(gâteauxCuits.Last());

                var terminéeEnPremier = await Task.WhenAny(gâteauEmballé1Task, gâteauEmballé2Task);
                yield return await terminéeEnPremier;

                var terminéeEnDernier =
                    gâteauEmballé1Task == terminéeEnPremier ? gâteauEmballé2Task : gâteauEmballé1Task;

                yield return await terminéeEnDernier;
            }
        }
    }
}
