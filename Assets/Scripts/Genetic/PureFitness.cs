using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PureFitness : IFitness
{
    public IList<PureSlot> Slots { get; private set; }
    public Vector3Int size { get; private set; }

    public PureFitness(Transform emptyModule, Vector3Int size)
    {
        var numberOfSlots = size.x * size.y * size.z;
        this.size = size;

        Slots = new List<PureSlot>(numberOfSlots);

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    var slot = new PureSlot { position = new Vector3Int(x, y, z), module = emptyModule.GetComponent<Module>() };
                    Slots.Add(slot);
                } 
            }
        }
    }

    public double Evaluate(IChromosome chromosome)
    {
        var genes = chromosome.GetGenes();
        List<int2> unwalkable = new();

        for (int i = 0; i < genes.Length; i++) 
        {
            var gen = genes[i];
            if(!((Module)gen.Value).isWalkable) unwalkable.Add(new int2(i % size.z, i / size.z));        
        }

        double fitness = CheckConnectivity.CheckIsAll(size.x, size.z, new int2(0, 0), unwalkable.ToArray());

        ((PureChromosome)chromosome).Enclosement = fitness;

        if (fitness < 0)
        {
            fitness = 0;
        }

        return fitness;
    }
}
