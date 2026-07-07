namespace StageLIRIS;

public class DominantSet : BaseSet
{
    public DominantSet(Graph graph, int k): base(graph, k)
    {
    }

    public override bool Equals(object? obj)
    {
        if (obj is not DominantSet oth)
        {
            return false;
        }
        return States == oth.States &&  MaxSize == oth.MaxSize && CurrSize == oth.CurrSize;
    }

    public override int GetHashCode()
    {
      return States.GetHashCode();
    }

    public override DominantSet Clone()
    {
        DominantSet newSet = new DominantSet(Graph, MaxSize);
        newSet.CurrSize = CurrSize;
        newSet.States = States;
        return newSet;
    }

    public override bool CanAddVert(int vert)
    {
        return true;
    }
}