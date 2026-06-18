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
        
        Graph graph = new Graph(nbVertices);
        
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
        
        Graph graph = new Graph(nbVertices);
        
        // Traitement des aretes
        for (int i = 0; i < nbVertices; i++)
        {
            int from = int.Parse(lines[i].Split(": ")[0])-1;
            string[] toList = lines[i].Split(": ")[1].Split(" ");

            for (int j = 0; j < toList.Length; j++)
            {
                int to = int.Parse(toList[j])-1;
                graph.AddEdge(from, to);
            }
        }

        return graph;
    }
}