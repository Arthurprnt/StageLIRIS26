namespace StageLIRIS;

public class IndepSet
{
    public Graph graph;
    public int[] states;
    public int maxSize;
    public int currSize;
    
    public IndepSet(Graph graph, int k)
    {
        this.graph = graph;
        states = new int[graph.nbVert];
        maxSize = k;
    }

    public IndepSet clone()
    {
        IndepSet newSet = new IndepSet(graph, maxSize);
        for (int i = 0; i < graph.nbVert; i++)
        {
            newSet.states[i] = states[i];
        }
        return newSet;
    }

    public void write()
    {
        for (int i = 0; i < graph.nbVert; i++)
        {
            Console.Write(states[i] + " ");
        }
        Console.WriteLine();
    }

    public bool canAddVert(int vert)
    {
        for (int i = 0; i < graph.vois[vert].Count; i++)
        {
            if (graph.vois[vert][i] != vert && states[graph.vois[vert][i]] == 1)
            {
                return false;
            }
        }
        return true;
    }

    public void addVert(int vert)
    {
        if (states[vert] == 0)
        {
            states[vert] = 1;
            currSize++;
        }
    }

    public void removeVert(int vert)
    {
        if (states[vert] == 1)
        {
            states[vert] = 0;
            currSize--;
        }
    }
}