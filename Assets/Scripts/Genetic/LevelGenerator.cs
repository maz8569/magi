using GeneticSharp.Domain;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Terminations;
using GeneticSharp.Infrastructure.Framework.Threading;
using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    private GeneticAlgorithm m_ga;
    private Thread m_gaThread;

    private bool startedEvolution = false;

    [SerializeField] private Transform moduleParent;
    [SerializeField] private Vector3Int size;

    private Pathfinding pathfinding;
    public List<int2> unwalkable = new();
    public IList<PureSlot> slots;

    public List<Module> availableModules = new();

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < moduleParent.childCount; i++)
        {
            availableModules.Add(moduleParent.GetChild(i).GetComponent<Module>());
            availableModules[i].CheckWalkability();
        }

        pathfinding = GetComponent<Pathfinding>();

        var chromosome = new PureChromosome(availableModules, size.x * size.y * size.z);
        var fitness = new PureFitness(moduleParent.GetChild(2), size);
        fitness.Slots[1].module = moduleParent.GetChild(0).GetComponent<Module>();

        fitness.Slots[5].module = moduleParent.GetChild(0).GetComponent<Module>();

        var crossover = new OrderedCrossover();
        var mutation = new ReverseSequenceMutation();
        var selection = new RouletteWheelSelection();

        var population = new Population(50, 100, chromosome);

        m_ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation); 
        m_ga.Termination = new TimeEvolvingTermination(System.TimeSpan.FromHours(1));

        // The fitness evaluation of whole population will be running on parallel.
        m_ga.TaskExecutor = new ParallelTaskExecutor
        {
            MinThreads = 100,
            MaxThreads = 200
        };

        // Everty time a generation ends, we log the best solution.
        m_ga.GenerationRan += delegate
        {
            var enclosement = ((PureChromosome)m_ga.BestChromosome).Enclosement;
            Debug.Log($"Generation: {m_ga.GenerationsNumber} - Enclosement: ${enclosement}");
        };

        slots = ((PureFitness)m_ga.Fitness).Slots;

        VisualizeMap();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;
            if (slot.position.y > 0) continue;

            if (slot.module.isWalkable) unwalkable.Add(new int2(slot.position.x, slot.position.z));
        }

        //StartEvolving();
    }

    public void StartEvolving()
    {
        startedEvolution = true;
        // Starts the genetic algorithm in a separate thread.
        m_gaThread = new Thread(() => m_ga.Start());
        m_gaThread.Start();
    }

    public void VisualizeMap()
    {
        ClearMap();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;

            var module = Instantiate(slot.module, slot.position * 2, slot.module.transform.rotation, transform);
            if (slot.module.name != "Empty") 
            {
                BoxCollider col = module.AddComponent<BoxCollider>();
                col.size = new Vector3(2, 2, 2);
            }
        }
    }

    public void ClearMap()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            DestroyImmediate(transform.GetChild(i).gameObject);
#else
            Destroy(transform.GetChild(i).gameObject); 
#endif
        }
    }

    public void StopExecution()
    {
        if (!startedEvolution) return;
        // When the script is destroyed we stop the genetic algorithm and abort its thread too.
        m_ga.Stop();
        m_gaThread.Abort();
        startedEvolution = false;

        PureChromosome fittest = m_ga.Population.CurrentGeneration.BestChromosome as PureChromosome;
        if (fittest != null)
        {
            for (int i = 0; slots.Count > i; i++)
            {
                slots[i].module = (Module)fittest.GetGene(i).Value;
            } 
        }

        unwalkable = new();

        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i];
            if (slot == null) continue;
            if (slot.position.y > 0) continue;

            if (slot.module.isWalkable) unwalkable.Add(new int2(slot.position.x, slot.position.z));
        }
    }

    private void OnDestroy()
    {
        StopExecution();
    }

    public List<Vector3> FindPath(int2 startPos, int2 endPos)
    {
        if (pathfinding != null)
        {
            return ChangeToListVector(pathfinding.FindPath(size.x, size.z, startPos, endPos, unwalkable.ToArray()));
        }
        else
        {
            return new List<Vector3>();
        }
    }

    private List<Vector3> ChangeToListVector(List<int2> originList)
    {
        List<Vector3> result = new List<Vector3>();

        for (int i = 0; i < originList.Count - 1; i++)
        {
            result.Add(new Vector3(originList[i].x * 2, 0, originList[i].y * 2));
        }
        result.Reverse();

        return result;
    }

    public void CheckConnect()
    {
        Debug.Log(CheckConnectivity.CheckIsAll(size.x, size.z, new int2(0, 0), unwalkable.ToArray()));
    }

}
