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
    public List<BaseSet> AllSets = new List<BaseSet>();
    public Dictionary<long, int> SetToIndex = new Dictionary<long, int>();
    public Dictionary<int, long> IndexToSet = new Dictionary<int, long>();
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

        for(int i=0; i<Reconfig.NbVert; i++) {
          if(Reconfig.Vois[i].Count() == 0) dot.AppendLine($"  \"{BaseSet.toString(IndexToSet[i])}\";");
        }


        for (int i = 0; i < Reconfig.Vois.Count; i++)
        {
            for (int j = 0; j < Reconfig.Vois[i].Count; j++)
            {
                if(Reconfig.Vois[i][j] > i) dot.AppendLine($"  \"{BaseSet.toString(IndexToSet[i])}\" -- \"{BaseSet.toString(IndexToSet[Reconfig.Vois[i][j]])}\";");
            }
        }

        dot.AppendLine("}");
        return dot.ToString();
    }

    public void WriteDict()
    {
        foreach (var key in SetToIndex.Keys)
        {
            Console.WriteLine(BaseSet.toString(key) + ": " + SetToIndex[key]);
        }
    }

    public bool isIndepInDict(BaseSet currSet)
    {
        // Détection rapide de la présence de l'IS dans AllIs
        // Car si un IS a été visité, il est dans le graphe et il possède ainsi un indice dans le dico
        return SetToIndex.ContainsKey(currSet.States);
    }
    
    public int AddVertex(BaseSet currSet)
    {
        // Ajoute l'indépendant dans le graph reconfig
        if(currSet.CurrSize != currSet.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        int indepSetId = Reconfig.NbVert;
        // On sauvegarde sont indice de sommet associé
        SetToIndex.Add(currSet.States, indepSetId);
        IndexToSet.Add(indepSetId, currSet.States);
        AllSets.Add(currSet.Clone());
        Reconfig.AddVertex();
        Graph.NbIs++;
        return indepSetId;
    }
    
    public List<BaseSet> CalcAllSetsIte(char setType)
    {
      // Calcul tous les sets par brutforce
      // Fonction optimisée à l'IA (passage à l'itératif notamment)

      AllSets = new List<BaseSet>();
      Reconfig = new Graph(0);

      int[] chosen = new int[K];
      int level = 0;

      // Commence avec le sommet 0 au niveau 0
      chosen[0] = 0;

      BaseSet currSet;
      if(setType == 'I') currSet = new IndepSet(Graph, K);
      else if (setType == 'D') currSet = new DominantSet(Graph, K);
      else throw new Exception("This type of set does not exist.");

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
              if((currSet is IndepSet || (currSet is DominantSet currDomSet && currDomSet.IsValid())))
              {
                // On ajoue le nouvel ensemble (indep ou dominant)
                int setId = AddVertex(currSet);
                for(int prevId=0; prevId<setId; prevId++)
                {
                  long prevStates = IndexToSet[prevId];
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
      return AllSets;
    }
}
