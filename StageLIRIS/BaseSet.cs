namespace StageLIRIS;

public abstract class BaseSet
{
    public readonly Graph Graph;
    public long States = 0L;
    public int MaxSize;
    public int CurrSize = 0;

    public BaseSet(Graph graph, int k)
    {
        Graph = graph;
        MaxSize = k;
    }

    public override int GetHashCode()
    {
      return States.GetHashCode();
    }

    public abstract BaseSet Clone();

    public void Set(int ind, bool val)
    {
        // Setter du tableau States
        if (val) States += 1L << ind;
        else States -= 1L << ind;
    }

    public bool Get(int ind)
    {
        // Getter du tableau States
        return (States & (1L << ind)) != 0;
    }
    
    public static bool Get(long states, int ind)
    {
        // Getter du tableau States
        return (states & (1L << ind)) != 0;
    }

    public void Write()
    {
        for (int i = 0; i < 64; i++)
        {
            if (Get(i))
            {
                Console.Write(i + " ");
            }
        }
        Console.WriteLine();
    }

    public static string toString(long states)
    {
        string result = "";
        for (int i = 0; i < 64; i++)
        {
            if (Get(states, i))
            {
                result += i + " ";
            }
        }
        return result;
    }

    public void WriteStates()
    {
        for (int i = 0; i < Graph.NbVert; i++)
        {
            Console.Write(Get(i) + " ");
        }
        Console.WriteLine();
    }

    public abstract bool CanAddVert(int vert);

    public void AddVert(int vert)
    {
        // Ajoute le sommet à l'IS
        if (!Get(vert) && CurrSize < MaxSize)
        {
            Set(vert, true);
            CurrSize++;
        }
    }
    
    public void RemoveVert(int vert)
    {
        // L'enlève de l'IS
        if (Get(vert))
        {
            Set(vert, false);
            CurrSize--;
        }
    }

    public void ReplaceVert(int oldVert, int newVert)
    {
        RemoveVert(oldVert);
        AddVert(newVert);
    }

    public static BaseSet? CreateSet(Graph graph, int k, int from) {
        return null;
    }

    public abstract bool IsValid();
}