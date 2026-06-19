// See https://aka.ms/new-console-template for more information

using StageLIRIS;

//Benchmark.RunBenchmark(50, 64, 1, 3,'S');
Benchmark.GetDiamOfIsGraph("graphs/hog/database/64/graphe_0001.txt", 3, 'S', [1, 0, 0, 0], 3);
Benchmark.GetDiamOfIsGraph("graphs/hog/database/64/graphe_0001.txt", 3, 'J', [1, 0, 0, 0], 3);