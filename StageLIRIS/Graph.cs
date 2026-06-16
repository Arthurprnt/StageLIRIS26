using System.Collections;

namespace StageLIRIS;

public class Graph
{
    public int[,] mat;
    public List<List<int>> vois;
    public int nbVert;
    public int nbEdges;

    public Graph(int nbVertices)
    {
        mat = new int[nbVertices, nbVertices];
        vois = new List<List<int>>(nbVertices);
        for (int i = 0; i < nbVertices; i++)
        {
            vois.Add(new List<int>());
        }
        nbVert = nbVertices;
    }

    public void WriteMat()
    {
        for (int i = 0; i < nbVert; i++)
        {
            for (int j = 0; j < nbVert; j++)
            {
                Console.Write(mat[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public void WriteVois()
    {
        for (int i = 0; i < nbVert; i++)
        {
            Console.Write(i + ": [ ");
            for (int j = 0; j < vois[i].Count; j++)
            {
                Console.Write(vois[i][j] + " ");
            }
            Console.WriteLine("]");
        }
    }
    
    public void AddEdge(int from, int to)
    {
        mat[from, to] = 1;
        mat[to, from] = 1;
        
        vois[from].Add(to);
        vois[to].Add(from);
        
        nbEdges++;
    }
    
    public List<int> GetDistFromVertice(int vert) {
        List<int> dist = new List<int>();
        List<bool> visited = new List<bool>();

        for (int i = 0; i < nbVert; i++) {
            dist.Add(-1);
            visited.Add(false);
        }

        visited[vert] = true;
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(vert);
        dist[vert] = 0;

        while (queue.Count != 0) {
            
            int current = queue.Dequeue();
            for (int i = 0; i < vois[current].Count; i++)
            {
                int neigh = vois[current][i];
                if (neigh != current && !visited[neigh])
                {
                    visited[neigh] = true;
                    dist[neigh] = dist[current] + 1;
                    queue.Enqueue(neigh);
                }
            }
        }
        return dist;
    }
    
    public int[] GetMaxDistFromVertice(int vert)
    {
        int maxDist = 0;
        int maxNeigh = vert;
        List<int> dist = new List<int>();
        List<bool> visited = new List<bool>();

        for (int i = 0; i < nbVert; i++) {
            dist.Add(-1);
            visited.Add(false);
        }

        visited[vert] = true;
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(vert);
        dist[vert] = 0;

        while (queue.Count != 0) {
            
            int current = queue.Dequeue();
            for (int i = 0; i < vois[current].Count; i++)
            {
                int neigh = vois[current][i];
                if (neigh != current && !visited[neigh])
                {
                    visited[neigh] = true;
                    int newDist = dist[current] + 1;
                    dist[neigh] = newDist;
                    if (newDist > maxDist)
                    {
                        maxDist = newDist;
                        maxNeigh = neigh;
                    }
                    queue.Enqueue(neigh);
                }
            }
        }
        return [maxDist, maxNeigh];
    }
    
    public int GetDiameter(List<int> verts) {
        verts.Add(-1); verts.Add(-1);
        
        List<int> tempVerts = new List<int>();
        tempVerts.Add(-1); tempVerts.Add(-1);
        
        int maxi = -1;
        for(int i=0; i<nbVert; i++)
        {
            // maxs = [maxDist, maxNeigh]
            int[] maxs = GetMaxDistFromVertice(i);
            int tempMax = maxs[0];
            if(tempMax > maxi) {
                maxi = tempMax;
                verts[0] = i;
                verts[1] = maxs[1];
            }
        }
        return maxi;
    }
    
    public int GetDiameter()
    {
        return GetDiameter(new List<int>());
    }
}