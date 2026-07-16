using System.Diagnostics;

namespace StageLIRIS;

public class Benchmark
{
    private static string RunCommand(string command, string args = "")
    {
        var psi = new ProcessStartInfo();
        psi.FileName = command;
        psi.Arguments = args;
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        using var process = Process.Start(psi);

        if (process != null)
        {
            process.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            return output;
        }
        return "";
    }

    private static double roundTo2Decimals(double nb)
    {
        return Math.Truncate(nb * 100) / 100;
    }

    public static IEnumerable<string> ReadFromPipeInput()
    {
        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            yield return line;
        }
    }

    public static int GetDiamOfIsGraph(Graph graph, int k, char mode, char setType, bool estimate)
    {
        // Mode: Token sliding -> S | Token jumping -> J
        GraphReconfig graphReconfig = new GraphReconfig(graph, k, mode, setType);
        graphReconfig.CalcAllSetsIte();
        Graph graphIs = graphReconfig.Reconfig;
        int diameter;
        if (estimate)
            diameter = graphIs.EstimateDiameter();
        else
            diameter = graphIs.GetDiameter();
        //diameter = graphIs.GetDiameter(vertsDiametre);

        if (graphReconfig.isValid)
            return diameter;
        return -1;
    }

    private static void TestDirectory(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    private static void CreateNeededDirectories()
    {
        TestDirectory("graphs_found");
        TestDirectory("graphs_found/estimated");
        TestDirectory("graphs_found/estimated/indep_sets");
        TestDirectory("graphs_found/estimated/dominant_sets");
        TestDirectory("graphs_found/indep_sets");
        TestDirectory("graphs_found/dominant_sets");
    }

    public static int CalcBiggestDiameter(
        string file,
        int n,
        int k,
        char mode,
        char setType,
        bool estimate,
        bool trueBiggest
    )
    {
        // Renvoie le diamètre max du graphe IS trouvé
        // Ecrit les graphes trouvés dans le fichier graphs_found/n<n>k<k>.txt
        bool useVerb = !Console.IsOutputRedirected;
        List<Graph> graphs = new List<Graph>();
        int maxDiam = 0;

        int ind = 1;
        int nbLignes = File.ReadLines(file).Count();

        bool readingEdges = false;
        int nbVert = 0;
        int vertsDone = 0;
        string[] graphLines = new string[0];

        foreach (string line in File.ReadLines(file))
        {
            if (!readingEdges)
            {
                nbVert = int.Parse(line);
                vertsDone = 0;
                readingEdges = true;
                graphLines = new string[nbVert];
            }
            else
            {
                graphLines[vertsDone] = line;
                vertsDone++;
                if (vertsDone == nbVert)
                {
                    Graph graphe = GraphGenerator.GetListgGraph(graphLines);
                    int diameter = GetDiamOfIsGraph(graphe, k, mode, setType, estimate);
                    if (diameter > maxDiam)
                    {
                        graphs = new List<Graph>();
                        graphs.Add(graphe);
                        maxDiam = diameter;
                    }
                    else if (diameter == maxDiam)
                    {
                        graphs.Add(graphe);
                    }
                    ind += nbVert + 1;
                    readingEdges = false;
                }
            }
        }

        if (useVerb)
            Console.WriteLine(
                "Plus gros diamètre trouvé pour les graphes de reconfig avec n="
                    + n
                    + " et k="
                    + k
                    + ": "
                    + maxDiam
            );
        else
        {
            for (int i = 0; i < graphs.Count(); i++)
            {
                if (i > 0)
                    Console.WriteLine();
                Console.Write(graphs[i].ToDot());
            }
        }

        string setTypeString = "";
        if (setType == 'I')
            setTypeString = "indep_sets";
        else if (setType == 'D')
            setTypeString = "dominant_sets";
        CreateNeededDirectories();
        string resultDirectory =
            "graphs_found/" + setTypeString + "/n" + n + "k" + k + "m" + mode + "s" + setType;
        if (!trueBiggest)
            resultDirectory =
                "graphs_found/estimated/"
                + setTypeString
                + "/n"
                + n
                + "k"
                + k
                + "m"
                + mode
                + "s"
                + setType;
        TestDirectory(resultDirectory);

        string outputFile = resultDirectory + "/infos.txt";
        File.WriteAllText(outputFile, "Plus gros diamètre trouvé: " + maxDiam + "\n");
        File.AppendAllText(outputFile, "Liste des graphes (" + graphs.Count + "):\n\n");
        foreach (Graph graph in graphs)
        {
            String graphFilePath = resultDirectory + "/graphe" + graph.GetHashCode() + ".txt";

            File.AppendAllText(outputFile, graphFilePath + "\n");
            File.AppendAllText(
                outputFile,
                "NbVert: " + graph.NbVert + ", NbEdge: " + graph.NbEdges + "\n"
            );
            File.AppendAllText(
                outputFile,
                "degMin: " + graph.DegMin() + ", degMax: " + graph.DegMax() + "\n"
            );
            File.AppendAllText(outputFile, "nbIs: " + graph.NbIs + "\n");
            File.AppendAllText(outputFile, "\n");
            File.AppendAllText(outputFile, "\n");
            File.AppendAllText(outputFile, "\n");

            File.WriteAllText(graphFilePath, graph.ToDot());
        }

        return maxDiam;
    }

    public static int GetBiggestDiameterGraph(
        int n,
        int k,
        char mode,
        char setType,
        bool estimate,
        int prevBiggestDiam = 0
    )
    {
        // Automatise la fonction CalcBiggestDiameterFromFolder
        // prevBiggestDiam correspond au plus gros diamètre trouvé pour (n-1) avec le même k
        int biggestDiam = -1;
        if (!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }

        long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        RunCommand(
            "/bin/geng",
            "-c "
                + n
                + " "
                + (n - 1)
                + ":"
                + (n * (n - 1) / 2 - k * (k - 1) / 2 - (k - 1) * prevBiggestDiam)
                + " temp/code"
                + timestamp
                + ".txt"
        );
        RunCommand(
            "/bin/listg",
            " -q temp/code" + timestamp + ".txt temp/output" + timestamp + ".txt"
        );
        int currDiam = CalcBiggestDiameter(
            "temp/output" + timestamp + ".txt",
            n,
            k,
            mode,
            setType,
            estimate,
            true
        );
        //RunCommand("rm", "-rf temp");
        RunCommand("rm", "temp/code" + timestamp + ".txt");
        RunCommand("rm", "temp/output" + timestamp + ".txt");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }

    public static int EstimateBiggestDiameterGraph(
        int nbGraphs,
        int n,
        int k,
        char mode,
        char setType,
        bool estimate,
        int prevBiggestDiam = 0
    )
    {
        // Automatise la fonction CalcBiggestDiameterFromFolder sans calculer tous les graphes
        // Génère un échantillon pour ensuite le tester comme le fait GetBiggestDiameterGraph
        // prevBiggestDiam correspond au plus gros diamètre trouvé pour (n-1) avec le même k

        int biggestDiam = -1;
        if (!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }

        long timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

        string commande =
            "/bin/genrang -P1/2 "
            + n
            + " "
            + nbGraphs
            + " | /bin/pickg -c1: -e"
            + (n - 1)
            + ":"
            + (n * (n - 1) / 2 - k * (k - 1) / 2 - (k - 1) * prevBiggestDiam)
            + " > temp/code"
            + timestamp
            + ".g6";
        RunCommand("/bin/bash", $"-c \"{commande}\"");
        RunCommand(
            "/bin/listg",
            " -q temp/code" + timestamp + ".g6 temp/output" + timestamp + ".txt"
        );
        int currDiam = CalcBiggestDiameter(
            "temp/output" + timestamp + ".txt",
            n,
            k,
            mode,
            setType,
            estimate,
            false
        );
        //RunCommand("rm", "-rf temp");
        RunCommand("rm", "temp/code" + timestamp + ".g6");
        RunCommand("rm", "temp/output" + timestamp + ".txt");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }

    //COLORATION EDGES==================================================================================================

    public static void ColorGraphs(int n)
    {
        if (!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }
        if (n % 2 == 0)
            RunCommand(
                "/bin/geng",
                "-c -d"
                    + Math.Floor((double)(n + 1) / 2)
                    + " -D"
                    + Math.Floor((double)(n + 1) / 2)
                    + " "
                    + n
                    + " temp/code.txt"
            );
        else
            RunCommand(
                "/bin/geng",
                "-c -d"
                    + Math.Floor((double)(n + 1) / 2)
                    + " -D"
                    + (Math.Floor((double)(n + 1) / 2) + 1)
                    + " "
                    + n
                    + " temp/code.txt"
            );
        RunCommand("/bin/listg", " -q temp/code.txt temp/output.txt");

        int idGraph = 0;

        bool readingEdges = false;
        int nbVert = 0;
        int vertsDone = 0;
        string[] graphLines = new string[0];

        foreach (string line in File.ReadLines("temp/output.txt"))
        {
            if (!readingEdges)
            {
                nbVert = int.Parse(line);
                vertsDone = 0;
                readingEdges = true;
                graphLines = new string[nbVert];
            }
            else
            {
                graphLines[vertsDone] = line;
                vertsDone++;
                if (vertsDone == nbVert)
                {
                    Console.WriteLine("id graph: " + idGraph);
                    Graph graphe = GraphGenerator.GetListgGraph(graphLines);
                    Coloration coloration = new Coloration(graphe);
                    //Console.WriteLine(idGraph);
                    //Console.WriteLine("Graphe:");
                    //graphe.WriteVois();
                    try
                    {
                        coloration.ColorGraph();
                    }
                    catch (Exception)
                    {
                        coloration.BrutForceColoration();
                    }
                    /*Console.WriteLine("Graphe:");
                    graphe.WriteVois();
                    Console.WriteLine("Coloration:");
                    coloration.WriteCol();
                    Console.WriteLine("\n\n");*/
                    readingEdges = false;
                    idGraph++;
                }
            }
        }

        RunCommand("rm", "-rf temp");
        Console.WriteLine("Tous les graphes à " + n + " sont coloriables !");
    }

    public static void SplitColorGraphs(int n, int nbSplit)
    {
        if (!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }

        for (int i = 0; i < nbSplit; i++)
        {
            if (n % 2 == 0)
                RunCommand(
                    "/bin/geng",
                    "-c -d"
                        + Math.Floor((double)(n + 1) / 2)
                        + " -D"
                        + Math.Floor((double)(n + 1) / 2)
                        + " "
                        + n
                        + " "
                        + i
                        + "/"
                        + nbSplit
                        + " temp/code.txt"
                );
            else
                RunCommand(
                    "/bin/geng",
                    "-c -d"
                        + Math.Floor((double)(n + 1) / 2)
                        + " -D"
                        + (Math.Floor((double)(n + 1) / 2) + 1)
                        + " "
                        + n
                        + " "
                        + i
                        + "/"
                        + nbSplit
                        + " temp/code.txt"
                );
            RunCommand("/bin/listg", " -q temp/code.txt temp/output.txt");

            int idGraph = 0;

            bool readingEdges = false;
            int nbVert = 0;
            int vertsDone = 0;
            string[] graphLines = new string[0];

            foreach (string line in File.ReadLines("temp/output.txt"))
            {
                if (!readingEdges)
                {
                    nbVert = int.Parse(line);
                    vertsDone = 0;
                    readingEdges = true;
                    graphLines = new string[nbVert];
                }
                else
                {
                    graphLines[vertsDone] = line;
                    vertsDone++;
                    if (vertsDone == nbVert)
                    {
                        Graph graphe = GraphGenerator.GetListgGraph(graphLines);
                        Coloration coloration = new Coloration(graphe);
                        //Console.WriteLine(idGraph);
                        try
                        {
                            coloration.ColorGraph();
                        }
                        catch (Exception)
                        {
                            coloration.BrutForceColoration();
                        }
                        readingEdges = false;
                        idGraph++;
                    }
                }
            }
            Console.WriteLine((i + 1) + "/" + nbSplit + " " + "sont valides.");
        }

        RunCommand("rm", "-rf temp");
        Console.WriteLine("Tous les graphes à " + n + " sont coloriables !");
    }

    public static void CountTriangles(int n)
    {
        if (!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }
        if (n % 2 == 0)
            RunCommand(
                "/bin/geng",
                "-c -d"
                    + Math.Floor((double)n / 2)
                    + " -D"
                    + Math.Floor((double)n / 2)
                    + " "
                    + n
                    + " temp/code.txt"
            );
        else
            RunCommand(
                "/bin/geng",
                "-c -d"
                    + Math.Floor((double)(n + 1) / 2)
                    + " -D"
                    + (Math.Floor((double)(n + 1) / 2) + 1)
                    + " "
                    + n
                    + " temp/code.txt"
            );
        RunCommand("/bin/listg", " -q temp/code.txt temp/output.txt");

        int idGraph = 0;

        bool readingEdges = false;
        int nbVert = 0;
        int vertsDone = 0;
        string[] graphLines = new string[0];

        foreach (string line in File.ReadLines("temp/output.txt"))
        {
            if (!readingEdges)
            {
                nbVert = int.Parse(line);
                vertsDone = 0;
                readingEdges = true;
                graphLines = new string[nbVert];
            }
            else
            {
                graphLines[vertsDone] = line;
                vertsDone++;
                if (vertsDone == nbVert)
                {
                    Console.WriteLine("id graph: " + idGraph);
                    Graph graphe = GraphGenerator.GetListgGraph(graphLines);

                    Console.WriteLine("Graphe:");
                    graphe.WriteVois();
                    Console.WriteLine("Nb triangle: " + graphe.CountTriangles());
                    Console.WriteLine("\n\n");

                    readingEdges = false;
                    idGraph++;
                }
            }
        }

        RunCommand("rm", "-rf temp");
    }
}
