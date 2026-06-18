namespace StageLIRIS;

public class GraphReconfig2
{
    // Contrairement à la v1, on calcul directement les arêtes du graphe reconfig à la volée
    // Evite la double boucle pour vérifier si deux IS sont voisins ou non
    
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
        // Ajoute l'indépendant dans le graph reconfig
        if(indepSet.CurrSize != indepSet.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        int indepSetId = GraphReconfig.NbVert;
        // On sauvegarde sont indice de sommet associé
        IsToIndex.Add(indepSet.ToString(), indepSetId);
        AllIs.Add(indepSet.Clone());
        GraphReconfig.AddVertex();
        return indepSetId;
    }

    public IndepSet2 CalcIs(int from)
    {
        // On calcul un IS de taille K contenant le sommet from
        // Sert à avoir un point de départ pour trouver les voisins ensuite
        int ind = 0;
        // On ajoute manuellement le sommet from
        IndepSet2 indepSet = new IndepSet2(Graph, K);
        indepSet.AddVert(from);
        
        while (indepSet.CurrSize < indepSet.MaxSize &&  ind < Graph.NbVert)
        {
            // Maintenant on ajoute tous les sommets qui maintiennent l'indépendance de l'ensemble
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
        // Fonction auxiliaire qui calcule de proche en proche tous les IS de taille K
        // Prends en charge seulement le token sliding, cf GraphReconfig3 pour le token jumping
        if (currIs.CurrSize != currIs.MaxSize) throw new Exception("L'IS n'est pas de taille k");
        if (!AllIs.Contains(currIs))
        {
            int currSetId = AddVertex(currIs);
            for (int i = 0; i < currIs.MaxSize; i++)
            {
                int currVert = currIs.Verts[i];
                // On regarde où peut slide le token courant
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
                            // Comme le nouveau IS à un seul de diff, c'est un voisin
                            // On ajoute l'arête entre les deux sommets
                            // -> Optimisation par rapport à la version 1
                            GraphReconfig.AddEdge(currSetId, nextSetId);
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