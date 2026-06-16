namespace StageLIRIS;

public class GraphReconfig
{

    public readonly Graph Graph;
    public readonly int K;
    public List<IndepSet> AllIs;

    public GraphReconfig(Graph graph, int k)
    {
        this.Graph = graph;
        this.K = k;
    }

    public void CalcAllIsRec(int from, IndepSet currSet)
    {
        if (currSet.CurrSize == K)
        {
            // On ajoute ce IS à la liste
            AllIs.Add(currSet.Clone());
        }
        else if(from < Graph.NbVert) // Si from >= nbVert: on a pas trouvé de IS de taille K
        {
            // On continue la construction
            // On n'ajoute pas ce sommet à l'IS
            CalcAllIsRec(from+1, currSet);
            // On ajoute ce sommet à l'IS
            if (currSet.CanAddVert(from))
            {
                currSet.AddVert(from);
                CalcAllIsRec(from+1, currSet);
                currSet.RemoveVert(from); // on le renlève (propreté)
            }
            
        }
    }
    
    public List<IndepSet> CalcAllIs()
    {
        AllIs = new List<IndepSet>();
        CalcAllIsRec(0, new IndepSet(Graph, K));
        return AllIs;
    }

    public int[] FindDiffBtwIs(IndepSet set1, IndepSet set2)
    {
        // Renvoie les deux indices des sommets qui diffèrent entre les IS
        // Renvoie une paire de -1 si il y a plus d'une différence
        bool fstDiffFound = false;
        bool sndDiffFound = false;
        int ind1 = -1;
        int ind2 = -1;
        for (int i = 0; i < set1.Graph.NbVert; i++)
        {
            if (!fstDiffFound && set1.States[i] != set2.States[i])
            {
                fstDiffFound = true;
                ind1 = i;
            }
            else if(!sndDiffFound && set1.States[i] != set2.States[i])
            {
                sndDiffFound = true;
                ind2 = i;
            }
            else if (set1.States[i] != set2.States[i])
            {
                return [-1, -1];
            }
        }
        return [ind1, ind2];
    }

    public bool AreISNeighbours(IndepSet set1, IndepSet set2)
    {
        // Il faut être sur que les IS proviennent du même graphe
        // Possibilité de rajouter la vérif plus tard
        int[] inds =  FindDiffBtwIs(set1, set2);
        if (inds[0] == inds[1])
        {
            // Le seul cas où les deux vals sont égales c'est -1
            return false;
        }
        else
        {
            return set1.Graph.Mat[inds[0], inds[1]] == 1;
        }
    }

    public Graph BuildReconfigGraph()
    {
        CalcAllIs();
        Graph graphReconfig = new Graph(AllIs.Count);

        for (int i=0; i < AllIs.Count; i++)
        {
            for (int j = 0; j < AllIs.Count; j++)
            {
                if (!AllIs[i].Equals(AllIs[j]) && AreISNeighbours(AllIs[i], AllIs[j]))
                {
                    graphReconfig.AddEdge(i, j);
                }
            }
        }
        
        return graphReconfig;
    }
}