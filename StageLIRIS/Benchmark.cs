using System.Diagnostics;

namespace StageLIRIS;

public class Benchmark
{
    public static long BuildReconfigGraph(Graph graph, int k, int[] shows)
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
        return stopwatch.ElapsedMilliseconds;
    }

    public static long BuildReconfigGraph2(Graph graph, int k, int[] shows)
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
        return stopwatch.ElapsedMilliseconds;
    }

    public static long BuildReconfigGraph3(Graph graph, int k, int[] shows)
    {
        // Chronomètre
        Stopwatch stopwatch = Stopwatch.StartNew();
        
        GraphReconfig3 graphReconfig = new GraphReconfig3(graph, k);
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
        
        if(shows[0] == 1)Console.WriteLine("Il y a " + graphIs.NbEdges + " arêtes dans le graph des IS");
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
        return stopwatch.ElapsedMilliseconds;
    }

    public static long GetDiamOfIsGraph(string filePath, int k, int[] shows, int version, List<int> listToGraphSize)
    {
        // Format shows: [showSteps, showMats, showAdjLst, showIS]
        if(shows[0] == 1) Console.WriteLine("===========| " + filePath.Split('/')[^1].ToUpper() + " | " + k + " | v" + version + " |===========");

        if(shows[0] == 1) Console.WriteLine("Construction du graph...");
        Graph graph = GraphGenerator.GetHogGraph(filePath);
        listToGraphSize.Add(graph.NbVert);
        if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph");

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
                return BuildReconfigGraph(graph, k, shows);
            case 2:
                return BuildReconfigGraph2(graph, k, shows);
            case 3:
                return BuildReconfigGraph3(graph, k, shows);
        }
        throw new Exception("Version " + version + " not supported");
    }

    public static long GetDiamOfIsGraph(string filePath, int k, int[] shows, int version)
    {
        return GetDiamOfIsGraph(filePath, k, shows, version, new List<int>());
    }

    public static void runBenchmark(int nbGraph)
    {
        string[] directory = Directory.GetFiles("graphs/hog/database/");
        
        Dictionary<int, Dictionary<int, long[]>> timesElapsed = new Dictionary<int, Dictionary<int, long[]>>();
        // algo-version -> taille-graphe -> [temps total; nb exec]
        
        for (int v = 1; v <= 3; v++)
        {
            timesElapsed.Add(v, new Dictionary<int, long[]>());
        }
        
        for (int i = 0; i < nbGraph; i++)
        {
            for (int v = 1; v <= 3; v++)
            {
                List<int> graphSize = new List<int>();
                long timeElapsed = GetDiamOfIsGraph(directory[i], 3, [0, 0, 0, 0], v, graphSize);
                if(!timesElapsed[v].ContainsKey(graphSize[0]))  timesElapsed[v].Add(graphSize[0], [0, 0]);
                timesElapsed[v][graphSize[0]][0] += timeElapsed;
                timesElapsed[v][graphSize[0]][1] ++;
            }
        }
        Console.WriteLine("Temps moyen de la création du graphe en fonctions des tailles de graphe:");
        Console.WriteLine("Version:\t1\t2\t3");
        foreach(int key in timesElapsed[1].Keys)
        {
            Console.WriteLine(key + ":\t" +  timesElapsed[1][key][0]/timesElapsed[1][key][1] + "\t" + timesElapsed[2][key][0]/timesElapsed[2][key][1] + "\t" + timesElapsed[3][key][0]/timesElapsed[3][key][1]);
        }
    }
}