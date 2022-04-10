using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;
using CakeMachine.Fabrication.Op�rations;
using CakeMachine.Utils;

namespace CakeMachine.Simulation.Algorithmes
{
    internal class AxelBorget : Algorithme
    {
        /// <inheritdoc />
        public override bool SupportsAsync => true;

        /// <inheritdoc />
        public override void ConfigurerUsine(IConfigurationUsine builder)
        {
            builder.NombrePr�parateurs = 10;
            builder.NombreFours = 15;
            builder.NombreEmballeuses = 10;
        }

        private class OrdreProduction
        {
            private readonly Usine _usine;
            private readonly CancellationToken _token;
            private readonly Ring<Emballage> _emballeuses;
            private readonly Ring<Cuisson> _fours;
            private readonly Ring<Pr�paration> _pr�paratrices;

            public OrdreProduction(Usine usine, CancellationToken token)
            {
                _usine = usine;
                _token = token;
                _emballeuses = new Ring<Emballage>(usine.Emballeuses);
                _fours = new Ring<Cuisson>(usine.Fours);
                _pr�paratrices = new Ring<Pr�paration>(usine.Pr�parateurs);
            }

            public async IAsyncEnumerable<G�teauEmball�> ProduireAsync()
            {
                while (!_token.IsCancellationRequested)
                {
                    var g�teauxCuits = ProduireEtCuireParBains(_usine.OrganisationUsine.Param�tresCuisson.NombrePlaces, 6);

                    var t�chesEmballage = new List<Task<G�teauEmball�>>(
                        _usine.OrganisationUsine.Param�tresCuisson.NombrePlaces * _usine.OrganisationUsine.NombreFours
                    );

                    await foreach (var g�teauCuit in g�teauxCuits.WithCancellation(_token))
                        t�chesEmballage.Add(_emballeuses.Next.EmballerAsync(g�teauCuit));

                    await foreach (var g�teauEmball� in t�chesEmballage.EnumerateCompleted().WithCancellation(_token))
                    {
                        if (g�teauEmball�.EstConforme) yield return g�teauEmball�;
                        else _usine.MettreAuRebut(g�teauEmball�);
                    }
                }
            }

            private async IAsyncEnumerable<G�teauCuit> ProduireEtCuireParBains(
                ushort nombrePlacesParFour,
                ushort nombreBains)
            {
                var g�teauxCrus = Pr�parerConformesParBainAsync(nombrePlacesParFour, nombreBains);

                var tachesCuisson = new List<Task<G�teauCuit[]>>();
                await foreach (var bainG�teauxCrus in g�teauxCrus.WithCancellation(_token))
                    tachesCuisson.Add(_fours.Next.CuireAsync(bainG�teauxCrus));

                await foreach (var bainG�teauxCuits in tachesCuisson.EnumerateCompleted().WithCancellation(_token))
                    foreach (var g�teauCuit in bainG�teauxCuits)
                    {
                        if (g�teauCuit.EstConforme) yield return g�teauCuit;
                        else _usine.MettreAuRebut(g�teauCuit);
                    }
            }

            private async IAsyncEnumerable<G�teauCru[]> Pr�parerConformesParBainAsync(
                ushort g�teauxParBain, ushort bains)
            {
                var totalAPr�parer = (ushort)(bains * g�teauxParBain);
                var g�teauxConformes = 0;
                var g�teauxRat�s = 0;
                var g�teauxPr�ts = new ConcurrentBag<G�teauCru>();

                async Task TakeNextAndSpawnChild(uint depth)
                {
                    _token.ThrowIfCancellationRequested();

                    while (depth >= totalAPr�parer + g�teauxRat�s)
                    {
                        _token.ThrowIfCancellationRequested();
                        if (g�teauxConformes == totalAPr�parer) return;
                        await Task.Delay(_usine.OrganisationUsine.Param�tresPr�paration.TempsMin / 2, _token);
                    }

                    if (g�teauxConformes == totalAPr�parer) return;

                    var child = TakeNextAndSpawnChild(depth + 1);
                    await Pr�parerPlat(_pr�paratrices.Next);
                    await child;
                }

                async Task Pr�parerPlat(Pr�paration pr�paratrice)
                {
                    _token.ThrowIfCancellationRequested();

                    var gateau = await pr�paratrice.Pr�parerAsync(_usine.StockInfiniPlats.First());
                    if (gateau.EstConforme && gateau.PlatSousJacent.EstConforme)
                    {
                        g�teauxPr�ts!.Add(gateau);
                        Interlocked.Increment(ref g�teauxConformes);
                    }
                    else Interlocked.Increment(ref g�teauxRat�s);
                }

                var spawner = TakeNextAndSpawnChild(0);

                var buffer = new List<G�teauCru>(g�teauxParBain);
                for (var i = 0; i < totalAPr�parer; i++)
                {
                    _token.ThrowIfCancellationRequested();

                    G�teauCru g�teauPr�t;
                    while (!g�teauxPr�ts.TryTake(out g�teauPr�t!))
                    {
                        _token.ThrowIfCancellationRequested();
                        await Task.Delay(_usine.OrganisationUsine.Param�tresPr�paration.TempsMin / 2, _token);
                    }

                    buffer.Add(g�teauPr�t);

                    if (buffer.Count != g�teauxParBain) continue;

                    yield return buffer.ToArray();

                    buffer.Clear();
                }

                await spawner;
            }
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<G�teauEmball�> ProduireAsync(
            Usine usine,
            [EnumeratorCancellation] CancellationToken token)
        {
            var ligne = new OrdreProduction(usine, token);
            await foreach (var g�teauEmball� in ligne.ProduireAsync().WithCancellation(token))
                yield return g�teauEmball�;
        }
    }
}