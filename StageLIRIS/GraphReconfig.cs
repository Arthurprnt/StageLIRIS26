namespace StageLIRIS;

public class GraphReconfig
{

    public static void getAllISRec(Graph graph, int k, int from, IndepSet currSet, List<IndepSet> ensemble)
    {
        if (currSet.currSize == k)
        {
            // On ajoute ce IS à la liste
            ensemble.Add(currSet.clone());
        }
        else if(from < graph.nbVert) // Si from >= nbVert: on a pas trouvé de IS de taille K
        {
            // On continue la construction
            // On n'ajoute pas ce sommet à l'IS
            getAllISRec(graph, k, from+1, currSet, ensemble);
            // On ajoute ce sommet à l'IS
            if (currSet.canAddVert(from))
            {
                currSet.addVert(from);
                getAllISRec(graph, k, from+1, currSet, ensemble);
                currSet.removeVert(from); // on le renlève (propreté)
            }
            
        }
    }
    
    public static List<IndepSet> getAllIS(Graph graph, int k)
    {
        List<IndepSet> ensemble = new List<IndepSet>();
        getAllISRec(graph, k, 0, new IndepSet(graph, k), ensemble);
        return ensemble;
    }
}