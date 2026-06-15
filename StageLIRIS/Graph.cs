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
        vois = new List<List<int>>();
        for (int i = 0; i < nbVertices; i++)
        {
            vois.Add(new List<int>());
        }
        nbVert = nbVertices;
    }

    public void writeMat()
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

    public void writeVois()
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
    
    public void addEdge(int from, int to)
    {
        mat[from, to] = 1;
        mat[to, from] = 1;
        
        vois[from].Add(to);
        vois[to].Add(from);
        
        nbEdges++;
    }
    
    public List<int> getDistFromVertice(int vert) {
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
    
    public double getDiameter() {
        int maxi = -1;
        for(int i=0; i<nbVert; i++) {
            int tempMax = -1;
            List<int> dists = getDistFromVertice(i);
            for (int j = 0; j < dists.Count; j++)
            {
                if (dists[j] >= 0 && dists[j] > tempMax)
                {
                    tempMax = dists[j];
                }
            }
            if(tempMax > maxi) {
                maxi = tempMax;
            }
        }
        return maxi;
    }
}