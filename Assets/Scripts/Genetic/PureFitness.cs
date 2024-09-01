using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PureFitness : IFitness
{
    public IList<PureSlot> Slots { get; private set; }

    public PureFitness(Transform emptyModule, Vector3Int size)
    {
        var numberOfSlots = size.x * size.y * size.z;
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
        throw new System.NotImplementedException();
    }
}
