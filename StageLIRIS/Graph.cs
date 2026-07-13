using System.Text;

namespace StageLIRIS;

public class Graph
{
    // La matrice est valide tant que AddVert() n'est pas appelé
    // Il faut utilise RebuildMat() pour la reconstruire
    public string Name;
    public List<List<int>> Mat;
    public List<List<int>> Vois;
    public int NbVert;
    public int NbEdges;

    // Indique si la matrice est valide ou non
    public bool IsMatSync = true;
    public int NbIs = 0;

    public Graph(int nbVertices, string name)
    {
        Mat = new List<List<int>>(nbVertices);
        Vois = new List<List<int>>(nbVertices);
        for (int i = 0; i < nbVertices; i++)
        {
            Vois.Add(new List<int>());
            Mat.Add(new List<int>(nbVertices));
            for (int j = 0; j < nbVertices; j++)
            {
                Mat[i].Add(0);
            }
        }
        NbVert = nbVertices;
        Name = name;
    }

    public Graph(int nbVertices)
        : this(nbVertices, "NoName") { }

    public override int GetHashCode()
    {
        // Algorithme de Weisfeiler-Lehman
        if (NbVert == 0)
            return 0;

        // L'étiquette de départ est le degré du sommet (nombre de voisins)
        int[] nodeLabels = new int[NbVert];
        for (int i = 0; i < NbVert; i++)
        {
            nodeLabels[i] = Vois[i].Count;
        }

        // Liste pour accumuler les signatures structurelles générées au fil des étapes
        List<int> allFeatureHashes = new List<int>(nodeLabels);

        // Raffinement des étiquettes (2 itérations suffisent pour la majorité des cas)
        for (int iter = 0; iter < 2; iter++)
        {
            int[] nextLabels = new int[NbVert];

            for (int currentVert = 0; currentVert < NbVert; currentVert++)
            {
                // Récupérer les étiquettes actuelles des voisins du sommet courant
                // Le OrderBy() garantit l'invariance par permutation (isomorphisme)
                var sortedNeighborLabels = Vois[currentVert]
                    .Select(v => nodeLabels[v])
                    .OrderBy(label => label);

                // Combiner l'étiquette actuelle du sommet avec celles triées de ses voisins
                int combinedHash = nodeLabels[currentVert];
                foreach (int neighborLabel in sortedNeighborLabels)
                {
                    combinedHash = HashCode.Combine(combinedHash, neighborLabel);
                }

                nextLabels[currentVert] = combinedHash;
            }

            // Mise à jour des étiquettes pour la prochaine itération
            nodeLabels = nextLabels;
            allFeatureHashes.AddRange(nodeLabels);
        }

        // Agrégation finale de toutes les caractéristiques
        // On trie le catalogue entier pour gommer l'ordre initial des sommets dans la liste Vois
        allFeatureHashes.Sort();

        int finalHash = 17;
        foreach (int hash in allFeatureHashes)
        {
            finalHash = finalHash * 31 + hash;
        }

        return finalHash;
    }

    public void WriteMat()
    {
        for (int i = 0; i < NbVert; i++)
        {
            for (int j = 0; j < NbVert; j++)
            {
                Console.Write(Mat[i][j] + " ");
            }
            Console.WriteLine();
        }
    }

    public Graph Clone()
    {
        Graph newGraph = new Graph(NbVert, Name);
        for (int i = 0; i < NbVert; i++)
        {
            for (int v = 0; v < Vois[i].Count(); v++)
            {
                newGraph.AddEdge(i, Vois[i][v]);
            }
        }
        return newGraph;
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

    public string ToDot()
    {
        // Renvoie le graphe sous le format .dot
        StringBuilder dot = new StringBuilder();
        dot.AppendLine("graph G {");

        for (int i = 0; i < NbVert; i++)
        {
            if (Vois[i].Count() == 0)
                dot.AppendLine($"  \"{i}\";");
        }

        for (int i = 0; i < Vois.Count; i++)
        {
            for (int j = 0; j < Vois[i].Count; j++)
            {
                if (Vois[i][j] > i)
                    dot.AppendLine($"  \"{i}\" -- \"{Vois[i][j]}\";");
            }
        }

        dot.AppendLine("}");
        return dot.ToString();
    }

    public void AddVertex()
    {
        // /!\ Désyncronise la matrice d'adjacence
        IsMatSync = false;
        Vois.Add(new List<int>(NbVert));
        NbVert++;
    }

    public void RemoveVertex(int vert)
    {
        // On commence par supprimer toutes les arêtes pouvant contenir vert
        for (int i = 0; i < NbVert; i++)
        {
            RemoveEdge(vert, i);
        }

        // On supprime ensuite la ligne et colonne de la matrice
        Mat.RemoveAt(vert);
        for (int i = 0; i < NbVert - 1; i++)
        {
            Mat[i].RemoveAt(vert);
        }

        // Enfin on renomme les voisins dans les list de voisins
        Vois.RemoveAt(vert);
        for (int v = 0; v < NbVert - 1; v++)
        {
            for (int voisInd = 0; voisInd < Vois[v].Count(); voisInd++)
            {
                if (Vois[v][voisInd] > vert)
                    Vois[v][voisInd]--;
            }
        }

        NbVert--;
    }

    public void AddEdge(int from, int to)
    {
        if (IsMatSync)
        {
            Mat[from][to] = 1;
            Mat[to][from] = 1;
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

        if (isNewEdge == 2)
            NbEdges++;
    }

    public void RemoveEdge(int from, int to)
    {
        if (IsMatSync)
        {
            Mat[from][to] = 0;
            Mat[to][from] = 0;
        }

        // Supprime les potentiels voisins tout en faisant la vérification
        bool b1 = Vois[from].Remove(to);
        bool b2 = Vois[to].Remove(from);
        if (b1 && b2)
            NbEdges--;
    }

    public void RebuildMat()
    {
        // Resynchronise la matrice d'adjacence
        Mat = new List<List<int>>(NbVert);
        for (int i = 0; i < NbVert; i++)
        {
            Mat.Add(new List<int>(NbVert));
            for (int j = 0; j < NbVert; j++)
            {
                Mat[i].Add(0);
            }
            for (int v = 0; v < Vois[i].Count; v++)
            {
                Mat[i][Vois[i][v]] = 1;
            }
        }
        IsMatSync = true;
    }

    public List<int> GetDistFromVertice(int vert)
    {
        // Renvoie les distances entre un sommet et tous les autres du graphe
        List<int> dist = new List<int>();
        List<bool> visited = new List<bool>();

        for (int i = 0; i < NbVert; i++)
        {
            dist.Add(-1);
            visited.Add(false);
        }

        visited[vert] = true;
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(vert);
        dist[vert] = 0;

        while (queue.Count != 0)
        {
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

    public int[] GetMaxDistFromVertice(int vert, bool[] startVisited)
    {
        // Renvoie seulement la plus grande distance entre le sommet Vert et un autre sommet du graphe
        // Utile lors du calcul du diamètre pour ne pas reparcourir un tableau inutilement
        // Renvoie la distance et le sommet associé
        int maxDist = 0;
        int maxNeigh = vert;
        int[] dist = new int[NbVert];
        bool[] visited = new bool[NbVert];

        visited[vert] = true;
        startVisited[vert] = true;
        Queue<int> queue = new Queue<int>();
        queue.Enqueue(vert);
        dist[vert] = 0;

        while (queue.Count != 0)
        {
            int current = queue.Dequeue();
            for (int i = 0; i < Vois[current].Count; i++)
            {
                int neigh = Vois[current][i];
                if (!visited[neigh])
                {
                    visited[neigh] = true;
                    startVisited[neigh] = true;
                    int newDist = dist[current] + 1;
                    dist[neigh] = newDist;
                    maxDist = newDist;
                    maxNeigh = neigh;
                    queue.Enqueue(neigh);
                }
            }
        }
        return [maxDist, maxNeigh];
    }

    public int[] GetMaxDistFromVertice(int vert)
    {
        bool[] visited = new bool[NbVert];
        return GetMaxDistFromVertice(vert, visited);
    }

    public int GetDiameter(List<int> verts)
    {
        // Stock dans verts les deux extrémités du diamètre
        if (verts.Count < 2)
        {
            verts.Add(-1);
            verts.Add(-1);
        }

        int maxi = -1;
        for (int i = 0; i < NbVert; i++)
        {
            // maxs = [maxDist, maxNeigh]
            int[] maxs = GetMaxDistFromVertice(i);
            int tempMax = maxs[0];
            if (tempMax > maxi)
            {
                maxi = tempMax;
                verts[0] = i;
                verts[1] = maxs[1];
            }
        }
        return maxi;
    }

    public int EstimateDiameter()
    {
        if (NbVert <= 1)
            return 0;
        int nbIteration = (int)Math.Sqrt(NbVert);
        if (nbIteration < 1)
            throw new Exception("Il faut au moins itérer une fois sur le graph.");
        // Stock dans verts les deux extrémités du diamètre
        /*if (verts.Count < 2)
        {
            verts.Add(-1); verts.Add(-1);
        }*/

        bool[] visited = new bool[NbVert];

        Random rnd = new Random();
        int selectedVertex = rnd.Next() % NbVert;
        int diameter = 0;

        for (int i = 0; i < nbIteration; i++)
        {
            int[] maxs = GetMaxDistFromVertice(selectedVertex, visited);
            diameter = maxs[0];
            /*verts[0] = selectedVertex;
            verts[1] = maxs[1];*/
            selectedVertex = maxs[1];
        }

        for (int vert = 0; vert < NbVert; vert++)
        {
            if (!visited[vert])
            {
                for (int i = 0; i < nbIteration; i++)
                {
                    int[] maxs = GetMaxDistFromVertice(selectedVertex, visited);
                    if (maxs[0] > diameter)
                    {
                        diameter = maxs[0];
                        /*verts[0] = vert;
                        verts[1] = maxs[1];*/
                    }
                }
            }
        }

        return diameter;
    }

    public int GetDiameter()
    {
        // Polymorphisme
        return GetDiameter(new List<int>());
    }

    public int DegMin()
    {
        int mini = Vois[0].Count;
        for (int i = 1; i < Vois.Count; i++)
        {
            if (Vois[i].Count < mini)
                mini = Vois[i].Count;
        }

        return mini;
    }

    public int DegMax()
    {
        int maxi = Vois[0].Count;
        for (int i = 1; i < Vois.Count; i++)
        {
            if (Vois[i].Count > maxi)
                maxi = Vois[i].Count;
        }

        return maxi;
    }

    public int CountTriangles()
    {
        int somme = 0;
        for (int i = 0; i < NbVert; i++)
        {
            for (int j = i + 1; j < NbVert; j++)
            {
                for (int k = j + 1; k < NbVert; k++)
                {
                    if (Mat[i][j] == 1 && Mat[j][k] == 1 && Mat[k][i] == 1)
                        somme++;
                }
            }
        }

        return somme;
    }

    public Graph UpgradeGraph(IndepSet alpha, IndepSet beta)
    {
        // Implémente la construction du 3. du papier Graph track description
        // A noter que la fonction modifie directement les IS alpha et beta (penser à la cloner avant l'appel si besoin)

        Graph upgradedGraph = Clone();
        int prevLastVert = NbVert - 1;

        for (int i = 0; i < 10; i++)
        {
            upgradedGraph.AddVertex();
            if (i == 1)
            {
                for (int u = 0; u < NbVert; u++)
                {
                    if (!alpha.Get(u))
                        upgradedGraph.AddEdge(u, upgradedGraph.NbVert - 1);
                }
            }
            else if (i == 5)
            {
                for (int v = 0; v < NbVert; v++)
                {
                    if (!beta.Get(v))
                        upgradedGraph.AddEdge(v, upgradedGraph.NbVert - 1);
                }
            }
        }

        int vertZero = prevLastVert + 1;
        upgradedGraph.AddEdge(vertZero, vertZero + 3);
        upgradedGraph.AddEdge(vertZero, vertZero + 4);
        upgradedGraph.AddEdge(vertZero, vertZero + 6);
        upgradedGraph.AddEdge(vertZero, vertZero + 7);
        upgradedGraph.AddEdge(vertZero, vertZero + 9);
        upgradedGraph.AddEdge(vertZero + 1, vertZero + 4);
        upgradedGraph.AddEdge(vertZero + 1, vertZero + 5);
        upgradedGraph.AddEdge(vertZero + 1, vertZero + 8);
        upgradedGraph.AddEdge(vertZero + 1, vertZero + 9);
        upgradedGraph.AddEdge(vertZero + 2, vertZero + 5);
        upgradedGraph.AddEdge(vertZero + 2, vertZero + 6);
        upgradedGraph.AddEdge(vertZero + 2, vertZero + 7);
        upgradedGraph.AddEdge(vertZero + 2, vertZero + 9);
        upgradedGraph.AddEdge(vertZero + 3, vertZero + 6);
        upgradedGraph.AddEdge(vertZero + 3, vertZero + 7);
        upgradedGraph.AddEdge(vertZero + 3, vertZero + 8);
        upgradedGraph.AddEdge(vertZero + 3, vertZero + 9);
        upgradedGraph.AddEdge(vertZero + 4, vertZero + 8);
        upgradedGraph.AddEdge(vertZero + 4, vertZero + 9);
        upgradedGraph.AddEdge(vertZero + 5, vertZero + 7);
        upgradedGraph.AddEdge(vertZero + 6, vertZero + 8);
        upgradedGraph.AddEdge(vertZero + 6, vertZero + 9);

        alpha.MaxSize += 3;
        alpha.AddVert(prevLastVert + 7);
        alpha.AddVert(prevLastVert + 8);
        alpha.AddVert(prevLastVert + 9);

        beta.MaxSize += 3;
        beta.AddVert(prevLastVert + 7);
        beta.AddVert(prevLastVert + 8);
        beta.AddVert(prevLastVert + 9);

        upgradedGraph.RebuildMat();
        return upgradedGraph;
    }

    public int GetDiamOfReconfig(int k, char mode, char setType, bool estimate = true)
    {
        GraphReconfig reconfig = new GraphReconfig(this, k, mode, setType);
        reconfig.CalcAllSetsIte();
        if (estimate)
            return reconfig.Reconfig.EstimateDiameter();
        else
            return reconfig.Reconfig.GetDiameter();
    }

    private int TryWithEdge(
        int k,
        char mode,
        char setType,
        bool estimate,
        int i,
        int j,
        List<Graph> listOfGra,
        int biggestDiam
    )
    {
        AddEdge(i, j);
        int bigDiam = biggestDiam;
        int tempDiam = GetDiamOfReconfig(k, mode, setType, estimate);
        if (tempDiam > bigDiam)
        {
            listOfGra.Clear();
            listOfGra.Add(Clone());
            bigDiam = tempDiam;
        }
        else if (tempDiam == bigDiam)
        {
            listOfGra.Add(Clone());
        }
        RemoveEdge(i, j);
        if (Mat[i][j] > 0)
            throw new Exception("err de replacement d'arete");
        return bigDiam;
    }

    public List<Graph> SearchLocally(int k, char mode, char setType, bool estimate = true)
    {
        if (!IsMatSync)
            throw new Exception("The matrice must be syncronised to search locally.");

        int biggestDiam = GetDiamOfReconfig(k, mode, setType, estimate);
        List<Graph> graphsFound = new List<Graph>();

        for (int i = 0; i < NbVert; i++)
        {
            for (int j = i + 1; j < NbVert; j++)
            {
                if (Mat[i][j] == 0)
                {
                    biggestDiam = TryWithEdge(
                        k,
                        mode,
                        setType,
                        estimate,
                        i,
                        j,
                        graphsFound,
                        biggestDiam
                    );
                }
                else
                {
                    // Il y a déjà une arête entre i et j
                    int prevNbEdges = NbEdges;
                    RemoveEdge(i, j);
                    for (int m = 0; m < NbVert; m++)
                    {
                        for (int l = m + 1; l < NbVert; l++)
                        {
                            if (!(m == i && l == j) && !(m == j && l == i) && Mat[m][l] == 0)
                            {
                                biggestDiam = TryWithEdge(
                                    k,
                                    mode,
                                    setType,
                                    estimate,
                                    m,
                                    l,
                                    graphsFound,
                                    biggestDiam
                                );
                            }
                        }
                    }
                    AddEdge(i, j);
                }
            }
        }

        return graphsFound;
    }

    public List<Graph> DeepSearchLocally(
        int depth,
        int k,
        char mode,
        char setType,
        bool estimate = true
    )
    {
        int tryLeft = depth - 1;
        int biggestDiam = GetDiamOfReconfig(k, mode, setType, estimate);
        List<Graph> graCollection = SearchLocally(k, mode, setType, estimate);
        List<Graph> graFound = new List<Graph>();
        List<int> graStocked = new List<int>();
        while (tryLeft > 0)
        {
            graStocked.Clear();
            Console.WriteLine("taille de la liste: " + graCollection.Count());
            for (int i = 0; i < graCollection.Count(); i++)
            {
                graFound.AddRange(graCollection[i].SearchLocally(k, mode, setType, estimate));
            }
            graCollection.Clear();
            //int biggestDiam = graFound[0].GetDiamOfReconfig(k, mode, setType, estimate);
            //graCollection.Add(graFound[0]);
            for (int i = 1; i < graFound.Count(); i++)
            {
                if (graStocked.Contains(graFound[i].GetHashCode()))
                    continue;
                int tempDiam = graFound[i].GetDiamOfReconfig(k, mode, setType, estimate);
                if (tempDiam > biggestDiam)
                {
                    graCollection.Clear();
                    graCollection.Add(graFound[i]);
                    graStocked.Add(graFound[i].GetHashCode());
                    biggestDiam = tempDiam;
                }
                else if (tempDiam == biggestDiam)
                {
                    graCollection.Add(graFound[i]);
                    graStocked.Add(graFound[i].GetHashCode());
                }
            }
            //graCollection = [..graFound];
            graFound.Clear();
            tryLeft--;
        }
        return graCollection;
    }
}
