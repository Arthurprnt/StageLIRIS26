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
      return CurrSize < MaxSize;
    }

    public static DominantSet? CreateSetAux(Graph graph, int from, DominantSet currSet)
    {
      if(currSet.CurrSize == currSet.MaxSize)
      {
        if(currSet.IsValid()) return currSet.Clone();
        else return null;
      }
      else if(from >= graph.NbVert)
      {
        return null;
      }
      else {
        if(currSet.Get(from)) return CreateSetAux(graph, from+1, currSet);
        else
        {
          DominantSet? returnedSet = CreateSetAux(graph, from+1, currSet);
          if(returnedSet != null) return returnedSet;
          else
          {
            currSet.AddVert(from);
            return CreateSetAux(graph, from+1, currSet);
          }
        }
      }
    }

    public new static DominantSet? CreateSet(Graph graph, int k, int from)
    {
      DominantSet domSet = new DominantSet(graph, k);
      domSet.AddVert(from);
      return CreateSetAux(graph, 0, domSet);
    }

    public override bool IsValid() {
      bool[] vertMarked = new bool[Graph.NbVert];

      for(int i=0; i<Graph.NbVert; i++)
      {
        if(Get(i))
        {
          vertMarked[i] = true;
          for(int v=0; v<Graph.Vois[i].Count(); v++)
          {
            vertMarked[Graph.Vois[i][v]] = true;
          }
        }
      }

      for(int i=0; i<Graph.NbVert; i++)
      {
        if(!vertMarked[i]) return false;
      }

      return true;
    }
}
