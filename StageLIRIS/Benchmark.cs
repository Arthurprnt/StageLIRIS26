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

    public static int GetDiamOfIsGraph(Graph graph, int k, char mode)
    {
        // Mode: Token sliding -> S | Token jumping -> J
        GraphReconfig graphReconfig = new GraphReconfig(graph, k, mode);
        graphReconfig.CalcAllIsIte();
        Graph graphIs = graphReconfig.Reconfig;

        List<int> vertsDiametre = new List<int>();
        int nbIteration = (int)Math.Sqrt(graphIs.NbVert);
        int diameter = graphIs.EstimateDiameter(nbIteration, vertsDiametre);
        //diameter = graphIs.GetDiameter(vertsDiametre);

        if(graphReconfig.isValid) return diameter;
        return -1;
    }

    public static int CalcBiggestDiameter(string file, int n, int k, char mode)
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
              int diameter = GetDiamOfIsGraph(graphe, k, mode);
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

        Console.WriteLine("Plus gros diamètre trouvé: " + maxDiam);
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

    public static int GetBiggestDiameterGraph(int n, int k, char mode, int prevBiggestDiam=0)
    {
        // Automatise la fonction CalcBiggestDiameterFromFolder
        int biggestDiam = -1;
        if(!Directory.Exists("temp"))
        {
          Directory.CreateDirectory("temp");
        }
        RunCommand("/bin/geng", "-c "+n+" "+(n-1)+":"+(n*(n-1)/2-k*(k-1)/2-(k-1)*prevBiggestDiam)+" temp/code.txt");
        RunCommand("/bin/listg", " -q temp/code.txt temp/output.txt");
        int currDiam = CalcBiggestDiameter("temp/output.txt", n, k, mode);
        RunCommand("rm", "-rf temp");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }

    public static int EstimateBiggestDiameterGraph(int n, int k, char mode, int prevBiggestDiam=0)
    {
        // Automatise la fonction CalcBiggestDiameterFromFolder sans calculer tous les graphes
        // Génère un échantillon pour ensuite le tester comme le fait GetBiggestDiameterGraph
        int biggestDiam = -1;
        if(!Directory.Exists("temp"))
        {
          Directory.CreateDirectory("temp");
        }
        string commande = "/bin/genrang -P1/2 "+n+" 10000000 | /bin/pickg -c1 -e"+(n-1)+":"+(n*(n-1)/2-k*(k-1)/2-(k-1)*prevBiggestDiam)+" > temp/code.g6";
        RunCommand("/bin/bash", $"-c \"{commande}\"");
        RunCommand("/bin/listg", " -q temp/code.g6 temp/output.txt");
        int currDiam = CalcBiggestDiameter("temp/output.txt", n, k, mode);
        RunCommand("rm", "-rf temp");
        if (currDiam > biggestDiam)
        {
            biggestDiam = currDiam;
        }
        return biggestDiam;
    }
}
