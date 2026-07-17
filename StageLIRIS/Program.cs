// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using StageLIRIS;

/*
Pour que les fonctions d'obtention de plus gros diamètres fonctionne,
il faut impérativement installer les outils nauty (geng, listg, genrang)

Pour recompiler le programme:
En debug:
    dotnet build -c Debug
Vers linux:
    dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
Vers windobe:
    dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
Vers macos:
    Pour les macs sous arm:
      dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
    Pour les vieux macs (~avant 2020):
      dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
*/

static int ShowUsages()
{
    Console.Error.WriteLine(
        "Default usage: ./StageLIRIS -m <mode> -s <set_type> -n <nb_vert> -k <set_size> [-f <file_path>] [-t <file_type>] [-e <graph_nb>] [-c] [-p <prev_diam>]"
    );
    Console.Error.WriteLine(
        "Reconfig usage: ./StageLIRIS reconfig -m <mode> -s <set_type> -k <set_size> -f <file_path> -t <file_type>"
    );
    Console.Error.WriteLine(
        "Local search usage: ./StageLIRIS search -m <mode> -s <set_type> -d <search_depth> -k <set_size> -f <file_path> -t <file_type> [-l <limit>] [-e <edge_nb>] [-c]"
    );
    Console.Error.WriteLine(
        "Upgrading usage: ./StageLIRIS upgrade -k <indep_size> -a <alpha> -b <beta> -f <file_path> -t <file_type>"
    );
    Console.Error.WriteLine("Run './StageLIRIS -h' for help.\n");
    Environment.Exit(1);
    return 1;
}

static int ShowError(string param)
{
    Console.Error.WriteLine("\nError: Missing required argument '-" + param + " <value>'.");
    return ShowUsages();
}

static int ShowErrorMultiParam()
{
    Console.Error.WriteLine("\nError: You're using too much arguments.");
    return ShowUsages();
}

static int ShowManual()
{
    Console.WriteLine(
        "\nDefault usage: ./StageLIRIS -m <mode> -s <set_type> -n <nb_vert> -k <set_size> [-f <file_path>] [-t <file_type>] [-e <graph_nb>] [-c] [-p <prev_diam>]"
    );
    Console.WriteLine(
        "Reconfig usage: ./StageLIRIS reconfig -m <mode> -s <set_type> -k <set_size> -f <file_path> -t <file_type>"
    );
    Console.WriteLine(
        "Local search usage: ./StageLIRIS search -m <mode> -s <set_type> -d <search_depth> -k <set_size> -f <file_path> -t <file_type> [-l <limit>] [-e <edge_nb>] [-c]"
    );
    Console.WriteLine(
        "Upgrading usage: ./StageLIRIS upgrade -k <indep_size> -a <alpha> -b <beta> -f <file_path> -t <file_type>"
    );
    Console.WriteLine("\nParameters:");
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
        "-e <edge_nb>: The number of time the tool will try to add, switch or remove an edge from the graph. Highly recommanded from graphs over 12 vertices because else it's O(n^8)."
    );
    Console.WriteLine(
        "-e <graph_nb>: To use only if not generating a graph from a file. If used, it will only generate a fixed number of graphs and keep the ones respecting the given criterias."
    );
    Console.WriteLine(
        "-f <file_path>: If used will run the program only for one graph (the one in the file). If not used, will run the program for all graphs of size n."
    );
    Console.WriteLine(
        "-k <indep_size>: The size of the independent sets used for the reconfig graph(s)."
    );
    Console.WriteLine(
        "-l <limit>: Limit the number of graphs kept on each layer of the deep search."
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
        "\nPlease note that you also need to have the following nauty tools installed on your computer: geng, listg, genrang.\n"
    );
    return 1;
}

// Pipe parameters
bool useVerb = !Console.IsOutputRedirected;
bool usePipeInput = Console.IsInputRedirected;

// Mode parameters
int reconfigIndex = Array.IndexOf(args, "reconfig");
if (reconfigIndex == -1)
    reconfigIndex = Array.IndexOf(args, "r");
int searchIndex = Array.IndexOf(args, "search");
if (searchIndex == -1)
    searchIndex = Array.IndexOf(args, "s");
int upIndex = Array.IndexOf(args, "upgrade");
if (upIndex == -1)
    upIndex = Array.IndexOf(args, "u");

// Variables parameters
bool calcDiam = false;
int calcIndex = Array.IndexOf(args, "-c");
if (calcIndex != -1)
    calcDiam = true;
int eIndex = Array.IndexOf(args, "-e");
int fileIndex = Array.IndexOf(args, "-f");
string file = "";
int helpIndex = Array.IndexOf(args, "-h");
if (helpIndex != -1)
    return ShowManual();
int kIndex = Array.IndexOf(args, "-k");
if (!(kIndex != -1 && kIndex + 1 < args.Length))
    return ShowError("k");
int k = int.Parse(args[kIndex + 1]);
int modeIndex = Array.IndexOf(args, "-m");
int nIndex = Array.IndexOf(args, "-n");
int setTypeIndex = Array.IndexOf(args, "-s");
int fileTypeIndex = Array.IndexOf(args, "-t");
string[] possibleTypes = { "dot", "hog", "gra" };
string fileType = args[fileTypeIndex + 1];

List<Graph> GetGraphs(string file = "")
{
    List<Graph> graphes = new List<Graph>();
    if (!usePipeInput)
    {
        switch (fileType)
        {
            case "dot":
                graphes.Add(GraphGenerator.GetDotGraph(file));
                break;
            case "hog":
                graphes.Add(GraphGenerator.GetHogGraph(file));
                break;
            case "gra":
                graphes.Add(GraphGenerator.GetGraGraph(file));
                break;
        }
    }
    else
    {
        graphes = GraphGenerator.GetPipeGraph();
        if (graphes.Count() == 0)
        {
            Console.Error.WriteLine("No graph was found in the pipe. Aborting...\n");
            Environment.Exit(1);
        }
    }
    return graphes;
}

int nbModeUsed = 0;
if (upIndex != -1 || reconfigIndex != -1 || searchIndex != -1)
{
    if (upIndex != -1)
        nbModeUsed++;
    if (reconfigIndex != -1)
        nbModeUsed++;
    if (searchIndex != -1)
        nbModeUsed++;

    if (nbModeUsed > 1)
        return ShowErrorMultiParam();
}

Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();

if (upIndex == -1)
{
    if (modeIndex == -1 || modeIndex + 1 >= args.Length)
        return ShowError("m");
    char mode = char.Parse(args[modeIndex + 1]);

    if (setTypeIndex == -1 || setTypeIndex + 1 >= args.Length)
        return ShowError("s");
    char setType = char.Parse(args[setTypeIndex + 1]);

    if (nbModeUsed == 0)
    {
        // Using the default mode
        if (nIndex == -1 || nIndex + 1 >= args.Length)
            return ShowError("n");
        int n = int.Parse(args[nIndex + 1]);

        if ((fileIndex != -1 && fileIndex + 1 < args.Length) || usePipeInput)
        {
            // Using existing graphs
            if (fileIndex == -1)
                file = "Nameless";

            if (!usePipeInput && (fileIndex == -1 || fileIndex + 1 >= args.Length))
                return ShowError("f");
            else
                file = args[fileIndex + 1];
            if (!usePipeInput && !File.Exists(file))
                return ShowError("f");
            if (
                !usePipeInput
                && (
                    fileTypeIndex == -1
                    || fileTypeIndex + 1 >= args.Length
                    || !possibleTypes.Contains(fileType)
                )
            )
                return ShowError("t");

            List<Graph> graphs = GetGraphs(file);

            for (int i = 0; i < graphs.Count(); i++)
            {
                if (i > 0)
                    Console.WriteLine();
                GraphReconfig reconfig = new GraphReconfig(graphs[i], k, mode, setType);
                reconfig.CalcAllSetsIte();
                if (useVerb)
                {
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
                    if (calcDiam)
                        Console.WriteLine(reconfig.Reconfig.GetDiameter());
                    else
                        Console.WriteLine(reconfig.Reconfig.EstimateDiameter());
                }
            }
        }
        else
        {
            // Pas de file
            int prevDiam = 0;
            int prevDiamIndex = Array.IndexOf(args, "-p");
            if (prevDiamIndex != -1 && prevDiamIndex + 1 < args.Length)
            {
                prevDiam = int.Parse(args[prevDiamIndex + 1]);
            }

            if (eIndex != -1 && eIndex + 1 < args.Length)
            {
                // On utilise l'estimation de graphs
                int nbGraphs = int.Parse(args[eIndex + 1]);
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
    else
    {
        // A specific mode is used
        // So graphs must be provided
        if (!usePipeInput && (fileIndex == -1 || fileIndex + 1 >= args.Length))
            return ShowError("f");
        else
            file = args[fileIndex + 1];
        if (!usePipeInput && !File.Exists(file))
            return ShowError("f");
        if (
            !usePipeInput
            && (
                fileTypeIndex == -1
                || fileTypeIndex + 1 >= args.Length
                || !possibleTypes.Contains(fileType)
            )
        )
            return ShowError("t");

        List<Graph> graphs = GetGraphs(file);

        if (reconfigIndex != -1)
        {
            // Using the reconfig mode
            for (int i = 0; i < graphs.Count(); i++)
            {
                if (i > 0)
                    Console.WriteLine();
                GraphReconfig graphReconfig = new GraphReconfig(graphs[i], k, mode, setType);
                graphReconfig.CalcAllSetsIte();
                if (useVerb)
                    Console.WriteLine("Graphe reconfiguré du graphe " + file + ":");
                Console.Write(graphReconfig.ToDot());
                if (useVerb)
                    Console.WriteLine();
            }
        }
        else
        {
            // Using the local search mode
            int depthIndex = Array.IndexOf(args, "-d");
            if (depthIndex == -1 || depthIndex + 1 >= args.Length)
                return ShowError("d");

            int edgeLimit = -1;
            if (eIndex != -1 && eIndex + 1 < args.Length)
                edgeLimit = int.Parse(args[eIndex + 1]);

            int depth = int.Parse(args[depthIndex + 1]);

            int lim = -1;
            int limIndex = Array.IndexOf(args, "-l");
            if (limIndex != -1 && limIndex + 1 < args.Length)
                lim = int.Parse(args[limIndex + 1]);

            int biggestDiam = -1;
            List<Graph> bestGraphs = new List<Graph>();

            for (int i = 0; i < graphs.Count(); i++)
            {
                var resDeepSearch = graphs[i]
                    .DeepSearchLocally(depth, k, mode, setType, !calcDiam, lim, edgeLimit);
                if (resDeepSearch.diameter > biggestDiam)
                {
                    biggestDiam = resDeepSearch.diameter;
                    bestGraphs.Clear();
                    bestGraphs.AddRange(resDeepSearch.graphs);
                }
                else if (resDeepSearch.diameter == biggestDiam)
                {
                    bestGraphs.AddRange(resDeepSearch.graphs);
                }
            }
            for (int i = 0; i < bestGraphs.Count(); i++)
            {
                if (i > 0)
                    Console.WriteLine();
                Console.Write(bestGraphs[i].ToDot());
            }
            if (useVerb)
                Console.WriteLine(
                    "\nPlus gros diamètre de reconfig trouvé en recherche locale: " + biggestDiam
                );
        }
    }
}
else
{
    // Using the upgrading mode
    if (!usePipeInput && (fileIndex == -1 || fileIndex + 1 >= args.Length))
        return ShowError("f");
    else
        file = args[fileIndex + 1];
    if (!usePipeInput && !File.Exists(file))
        return ShowError("f");
    if (
        !usePipeInput
        && (
            fileTypeIndex == -1
            || fileTypeIndex + 1 >= args.Length
            || !possibleTypes.Contains(fileType)
        )
    )
        return ShowError("t");
    List<Graph> graphs = GetGraphs(file);

    int alphaIndex = Array.IndexOf(args, "-a");
    if (!(alphaIndex != -1 && alphaIndex + k < args.Length))
        return ShowError("a");

    int betaIndex = Array.IndexOf(args, "-b");
    if (!(betaIndex != -1 && betaIndex + k < args.Length))
        return ShowError("b");

    IndepSet alpha = new IndepSet(graphs[0], k);
    IndepSet beta = new IndepSet(graphs[0], k);
    for (int i = 1; i <= k; i++)
    {
        alpha.AddVert(int.Parse(args[alphaIndex + i]));
        beta.AddVert(int.Parse(args[betaIndex + i]));
    }
    Graph upgradedGraph = graphs[0].UpgradeGraph(alpha, beta);
    if (useVerb)
        Console.WriteLine("Graphe amélioré:");
    Console.WriteLine(upgradedGraph.ToDot());
    if (useVerb)
        Console.WriteLine("Nouveau alpha:");
    alpha.Write();
    if (useVerb)
        Console.WriteLine("Nouveau beta:");
    beta.Write();
}

stopwatch.Stop();
if (useVerb)
    Console.WriteLine(
        "Programme éxécuté en "
            + Math.Round((double)stopwatch.Elapsed.TotalMilliseconds / 1000, 2)
            + " secondes."
    );

return 0;
