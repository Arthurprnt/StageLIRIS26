// See https://aka.ms/new-console-template for more information
using StageLIRIS;
using System;
using System.Diagnostics;

/*
Pour que les fonctions d'obtention de plus gros diamètres fonctionne,
il faut impérativement installer les outils nauty (geng, listg, genrang)

Pour recompiler le programme:
Vers linux:
    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
Vers windobe:
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
*/

static int ShowManual(string param)
{
    Console.WriteLine("\n");
    Console.WriteLine("Error: Missing required argument '-"+param+" <value>'.");
    Console.WriteLine("Usage: ./StageLIRIS [upgrade -s <is_size> -a <alpha> -b <beta>] [-m <mode> -n <nb_vert> -k <indep_size> [-f <file_path>] [-t <file_type>] [-c] [-p <prev_diam>]]");
    Console.WriteLine("Parameters:");
    Console.WriteLine("upgrade: Execute the reconfig upgrading instead of finding a diameter. If used, must provide a file with the -f and -t flags.");
    Console.WriteLine("-s <is_size>: The size of the following independent sets.");
    Console.WriteLine("-a <alpha> | -b <beta>: The independent sets for the upgrade. Should be used like that: '-a 0 1 2' for the independent set {0, 1, 2}.");
    Console.WriteLine("-m <mode>: Can either be J or S. J is for token jumping and S is for token sliding.");
    Console.WriteLine("-n <nb_vert>: The number of vertices in the graph(s).");
    Console.WriteLine("-k <indep_size>: The size of the independent sets used for the reconfig graph(s).");
    Console.WriteLine("-f <file_path>: If used will run the program only for one graph (the one in the file). If not used, will run the program for all graphs of size n.");
    Console.WriteLine("-t <file_type>: The encoding used for the graph in the given file. Can be dot, hog or gra. The graphs are considered as non-oriented, this means that when adding the edge 0--1, the edge 1--0 will automatically be added too. Exemple of encoding:");
    Console.WriteLine("\tdot:\n\t\tgraph G {\n\t\t  \"0\" -- \"1\";\n\t\t  \"1\" -- \"2\";\n\t\t  \"0\" -- \"2\";\n\t\t}");
    Console.WriteLine("\thog: \n\t\t0: 1 2\n\t\t1: 0 2\n\t\t2: 0 1");
    Console.WriteLine("\tgra: \n\t\tNBVERT: 3\n\t\tNBEDGES: 2\n\n\t\t0 1\n\t\t1 2\n\t\t0 2");
    Console.WriteLine("-c: If used, will fully calculate diameters instead of estimating it. Takes more time than estimating.");
    Console.WriteLine("-p <prev_diam>: Represents the biggest diameter found for n-1 vertices and the same k. Make the calculation for graphs a bit quicker.");
    Console.WriteLine("\nPlease note that you also need to have the following nauty tools installed on your computer: geng, listg, genrang.");
    Console.WriteLine("\n");
    return 1;
}

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

int upIndex = Array.IndexOf(args, "updrade");
if(upIndex == -1) upIndex = Array.IndexOf(args, "u");

int reconfigIndex = Array.IndexOf(args, "reconfig");
if(reconfigIndex == -1) reconfigIndex = Array.IndexOf(args, "r");


if(upIndex != -1 || reconfigIndex != -1)
{
    // Using the upgrading mode
    int fileIndex = Array.IndexOf(args, "-f");
    if (!(fileIndex != -1 && fileIndex + 1 < args.Length)) return ShowManual("f");

    string file = args[fileIndex + 1];
    if (!File.Exists(file)) return ShowManual("f");

    int fileTypeIndex = Array.IndexOf(args, "-t");
    if (!(fileTypeIndex != -1 && fileTypeIndex + 1 < args.Length)) return ShowManual("t");

    string types = args[fileTypeIndex + 1].ToLower();
    string[] possibleTypes = { "dot", "hog", "gra" };
    if (!possibleTypes.Contains(types)) return ShowManual("t");

    int sizeIndex = Array.IndexOf(args, "-s");
    if (!(sizeIndex != -1 && sizeIndex + 1 < args.Length)) return ShowManual("s");

    int isSize = int.Parse(args[sizeIndex+1]);
    int alphaIndex = Array.IndexOf(args, "-a");
    if (!(alphaIndex != -1 && alphaIndex + isSize < args.Length)) return ShowManual("a");

    int betaIndex = Array.IndexOf(args, "-b");
    if (!(betaIndex != -1 && betaIndex + isSize < args.Length)) return ShowManual("b");

    Graph graphe = new Graph(1);
    switch (types)
    {
        case "dot":
            graphe = GraphGenerator.GetDotGraph(file);
            break;
        case "hog":
            graphe = GraphGenerator.GetHogGraph(file);
            break;
        case "gra":
            graphe = GraphGenerator.GetGraGraph(file);
            break;
    }
    IndepSet alpha = new IndepSet(graphe, isSize);
    IndepSet beta = new IndepSet(graphe, isSize);
    for(int i=1; i<=isSize; i++)
    {
        alpha.AddVert(int.Parse(args[alphaIndex+i]));
        beta.AddVert(int.Parse(args[betaIndex+i]));
    }
    Graph upgradedGraph = graphe.UpgradeGraph(alpha, beta);
    Console.WriteLine("Graphe amélioré:");
    Console.WriteLine(upgradedGraph.ToDot());
    Console.WriteLine("Nouveau alpha:");
    alpha.Write();
    Console.WriteLine("Nouveau beta:");
    beta.Write();
} else
{
    int modeIndex = Array.IndexOf(args, "-m");
    if (!(modeIndex != -1 && modeIndex + 1 < args.Length)) return ShowManual("m");

    char mode = char.Parse(args[modeIndex + 1]);
    int nIndex = Array.IndexOf(args, "-n");
    if (!(nIndex != -1 && nIndex + 1 < args.Length)) return ShowManual("n");

    int n = int.Parse(args[nIndex + 1]);
    int kIndex = Array.IndexOf(args, "-k");
    if (!(kIndex != -1 && kIndex + 1 < args.Length)) return ShowManual("k");

    bool calcDiam = false;
    int calcIndex = Array.IndexOf(args, "-c");
    if (calcIndex != -1)
    {
        calcDiam = true;
    }
    int k = int.Parse(args[kIndex + 1]);
    int fileIndex = Array.IndexOf(args, "-f");
    if (fileIndex != -1 && fileIndex + 1 < args.Length)
    {
        string file = args[fileIndex + 1];
        if (!File.Exists(file)) return ShowManual("f");

        int fileTypeIndex = Array.IndexOf(args, "-t");
        if (!(fileTypeIndex != -1 && fileTypeIndex + 1 < args.Length)) return ShowManual("t");

        string types = args[fileTypeIndex + 1].ToLower();
        string[] possibleTypes = { "dot", "hog", "gra" };
        if (!possibleTypes.Contains(types)) return ShowManual("t");

        Graph graphe = new Graph(n);
        switch (types)
        {
            case "dot":
                graphe = GraphGenerator.GetDotGraph(file);
                break;
            case "hog":
                graphe = GraphGenerator.GetHogGraph(file);
                break;
            case "gra":
                graphe = GraphGenerator.GetGraGraph(file);
                break;
        }

        GraphReconfig reconfig = new GraphReconfig(graphe, k, mode);
        reconfig.CalcAllIsRec();
        if(calcDiam) Console.WriteLine("Diamètre calculé du graphe de reconfig pour "+file+$": "+reconfig.Reconfig.GetDiameter());
        else Console.WriteLine("Diamètre estimé du graphe de reconfig pour "+file+$": "+reconfig.Reconfig.EstimateDiameter());
    }
    else
    {
        // Pas de file
        int prevDiam = 0;
        int prevDiamIndex = Array.IndexOf(args, "-p");
        if(prevDiamIndex != -1 && prevDiamIndex + 1 < args.Length)
        {
        prevDiam = int.Parse(args[prevDiamIndex + 1]);
        }
        Benchmark.GetBiggestDiameterGraph(n, k, mode, !calcDiam, prevDiam);
    }
}

stopwatch.Stop();
Console.WriteLine("Programme éxécuté en "+Math.Round((double)stopwatch.Elapsed.TotalMilliseconds/1000, 2)+" secondes.");

return 0;