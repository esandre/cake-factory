using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Opérations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation.Algorithmes
{
    internal class Optimisée1Poste : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsAsync => false;

        /// <inheritdoc />
        public override async IAsyncEnumerable<GâteauEmballé> ProduireAsync(Usine usine,
            [EnumeratorCancellation] CancellationToken token)
        {
            var postePréparation = usine.Préparateurs.Single();
            var posteCuisson = usine.Fours.Single();
            var posteEmballage = usine.Emballeuses.Single();

            while (!token.IsCancellationRequested)
            {
                var gâteauxCuits = ProduireEtCuireParBains(
                    usine,
                    postePréparation,
                    posteCuisson,
                    usine.OrganisationUsine.ParamètresCuisson.NombrePlaces, 
                    6,
                    token);

                var tâchesEmballage = new List<Task<GâteauEmballé>>(usine.OrganisationUsine.ParamètresCuisson.NombrePlaces * 6);
                await foreach(var gâteauCuit in gâteauxCuits.WithCancellation(token))
                    tâchesEmballage.Add(posteEmballage.EmballerAsync(gâteauCuit));
                
                await foreach (var gâteauEmballé in tâchesEmballage.EnumerateCompleted().WithCancellation(token))
                    yield return gâteauEmballé;
            }
        }

        private static async IAsyncEnumerable<GâteauCuit> ProduireEtCuireParBains(
            Usine usine,
            Préparation postePréparation,
            Cuisson posteCuisson,
            ushort nombrePlacesParFour,
            ushort nombreBains,
            [EnumeratorCancellation] CancellationToken token)
        {
            var gâteauxCrus = PréparerConformesParBainAsync(usine, postePréparation, nombrePlacesParFour, nombreBains, token);

            await foreach (var bainGâteauxCrus in gâteauxCrus.WithCancellation(token))
            {
                var gâteauxCuits = await posteCuisson.CuireAsync(bainGâteauxCrus);
                foreach (var gâteauCuit in gâteauxCuits.Where(gâteau => gâteau.EstConforme))
                    yield return gâteauCuit;
            }
        }
        
        private static async IAsyncEnumerable<GâteauCru[]> PréparerConformesParBainAsync(
            Usine usine,
            Préparation postePréparation, 
            ushort gâteauxParBain, 
            ushort bains, 
            [EnumeratorCancellation] CancellationToken token)
        {
            var productionGâteauxConformes =
                PréparerConformes(usine, postePréparation, (ushort)(bains * gâteauxParBain), token);

            var buffer = new List<GâteauCru>(gâteauxParBain);
            await foreach(var gâteauCru in productionGâteauxConformes.WithCancellation(token))
            {
                buffer.Add(gâteauCru);

                if (buffer.Count != gâteauxParBain) continue;

                yield return buffer.ToArray();
                buffer.Clear();
            }
        }

        private static async IAsyncEnumerable<GâteauCru> PréparerConformes(
            Usine usine,
            Préparation postePréparation, 
            ushort total, 
            [EnumeratorCancellation] CancellationToken token)
        {
            var gâteauxConformes = 0;

            do
            {
                var plats = usine.StockInfiniPlats.Take(total - gâteauxConformes);

                var gâteauxCrus = plats.Select(postePréparation.PréparerAsync).EnumerateCompleted();
                
                await foreach(var gâteauCru in gâteauxCrus.WithCancellation(token))
                {
                    if (!gâteauCru.EstConforme) continue;
                    gâteauxConformes++;
                    yield return gâteauCru;
                }

            } while (gâteauxConformes < total);
        }
    }
}
