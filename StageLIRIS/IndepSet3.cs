using System.Collections;
using System.Collections.Specialized;

namespace StageLIRIS;

public class IndepSet3
{
    public readonly Graph Graph;
    public long States = 0L;
    public readonly int MaxSize;
    public int CurrSize = 0;

    public IndepSet3(Graph graph, int k)
    {
        this.Graph = graph;
        MaxSize = k;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(IndepSet3))
        {
            return false;
        }
        IndepSet3 oth = (IndepSet3)obj;
        return States == oth.States &&  MaxSize == oth.MaxSize && CurrSize == oth.CurrSize;
    }

    public void Set(int ind, bool val)
    {
        if (val) States += 1L << ind;
        else States -= 1L << ind;
    }

    public bool Get(int ind)
    {
        return (States & (1L << ind)) != 0;
    }

    public IndepSet3 Clone()
    {
        IndepSet3 newSet = new IndepSet3(Graph, MaxSize);
        newSet.CurrSize = CurrSize;
        newSet.States = States;
        return newSet;
    }
    
    public void Write()
    {
        for (int i = 0; i < Graph.NbVert; i++)
        {
            if (Get(i))
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
            Console.Write(Get(i) + " ");
        }
        Console.WriteLine();
    }
    
    public bool CanAddVert(int vert)
    {
        for (int i = 0; i < Graph.Vois[vert].Count; i++)
        {
            if (Graph.Vois[vert][i] != vert && Get(Graph.Vois[vert][i]))
            {
                return false;
            }
        }
        return true;
    }
    
    public void AddVert(int vert)
    {
        if (!Get(vert) && CurrSize < MaxSize)
        {
            Set(vert, true);
            CurrSize++;
        }
    }
    
    public void RemoveVert(int vert)
    {
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
}