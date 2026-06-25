// See https://aka.ms/new-console-template for more information
using StageLIRIS;

/*
Pour que les fonctions d'obtention de plus gros diamètres fonctionne,
il faut impérativement installer les outils nauty (geng, listg, genrang)
*/

int n = 10;
int k = 3;
Console.WriteLine("Plus gros diamètre trouvé pour n="+n+", k="+k+": "+Benchmark.GetBiggestDiameterGraph(n, k, 'J'));
Console.WriteLine("Plus gros diamètre estimé pour n="+n+", k="+k+": "+Benchmark.EstimateBiggestDiameterGraph(n, k, 'J'));

//Benchmark.RunBenchmark(8, 10, 1, 3,'S');
