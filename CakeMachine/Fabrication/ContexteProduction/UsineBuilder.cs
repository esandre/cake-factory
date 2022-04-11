using CakeMachine.Fabrication.Paramètres;
using CakeMachine.Utils;

namespace CakeMachine.Fabrication.ContexteProduction
{
    internal class UsineBuilder : IConfigurationUsine
    {
        private const ushort MultiplicateurTemps = 1000;
        private ParamètresUsine _paramètres;
        private readonly ThreadSafeRandomNumberGenerator _rng = new ();

        public UsineBuilder()
        {
            var paramètresPréparation =
                new ParamètresPréparation(3, 0.05, 
                    MultiplicateurTemps * TimeSpan.FromMilliseconds(5),
                    MultiplicateurTemps * TimeSpan.FromMilliseconds(8));

            var paramètresCuisson = new ParamètresCuisson(5, 0.05, MultiplicateurTemps * TimeSpan.FromMilliseconds(10));
            var paramètresEmballage = new ParamètresEmballage(2, 0.05, MultiplicateurTemps * TimeSpan.FromMilliseconds(2));

            _paramètres = new ParamètresUsine(1, 1, 1, paramètresPréparation, paramètresCuisson, paramètresEmballage);
        }


        public Usine Build() => new (_rng.Fork(), _paramètres);

        /// <inheritdoc />
        public ushort TailleMaxUsine => Usine.TailleMaxUsine;

        /// <inheritdoc />
        public ushort NombrePréparateurs
        {
            get => _paramètres.NombrePréparateurs;
            set => _paramètres = _paramètres with { NombrePréparateurs = value };
        }

        /// <inheritdoc />
        public ushort NombreFours
        {
            get => _paramètres.NombreFours;
            set => _paramètres = _paramètres with { NombreFours = value };
        }

        /// <inheritdoc />
        public ushort NombreEmballeuses
        {
            get => _paramètres.NombreEmballeuses;
            set => _paramètres = _paramètres with { NombreEmballeuses = value };
        }
    }
}
