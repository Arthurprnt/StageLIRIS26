using System.Diagnostics;

namespace StageLIRIS;

public class Benchmark
{
    public static double roundTo2Decimals(double nb)
    {
        return Math.Truncate(nb * 100) / 100;
    }
    
    public static long BuildReconfigGraph(Graph graph, int k, char mode, int[] shows)
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

    public static long BuildReconfigGraph2(Graph graph, int k, char mode, int[] shows)
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

    public static long BuildReconfigGraph3(Graph graph, int k, char mode, int[] shows)
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
        int diameter = graphIs.EstimateDiameter((int)Math.Sqrt(graphIs.NbVert), vertsDiametre);
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
        if(graphReconfig.isValid) return stopwatch.ElapsedMilliseconds;
        return -1;
    }

    public static long GetDiamOfIsGraph(string filePath, int k, char mode, int[] shows, int version)
    {
        // Mode: Token sliding -> S | Token jumping -> J
        // /!\ Le mode jumping n'est implémenté que pour la v3 (car la seule vrm opti)
        // Format shows: [showSteps, showMats, showAdjLst, showIS]
        if(shows[0] == 1) Console.WriteLine("===========| " + filePath.Split('/')[^1].ToUpper() + " | k=" + k + " | mode=" + mode + " | v" + version + " |===========");

        if(shows[0] == 1) Console.WriteLine("Construction du graph...");
        Graph graph = GraphGenerator.GetHogGraph(filePath);
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
        string[] dbDirectories = Directory.GetDirectories("graphs/hog/database/");
        Console.WriteLine("Temps moyen de la création du graphe en fonctions des tailles de graphe (en ms):");
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
}