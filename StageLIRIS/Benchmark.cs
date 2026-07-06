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

        if(process != null)
        {
          process.WaitForExit();
          var output = process.StandardOutput.ReadToEnd();
          return output;
        }
        return "";
    }
    
    public static double roundTo2Decimals(double nb)
    {
        return Math.Truncate(nb * 100) / 100;
    }

    public static int GetDiamOfIsGraph(Graph graph, int k, char mode, bool estimate)
    {
        // Mode: Token sliding -> S | Token jumping -> J
        GraphReconfig graphReconfig = new GraphReconfig(graph, k, mode);
        graphReconfig.CalcAllIsIte();
        Graph graphIs = graphReconfig.Reconfig;
        int diameter;
        if(estimate) diameter = graphIs.EstimateDiameter();
        else diameter = graphIs.GetDiameter();
        //diameter = graphIs.GetDiameter(vertsDiametre);

        if(graphReconfig.isValid) return diameter;
        return -1;
    }

    public static int CalcBiggestDiameter(string file, int n, int k, char mode, bool estimate)
    {
        // Renvoie le diamètre max du graphe IS trouvé
        // Ecrit les graphes trouvés dans le fichier graphs_found/n<n>k<k>.txt
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
              int diameter = GetDiamOfIsGraph(graphe, k, mode, estimate);
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

        Console.WriteLine("Plus gros diamètre trouvé pour les graphes de reconfig avec n="+n+" et k="+k+": " + maxDiam);
        if(!Directory.Exists("graphs_found"))
        {
          Directory.CreateDirectory("graphs_found");
        }
        string outputFile = "graphs_found/n"+n+"k"+k+".txt";
        File.WriteAllText(outputFile, "Liste des graphes (" + graphs.Count + "):\n");
        foreach (Graph graph in graphs)
        {
          File.AppendAllText(outputFile, graph.Name+"\n");
          File.AppendAllText(outputFile, "NbVert: " + graph.NbVert + ", NbEdge: " + graph.NbEdges+"\n");
          File.AppendAllText(outputFile, "degMin: " + graph.DegMin() + ", degMax: " + graph.DegMax()+"\n");
          File.AppendAllText(outputFile, "nbIs: " + graph.NbIs+"\n");
          File.AppendAllText(outputFile, graph.ToDot()+"\n");
          File.AppendAllText(outputFile, "\n"); File.AppendAllText(outputFile, "\n"); File.AppendAllText(outputFile, "\n");
        }

        return maxDiam;
    }

    public static int GetBiggestDiameterGraph(int n, int k, char mode, bool estimate, int prevBiggestDiam=0)
    {
        // Automatise la fonction CalcBiggestDiameterFromFolder
        // prevBiggestDiam correspond au plus gros diamètre trouvé pour (n-1) avec le même k
        int biggestDiam = -1;
        if(!Directory.Exists("temp"))
        {
          Directory.CreateDirectory("temp");
        }
        RunCommand("/bin/geng", "-c "+n+" "+(n-1)+":"+(n*(n-1)/2-k*(k-1)/2-(k-1)*prevBiggestDiam)+" temp/code.txt");
        RunCommand("/bin/listg", " -q temp/code.txt temp/output.txt");
        int currDiam = CalcBiggestDiameter("temp/output.txt", n, k, mode, estimate);
        RunCommand("rm", "-rf temp");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }

    public static int EstimateBiggestDiameterGraph(int n, int k, char mode, bool estimate, int prevBiggestDiam=0)
    {
        // Automatise la fonction CalcBiggestDiameterFromFolder sans calculer tous les graphes
        // Génère un échantillon pour ensuite le tester comme le fait GetBiggestDiameterGraph
        // prevBiggestDiam correspond au plus gros diamètre trouvé pour (n-1) avec le même k

        int biggestDiam = -1;
        if(!Directory.Exists("temp"))
        {
          Directory.CreateDirectory("temp");
        }
        string commande = "/bin/genrang -P1/2 "+n+" 10000000 | /bin/pickg -c1 -e"+(n-1)+":"+(n*(n-1)/2-k*(k-1)/2-(k-1)*prevBiggestDiam)+" > temp/code.g6";
        RunCommand("/bin/bash", $"-c \"{commande}\"");
        RunCommand("/bin/listg", " -q temp/code.g6 temp/output.txt");
        int currDiam = CalcBiggestDiameter("temp/output.txt", n, k, mode, estimate);
        RunCommand("rm", "-rf temp");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }
    
    //COLORATION EDGES==================================================================================================

    public static void ColorGraphs(int n)
    {
        if(!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }
        if(n%2 == 0) RunCommand("/bin/geng", "-c -d"+Math.Floor((double)(n + 1)/2)+" -D"+Math.Floor((double)(n + 1)/2)+" "+n+" temp/code.txt");
        else RunCommand("/bin/geng", "-c -d"+Math.Floor((double)(n + 1)/2)+" -D"+ (Math.Floor((double)(n + 1) / 2) + 1)+" "+n+" temp/code.txt");
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
                    Console.WriteLine("id graph: "+idGraph);
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
        Console.WriteLine("Tous les graphes à "+n+" sont coloriables !");
    }

    public static void SplitColorGraphs(int n, int nbSplit)
    {
        if(!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }

        for (int i = 0; i < nbSplit; i++)
        {
            if(n%2 == 0) RunCommand("/bin/geng", "-c -d"+Math.Floor((double)(n + 1)/2)+" -D"+Math.Floor((double)(n + 1)/2)+" "+n+" "+i+"/"+nbSplit+" temp/code.txt");
            else RunCommand("/bin/geng", "-c -d"+Math.Floor((double)(n + 1)/2)+" -D"+ (Math.Floor((double)(n + 1) / 2) + 1)+" "+n+" "+i+"/"+nbSplit+" temp/code.txt");
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
            Console.WriteLine((i + 1)+"/"+nbSplit+" "+"sont valides.");
        }
        
        RunCommand("rm", "-rf temp");
        Console.WriteLine("Tous les graphes à "+n+" sont coloriables !");
    }

    public static void CountTriangles(int n)
    {
        if(!Directory.Exists("temp"))
        {
            Directory.CreateDirectory("temp");
        }
        if(n%2 == 0) RunCommand("/bin/geng", "-c -d"+Math.Floor((double)n/2)+" -D"+Math.Floor((double)n/2)+" "+n+" temp/code.txt");
        else RunCommand("/bin/geng", "-c -d"+Math.Floor((double)(n + 1)/2)+" -D"+ (Math.Floor((double)(n + 1) / 2) + 1)+" "+n+" temp/code.txt");
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
                    Console.WriteLine("id graph: "+idGraph);
                    Graph graphe = GraphGenerator.GetListgGraph(graphLines);
                    
                    Console.WriteLine("Graphe:");
                    graphe.WriteVois();
                    Console.WriteLine("Nb triangle: "+graphe.CountTriangles());
                    Console.WriteLine("\n\n");
                    
                    readingEdges = false;
                    idGraph++;
                }
            }
        }
        
        RunCommand("rm", "-rf temp");
    }
}
