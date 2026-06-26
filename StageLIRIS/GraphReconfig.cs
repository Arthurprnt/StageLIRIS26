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
    public List<IndepSet> AllIs = new List<IndepSet>();
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

    public bool isIndepInDict(IndepSet indepSet)
    {
        // Détection rapide de la présence de l'IS dans AllIs
        // Car si un IS a été visité, il est dans le graphe et il possède ainsi un indice dans le dico
        return IsToIndex.ContainsKey(indepSet.States);
    }
    
    public int AddVertex(IndepSet indepSet)
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
    
    public IndepSet? CalcIs(int from)
    {
        // On calcul un IS de taille K contenant le sommet from
        // Sert à avoir un point de départ pour trouver les voisins ensuite
        int ind = 0;
        // On ajoute manuellement le sommet from
        IndepSet indepSet = new IndepSet(Graph, K);
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
            return null;
        }
        return indepSet;
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

    public void handleNeigh(IndepSet currIs, int currVert, int vertNeigh, Queue<IndepSet> queue)
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
                IndepSet nextIsClone = currIs.Clone(); 
                                
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
    
    public List<IndepSet> CalcAllIsRec()
    {
        AllIs = new List<IndepSet>();
        Reconfig = new Graph(0);

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet? calcedIs = CalcIs(i);
            if(calcedIs != null)
            {
              CalcAllIsRecAux(calcedIs);
            }
        }
        
        return AllIs;
    }
    
    public List<IndepSet> CalcAllIsIte()
    {
        // Version itérative du calcul de tous les IS
        AllIs = new List<IndepSet>();
        Reconfig = new Graph(0);
        
        Queue<IndepSet> queue = new Queue<IndepSet>();

        for (int i = 0; i < Graph.NbVert; i++)
        {
            IndepSet? calcedIs = CalcIs(i);
            if (calcedIs != null)
            {
                if (calcedIs.CurrSize != calcedIs.MaxSize) 
                    throw new Exception("L'IS n'est pas de taille k");

                if (!isIndepInDict(calcedIs))
                {
                    AddVertex(calcedIs);
                    queue.Enqueue(calcedIs);
                }
            }
            else
            {
                isValid = false;
            }
        }

        while (queue.Count > 0)
        {
            IndepSet currIs = queue.Dequeue();

            for (int i = 0; i < Graph.NbVert; i++)
            {
                if (currIs.Get(i))
                {
                    int currVert = i;
                    // Pour chaque sommet du IS on check ses potentiels voisins
                    // Mode sliding
                    if (Mode == 'S')
                    {
                        for (int v = 0; v < Graph.Vois[currVert].Count; v++)
                        {
                            handleNeigh(currIs, currVert, Graph.Vois[currVert][v], queue);
                        }
                    }
                    // Mode jumping
                    else if(Mode == 'J')
                    {
                        for (int vertNeigh = 0; vertNeigh < Graph.NbVert; vertNeigh++)
                        {
                            if (vertNeigh != i && !currIs.Get(vertNeigh))
                            {
                                handleNeigh(currIs, currVert, vertNeigh, queue);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Ce mode de déplacement de token n'existe pas (utiliser S ou J).");
                    }
                    
                }
            }
        }
        return AllIs;
    }
}
