namespace StageLIRIS;

public class Graph
{
    // La matrice est valide tant que AddVert() n'est pas appelé
    // Il faut utilise RebuildMat() pour la reconstruire
    public int[,] Mat;
    public List<List<int>> Vois;
    public int NbVert;
    public int NbEdges;
    // Indique si la matrice est valide ou non
    public bool IsMatSync = true;

    public Graph(int nbVertices)
    {
        Mat = new int[nbVertices, nbVertices];
        Vois = new List<List<int>>(nbVertices);
        for (int i = 0; i < nbVertices; i++)
        {
            Vois.Add(new List<int>());
        }
        NbVert = nbVertices;
    }

    public void WriteMat()
    {
        for (int i = 0; i < NbVert; i++)
        {
            for (int j = 0; j < NbVert; j++)
            {
                Console.Write(Mat[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    public void WriteVois()
    {
        for (int i = 0; i < NbVert; i++)
        {
            Console.Write(i + ": [ ");
            for (int j = 0; j < Vois[i].Count; j++)
            {
                Console.Write(Vois[i][j] + " ");
            }
            Console.WriteLine("]");
        }
    }

    public void AddVertex()
    {
        // /!\ Désyncronise la matrice d'adjacence
        IsMatSync = false;
        Vois.Add(new List<int>(NbVert));
        NbVert++;
    }
    
    public void AddEdge(int from, int to)
    {
        if (IsMatSync)
        {
            Mat[from, to] = 1;
            Mat[to, from] = 1;
        }

        // On incrémente le compteur d'arête seulement si elle n'est pas déjà présente
        int isNewEdge = 0;
        if (!Vois[from].Contains(to))
        {
            Vois[from].Add(to);
            isNewEdge++;
        }
        if (!Vois[to].Contains(from))
        {
            Vois[to].Add(from);
            isNewEdge++;
        }
        
        if(isNewEdge == 2) NbEdges++;
    }

    public void RebuildMat()
    {
        // Resynchronise la matrice d'adjacence
        Mat = new int[NbVert, NbVert];
        for (int i = 0; i < NbVert; i++)
        {
            for (int v = 0; v < Vois[i].Count; v++)
            {
                Mat[i, v] = 1;
            }
        }
        IsMatSync = true;
    }
    
    public List<int> GetDistFromVertice(int vert) {
        // Renvoie les distances entre un sommet et tous les autres du graphe
        List<int> dist = new List<int>();
        List<bool> visited = new List<bool>();

        for (int i = 0; i < NbVert; i++) {
            dist.Add(-1);
            visited.Add(false);
        }

        visited[vert] = true;
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(vert);
        dist[vert] = 0;

        while (queue.Count != 0) {
            int current = queue.Dequeue();
            for (int i = 0; i < Vois[current].Count; i++)
            {
                int neigh = Vois[current][i];
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
        // Renvoie seulement la plus grande distance entre le sommet Vert et un autre sommet du graphe
        // Utile lors du calcul du diamètre pour ne pas reparcourir un tableau inutilement
        // Renvoie la distance et le sommet associé
        int maxDist = 0;
        int maxNeigh = vert;
        int[] dist = new int[NbVert];
        bool[] visited = new bool[NbVert];

        visited[vert] = true;
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(vert);
        dist[vert] = 0;

        while (queue.Count != 0) {
            
            int current = queue.Dequeue();
            for (int i = 0; i < Vois[current].Count; i++)
            {
                int neigh = Vois[current][i];
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
        // Stock dans verts les deux extrémités du diamètre
        verts.Add(-1); verts.Add(-1);
        
        int maxi = -1;
        for(int i=0; i<NbVert; i++)
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
        // Polymorphisme
        return GetDiameter(new List<int>());
    }
}