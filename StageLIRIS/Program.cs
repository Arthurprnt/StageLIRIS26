// See https://aka.ms/new-console-template for more information

using StageLIRIS;

Graph graph = GraphGenerator.getGraph("graphs/graph.txt");

graph.writeMat();
Console.WriteLine();
graph.writeVois();
Console.WriteLine();

List<IndepSet> ensemble = GraphReconfig.getAllIS(graph, 2);

Console.WriteLine("Liste des IS trouvés, il y en a " +  ensemble.Count);
for (int i = 0; i < ensemble.Count; i++)
{
    for (int j = 0; j < graph.nbVert; j++)
    {
        Console.Write(ensemble[i].states[j] + " ");
    }
    Console.WriteLine();
}