// See https://aka.ms/new-console-template for more information
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

static void ShowManual(string param)
{
    Console.WriteLine("\n");
    Console.WriteLine("Error: Missing required argument '-"+param+" <value>'.");
    Console.WriteLine("Usage: ./StageLIRIS -m <mode> -n <nb_vert> -k <indep_size> [-f <file_path>] [-t <file_type>] [-c] [-p <prev_diam>]");
    Console.WriteLine("Parameters:");
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
}

int modeIndex = Array.IndexOf(args, "-m");
if (modeIndex != -1 && modeIndex + 1 < args.Length)
{
    char mode = char.Parse(args[modeIndex + 1]);
    int nIndex = Array.IndexOf(args, "-n");
    if (nIndex != -1 && nIndex + 1 < args.Length)
    {
        int n = int.Parse(args[nIndex + 1]);
        int kIndex = Array.IndexOf(args, "-k");
        if (kIndex != -1 && kIndex + 1 < args.Length)
        {
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
                {
                    ShowManual("f");
                }
                else
                {
                    int fileTypeIndex = Array.IndexOf(args, "-t");
                    if (fileTypeIndex != -1 && fileTypeIndex + 1 < args.Length)
                    {
                        string types = args[fileTypeIndex + 1].ToLower();
                        string[] possibleTypes = { "dot", "hog", "gra" };
                        if (possibleTypes.Contains(types))
                        {
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
                            ShowManual("t");
                        }
                    }
                    else
                    {
                        ShowManual("t");
                    }
                } 
            }
            else
            {
                // Pas de file
                int prevDiam = 0;
                int prevDiamIndex = Array.IndexOf(args, "-p");
                if(prevDiamIndex != 1 && prevDiamIndex + 1 < args.Length)
                {
                  prevDiam = int.Parse(args[prevDiamIndex + 1]);
                }
                Benchmark.GetBiggestDiameterGraph(n, k, mode, !calcDiam, prevDiam);
            }
        }
        else
        {
            ShowManual("k");
        }
    }
    else
    {
        ShowManual("n");
    }
}
else
{
    ShowManual("m");
}

