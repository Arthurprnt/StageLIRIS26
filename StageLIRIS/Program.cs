// See https://aka.ms/new-console-template for more information

using System.Diagnostics;
using StageLIRIS;

void GetDiamOfIsGraph(string filePath, int k, int[] shows)
{
    // Format shows: [showSteps, showMats, showAdjLst, showIS]
    Console.WriteLine("===========| " + filePath.Split('/')[^1].ToUpper() + " | " + k + " |===========");
    
    // Chronomètre
    Stopwatch stopwatch = Stopwatch.StartNew(); 

    if(shows[0] == 1) Console.WriteLine("Construction du graph...");
    Graph graph;
    graph = GraphGenerator.GetHogGraph(filePath);
    if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph");

    if (shows[1] == 1)
    {
        graph.WriteMat();
        Console.WriteLine();
    }

    if (shows[2] == 1)
    {
        graph.WriteVois();
        Console.WriteLine();
    }
    
    GraphReconfig graphReconfig = new GraphReconfig(graph, k);
    if(shows[0] == 1) Console.WriteLine("Début du calcul des IS...");
    graphReconfig.CalcAllIS();
    Console.WriteLine("Liste des IS trouvés, il y en a " +  graphReconfig.AllIs.Count);

    if (shows[3] == 1)
    {
        for (int i = 0; i < graphReconfig.AllIs.Count; i++)
        {
            graphReconfig.AllIs[i].Write();
        }
    }
    
    if(shows[0] == 1) Console.WriteLine("Début de la construction du graph des IS...");
    Graph graphIS = graphReconfig.BuildReconfigGraph();
    if(shows[0] == 1) Console.WriteLine("Fin de la construction du graph des IS");

    if (shows[1] == 1)
    {
        Console.WriteLine("Graph des IS:");
        graphIS.WriteMat();
    }

    if (shows[2] == 1)
    {
        Console.WriteLine("Graph des IS:");
        graphIS.WriteVois();
    }
    
    Console.WriteLine("Il y a " + graphIS.NbEdges + " arêtes dans le graph des IS");
    if(shows[0] == 1) Console.WriteLine("Début du calcul du diamètre du graph des IS...");
    List<int> vertsDiametre = new List<int>();
    int diameter = graphIS.GetDiameter(vertsDiametre);
    Console.WriteLine("Le diamètre du graph des IS est " + diameter + " est les IS sont:");
    graphReconfig.AllIs[vertsDiametre[0]].Write();
    graphReconfig.AllIs[vertsDiametre[1]].Write();

    stopwatch.Stop();
    Console.WriteLine("Temps d'éxécution en secondes: " + stopwatch.ElapsedMilliseconds/1000);
    Console.WriteLine("========================================");
    Console.WriteLine();
}

GetDiamOfIsGraph("graphs/hog/graph_1173.txt", 3, [1, 0, 0, 0]);