using System.Runtime.CompilerServices;
using CakeMachine.Fabrication.ContexteProduction;
using CakeMachine.Fabrication.Elements;

namespace CakeMachine.Simulation.Algorithmes
{
    internal class AntiRebut : Algorithme
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
                GâteauCru? gâteauCru = default;
                do
                {
                    if(gâteauCru != default) usine.MettreAuRebut(gâteauCru);
                    gâteauCru = postePréparation.Préparer(usine.StockInfiniPlats.First());
                }
                while (!gâteauCru.EstConforme);
                
                var gâteauCuit = posteCuisson.Cuire(gâteauCru).Single();
                if(!gâteauCuit.EstConforme)
                {
                    usine.MettreAuRebut(gâteauCuit);
                    continue;
                }

                var gâteauEmballé = posteEmballage.Emballer(gâteauCuit);
                if (!gâteauEmballé.EstConforme)
                {
                    usine.MettreAuRebut(gâteauEmballé);
                    continue;
                }

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
                GâteauCru? gâteauCru = default;
                do
                {
                    if (gâteauCru != default) usine.MettreAuRebut(gâteauCru);
                    gâteauCru = await postePréparation.PréparerAsync(usine.StockInfiniPlats.First());
                }
                while (!gâteauCru.EstConforme);

                var gâteauCuit = (await posteCuisson.CuireAsync(gâteauCru)).Single();
                if (!gâteauCuit.EstConforme)
                {
                    usine.MettreAuRebut(gâteauCuit);
                    continue;
                }

                var gâteauEmballé = await posteEmballage.EmballerAsync(gâteauCuit);
                if (!gâteauEmballé.EstConforme)
                {
                    usine.MettreAuRebut(gâteauEmballé);
                    continue;
                }

                yield return gâteauEmballé;
            }
        }
    }
}
