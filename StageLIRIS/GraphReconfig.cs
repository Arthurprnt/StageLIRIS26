namespace StageLIRIS;

public class GraphReconfig
{

    public Graph graph;
    public int k;
    public List<IndepSet> allIS;

    public GraphReconfig(Graph graph, int k)
    {
        this.graph = graph;
        this.k = k;
    }

    public void CalcAllISRec(int from, IndepSet currSet)
    {
        if (currSet.currSize == k)
        {
            // On ajoute ce IS à la liste
            allIS.Add(currSet.Clone());
        }
        else if(from < graph.nbVert) // Si from >= nbVert: on a pas trouvé de IS de taille K
        {
            // On continue la construction
            // On n'ajoute pas ce sommet à l'IS
            CalcAllISRec(from+1, currSet);
            // On ajoute ce sommet à l'IS
            if (currSet.CanAddVert(from))
            {
                currSet.AddVert(from);
                CalcAllISRec(from+1, currSet);
                currSet.RemoveVert(from); // on le renlève (propreté)
            }
            
        }
    }
    
    public List<IndepSet> CalcAllIS()
    {
        allIS = new List<IndepSet>();
        CalcAllISRec(0, new IndepSet(graph, k));
        return allIS;
    }

    public int[] FindDiffBtwIS(IndepSet set1, IndepSet set2)
    {
        // Renvoie les deux indices des sommets qui diffèrent entre les IS
        // Renvoie une paire de -1 si il y a plus d'une différence
        bool fstDiffFound = false;
        bool sndDiffFound = false;
        int ind1 = -1;
        int ind2 = -1;
        for (int i = 0; i < set1.graph.nbVert; i++)
        {
            if (!fstDiffFound && set1.states[i] != set2.states[i])
            {
                fstDiffFound = true;
                ind1 = i;
            }
            else if(!sndDiffFound && set1.states[i] != set2.states[i])
            {
                sndDiffFound = true;
                ind2 = i;
            }
            else if (set1.states[i] != set2.states[i])
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
        int[] inds =  FindDiffBtwIS(set1, set2);
        if (inds[0] == inds[1])
        {
            // Le seul cas où les deux vals sont égales c'est -1
            return false;
        }
        else
        {
            return set1.graph.mat[inds[0], inds[1]] == 1;
        }
    }

    public Graph BuildReconfigGraph()
    {
        CalcAllIS();
        Graph graphReconfig = new Graph(allIS.Count);

        for (int i=0; i < allIS.Count; i++)
        {
            for (int j = 0; j < allIS.Count; j++)
            {
                if (!allIS[i].Equals(allIS[j]) && AreISNeighbours(allIS[i], allIS[j]))
                {
                    graphReconfig.AddEdge(i, j);
                }
            }
        }
        
        return graphReconfig;
    }
}