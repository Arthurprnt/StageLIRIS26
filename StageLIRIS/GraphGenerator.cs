namespace StageLIRIS;

public class GraphGenerator
{

    public static Graph getGraph(string filePath)
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
            graph.addEdge(from, to);
        }

        return graph;
    }
}