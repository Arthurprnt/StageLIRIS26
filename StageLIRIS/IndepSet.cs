using System.Collections;
using System.Collections.Specialized;

namespace StageLIRIS;

public class IndepSet : BaseSet
{
    // Optimisation de l'IndepSet1
    // Les états sont stocké sous forme de bits dans un long
        // -> Plus comptacte
        // -> Inconvénient: pas plus de 64 sommets dans le graphe de base

    public IndepSet(Graph graph, int k): base(graph, k)
    {
    }
    
    public override bool Equals(object? obj)
    {
        if (obj is not IndepSet oth)
        {
            return false;
        }
        return States == oth.States &&  MaxSize == oth.MaxSize && CurrSize == oth.CurrSize;
    }

    public override int GetHashCode()
    {
      return States.GetHashCode();
    }

    public override IndepSet Clone()
    {
        IndepSet newSet = new IndepSet(Graph, MaxSize);
        newSet.CurrSize = CurrSize;
        newSet.States = States;
        return newSet;
    }
    
    public override bool CanAddVert(int vert)
    {
        // Renvoie si l'IS serait valide en ajouter le sommet vert
        // Pour chaque voisin du sommet vert, on vérifie s'il est déjà dans l'IS
        for (int i = 0; i < Graph.Vois[vert].Count; i++)
        {
            if (Graph.Vois[vert][i] != vert && Get(Graph.Vois[vert][i]))
            {
                return false;
            }
        }
        return true;
    }

    public new static IndepSet? CreateSet(Graph graph, int k, int from) {
        // On calcul un IS de taille K contenant le sommet from
        // Sert à avoir un point de départ pour trouver les voisins ensuite
        int ind = 0;
        // On ajoute manuellement le sommet from
        IndepSet indepSet = new IndepSet(graph, k);
        indepSet.AddVert(from);
        while (indepSet.CurrSize < indepSet.MaxSize &&  ind < graph.NbVert)
        {
            // Maintenant on ajoute tous les sommets qui maintiennent l'indépendance de l'ensemble
            if (ind != from && indepSet.CanAddVert(ind))
            {
                indepSet.AddVert(ind);
            }
            ind++;
        }

        if (!indepSet.IsValid())
        {
            return null;
        }
        return indepSet;
    }

    public override bool IsValid() {
        return CurrSize == MaxSize;
    }
}
