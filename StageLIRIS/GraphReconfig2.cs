namespace StageLIRIS;

public class GraphReconfig2
{
    public readonly Graph Graph;
    public readonly int K;
    public List<IndepSet2> AllIs;
    public Dictionary<string, int> IsToIndex = new Dictionary<string, int>();
    public Graph GraphReconfig = new Graph(0);
    
    public GraphReconfig2(Graph graph, int k)
    {
        this.Graph = graph;
        this.K = k;
    }

    public int AddVertex(IndepSet2 indepSet)
    {
        if(indepSet.CurrSize != indepSet.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        int indepSetId = GraphReconfig.NbVert;
        IsToIndex.Add(indepSet.ToString(), indepSetId);
        AllIs.Add(indepSet.Clone());
        GraphReconfig.AddVertex();
        return indepSetId;
    }

    public IndepSet2 CalcIs(int from)
    {
        int ind = 0;
        IndepSet2 indepSet = new IndepSet2(Graph, K);
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

    public int CalcAllIsRec(IndepSet2 currIs, int prevVert)
    {
        if (currIs.CurrSize != currIs.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        if (!AllIs.Contains(currIs))
        {
            int currSetId = AddVertex(currIs);
            for (int i = 0; i < currIs.MaxSize; i++)
            {
                int currVert = currIs.Verts[i];
                for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                {
                    int savedNeigh = Graph.Vois[currVert][v];
                    if (savedNeigh != prevVert)
                    {
                        currIs.RemoveVert(currVert);
                        if (currIs.CanAddVert(savedNeigh))
                        {
                            currIs.AddVert(savedNeigh);
                            int nextSetId = CalcAllIsRec(currIs, currVert);
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
        return IsToIndex[currIs.ToString()];
    }

    public List<IndepSet2> CalcAllIs()
    {
        AllIs = new List<IndepSet2>();
        GraphReconfig = new Graph(0);

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet2 calcedIs = CalcIs(i);
            CalcAllIsRec(calcedIs, -1);
        }
        
        return AllIs;
    }
}