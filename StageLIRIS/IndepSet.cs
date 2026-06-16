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

    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(IndepSet))
        {
            return false;
        }
        else
        {
            return ((IndepSet)obj).states.Equals(this.states) && ((IndepSet)obj).graph.Equals(this.graph) && ((IndepSet)obj).maxSize == this.maxSize && ((IndepSet)obj).currSize == this.currSize;
        }
    }

    public IndepSet Clone()
    {
        IndepSet newSet = new IndepSet(graph, maxSize);
        for (int i = 0; i < graph.nbVert; i++)
        {
            newSet.states[i] = states[i];
        }
        return newSet;
    }

    public void Write()
    {
        for (int i = 0; i < graph.nbVert; i++)
        {
            if (states[i] == 1)
            {
                Console.Write(i + " ");
            }
        }
        Console.WriteLine();
    }
    
    public void WriteStates()
    {
        for (int i = 0; i < graph.nbVert; i++)
        {
            Console.Write(states[i] + " ");
        }
        Console.WriteLine();
    }

    public bool CanAddVert(int vert)
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

    public void AddVert(int vert)
    {
        if (states[vert] == 0)
        {
            states[vert] = 1;
            currSize++;
        }
    }

    public void RemoveVert(int vert)
    {
        if (states[vert] == 1)
        {
            states[vert] = 0;
            currSize--;
        }
    }
}