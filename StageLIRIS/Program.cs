// See https://aka.ms/new-console-template for more information
using StageLIRIS;

int n = 10;
int k = 5;
Console.WriteLine("Plus gros diamètre trouvé pour n="+n+", k="+k+": "+Benchmark.IncrGetBiggestDiameterGraphFromFile(n, k, 'J'));

//Benchmark.RunBenchmark(8, 10, 1, 3,'S');
