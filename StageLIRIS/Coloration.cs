namespace StageLIRIS;

public class Coloration
{
    // Il existe que deux couleurs pour la coloration: 1 et 2
    // Le 0 correspond à une arête qui n'est pas encore colorée
    public Graph BaseGraph;
    public int[,] Colors;
    public int[,] NbPath;
    public bool[] Marks;
    public bool FoundColoration = false;

    public Coloration(Graph graph)
    {
        BaseGraph = graph;
        Colors = new int[graph.NbVert, graph.NbVert];
        NbPath = new int[graph.NbVert, graph.NbVert];
        Marks = new bool[graph.NbVert];
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
        if (c == 2)
        {
            Marks[i] = true;
            Marks[j] = true;
        }
        else
        {
            Marks[i] = false;
            Marks[j] = false;
        }
        for (int v = 0; v < BaseGraph.NbVert; v++)
        {
            if (BaseGraph.Mat[i, v] == 1 && BaseGraph.Mat[j, v] == 0 && Colors[i, v] == c)
            {
                PutInNbPath(j, v, NbPath[j, v]-1);
            }
            if (BaseGraph.Mat[j, v] == 1 && BaseGraph.Mat[i, v] == 0 && Colors[j, v] == c)
            {
                PutInNbPath(i, v, NbPath[i, v]-1);
            }
        }
    }

    public void PutInNbPath(int i, int j, int nb)
    {
        NbPath[i, j] = nb;
        NbPath[j, i] = nb;
    }

    public int[] GetNextNonEdge()
    {
        int minNb = Int32.MaxValue;
        int mini = Int32.MaxValue;
        int minj = Int32.MaxValue;
        for (int i = 0; i < BaseGraph.NbVert; i++)
        {
            for (int j = i+1; j < BaseGraph.NbVert; j++)
            {
                if (i != j && NbPath[i, j] <= minNb)
                {
                    minNb = NbPath[i, j];
                    mini = i;
                    minj = j;
                }
            }
        }

        return [mini, minj];
    }

    public void ColorPath(int i, int j)
    {
        bool nonEdgeColored = false;
        for (int w = 0; w < BaseGraph.NbVert; w++)
        {
            // On vérifie d'abord s'il existe un chemin de couleur alternée
            if (w != i && w != j && BaseGraph.Mat[i, w] == 1 && BaseGraph.Mat[w, j] == 1 && Colors[w, j] > 0 && Colors[i, w] > 0 && Colors[i, w] != Colors[w, j])
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
                if (!nonEdgeColored && w!= i && w != j && BaseGraph.Mat[i, w] == 1 && BaseGraph.Mat[w, j] == 1 && Colors[i, w] == 0 && Colors[w, j] == 0)
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
            /*Console.WriteLine("Matrice:");
            BaseGraph.WriteVois();
            Console.WriteLine("\n\n");
            Console.WriteLine("Coloration:");
            WriteCol();*/
            throw new Exception("Bloquage dans la coloration sur la non arête "+i+"--"+j);
        }
    }

    public void ColorGraph()
    {
        Colors = new int[BaseGraph.NbVert, BaseGraph.NbVert];
        NbPath = new int[BaseGraph.NbVert, BaseGraph.NbVert];
        
        // Init du nombre de chemin entre chaque paire de non arêtes
        for (int i = 0; i < BaseGraph.NbVert; i++)
        {
            for (int j = i+1; j < BaseGraph.NbVert; j++)
            {
                if (BaseGraph.Mat[i, j] == 1)
                {
                    PutInNbPath(i, j, Int32.MaxValue);
                }
                else
                {
                    for (int w = 0; w < BaseGraph.NbVert; w++)
                    {
                        if (BaseGraph.Mat[i, w] == 1 && BaseGraph.Mat[w, j] == 1)
                        {
                            PutInNbPath(i, j, NbPath[i, j]+1);
                        }
                    }
                }
            }
        }
        
        int[] indMinimum = GetNextNonEdge();
        while (NbPath[indMinimum[0], indMinimum[1]] != Int32.MaxValue)
        {
            // On continue
            ColorPath(indMinimum[0], indMinimum[1]);
            PutInNbPath(indMinimum[0], indMinimum[1], Int32.MaxValue);
            indMinimum = GetNextNonEdge();
        }
    }

    public bool CheckColoration()
    {
      for(int i=0; i<BaseGraph.NbVert; i++)
      {
        for(int j=i+1; j<BaseGraph.NbVert; j++)
        {
          if(BaseGraph.Mat[i, j] == 0)
          {
            bool foundSmtg = false;
            for(int w=0; w<BaseGraph.NbVert; w++)
            {
              if(Colors[i, w] > 0 && Colors[w, j] > 0 && Colors[i, w] != Colors[w, j])
              {
                foundSmtg = true;
              }
            }
            if(!foundSmtg) return false;
          }
        }
      }
      return true;
    }
    
    public void CallNextColo(int i, int j)
    {
        if(j == BaseGraph.NbVert-1)
        {
          BrutForceColorationAux(i+1, 0);
        } else
        {
          BrutForceColorationAux(i, j+1);
        }
    }
    
    public void CallNextColoCouplage(int i, int j)
    {
        if(j == BaseGraph.NbVert-1)
        {
            BrutForceColorationCouplageAux(i+1, 0);
        } else
        {
            BrutForceColorationCouplageAux(i, j+1);
        }
    }

    public void CheckIfColoration()
    {
        if (FoundColoration)
        {
            //Console.WriteLine("Coloration trouvée:");
            //WriteCol();
        }
        else
        {
            throw new Exception("Pas de coloration trouvé pour ce graphe...");
        }
    }

    public void BrutForceColorationAux(int i, int j) {
      if(i == BaseGraph.NbVert)
      {
        // Vérif la coloration
        if(CheckColoration()) {
          FoundColoration = true;
        }
      } else if(!FoundColoration)
      {
        if(BaseGraph.Mat[i, j] == 1) 
        {
          ColorEdge(i, j, 1);
          CallNextColo(i, j);
          if(!FoundColoration)
          {
            ColorEdge(i, j, 2);
            CallNextColo(i, j);
          }
        } else
        {
          CallNextColo(i, j);
        }
      }
    }

    public void BrutForceColoration() {
      FoundColoration = false;
      Colors = new int[BaseGraph.NbVert, BaseGraph.NbVert];
      BrutForceColorationAux(0, 0);
      CheckIfColoration();
    }

    public void BrutForceColorationCouplageAux(int i, int j) {
      if(i == BaseGraph.NbVert)
      {
        // Vérif la coloration
        if(CheckColoration()) {
          FoundColoration = true;
        }
      } else if(!FoundColoration)
      {
        if(BaseGraph.Mat[i, j] == 1) 
        {
          if (!Marks[i] && !Marks[j])
          {
              ColorEdge(i, j, 2);
              CallNextColoCouplage(i, j);
              if (!FoundColoration)
              {
                  ColorEdge(i, j, 1);
              }
          }

          if (!FoundColoration)
          {
              CallNextColoCouplage(i, j);
          }
        } else
        {
            CallNextColoCouplage(i, j);
        }
      }
    }

    public void BrutForceColorationCouplage() {
      FoundColoration = false;
      Marks = new bool[BaseGraph.NbVert];
      Colors = new int[BaseGraph.NbVert, BaseGraph.NbVert];
      for (int i = 0; i < Colors.GetLength(0); i++)
      {
          for (int j = 0; j < Colors.GetLength(1); j++)
          {
              if(BaseGraph.Mat[i, j] == 1) Colors[i, j] = 1;
          }
      }
      BrutForceColorationCouplageAux(0, 0);
      CheckIfColoration();
    }
}
