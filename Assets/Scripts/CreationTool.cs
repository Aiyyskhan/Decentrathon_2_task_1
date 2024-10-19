using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Genome
{
    public Dictionary<string, float[][]> Skin = new Dictionary<string, float[][]>();
    public Dictionary<string, float[][]> Brain = new Dictionary<string, float[][]>();
}

public class CreationTool : MonoBehaviour
{
    public WaypointContainer waypointCont;
    public GameObject RobotPrefab;
    public int NumRobots = 8;
    private Dictionary<string, int[]> skinParameters = new Dictionary<string, int[]>()
    {
        {"colors", new int[]{6, 3}}
    };
    private Dictionary<string, int[]> brainParameters = new Dictionary<string, int[]>()
    {
        {"iWeights", new int[]{5, 100}},
        {"hWeights", new int[]{100, 100}},
        {"oWeights", new int[]{100, 3}},
        {"hBiases", new int[]{1, 100}}
    };

    private List<GameObject> characterList = new List<GameObject>();
    public List<Genome> Population = new List<Genome>();
    public List<Texture2D> Snapshots = new List<Texture2D>();
    private Vector3 robotPosition;
    private Quaternion robotRotation;

    void Start()
    {
        StartCoroutine(StartInit());
    }

    public void NewStart()
    {
        StartCoroutine(NewStartInit());
    }

    IEnumerator StartInit()
    {
        InitializePopulation(NumRobots, skinParameters, brainParameters);
        yield return new WaitUntil(() => Population.Count == NumRobots);
        PopulationInit();
        yield return new WaitUntil(() => characterList.Count == NumRobots);
        AllSetActive();
    }

    IEnumerator NewStartInit()
    {
        PopulationInit();
        yield return new WaitUntil(() => characterList.Count == NumRobots);
        AllSetActive();
    }

    void PopulationInit()
    {
        robotPosition = transform.position + new Vector3(-9f, 0, 0);
        robotPosition.y = 0.1f;
        robotRotation = transform.rotation; //Quaternion.Euler(0.0f, 0.5f, 0.0f);

        Debug.Log(robotRotation);

        for (int i = 0; i < NumRobots; i++)
        {
            robotPosition.x += 2.0f;
            GameObject character = Instantiate(RobotPrefab, robotPosition, robotRotation);
            character.SetActive(false);

            CarController carScript = character.GetComponentInChildren<CarController>();

            carScript.Id = i;
            carScript.Genome = Population[i];
            carScript.Init();
            carScript.waypointCont = waypointCont;

            characterList.Add(character);
        }
    }

    void AllSetActive()
    {
        foreach (GameObject character in characterList)
        {
            character.SetActive(true);
        }
        CarController.Pause = false;
    }

    public void Pause(bool p)
    {
        CarController.Pause = p;
    }

    public void Evolution(List<int> selectedCarsId, bool onlyLead, float fpProb, float mutProb, float mutLev)
    {
        Debug.Log("Evolve. Population before count: " + Population.Count);
        List<Genome> newPopulation = new List<Genome>(Population.Count);

        int leadIdx = 0;
        for (int i = 0; i < Population.Count; i++)
        {
            Genome p1 = Population[selectedCarsId[leadIdx]];
            Genome p2;
            if (onlyLead)
            {
                p2 = Population[selectedCarsId[Random.Range(0, selectedCarsId.Count)]];
            }
            else
            {
                p2 = Population[Random.Range(0, Population.Count)];
            }

            Mutation(ref p1, mutProb, mutLev);
            Mutation(ref p2, mutProb, mutLev);
            newPopulation.Add(Crossover(p1, p2, fpProb));

            leadIdx++;
            if (leadIdx == selectedCarsId.Count)
            {
                leadIdx = 0;
            }
        }
        if (characterList.Count > 0)
        {
            foreach (var character in characterList)
            {
                Destroy(character);
            }
            characterList.Clear();
        }
        Population.Clear();
        Population = newPopulation;
        // newPopulation.Clear();
        Debug.Log("Evolve. Population after count: " + Population.Count);
    }

    public void InitializePopulation(int populationSize, Dictionary<string, int[]> skinParameters, Dictionary<string, int[]> brainParameters)
    {
        for (int i = 0; i < populationSize; i++)
        {
            Population.Add(_InitializeRandomGenome(skinParameters, brainParameters));
        }
    }

    public Genome Crossover(Genome parent1_genome, Genome parent2_genome, float fpProb)
    {
        Genome child_genome = new Genome();

        _Hybridization(ref child_genome.Skin, parent1_genome.Skin, parent2_genome.Skin, fpProb);
        _Hybridization(ref child_genome.Brain, parent1_genome.Brain, parent2_genome.Brain, fpProb);

        return child_genome;
    }

    public void Mutation(ref Genome genome, float mutProb, float mutLev)
    {
        _Mutate(ref genome.Skin, mutProb, mutLev, new float[2]{0.0f, 1.0f});
        _Mutate(ref genome.Brain, mutProb, mutLev);
    }

    private Genome _InitializeRandomGenome(Dictionary<string, int[]> skinParameters, Dictionary<string, int[]> brainParameters)
    {
        Genome newGenome = new Genome();
        newGenome.Skin = _Initializer(skinParameters, 0.0f, 1.0f);
        newGenome.Brain = _Initializer(brainParameters, -7.0f, 7.0f);
        return newGenome;
    }

    private Dictionary<string, float[][]> _Initializer(Dictionary<string, int[]> parameters, float minVal, float maxVal)
    {
        Dictionary<string, float[][]> result = new Dictionary<string, float[][]>();
        foreach (var parameter in parameters)
        {
            result.Add(parameter.Key, new float[parameter.Value[0]][]);
            for (int i = 0; i < parameter.Value[0]; i++)
            {
                result[parameter.Key][i] = new float[parameter.Value[1]];
                for (int j = 0; j < parameter.Value[1]; j++)
                {
                    result[parameter.Key][i][j] = Random.Range(minVal, maxVal);
                }
            }
        }
        return result;
    }

    private void _Hybridization(ref Dictionary<string, float[][]> ch_chromo, Dictionary<string, float[][]> p1_chromo, Dictionary<string, float[][]> p2_chromo, float fpProb)
    {
        foreach (var gene in p1_chromo)
        {
            int rows = gene.Value.Length;
            ch_chromo.Add(gene.Key, new float[rows][]);

            for (int i = 0; i < rows; i++)
            {
                int columns = gene.Value[i].Length;
                ch_chromo[gene.Key][i] = new float[columns];
                for (int j = 0; j < columns; j++)
                {
                    if (Random.Range(0.0f, 1.0f) < fpProb)
                    {
                        ch_chromo[gene.Key][i][j] = gene.Value[i][j];
                    }
                    else
                    {
                        ch_chromo[gene.Key][i][j] = p2_chromo[gene.Key][i][j];
                    }
                }
            }
        }
    }

    private void _Mutate(ref Dictionary<string, float[][]> chromo, float mutProb, float mutLev, float[] minmax = null)
    {
        foreach (var gene in chromo)
        {
            int rows = gene.Value.Length;
            for (int i = 0; i < rows; i++)
            {
                int columns = gene.Value[i].Length;
                for (int j = 0; j < columns; j++)
                {
                    if (Random.Range(0.0f, 1.0f) < mutProb)
                    {
                        if (minmax != null)
                        {
                            chromo[gene.Key][i][j] = Mathf.Clamp(gene.Value[i][j] + Random.Range(-mutLev, mutLev), minmax[0], minmax[1]);
                        }
                        else
                        {
                            chromo[gene.Key][i][j] = gene.Value[i][j] + Random.Range(-mutLev, mutLev);
                        }
                    }
                }
            }
        }
    }
}
