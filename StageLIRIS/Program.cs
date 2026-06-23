// See https://aka.ms/new-console-template for more information
using StageLIRIS;

int n = 8;
int k = 3;
Console.WriteLine("Plus gros diamètre trouvé pour n="+n+", k="+k+": "+Benchmark.IncrGetBiggestDiameterGraph(n, k, 'J', false, 7));

//Benchmark.RunBenchmark(8, 10, 1, 3,'S');