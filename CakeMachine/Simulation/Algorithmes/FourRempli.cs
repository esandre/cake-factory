using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Utils;

namespace CakeMachine.Simulation.Algorithmes
{
    internal class FourRempli : Algorithme
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
                var plats = usine.StockInfiniPlats.Take(usine.OrganisationUsine.ParamètresCuisson.NombrePlaces);

                var gâteauxCrus = plats
                    .AsParallel()
                    .Select(postePréparation.Préparer)
                    .ToArray();

                var gâteauxCuits = posteCuisson.Cuire(gâteauxCrus);
                var gâteauxEmballés = gâteauxCuits
                    .AsParallel()
                    .Select(posteEmballage.Emballer);

                foreach (var gâteauEmballé in gâteauxEmballés)
                    yield return gâteauEmballé;
            }
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(Usine usine,
            [EnumeratorCancellation] CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var plats = usine.StockInfiniPlats.Take(usine.OrganisationUsine.ParamètresCuisson.NombrePlaces);

                var gâteauxCrus = await Task.WhenAll(plats.Select(postePréparation.PréparerAsync));
                var gâteauxCuits = await posteCuisson.CuireAsync(gâteauxCrus.ToArray());

                var gâteauxEmballés = gâteauxCuits
                    .Select(posteEmballage.EmballerAsync)
                    .EnumerateCompleted();

                await foreach (var gâteauEmballé in gâteauxEmballés.WithCancellation(token))
                    yield return gâteauEmballé;
            }
        }
    }
}
