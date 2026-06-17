namespace StageLIRIS;

public class GraphReconfig2
{
    public readonly Graph Graph;
    public readonly int K;
    public List<IndepSet2> AllIs;
    public Dictionary<string, int> IsToIndex = new Dictionary<string, int>();
    public Graph GraphReconfig;
    
    public GraphReconfig2(Graph graph, int k)
    {
        this.Graph = graph;
        GraphReconfig = new Graph(0);
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

    public int CalcAllIsRec(IndepSet2 currIS, int prevVert)
    {
        if (currIS.CurrSize != currIS.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        if (!AllIs.Contains(currIS))
        {
            int currSetId = AddVertex(currIS);
            for (int i = 0; i < currIS.MaxSize; i++)
            {
                int currVert = currIS.Verts[i];
                for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                {
                    int savedNeigh = Graph.Vois[currVert][v];
                    if (savedNeigh != prevVert)
                    {
                        currIS.RemoveVert(currVert);
                        if (currIS.CanAddVert(savedNeigh))
                        {
                            currIS.AddVert(savedNeigh);
                            int nextSetId = CalcAllIsRec(currIS, currVert);
                            GraphReconfig.AddEdge(currSetId, nextSetId);
                            currIS.ReplaceVert(savedNeigh, currVert);
                        }
                        else
                        {
                            currIS.AddVert(currVert);
                        }
                    }
                }
            }
            return currSetId;
        }
        return IsToIndex[currIS.ToString()];
    }

    public List<IndepSet2> CalcAllIs()
    {
        AllIs = new List<IndepSet2>();
        GraphReconfig = new Graph(0);

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet2 calcedIs = calcIs(i);
            CalcAllIsRec(calcedIs, -1);
        }
        
        GraphReconfig.RebuildMat();
        return AllIs;
    }
}