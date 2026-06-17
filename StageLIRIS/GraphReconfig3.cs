using System.Collections;
using System.Collections.Specialized;

namespace StageLIRIS;

public class GraphReconfig3
{
    public readonly Graph Graph;
    public readonly int K;
    public List<IndepSet3> AllIs;
    public Dictionary<BitVector32, Dictionary<BitVector32, int>> IsToIndex = new Dictionary<BitVector32, Dictionary<BitVector32, int>>();
    public Graph GraphReconfig = new Graph(0);

    public GraphReconfig3(Graph graph, int k)
    {
        Graph = graph;
        K = k;
    }
    
    public int AddVertex(IndepSet3 indepSet)
    {
        if(indepSet.CurrSize != indepSet.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        int indepSetId = GraphReconfig.NbVert;
        if (!IsToIndex.ContainsKey(indepSet.States1))
        {
            IsToIndex.Add(indepSet.States1, new Dictionary<BitVector32, int>());
        }
        IsToIndex[indepSet.States1].Add(indepSet.States2, indepSetId);
        AllIs.Add(indepSet.Clone());
        GraphReconfig.AddVertex();
        return indepSetId;
    }
    
    public IndepSet3 CalcIs(int from)
    {
        int ind = 0;
        IndepSet3 indepSet = new IndepSet3(Graph, K);
        indepSet.AddVert(from);
        while (indepSet.CurrSize < indepSet.MaxSize &&  ind < Graph.NbVert)
        {
            if (ind != from && indepSet.CanAddVert(ind))
            {
                indepSet.AddVert(ind);
            }
            ind++;
        }

        if (indepSet.CurrSize != indepSet.MaxSize)
        {
            throw new Exception("Il n'existe pas d'IS de taille " + K);
        }
        return indepSet;
    }
    
    public int CalcAllIsRec(IndepSet3 currIs, int prevVert, int recur)
    {
        if (currIs.CurrSize != currIs.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        if (!AllIs.Contains(currIs))
        {
            int currSetId = AddVertex(currIs);
            for (int i = 0; i < Graph.NbVert; i++)
            {
                if (currIs.Get(i))
                {
                    int currVert = i;
                    for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                    {
                        int savedNeigh = Graph.Vois[currVert][v];
                        if (savedNeigh != prevVert)
                        {
                            currIs.RemoveVert(currVert);
                            if (currIs.CanAddVert(savedNeigh))
                            {
                                currIs.AddVert(savedNeigh);
                                int nextSetId = CalcAllIsRec(currIs, currVert, recur+1);
                                GraphReconfig.AddEdge(currSetId, nextSetId);
                                currIs.ReplaceVert(savedNeigh, currVert);
                            }
                            else
                            {
                                currIs.AddVert(currVert);
                            }
                        }
                    }
                }
            }
            return currSetId;
        }
        return IsToIndex[currIs.States1][currIs.States2];
    }
    
    public List<IndepSet3> CalcAllIs()
    {
        AllIs = new List<IndepSet3>();
        GraphReconfig = new Graph(0);

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet3 calcedIs = CalcIs(i);
            CalcAllIsRec(calcedIs, -1, 0);
        }
        
        GraphReconfig.RebuildMat();
        return AllIs;
    }
}