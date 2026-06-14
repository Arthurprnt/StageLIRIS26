// See https://aka.ms/new-console-template for more information

using StageLIRIS;

Graph graph = GraphGenerator.getGraph("graphs/graph.txt");

for (int i = 0; i < graph.nbVert; i++)
{
    for (int j = 0; j < graph.nbVert; j++)
    {
        Console.Write(graph.mat[i, j] + " ");
    }
    Console.WriteLine();
}

Console.WriteLine();

List<int> dists = graph.getDistFromVertice(0);
for (int i = 0; i < dists.Count; i++)
{
    Console.Write(dists[i] + " ");
}
Console.WriteLine();

Console.WriteLine();

Console.WriteLine(graph.getDiameter());