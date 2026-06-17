using System.Collections;
using System.Collections.Specialized;

namespace StageLIRIS;

public class IndepSet3
{
    public readonly Graph Graph;
    public BitVector32 States1;
    public BitVector32 States2;
    public readonly int MaxSize;
    public int CurrSize = 0;

    public IndepSet3(Graph graph, int k)
    {
        this.Graph = graph;
        States1 = new BitVector32(0);
        States2 = new BitVector32(0);
        MaxSize = k;
    }
    
    public override bool Equals(object? obj)
    {
        if (obj.GetType() != typeof(IndepSet3))
        {
            return false;
        }
        IndepSet3 oth = (IndepSet3)obj;
        return States1.Equals(oth.States1) && States2.Equals(oth.States2);
    }

    public void Set(int ind, bool val)
    {
        if (ind < 32)
        {
            int mask = 1 << ind;
            States1[mask] = val;   
        }
        else
        {
            int mask = 1 << (ind - 32);
            States2[mask] = val;
        }
        
    }

    public bool Get(int ind)
    {
        if (ind < 32)
        {
            int mask = 1 << ind;
            return States1[mask];   
        }
        else
        {
            int mask = 1 << (ind - 32);
            return States2[mask];
        }
    }

    public IndepSet3 Clone()
    {
        IndepSet3 newSet = new IndepSet3(Graph, MaxSize);
        newSet.CurrSize = CurrSize;
        newSet.States1 = States1;
        newSet.States2 = States2;
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