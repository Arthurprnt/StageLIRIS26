using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace StageLIRIS;

public class GraphReconfig
{
    // Par rapport à la v2:
        // - Fonction de construction itérative
        // - IndepSet sous forme de long (plus compacte mais 64 max dans le graphe d'origine)
        // - Détection de l'appartenance beaucoup plus rapide (merci Sven)
        // - Prise en charge du token jumping
    
    public readonly Graph Graph;
    public readonly int K;
    public readonly char Mode;
    public List<BaseSet> AllIs = new List<BaseSet>();
    public Dictionary<long, int> IsToIndex = new Dictionary<long, int>();
    public Dictionary<int, long> IndexToIs = new Dictionary<int, long>();
    public Graph Reconfig = new Graph(0);
    public bool isValid = true;

    public GraphReconfig(Graph graph, int k, char mode)
    {
        Graph = graph;
        K = k;
        Mode = mode;
    }

    public string ToDot()
    {
        // Renvoie le grapheReconfig sous le format .dot
        StringBuilder dot = new StringBuilder();
        dot.AppendLine("graph G {");

        for (int i = 0; i < Reconfig.Vois.Count; i++)
        {
            for (int j = 0; j < Reconfig.Vois[i].Count; j++)
            {
                if(Reconfig.Vois[i][j] > i) dot.AppendLine($"  \"{IndepSet.toString(IndexToIs[i])}\" -- \"{IndepSet.toString(IndexToIs[Reconfig.Vois[i][j]])}\";");
            }
        }

        dot.AppendLine("}");
        return dot.ToString();
    }

    public void WriteDict()
    {
        foreach (var key in IsToIndex.Keys)
        {
            Console.WriteLine(IndepSet.toString(key) + ": " + IsToIndex[key]);
        }
    }

    public bool isIndepInDict(BaseSet indepSet)
    {
        // Détection rapide de la présence de l'IS dans AllIs
        // Car si un IS a été visité, il est dans le graphe et il possède ainsi un indice dans le dico
        return IsToIndex.ContainsKey(indepSet.States);
    }
    
    public int AddVertex(BaseSet indepSet)
    {
        // Ajoute l'indépendant dans le graph reconfig
        if(indepSet.CurrSize != indepSet.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        int indepSetId = Reconfig.NbVert;
        // On sauvegarde sont indice de sommet associé
        IsToIndex.Add(indepSet.States, indepSetId);
        IndexToIs.Add(indepSetId, indepSet.States);
        AllIs.Add(indepSet.Clone());
        Reconfig.AddVertex();
        Graph.NbIs++;
        return indepSetId;
    }
    
    public int CalcAllIsRecAux(IndepSet currIs)
    {
        // Fonction auxiliaire qui calcule de proche en proche tous les IS de taille K
        // Prends en charge seulement le token sliding, cf la version itérative pour le token jumping
        if (currIs.CurrSize != currIs.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        if (!isIndepInDict(currIs))
        {
            int currSetId = AddVertex(currIs);
            for (int i = 0; i < Graph.NbVert; i++)
            {
                if (currIs.Get(i))
                {
                    int currVert = i;
                    // On regarde où peut slide le token courant
                    for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                    {
                        int savedNeigh = Graph.Vois[currVert][v];
                        currIs.RemoveVert(currVert);
                        if (currIs.CanAddVert(savedNeigh))
                        {
                            currIs.AddVert(savedNeigh);
                            int nextSetId = CalcAllIsRecAux(currIs);
                            // Comme le nouveau IS à un seul de diff, c'est un voisin
                            // On ajoute l'arête entre les deux sommets
                            Reconfig.AddEdge(currSetId, nextSetId);
                            currIs.ReplaceVert(savedNeigh, currVert);
                        }
                        else
                        {
                            // S'il n'est pas valide on restore son état d'origine
                            currIs.AddVert(currVert);
                        }
                    }
                }
            }
            return currSetId;
        }
        return IsToIndex[currIs.States];
    }

    public void handleNeigh(BaseSet currIs, int currVert, int vertNeigh, Queue<BaseSet> queue)
    {
        // Fonction qui fait le traitement de passer d'un IS à l'autre
        // Utile car différente façon de déplacer le token
        // Evite la redondance de code
        int currSetId = IsToIndex[currIs.States];
        
        currIs.RemoveVert(currVert);
        if (currIs.CanAddVert(vertNeigh))
        {
            currIs.AddVert(vertNeigh);

            int nextSetId;
            // Evite de mettre deux fois le même IS dans la file
            if (isIndepInDict(currIs))
            {
                nextSetId = IsToIndex[currIs.States];
            }
            else
            {
                BaseSet nextIsClone = currIs.Clone(); 
                                
                nextSetId = AddVertex(nextIsClone);
                queue.Enqueue(nextIsClone);
            }
            // Comme le nouveau IS à un seul de diff, c'est un voisin
            Reconfig.AddEdge(currSetId, nextSetId);

            currIs.ReplaceVert(vertNeigh, currVert);
        }
        else
        {
            currIs.AddVert(currVert);
        }
    }
    
    public List<BaseSet> CalcAllIsRec()
    {
        // Construit également le graphe de reconfig
        AllIs = new List<BaseSet>();
        Reconfig = new Graph(0);

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet? calcedIs = IndepSet.CreateSet(Graph, K, i);
            if(calcedIs != null)
            {
              CalcAllIsRecAux(calcedIs);
            }
        }
        
        return AllIs;
    }
    
    public List<BaseSet> CalcAllIsIte()
    {
      // Calcul tous les sets par brutforce
      // Fonction optimisée à l'IA (passage à l'itératif notamment)

      AllIs = new List<BaseSet>();
      Reconfig = new Graph(0);

      int[] chosen = new int[K];
      int level = 0;

      // Commence avec le sommet 0 au niveau 0
      chosen[0] = 0;

      BaseSet currSet = new IndepSet(Graph, K);

      while(level >= 0)
      {
        // Si on n'a pas encore dépassé le nombre total de sommets pour ce niveau
        if(chosen[level] < Graph.NbVert)
        {
          int vert = chosen[level];
          // Elegage car pas assez de sommets restant
          if(level + (Graph.NbVert - vert) < K)
          {
            chosen[level] = Graph.NbVert;
            continue;
          }

          if(currSet.CanAddVert(vert))
          {
            currSet.AddVert(vert);

            if(level == K-1)
            {
              // On ajoue le nouvel ensemble (indep ou dominant)
              int setId = AddVertex(currSet);
              for(int prevId=0; prevId<setId; prevId++)
              {
                long prevStates = IndexToIs[prevId];
                int intersectionSize = System.Numerics.BitOperations.PopCount((ulong)(currSet.States & prevStates));
                if(intersectionSize == K-1)
                {
                  if(Mode == 'J')
                  {
                    Reconfig.AddEdge(setId, prevId);
                  }
                  else if(Mode == 'S')
                  {
                    int uniqueInCurr = BaseSet.DiffBtw(currSet.States, prevStates);
                    int uniqueInPrev = BaseSet.DiffBtw(prevStates, currSet.States);
                    if(Graph.Vois[uniqueInCurr].Contains(uniqueInPrev)) Reconfig.AddEdge(setId, prevId);
                  }
                }
              }
              //==========
              
              currSet.RemoveVert(vert);
              chosen[level]++;
            }
            else
            {
              level++;
              chosen[level] = vert+1;
            }
          }
          else
          {
            // Pas valide, on passe au sommet suivant pour ce niveau
            chosen[level]++;
          }
        }
        else
        {
          // Backtracking: on a épuisé toutes les possibilités pour ce niveau, on remonte d'un cran
          level--;
          if(level >= 0)
          {
            // On retire le sommet du niveau supérieur de notre set actuel avant de l'incrémenter
            currSet.RemoveVert(chosen[level]);
            chosen[level]++;
          }
        }
      }
      return AllIs;
    }
}
