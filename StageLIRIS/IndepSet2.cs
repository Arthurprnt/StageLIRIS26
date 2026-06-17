namespace StageLIRIS;

public class IndepSet2
{
    public readonly Graph Graph;
    public readonly int MaxSize;
    public int CurrSize;
    public int[] Verts;
    
    public IndepSet2(Graph graph, int size)
    {
        Graph = graph;
        Verts = new int[size];
        MaxSize = size;
        CurrSize = 0;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(IndepSet2))
        {
            return false;
        }
        else
        {
            IndepSet2 oth = (IndepSet2)obj;
            if(oth.CurrSize != CurrSize) return false;
            for (int i = 0; i < MaxSize; i++)
            {
                if (oth.Verts[i] != Verts[i])
                {
                    return false;
                }
            }
            return true;
        }
    }

    public IndepSet2 Clone()
    {
        IndepSet2 newIndepSet  = new IndepSet2(Graph, MaxSize);
        newIndepSet.CurrSize = CurrSize;
        for (int i = 0; i < CurrSize; i++)
        {
            newIndepSet.Verts[i] = Verts[i];
        }
        return newIndepSet;
    }

    public void Write()
    {
        for (int i = 0; i < MaxSize; i++)
        {
            Console.Write(Verts[i] + " ");
        }
        Console.WriteLine();
    }

    public string ToString()
    {
        string code = "";
        for (int i = 0; i < CurrSize; i++)
        {
            if (i != 0) code += " ";
            code += Verts[i];
        }
        return code;
    }
    
    public int RechDicho(int ind)
    {
        int debut = 0;
        int fin = CurrSize - 1;

        while (debut <= fin)
        {
            int milieu = debut + (fin - debut) / 2;
            if (Verts[milieu] == ind)
            {
                return milieu; 
            }
            else if (Verts[milieu] > ind)
            {
                fin = milieu - 1;
            }
            else
            {
                debut = milieu + 1;
            }
        }
        return -1;
    }
    
    public bool CanAddVert(int vert)
    {
        // Pour chaque sommet déjà dans le IS on vérifie s'il a vert comme voisin
        for (int i = 0; i < CurrSize; i++)
        {
            if (Graph.Mat[Verts[i], vert] == 1)
            {
                return false;
            }
        }
        return true;
    }
    
    public void AddVert(int vert)
    {
        if (CurrSize < MaxSize && RechDicho(vert) == -1)
        {
            // On peut ajouter le sommet
            Verts[CurrSize] = vert;
            for (int i = CurrSize - 1; i >= 0; i--)
            {
                if (Verts[i] > Verts[i + 1])
                {
                    // On echange pour garder le tableau trié
                    (Verts[i], Verts[i + 1]) = (Verts[i + 1], Verts[i]);
                }
            }
            CurrSize++;
        }
    }
    
    public void RemoveVert(int vert)
    {
        int ind = RechDicho(vert);
        if (ind != -1)
        {
            for (int i = ind; i < CurrSize-1; i++)
            {
                Verts[i] = Verts[i + 1];
            }
            CurrSize--;
        }
    }
    
    public void ReplaceVert(int vert, int newVert)
    {
        RemoveVert(vert);
        AddVert(newVert);
    }
}