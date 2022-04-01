using System;
using System.Threading.Tasks;
using CakeMachine.Simulation;
using CakeMachine.Simulation.Algorithmes;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace CakeMachine.Test
{
    public class TestAlgorithme
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TestAlgorithme(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Theory]
        [InlineData(typeof(SingleThread), true)]
        [InlineData(typeof(SingleThread), false)]
        [InlineData(typeof(DeuxParDeux), true)]
        [InlineData(typeof(DeuxParDeux), false)]
        [InlineData(typeof(DixParDix), true)]
        [InlineData(typeof(DixParDix), false)]
        [InlineData(typeof(FourRempli), true)]
        [InlineData(typeof(FourRempli), false)]
        [InlineData(typeof(FourRempliSansRebut), true)]
        [InlineData(typeof(FourRempliSansRebut), false)]
        [InlineData(typeof(AntiRebut), true)]
        [InlineData(typeof(AntiRebut), false)]
        [InlineData(typeof(Optimisée1Poste), false)]
        [InlineData(typeof(UsineEtalon), false)]
        public async Task TestAlgoOptimisé(Type algorithme, bool sync)
        {
            var runner = new SingleAlgorithmRunner(algorithme);
            var result = await runner.ProduirePendantAsync(TimeSpan.FromSeconds(5), sync);
            
            if(result is null) throw new XunitException("No algorithm");
            _testOutputHelper.WriteLine(result.ToString());
        }
    }
}