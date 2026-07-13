// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using StageLIRIS;

/*
Pour que les fonctions d'obtention de plus gros diamètres fonctionne,
il faut impérativement installer les outils nauty (geng, listg, genrang)

Pour recompiler le programme:
Vers linux:
    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
Vers windobe:
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
*/

static int ShowUsages()
{
    Console.WriteLine(
        "Default usage: ./StageLIRIS -m <mode> -s <set_type> -n <nb_vert> -k <set_size> [-f <file_path>] [-t <file_type>] [-e <graph_nb>] [-c] [-p <prev_diam>]"
    );
    Console.WriteLine(
        "Reconfig usage: ./StageLIRIS reconfig -m <mode> -s <set_type> -k <set_size> -f <file_path> -t <file_type>"
    );
    Console.WriteLine(
        "Local search usage: ./StageLIRIS search -m <mode> -s <set_type> -d <search_depth> -k <set_size> -f <file_path> -t <file_type>"
    );
    Console.WriteLine(
        "Upgrading usage: ./StageLIRIS upgrade -k <indep_size> -a <alpha> -b <beta> -f <file_path> -t <file_type>"
    );
    Console.WriteLine("Run './StageLIRIS -h' for help.");
    Console.WriteLine("\n");
    return 1;
}

static int ShowError(string param)
{
    Console.WriteLine("\n");
    Console.WriteLine("Error: Missing required argument '-" + param + " <value>'.");
    return ShowUsages();
}

static int ShowErrorMultiParam()
{
    Console.WriteLine("\n");
    Console.WriteLine("Error: You're using too much arguments.");
    return ShowUsages();
}

static int ShowManual()
{
    Console.WriteLine("\n");
    Console.WriteLine(
        "Default usage: ./StageLIRIS -m <mode> -s <set_type> -n <nb_vert> -k <set_size> [-f <file_path>] [-t <file_type>] [-e <graph_nb>] [-c] [-p <prev_diam>]"
    );
    Console.WriteLine(
        "Reconfig usage: ./StageLIRIS reconfig -m <mode> -s <set_type> -k <set_size> -f <file_path> -t <file_type>"
    );
    Console.WriteLine(
        "Local search usage: ./StageLIRIS search -m <mode> -s <set_type> -d <search_depth> -k <set_size> -f <file_path> -t <file_type> [-c]"
    );
    Console.WriteLine(
        "Upgrading usage: ./StageLIRIS upgrade -k <indep_size> -a <alpha> -b <beta> -f <file_path> -t <file_type>"
    );
    Console.WriteLine("Parameters:");
    Console.WriteLine(
        "reconfig: Reconfig the given graph and provide the dot for reconfigured graph."
    );
    Console.WriteLine(
        "search: Will run a local search on the given graph to try finding graph with a bigger diameter for the reconfig graph."
    );
    Console.WriteLine(
        "upgrade: Execute the reconfig upgrading instead of finding a diameter. Only for independent sets."
    );
    Console.WriteLine(
        "-a <alpha> | -b <beta>: The independent sets for the upgrade. Should be used like that: '-a 0 1 2' for the independent set {0, 1, 2}."
    );
    Console.WriteLine(
        "-c: If used, will fully calculate diameters instead of estimating it. Takes more time than estimating."
    );
    Console.WriteLine("-d <depth>: The number of time it'll iterate the local search algorithm.");
    Console.WriteLine(
        "-e: To use only if not generating a graph from a file. If used, it will only generate a fixed number of graphs and keep the ones respecting the given criterias."
    );
    Console.WriteLine(
        "-f <file_path>: If used will run the program only for one graph (the one in the file). If not used, will run the program for all graphs of size n."
    );
    Console.WriteLine(
        "-k <indep_size>: The size of the independent sets used for the reconfig graph(s)."
    );
    Console.WriteLine(
        "-m <mode>: Can either be J or S. J is for token jumping and S is for token sliding."
    );
    Console.WriteLine("-n <nb_vert>: The number of vertices in the graph(s).");
    Console.WriteLine(
        "-p <prev_diam>: Represents the biggest diameter found for n-1 vertices and the same k. Make the calculation for graphs a bit quicker."
    );
    Console.WriteLine(
        "-s <set_type>: Can either be I or D. I is for independent sets and D is for dominant sets."
    );
    Console.WriteLine(
        "-t <file_type>: The encoding used for the graph in the given file. Can be dot, hog or gra. The graphs are considered as non-oriented, this means that when adding the edge 0--1, the edge 1--0 will automatically be added too. Exemple of encoding:"
    );
    Console.WriteLine(
        "\tdot:\n\t\tgraph G {\n\t\t  \"0\" -- \"1\";\n\t\t  \"1\" -- \"2\";\n\t\t  \"0\" -- \"2\";\n\t\t}"
    );
    Console.WriteLine("\thog: \n\t\t0: 1 2\n\t\t1: 0 2\n\t\t2: 0 1");
    Console.WriteLine("\tgra: \n\t\tNBVERT: 3\n\t\tNBEDGES: 2\n\n\t\t0 1\n\t\t1 2\n\t\t0 2");
    Console.WriteLine(
        "File saving: The graphs calculated by the tool will be saved here: graphs_found/[estimated/]<set_type>/<parameters>/... There's also a file named infos.txt for each subfolder which summarizes the graphs present in the folder."
    );
    Console.WriteLine(
        "\nPlease note that you also need to have the following nauty tools installed on your computer: geng, listg, genrang."
    );
    Console.WriteLine("\n");
    return 1;
}

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

int helpIndex = Array.IndexOf(args, "-h");
if (helpIndex != -1)
    return ShowManual();

int upIndex = Array.IndexOf(args, "updrade");
if (upIndex == -1)
    upIndex = Array.IndexOf(args, "u");

int reconfigIndex = Array.IndexOf(args, "reconfig");
if (reconfigIndex == -1)
    reconfigIndex = Array.IndexOf(args, "r");

int searchIndex = Array.IndexOf(args, "search");
if (searchIndex == -1)
    searchIndex = Array.IndexOf(args, "s");

if (upIndex != -1 || reconfigIndex != -1 || searchIndex != -1)
{
    int nbUsed = 0;
    if (upIndex != -1)
        nbUsed++;
    if (reconfigIndex != -1)
        nbUsed++;
    if (searchIndex != -1)
        nbUsed++;

    if (nbUsed > 1)
        return ShowErrorMultiParam();

    int fileIndex = Array.IndexOf(args, "-f");
    if (!(fileIndex != -1 && fileIndex + 1 < args.Length))
        return ShowError("f");

    string file = args[fileIndex + 1];
    if (!File.Exists(file))
        return ShowError("f");

    int fileTypeIndex = Array.IndexOf(args, "-t");
    if (!(fileTypeIndex != -1 && fileTypeIndex + 1 < args.Length))
        return ShowError("t");

    string types = args[fileTypeIndex + 1].ToLower();
    string[] possibleTypes = { "dot", "hog", "gra" };
    if (!possibleTypes.Contains(types))
        return ShowError("t");

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

    if (upIndex != -1)
    {
        // Using the upgrading mode
        int sizeIndex = Array.IndexOf(args, "-s");
        if (!(sizeIndex != -1 && sizeIndex + 1 < args.Length))
            return ShowError("s");

        int isSize = int.Parse(args[sizeIndex + 1]);
        int alphaIndex = Array.IndexOf(args, "-a");
        if (!(alphaIndex != -1 && alphaIndex + isSize < args.Length))
            return ShowError("a");

        int betaIndex = Array.IndexOf(args, "-b");
        if (!(betaIndex != -1 && betaIndex + isSize < args.Length))
            return ShowError("b");

        IndepSet alpha = new IndepSet(graphe, isSize);
        IndepSet beta = new IndepSet(graphe, isSize);
        for (int i = 1; i <= isSize; i++)
        {
            alpha.AddVert(int.Parse(args[alphaIndex + i]));
            beta.AddVert(int.Parse(args[betaIndex + i]));
        }
        Graph upgradedGraph = graphe.UpgradeGraph(alpha, beta);
        Console.WriteLine("Graphe amélioré:");
        Console.WriteLine(upgradedGraph.ToDot());
        Console.WriteLine("Nouveau alpha:");
        alpha.Write();
        Console.WriteLine("Nouveau beta:");
        beta.Write();
    }
    else
    {
        int kIndex = Array.IndexOf(args, "-k");
        if (!(kIndex != -1 && kIndex + 1 < args.Length))
            return ShowError("k");

        int k = int.Parse(args[kIndex + 1]);
        int modeIndex = Array.IndexOf(args, "-m");
        if (!(modeIndex != -1 && modeIndex + 1 < args.Length))
            return ShowError("m");

        int setTypeIndex = Array.IndexOf(args, "-s");
        if (!(setTypeIndex != -1 && setTypeIndex + 1 < args.Length))
            return ShowError("s");
        char setType = char.Parse(args[setTypeIndex + 1]);

        char mode = char.Parse(args[modeIndex + 1]);

        if (reconfigIndex != -1)
        {
            // Using the reconfiguration mode
            GraphReconfig graphReconfig = new GraphReconfig(graphe, k, mode, setType);
            graphReconfig.CalcAllSetsIte();
            Console.WriteLine("Graphe reconfiguré du graphe " + file + ":");
            Console.WriteLine(graphReconfig.ToDot());
        }
        else
        {
            // Using the local search mode
            int depthIndex = Array.IndexOf(args, "-d");
            if (!(depthIndex != -1 && depthIndex + 1 < args.Length))
                return ShowError("d");

            int d = int.Parse(args[depthIndex + 1]);

            bool calcDiam = false;
            int calcIndex = Array.IndexOf(args, "-c");
            if (calcIndex != -1)
            {
                calcDiam = true;
            }

            var resDeepSearch = graphe.DeepSearchLocally(d, k, mode, setType, !calcDiam);
            Console.WriteLine(
                "Plus gros diamètre de reconfig trouvé en recherche locale: "
                    + resDeepSearch.diameter
            );
            Console.WriteLine("Liste des graphs dont la reconfig a ce diamètre:");
            for (int i = 0; i < resDeepSearch.graphs.Count(); i++)
            {
                Console.WriteLine();
                Console.WriteLine(resDeepSearch.graphs[i].ToDot());
            }
        }
    }
}
else
{
    int modeIndex = Array.IndexOf(args, "-m");
    if (!(modeIndex != -1 && modeIndex + 1 < args.Length))
        return ShowError("m");

    char mode = char.Parse(args[modeIndex + 1]);
    int nIndex = Array.IndexOf(args, "-n");
    if (!(nIndex != -1 && nIndex + 1 < args.Length))
        return ShowError("n");

    int n = int.Parse(args[nIndex + 1]);
    int kIndex = Array.IndexOf(args, "-k");
    if (!(kIndex != -1 && kIndex + 1 < args.Length))
        return ShowError("k");

    int setTypeIndex = Array.IndexOf(args, "-s");
    if (!(setTypeIndex != -1 && setTypeIndex + 1 < args.Length))
        return ShowError("s");
    char setType = char.Parse(args[setTypeIndex + 1]);

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
        if (!File.Exists(file))
            return ShowError("f");

        int fileTypeIndex = Array.IndexOf(args, "-t");
        if (!(fileTypeIndex != -1 && fileTypeIndex + 1 < args.Length))
            return ShowError("t");

        string types = args[fileTypeIndex + 1].ToLower();
        string[] possibleTypes = { "dot", "hog", "gra" };
        if (!possibleTypes.Contains(types))
            return ShowError("t");

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

        GraphReconfig reconfig = new GraphReconfig(graphe, k, mode, setType);
        reconfig.CalcAllSetsIte();
        if (calcDiam)
            Console.WriteLine(
                "Diamètre calculé du graphe de reconfig pour "
                    + file
                    + $": "
                    + reconfig.Reconfig.GetDiameter()
            );
        else
            Console.WriteLine(
                "Diamètre estimé du graphe de reconfig pour "
                    + file
                    + $": "
                    + reconfig.Reconfig.EstimateDiameter()
            );
    }
    else
    {
        // Pas de file
        int estimateIndex = Array.IndexOf(args, "-e");
        int prevDiam = 0;
        int prevDiamIndex = Array.IndexOf(args, "-p");
        if (prevDiamIndex != -1 && prevDiamIndex + 1 < args.Length)
        {
            prevDiam = int.Parse(args[prevDiamIndex + 1]);
        }

        if (estimateIndex != -1 && estimateIndex + 1 < args.Length)
        {
            // On utilise l'estimation de graphs
            int nbGraphs = int.Parse(args[estimateIndex + 1]);
            Benchmark.EstimateBiggestDiameterGraph(
                nbGraphs,
                n,
                k,
                mode,
                setType,
                !calcDiam,
                prevDiam
            );
        }
        else
        {
            // On trouve tous les graphes respectant le critère demandé
            Benchmark.GetBiggestDiameterGraph(n, k, mode, setType, !calcDiam, prevDiam);
        }
    }
}

stopwatch.Stop();
Console.WriteLine(
    "Programme éxécuté en "
        + Math.Round((double)stopwatch.Elapsed.TotalMilliseconds / 1000, 2)
        + " secondes."
);

return 0;
