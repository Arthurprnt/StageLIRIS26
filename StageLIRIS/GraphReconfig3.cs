using System.Collections;
using System.Collections.Specialized;

namespace StageLIRIS;

public class GraphReconfig3
{
    public readonly Graph Graph;
    public readonly int K;
    public List<IndepSet3> AllIs;
    public Dictionary<long, int> IsToIndex = new Dictionary<long, int>();
    public Graph GraphReconfig = new Graph(0);

    public GraphReconfig3(Graph graph, int k)
    {
        Graph = graph;
        K = k;
    }

    public bool isIndepInDict(IndepSet3 indepSet)
    {
        return IsToIndex.ContainsKey(indepSet.States);
    }
    
    public int AddVertex(IndepSet3 indepSet)
    {
        if(indepSet.CurrSize != indepSet.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        int indepSetId = GraphReconfig.NbVert;
        IsToIndex.Add(indepSet.States, indepSetId);
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
    
    public int CalcAllIsRecAux(IndepSet3 currIs)
    {
        if (currIs.CurrSize != currIs.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        if (!isIndepInDict(currIs))
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
                        currIs.RemoveVert(currVert);
                        if (currIs.CanAddVert(savedNeigh))
                        {
                            currIs.AddVert(savedNeigh);
                            int nextSetId = CalcAllIsRecAux(currIs);
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
            return currSetId;
        }
        return IsToIndex[currIs.States];
    }
    
    public List<IndepSet3> CalcAllIsIte()
    {
        AllIs = new List<IndepSet3>();
        GraphReconfig = new Graph(0);

        Queue<IndepSet3> queue = new Queue<IndepSet3>();

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet3 calcedIs = CalcIs(i);
            if (calcedIs.CurrSize != calcedIs.MaxSize) 
                throw new Exception("L'IS n'est pas de taille k");

            if (!isIndepInDict(calcedIs))
            {
                AddVertex(calcedIs);
                queue.Enqueue(calcedIs);
            }
        }

        while (queue.Count > 0)
        {
            IndepSet3 currIs = queue.Dequeue();
            int currSetId = IsToIndex[currIs.States];

            for (int i = 0; i < Graph.NbVert; i++)
            {
                if (currIs.Get(i))
                {
                    int currVert = i;
                    for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                    {
                        int savedNeigh = Graph.Vois[currVert][v];
                        
                        currIs.RemoveVert(currVert);
                        if (currIs.CanAddVert(savedNeigh))
                        {
                            currIs.AddVert(savedNeigh);

                            int nextSetId;
                            if (isIndepInDict(currIs))
                            {
                                nextSetId = IsToIndex[currIs.States];
                            }
                            else
                            {
                                IndepSet3 nextIsClone = currIs.Clone(); 
                                
                                nextSetId = AddVertex(nextIsClone);
                                queue.Enqueue(nextIsClone);
                            }
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
        return AllIs;
    }
    
    public List<IndepSet3> CalcAllIsRec()
    {
        AllIs = new List<IndepSet3>();
        GraphReconfig = new Graph(0);

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet3 calcedIs = CalcIs(i);
            CalcAllIsRecAux(calcedIs);
        }
        
        return AllIs;
    }
}