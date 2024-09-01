using UnityEngine;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Randomizations;

public class PureChromosome : ChromosomeBase
{
    private Transform moduleParent;
    private int mapSize;

    public PureChromosome(Transform moduleParent, int mapSize) : base(mapSize)
    {
        this.moduleParent = moduleParent;
        this.mapSize = mapSize;

        var moduleIndexes = RandomizationProvider.Current.GetInts(mapSize, 0, moduleParent.childCount);

        for( int i = 0; i < moduleIndexes.Length; i++)
        {
            ReplaceGene(i, new Gene(moduleParent.GetChild(i)));
        }

    }

    public override IChromosome CreateNew()
    {
        return new PureChromosome(moduleParent, mapSize);
    }

    public override Gene GenerateGene(int geneIndex)
    {
        return new Gene(moduleParent.GetChild(RandomizationProvider.Current.GetInt(0, moduleParent.childCount)));
    }


    public override IChromosome Clone()
    {
        var clone = base.Clone() as PureChromosome;

        return clone;
    }
}


