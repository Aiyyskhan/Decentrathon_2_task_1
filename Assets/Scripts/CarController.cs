using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarController : MonoBehaviour
{   
    [SerializeField]
    private float angularVelocity = 0.5f;
    [SerializeField]
    private float moveSpeed = 4f;

    [SerializeField] private Brain brain;
    [SerializeField] private Sensor sensor;
    [SerializeField] private Renderer renderer;

    public List<Transform> waypoints;
    public int currentWaypointIdx;
    public float waypointRange = 10f;
    private float currentAngle = 0f;
    public WaypointContainer waypointCont { get; set; }

    public int Id { get; set; }
    public Genome Genome { get; set; }
    public static bool Pause { private get; set; } = true;

    private float rotationY = 0.0f;
    private float forward = 0.0f;

    // private Rigidbody rb;
    Vector3 moveAxis;
    private Vector3 oldPosition;
    private Vector3 originPosition;
    private Quaternion originRotation;
    private Material[] materials;

    void Start()
    {
        // rb = GetComponent<Rigidbody>();
        waypoints = waypointCont.waypoints;
        currentWaypointIdx = 0;

        // transform.eulerAngles = new Vector3(0f, 54f, 0f);

        originPosition = this.transform.position;
        originRotation = this.transform.rotation;
        
        rotationY = 0.0f;
    }

    public void Init()
    {
        BrainInit();
        ColorInit();
    }

    void BrainInit()
    {
        brain = new Brain(Genome.Brain); //5, 100, 3);
    }

    void ColorInit()
    {
        if (renderer != null)
        {
            // Получаем массив всех материалов
            materials = renderer.materials;

            // Debug.Log("Кол-во материалов: " + materials.Length.ToString());

            // Проходимся по каждому материалу и изменяем его цвет
            for (int i = 0; i < (materials.Length); i++)
            {
                if (!materials[i].name.StartsWith("Mat5")) // && !materials[i].name.StartsWith("Mat1"))
                {
                    materials[i].color = new Color(
                        Random.Range(0f, 1f), //Genome.Skin["colors"][i][0],
                        Random.Range(0f, 1f), //Genome.Skin["colors"][i][1],
                        Random.Range(0f, 1f) //Genome.Skin["colors"][i][2]
                        // Genome.Skin["colors"][i][3]
                    );
                }
            }
        }
        else
        {
            Debug.LogError("Компонент Renderer не найден на объекте.");
        }
    }

    private void FixedUpdate()
    {
        if (!Pause)
        {
            if (Vector3.Distance(waypoints[currentWaypointIdx].position, transform.position) < waypointRange)
            {
                currentWaypointIdx++;
                if (currentWaypointIdx == waypoints.Count)
                {
                    currentWaypointIdx = 0;
                }
            }
            currentAngle = Vector3.SignedAngle(waypoints[currentWaypointIdx].position - transform.position, transform.position, Vector3.up);
            Debug.DrawRay(transform.position, waypoints[currentWaypointIdx].position - transform.position, Color.yellow);

            // Debug.Log($"Distances: [{string.Join(",", sensor.distances)}]");

            float[] output = brain.Forward(sensor.distances);
            // Debug.Log($"Output: [{string.Join(",", output)}]");
            rotationY = currentAngle * (output[0] - output[1]) * angularVelocity;
            forward = output[2];

            Move();
            Rotate();
        }
    }

    public void Move()
    {
        moveAxis = new Vector3(0f, 0f, -forward);
        moveAxis = Vector3.ClampMagnitude(moveAxis, 1);
        transform.Translate(moveAxis * moveSpeed * Time.unscaledDeltaTime);
    }

    void Rotate(){            
        transform.eulerAngles = new Vector3(0f, rotationY, 0f);
    }
}
