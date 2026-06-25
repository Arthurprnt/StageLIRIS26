using System.Diagnostics;

namespace StageLIRIS;

public class Benchmark
{
    static string RunCommand(string command, string args = "")
    {
        var psi = new ProcessStartInfo();
        psi.FileName = command;
        psi.Arguments = args;
        psi.RedirectStandardOutput = true;
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;

        using var process = Process.Start(psi);

        process.WaitForExit();

        var output = process.StandardOutput.ReadToEnd();

        return output;
    }
    
    public static double roundTo2Decimals(double nb)
    {
        return Math.Truncate(nb * 100) / 100;
    }
    
    public static int BuildReconfigGraph(Graph graph, int k, char mode, int[] shows)
    {
        // Chronomètre
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        GraphReconfig graphReconfig = new GraphReconfig(graph, k);
        if(shows[0] == 1) Console.WriteLine("Début du calcul des IS...");
        graphReconfig.CalcAllIs();
        if(shows[0] == 1) Console.WriteLine("Liste des IS1 trouvés, il y en a " +  graphReconfig.AllIs.Count);

        if(shows[3] == 1)
        {
            for (int i = 0; i < graphReconfig.AllIs.Count; i++)
            {
                graphReconfig.AllIs[i].Write();
            }
        }
        
        if(shows[0] == 1) Console.WriteLine("Début de la construction du graph des IS...");
        Graph graphIs = graphReconfig.BuildReconfigGraph();
        if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph des IS");
        stopwatch.Stop();

        if(shows[1] == 1)
        {
            Console.WriteLine("Graph des IS:");
            graphIs.WriteMat();
        }

        if(shows[2] == 1)
        {
            Console.WriteLine("Graph des IS:");
            graphIs.WriteVois();
        }
        
        if(shows[0] == 1) Console.WriteLine("Il y a " + graphIs.NbEdges + " arêtes dans le graph des IS");
        if(shows[0] == 1) Console.WriteLine("Début du calcul du diamètre du graph des IS...");
        List<int> vertsDiametre = new List<int>();
        int diameter = graphIs.GetDiameter(vertsDiametre);
        if (shows[0] == 1)
        {
            Console.WriteLine("Le diamètre du graph des IS est " + diameter + " est les IS sont:");
            graphReconfig.AllIs[vertsDiametre[0]].Write();
            graphReconfig.AllIs[vertsDiametre[1]].Write();
            Console.WriteLine("Temps de création du graph reconfig en ms: " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("========================================");
            Console.WriteLine();
        }
        return diameter;
    }

    public static int BuildReconfigGraph2(Graph graph, int k, char mode, int[] shows)
    {
        // Chronomètre
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        GraphReconfig2 graphReconfig = new GraphReconfig2(graph, k);
        if(shows[0] == 1) Console.WriteLine("Début du calcul des IS...");
        graphReconfig.CalcAllIs();
        if(shows[0] == 1) Console.WriteLine("Liste des IS1 trouvés, il y en a " +  graphReconfig.AllIs.Count);

        if(shows[3] == 1)
        {
            for (int i = 0; i < graphReconfig.AllIs.Count; i++)
            {
                graphReconfig.AllIs[i].Write();
            }
        }
        
        if(shows[0] == 1) Console.WriteLine("Début de la construction du graph des IS...");
        Graph graphIs = graphReconfig.GraphReconfig;
        if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph des IS");
        stopwatch.Stop();

        if(shows[1] == 1)
        {
            Console.WriteLine("Graph des IS:");
            graphIs.WriteMat();
        }

        if(shows[2] == 1)
        {
            Console.WriteLine("Graph des IS:");
            graphIs.WriteVois();
        }
        
        if(shows[0] == 1) Console.WriteLine("Il y a " + graphIs.NbEdges + " arêtes dans le graph des IS");
        if(shows[0] == 1) Console.WriteLine("Début du calcul du diamètre du graph des IS...");
        List<int> vertsDiametre = new List<int>();
        int diameter = graphIs.GetDiameter(vertsDiametre);
        if(shows[0] == 1)
        {
            Console.WriteLine("Le diamètre du graph des IS est " + diameter + " est les IS sont:");
            graphReconfig.AllIs[vertsDiametre[0]].Write();
            graphReconfig.AllIs[vertsDiametre[1]].Write();
            Console.WriteLine("Temps de création du graph reconfig en ms: " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("========================================");
            Console.WriteLine();
        }
        return diameter;
    }

    public static int BuildReconfigGraph3(Graph graph, int k, char mode, int[] shows)
    {
        // Chronomètre
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        GraphReconfig3 graphReconfig = new GraphReconfig3(graph, k, mode);
        if(shows[0] == 1) Console.WriteLine("Début du calcul des IS...");
        graphReconfig.CalcAllIsIte();
        if(shows[0] == 1) Console.WriteLine("Liste des IS1 trouvés, il y en a " +  graphReconfig.AllIs.Count);

        if(shows[3] == 1)
        {
            for (int i = 0; i < graphReconfig.AllIs.Count; i++)
            {
                graphReconfig.AllIs[i].Write();
            }
        }
        
        if(shows[0] == 1) Console.WriteLine("Début de la construction du graph des IS...");
        Graph graphIs = graphReconfig.GraphReconfig;
        if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph des IS");

        if(shows[1] == 1)
        {
            Console.WriteLine("Graph des IS:");
            graphIs.WriteMat();
        }

        if(shows[2] == 1)
        {
            Console.WriteLine("Graph des IS:");
            graphIs.WriteVois();
        }
        
        if(shows[0] == 1)Console.WriteLine("Il y a " + graphIs.NbEdges + " arêtes dans le graph des IS");
        if(shows[0] == 1) Console.WriteLine("Début du calcul du diamètre du graph des IS...");
        List<int> vertsDiametre = new List<int>();
        int nbIteration = (int)Math.Sqrt(graphIs.NbVert);
        int diameter = graphIs.EstimateDiameter(nbIteration, vertsDiametre);
        //diameter = graphIs.GetDiameter(vertsDiametre);
        stopwatch.Stop();
        if(shows[0] == 1)
        {
            Console.WriteLine("Le diamètre du graph des IS est " + diameter + " est les IS sont:");
            graphReconfig.AllIs[vertsDiametre[0]].Write();
            graphReconfig.AllIs[vertsDiametre[1]].Write();
            Console.WriteLine("Temps de de calcul total en ms: " + stopwatch.ElapsedMilliseconds);
            Console.WriteLine("========================================");
            Console.WriteLine();
        }
        if(graphReconfig.isValid) return diameter;
        return -1;
    }

    public static int GetDiamOfIsGraph(string filePath, int k, char mode, int[] shows, int version)
    {
        // Mode: Token sliding -> S | Token jumping -> J
        // /!\ Le mode jumping n'est implémenté que pour la v3 (car la seule vrm opti)
        // Format shows: [showSteps, showMats, showAdjLst, showIS]
        if(shows[0] == 1) Console.WriteLine("===========| " + filePath.Split('/')[^1].ToUpper() + " | k=" + k + " | mode=" + mode + " | v" + version + " |===========");

        if(shows[0] == 1) Console.WriteLine("Construction du graph...");
        Graph graph = GraphGenerator.GetHogGraph(filePath);
        if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph");

        return GetDiamOfIsGraph(graph, k, mode, shows, version);
    }
    
    public static int GetDiamOfIsGraph(Graph graph, int k, char mode, int[] shows, int version)
    {
        // Mode: Token sliding -> S | Token jumping -> J
        // /!\ Le mode jumping n'est implémenté que pour la v3 (car la seule vrm opti)
        // Format shows: [showSteps, showMats, showAdjLst, showIS]
        if(shows[0] == 1) Console.WriteLine("===========| PreGeneratedGraph | k=" + k + " | mode=" + mode + " | v" + version + " |===========");

        if(shows[1] == 1)
        {
            graph.WriteMat();
            Console.WriteLine();
        }

        if(shows[2] == 1)
        {
            graph.WriteVois();
            Console.WriteLine();
        }

        switch(version)
        {
            case 1:
                return BuildReconfigGraph(graph, k, mode, shows);
            case 2:
                return BuildReconfigGraph2(graph, k, mode, shows);
            case 3:
                return BuildReconfigGraph3(graph, k, mode, shows);
        }
        throw new Exception("Version " + version + " not supported");
    }

    public static void RunBenchmark(int tailleGraMin, int tailleGraMax, int tailleIsMin, int tailleIsMax, char mode)
    {
        // Estime le temps moyen pour d'éxécution pour les graphes tels que:
        // tailleGraMin <= nbSommet <= tailleGraMax
        // tailleIsMin <= tailleIS <= tailleIsMax
        string[] dbDirectories = Directory.GetDirectories("graphs/hog/database/");
        Console.WriteLine("Temps moyen d'éxécution en fonctions des tailles de graphe (en ms):");
        Console.Write("NbVert\t\t");
        for (int i = tailleIsMin; i <= tailleIsMax; i++)
        {
            Console.Write("k="+i+"\t\t");
        }

        Console.Write("\n");
        for (int taille=tailleGraMin; taille<=tailleGraMax; taille++)
        {
            string directory = "graphs/hog/database/" + taille.ToString();
            if (Directory.Exists(directory))
            {
                string[] files = Directory.GetFiles(directory);
                int validFileNb = 0;
                int graphSize = Int32.Parse(directory.Split('/')[^1]);
                if (graphSize <= tailleGraMax)
                {
                    long[] times = new long[tailleIsMax-tailleIsMin+1];
                    foreach (var file in files)
                    {
                        for (int k = tailleIsMin; k <= tailleIsMax; k++)
                        {
                            long timeElapsed = GetDiamOfIsGraph(file, k, mode, [0, 0, 0, 0], 3);
                            if (timeElapsed > -1)
                            {
                                times[k - tailleIsMin] += timeElapsed;
                                validFileNb++;
                            }
                        }
                    }
                    Console.Write(graphSize+":\t\t");
                    for (int i = 0; i < times.Length; i++)
                    {
                        Console.Write(roundTo2Decimals((double)times[i]/(double)validFileNb)+"\t\t");
                    }
                    Console.Write("\n");
                }
            }
        }
    }

    public static int GetBiggestDiameterGraph(string folder, int k, char mode, bool showGraphs=true)
    {
        // Renvoie le diamètre max du graphe IS trouvé dans le dossier folder
        // Peut également afficher les graphes ayant ce diamètre si showGraphs est true
        List<Graph> graphs = new List<Graph>();
        int maxDiam = 0;
        
        string[] files = Directory.GetFiles(folder);
        foreach (string file in files)
        {
            Graph graphe = GraphGenerator.GetHogGraph(file);
            int diameter = GetDiamOfIsGraph(graphe, k, mode, [0, 0, 0, 0], 3);
            if (diameter > maxDiam)
            {
                graphs = new List<Graph>();
                graphs.Add(graphe);
                maxDiam = diameter;
            } else if (diameter == maxDiam)
            {
                graphs.Add(graphe);
            }
        }
        if (showGraphs)
        {
            Console.WriteLine("Plus gros diamètre trouvé: " + maxDiam);
            Console.WriteLine("Liste des graphes (" + graphs.Count + "):");
            foreach (Graph graph in graphs)
            {
                Console.WriteLine(graph.Name);
                Console.WriteLine("NbVert: " + graph.NbVert + ", NbEdge: " + graph.NbEdges);
                Console.WriteLine("degMin: " + graph.DegMin() + ", degMax: " + graph.DegMax());
                Console.WriteLine("nbIs: " + graph.NbIs);
                Console.WriteLine(graph.ToDot());
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            }
        }

        return maxDiam;
    }

    public static int GetBiggestDiameterGraphFromFile(string file, int k, char mode, bool showGraphs=true)
    {
        // Renvoie le diamètre max du graphe IS trouvé dans le dossier folder
        // Peut également afficher les graphes ayant ce diamètre si showGraphs est true
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
          if(!readingEdges) {
            nbVert = int.Parse(line);
            vertsDone = 0;
            readingEdges = true;
            graphLines = new string[nbVert];
          } else {
            graphLines[vertsDone] = line;
            vertsDone++;
            if(vertsDone == nbVert) {
              Graph graphe = GraphGenerator.GetListgGraph(graphLines);
              int diameter = GetDiamOfIsGraph(graphe, k, mode, [0, 0, 0, 0], 3);
              if (diameter > maxDiam)
              {
                graphs = new List<Graph>();
                graphs.Add(graphe);
                maxDiam = diameter;
              } else if (diameter == maxDiam)
              {
                graphs.Add(graphe);
              }
              ind += nbVert+1;
              readingEdges = false;
            }
          }
        }

        /*while(ind <= nbLignes)
        {
            int nbVert = int.Parse(File.ReadLines(file).Skip(ind-1).Take(1).ToArray()[0]);
            Graph graphe = GraphGenerator.GetListgGraph(file, ind+1, ind+nbVert);
            int diameter = GetDiamOfIsGraph(graphe, k, mode, [0, 0, 0, 0], 3);
            if (diameter > maxDiam)
            {
                graphs = new List<Graph>();
                graphs.Add(graphe);
                maxDiam = diameter;
            } else if (diameter == maxDiam)
            {
                graphs.Add(graphe);
            }
            ind += nbVert+1;
        }*/
        if (showGraphs)
        {
            Console.WriteLine("Plus gros diamètre trouvé: " + maxDiam);
            Console.WriteLine("Liste des graphes (" + graphs.Count + "):");
            foreach (Graph graph in graphs)
            {
                Console.WriteLine(graph.Name);
                Console.WriteLine("NbVert: " + graph.NbVert + ", NbEdge: " + graph.NbEdges);
                Console.WriteLine("degMin: " + graph.DegMin() + ", degMax: " + graph.DegMax());
                Console.WriteLine("nbIs: " + graph.NbIs);
                Console.WriteLine(graph.ToDot());
                Console.WriteLine(); Console.WriteLine(); Console.WriteLine();
            }
        }

        return maxDiam;
    }
    
    public static int IncrGetBiggestDiameterGraph(int n, int k, char mode, bool showGraphs=true, int prevBiggestDiam=1)
    {
        // Automatise la fonction GetBiggestDiameterGraph
        Console.WriteLine("Premier nettoyage du répertoire en cours...");
        RunCommand("rm", "-rf graphs_generator/graphes");
        RunCommand("rm", "-rf graphs_generator/graphes_aretes");
        Console.WriteLine("Fin du nettoyage, début du script...");
        int biggestDiam = -1;
        RunCommand("/bin/geng", "-c "+n+" "+(n-1)+":"+(n*(n-1)/2-k*(k-1)/2-(k-1)*(prevBiggestDiam-1))+" graphs_generator/code.txt");
        RunCommand("/bin/listg", " -q graphs_generator/code.txt graphs_generator/output.txt");
        Directory.SetCurrentDirectory("graphs_generator");
        RunCommand("./trad");
        Directory.SetCurrentDirectory("..");
        int currDiam = GetBiggestDiameterGraph("graphs_generator/graphes", k, mode, showGraphs);
        Console.WriteLine("Dernier nettoyage du répertoire en cours...");
        RunCommand("rm", "-rf graphs_generator/graphes");
        RunCommand("rm", "-rf graphs_generator/graphes_aretes");
        Console.WriteLine("Fin du nettoyage...");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }

    public static int IncrGetBiggestDiameterGraphFromFile(int n, int k, char mode, bool showGraphs=true, int prevBiggestDiam=1)
    {
        // Automatise la fonction GetBiggestDiameterGraph
        int biggestDiam = -1;
        RunCommand("/bin/geng", "-c "+n+" "+(n-1)+":"+(n*(n-1)/2-k*(k-1)/2-(k-1)*(prevBiggestDiam-1))+" graphs_generator/code.txt");
        RunCommand("/bin/listg", " -q graphs_generator/code.txt graphs_generator/output.txt");
        int currDiam = GetBiggestDiameterGraphFromFile("graphs_generator/output.txt", k, mode, showGraphs);
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }
}
