namespace StageLIRIS;

public class GraphGenerator
{
    // Permet de générer des graphes en mémoire à partir de fichiers
    // Il y a deux formats de fichiers accepté:
    // Les 'gra':
    // NB_VERTICES: 9
    // NB_EDGES: 14
    //
    // Liste des arêtes au format suivant:
    // Vert1 Vert2
    // Les 'hog':
    // Liste des listes d'adjacences au format suivant:
    // Vert: Vois1 Vois2 ... VoisN

    public static Graph GetGraGraph(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        // Données graph
        int nbVertices = int.Parse(lines[0].Split(' ')[1]);
        int nbEdges = int.Parse(lines[1].Split(' ')[1]);

        Graph graph = new Graph(nbVertices, filePath);

        // Traitement des aretes
        for (int i = 3; i < 3 + nbEdges; i++)
        {
            int from = int.Parse(lines[i].Split(' ')[0]);
            int to = int.Parse(lines[i].Split(' ')[1]);
            graph.AddEdge(from, to);
        }

        return graph;
    }

    public static Graph GetHogGraph(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        // Données graph
        int nbVertices = lines.Length;

        Graph graph = new Graph(nbVertices, filePath);

        // Traitement des aretes
        for (int i = 0; i < nbVertices; i++)
        {
            int from = int.Parse(lines[i].Split(": ")[0]);
            string[] toList = lines[i].Split(": ")[1].Split(" ");

            for (int j = 0; j < toList.Length; j++)
            {
                int to = int.Parse(toList[j]);
                graph.AddEdge(from, to);
            }
        }

        return graph;
    }

    public static Graph GetDotGraph(string filePath)
    {
        // Génère un graphe à partir d'un fichier au format .dot
        string[] lines = File.ReadAllLines(filePath);

        Graph graph = new Graph(0, filePath);

        for (int i = 1; i < lines.Count() - 1; i++)
        {
            string[] lineSplit = lines[i].Split('"');
            int from = int.Parse(lineSplit[1]);
            int to = int.Parse(lineSplit[3]);
            while (graph.NbVert < from + 1 || graph.NbVert < to + 1)
            {
                graph.AddVertex();
            }
            graph.AddEdge(from, to);
        }
        graph.RebuildMat();
        return graph;
    }

    public static Graph GetListgGraph(string[] lines)
    {
        // Génère un graphe à partir d'un fichier output.txt généré par listg
        // Pioche le graphe qui se situe entre les lignes startLine et endLine

        // Données graph
        int nbVertices = lines.Length;

        Graph graph = new Graph(nbVertices);

        // Traitement des aretes
        for (int i = 0; i < nbVertices; i++)
        {
            int from = int.Parse(lines[i].Split(" : ")[0].Replace(" ", ""));
            string[] toList = lines[i].Split(" : ")[1].Split(" ");

            for (int j = 0; j < toList.Length; j++)
            {
                int to = int.Parse(toList[j].Replace(";", ""));
                graph.AddEdge(from, to);
            }
        }

        return graph;
    }

    public static Graph GetPipeGraph()
    {
        int i = 0;
        bool continu = true;

        Graph graph = new Graph(0, "PipeGraph");

        foreach (string line in Benchmark.ReadFromPipeInput())
        {
            if (line == "}")
                continu = false;
            if (i > 0 && continu)
            {
                string[] lineSplit = line.Split('"');
                int from = int.Parse(lineSplit[1]);
                int to = int.Parse(lineSplit[3]);
                while (graph.NbVert < from + 1 || graph.NbVert < to + 1)
                {
                    graph.AddVertex();
                }
                graph.AddEdge(from, to);
            }
            i++;
        }
        graph.RebuildMat();
        return graph;
    }
}
