namespace StageLIRIS;

public class IndepSet
{
    public readonly Graph Graph;
    // Tableau contenant tous les sommets
        // 0: Le sommet n'est pas dans l'IS
        // 1: Il est dedans
    public int[] States;
    public readonly int MaxSize;
    public int CurrSize;
    
    public IndepSet(Graph graph, int k)
    {
        this.Graph = graph;
        States = new int[graph.NbVert];
        MaxSize = k;
    }

    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(IndepSet))
        {
            return false;
        }
        else
        {
            return ((IndepSet)obj).States.Equals(this.States) && ((IndepSet)obj).Graph.Equals(this.Graph) && ((IndepSet)obj).MaxSize == this.MaxSize && ((IndepSet)obj).CurrSize == this.CurrSize;
        }
    }

    public IndepSet Clone()
    {
        IndepSet newSet = new IndepSet(Graph, MaxSize);
        for (int i = 0; i < Graph.NbVert; i++)
        {
            newSet.States[i] = States[i];
        }
        return newSet;
    }

    public void Write()
    {
        for (int i = 0; i < Graph.NbVert; i++)
        {
            if (States[i] == 1)
            {
                Console.Write(i + " ");
            }
        }
        Console.WriteLine();
    }
    
    public void WriteStates()
    {
        for (int i = 0; i < Graph.NbVert; i++)
        {
            Console.Write(States[i] + " ");
        }
        Console.WriteLine();
    }

    public bool CanAddVert(int vert)
    {
        // Renvoie si l'IS serait valide en ajouter le sommet vert
        // Pour chaque voisin du sommet vert, on vérifie s'il est déjà dans l'IS
        for (int i = 0; i < Graph.Vois[vert].Count; i++)
        {
            if (Graph.Vois[vert][i] != vert && States[Graph.Vois[vert][i]] == 1)
            {
                return false;
            }
        }
        return true;
    }

    public void AddVert(int vert)
    {
        // Ajoute le sommet à l'IS
        if (States[vert] == 0)
        {
            States[vert] = 1;
            CurrSize++;
        }
    }

    public void RemoveVert(int vert)
    {
        // L'enlève de l'IS
        if (States[vert] == 1)
        {
            States[vert] = 0;
            CurrSize--;
        }
    }
}