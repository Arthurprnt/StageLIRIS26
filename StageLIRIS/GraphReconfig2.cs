namespace StageLIRIS;

public class GraphReconfig2
{
    public readonly Graph Graph;
    public readonly int K;
    public List<IndepSet2> AllIs;
    public Dictionary<int, int> IsToIndex = new Dictionary<int, int>();
    public Graph GraphReconfig;
    
    public GraphReconfig2(Graph graph, int k)
    {
        this.Graph = graph;
        GraphReconfig = new Graph(0);
        this.K = k;
    }

    public void AddVertex(IndepSet2 indepSet)
    {
        int indepSetInd = GraphReconfig.NbVert;
        IsToIndex.Add(indepSet.ToInt(), indepSetInd);
        GraphReconfig.AddVertex();
    }

    public IndepSet2 calcIs(int from)
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

    public void CalcAllIsRec(IndepSet2 currIS)
    {
        if (currIS.CurrSize < currIS.MaxSize)
        {
            throw new Exception("L'IS n'est pas de taille k");
        }
        if (!AllIs.Contains(currIS))
        {
            AllIs.Add(currIS.Clone());
            for (int i = 0; i < currIS.MaxSize; i++)
            {
                int currVert = currIS.Verts[i];
                for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                {
                    int savedNeigh = Graph.Vois[currVert][v];
                    currIS.RemoveVert(currVert);
                    if (currIS.CanAddVert(savedNeigh))
                    {
                        currIS.AddVert(savedNeigh);
                        CalcAllIsRec(currIS);
                        currIS.ReplaceVert(savedNeigh, currVert);
                    }
                    else
                    {
                        currIS.AddVert(currVert);
                    }
                }
            }
        }
    }

    public List<IndepSet2> CalcAllIs()
    {
        AllIs = new List<IndepSet2>();

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet2 calcedIs = calcIs(i);
            CalcAllIsRec(calcedIs);
        }
        return AllIs;
    }
}