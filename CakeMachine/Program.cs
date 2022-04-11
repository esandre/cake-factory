using System.Runtime.CompilerServices;
using CakeMachine.Simulation;

[assembly:InternalsVisibleTo("CakeMachine.Test")]

const int nombreGâteaux = 1487;
var tempsPassé = TimeSpan.FromSeconds(318);
var runner = new MultipleAlgorithmsRunner();

Console.WriteLine($"Produire {nombreGâteaux} gâteaux.");
Console.WriteLine();
await runner.ProduireNGâteaux(nombreGâteaux);

Console.WriteLine($"Produire des gateaux pendant {tempsPassé.TotalSeconds:F1} secondes.");
Console.WriteLine();
await runner.ProduirePendant(tempsPassé);