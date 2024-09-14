using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

public class MapBehaviour : MonoBehaviour, ILevelGenerator
{
    [SerializeField] private Transform moduleParent;
    [SerializeField] private Vector3Int size;
    [SerializeField] private GameObject player;

    private List<Slot> slots;
    private Module spawnPoint = null;
    private Pathfinding pathfinding;
    public List<int2> unwalkable = new();

    private void Awake()
    {
        pathfinding = GetComponent<Pathfinding>();
        List<Module> modules = new();

        foreach (Transform module in moduleParent)
        {
            modules.Add(module.GetComponent<Module>());
        }

        slots = new List<Slot>();

        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                for (int z = 0; z < size.z; z++)
                {
                    if(x == 0 || x == size.x - 1 || z == 0 || z == size.z - 1)
                    {
                        slots.Add(new Slot(new Vector3Int(x, y, z), new Module[] { modules[0] }, false));
                    }
                    else
                    {
                        slots.Add(new Slot(new Vector3Int(x, y, z), modules.ToArray(), false));
                    }
                }
            }
        }
    }

    private void Start()
    {
        CheckEntropy();
        StartCoroutine(VisualizeMap());
    }


    public void CheckEntropy()
    {
        List<Slot> temp = new(slots);
        temp.RemoveAll(s => s.collapsed);
        temp.Sort((a, b) => a.modules.Length - b.modules.Length);
        temp.RemoveAll(a => a.modules.Length != temp[0].modules.Length);

        if(temp.Count > 0) CollapseSlot(temp);
    }

    private void CollapseSlot(List<Slot> temp)
    {
        Slot slotToCollapse = temp[UnityEngine.Random.Range(0, temp.Count)];
        slotToCollapse.collapsed = true;

        Module selectedModule = slotToCollapse.modules[UnityEngine.Random.Range(0, slotToCollapse.modules.Length)];
        slotToCollapse.modules = new Module[] { selectedModule };

        UpdateGeneration(slotToCollapse);

    }

    private void UpdateGeneration(Slot collapsedSlot)
    {
        List<Slot> newGenerationSlots = new() { collapsedSlot };

        while (newGenerationSlots.Count > 0)
        {
            Slot cur = newGenerationSlots[0];
            newGenerationSlots.RemoveAt(0);

            foreach (var dir in ValidDirections(cur))
            {
                Slot otherSlot = FindSlotAtPosition(cur, dir);

                if (otherSlot == null) continue;

                List<Module> possibleModules = PossibleModules(cur, dir);

                foreach (var possibleModule in otherSlot.modules)
                {
                    if (!possibleModules.Contains(possibleModule))
                    {
                        List<Module> temp = otherSlot.modules.ToList();
                        temp.Remove(possibleModule);
                        otherSlot.modules = temp.ToArray();

                        if (!newGenerationSlots.Contains(otherSlot)) newGenerationSlots.Add(otherSlot);
                    }
                }
            }
        }

        CheckEntropy();
    }

    private List<Vector3Int> ValidDirections(Slot slot)
    {
        List<Vector3Int> validDirections = new();

        if (slot.position.x < size.x - 1) validDirections.Add(new Vector3Int(1, 0, 0));
        if (slot.position.x > 0) validDirections.Add(new Vector3Int(-1, 0, 0));
        if (slot.position.z < size.z - 1) validDirections.Add(new Vector3Int(0, 0, 1));
        if (slot.position.z > 0) validDirections.Add(new Vector3Int(0, 0, -1));
        if (slot.position.y < size.y -1) validDirections.Add(new Vector3Int(0, 1, 0));
        if (slot.position.y > 0) validDirections.Add(new Vector3Int(0, -1, 0));

        return validDirections;
    }

    private Slot FindSlotAtPosition(Slot slot, Vector3Int dir)
    {
        return slots.Find(s => s.position == slot.position + dir && !s.collapsed);
    }

    private List<Module> PossibleModules(Slot slot, Vector3Int dir)
    {
        List<Module> modules = new();
        foreach (var module in slot.modules)
        {
            modules = modules.Union(module.GetNeighboursFromDirection(dir)).ToList();
        }

        return modules;
    }

    private IEnumerator VisualizeMap()
    {
        foreach (var slotToCollapse in slots)
        {
            if (slotToCollapse.modules[0].isWalkable)
            {

                var module = Instantiate(slotToCollapse.modules[0], slotToCollapse.position * 2, slotToCollapse.modules[0].transform.rotation, transform);
                BoxCollider col = module.gameObject.AddComponent<BoxCollider>();
                col.size = new Vector3(2, 2, 2);

                if (spawnPoint == null) spawnPoint = module;

                yield return new WaitForSeconds(0.025f);
            }
            else
            {
                if(slotToCollapse.position.y == 0) {
                    unwalkable.Add(new int2(slotToCollapse.position.x, slotToCollapse.position.z));
                }
            }
        }

        SpawnPlayer();
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

    private void SpawnPlayer()
    {
        player.transform.position = spawnPoint.transform.position;
    }
}
