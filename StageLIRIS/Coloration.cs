namespace StageLIRIS;

public class Coloration
{
    // Il existe que deux couleurs pour la coloration: 1 et 2
    // Le 0 correspond à une arête qui n'est pas encore colorée
    public Graph BaseGraph;
    public int[,] Colors;

    public Coloration(Graph graph)
    {
        BaseGraph = graph;
        Colors = new int[graph.NbVert, graph.NbVert];
    }

    public void WriteCol()
    {
        for (int i = 0; i < BaseGraph.NbVert; i++)
        {
            for (int j = 0; j < BaseGraph.NbVert; j++)
            {
                Console.Write(Colors[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public int OtherCol(int c)
    {
        // Renvoie l'autre des deux couleurs
        return Math.Abs(c - 2) + 1;
    }

    public void ColorEdge(int i, int j, int c)
    {
        // Permet de colorer dans les deux sens d'un coup
        Colors[i, j] = c;
        Colors[j, i] = c;
    }

    public void ColorGraph()
    {
        Colors = new int[BaseGraph.NbVert, BaseGraph.NbVert];
        for (int i = 0; i < BaseGraph.NbVert; i++)
        {
            for (int j = i+1; j < BaseGraph.NbVert; j++)
            {
                if (i != j && BaseGraph.Mat[i, j] == 0)
                {
                    // Toutes les paires de non arêtes
                    bool nonEdgeColored = false;
                    for (int w = 0; w < BaseGraph.NbVert; w++)
                    {
                        // On vérifie d'abord s'il existe un chemin de couleur alternée
                        if (w != i && w != j && Colors[w, j] > 0 && Colors[i, w] > 0 && Colors[i, w] != Colors[w, j])
                        {
                            nonEdgeColored = true;
                        }
                    }

                    if (!nonEdgeColored)
                    {
                        for (int w = 0; w < BaseGraph.NbVert; w++)
                        {
                            if (!nonEdgeColored && w!=i && w != j && BaseGraph.Mat[i, w] == 1 && BaseGraph.Mat[w, j] == 1)
                            {
                                // Il existe une chemin entre i et j en passant par w
                                if (Colors[i, w] > 0 && Colors[w, j] == 0)
                                {
                                    nonEdgeColored = true;
                                    ColorEdge(w, j, OtherCol(Colors[i, w]));
                                } else if (Colors[w, j] > 0 && Colors[i, w] == 0)
                                {
                                    nonEdgeColored = true;
                                    ColorEdge(i, w, OtherCol(Colors[w, j]));
                                }
                            }
                        }
                    }
                    
                    if (!nonEdgeColored)
                    {
                        // On trouve une paire d'arete non coloriées
                        for (int w = 0; w < BaseGraph.NbVert; w++)
                        {
                            if (!nonEdgeColored && w!= i && w != j && BaseGraph.Mat[i, w] == 1 && BaseGraph.Mat[w, j] == 1)
                            {
                                nonEdgeColored = true;
                                ColorEdge(i, w, 1);
                                ColorEdge(w, j, 2);
                            }
                        }   
                    }

                    if (!nonEdgeColored)
                    {
                        // Cas où on ne trouve pas de coloration valide
                        Console.WriteLine("Matrice:");
                        BaseGraph.WriteMat();
                        Console.WriteLine("\n\n");
                        Console.WriteLine("Coloration:");
                        WriteCol();
                        throw new Exception("Bloquage dans la coloration sur la non arête "+i+"--"+j);
                    }
                }
            }
        }
    }
}
