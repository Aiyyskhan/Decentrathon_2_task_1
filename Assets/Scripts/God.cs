using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class God : MonoBehaviour
{
    [SerializeField] Transform playerCamera;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask ground;
    [SerializeField] LayerMask detectedCursor;
    [SerializeField] [Range(.0f, .5f)] float mouseSmoothTime = .01f;
    [SerializeField] float mouseSensitivity = 5.5f;
    [SerializeField] float speed = 20.0f;
    [SerializeField] [Range(.0f, .5f)] float moveSmoothTime = .3f;
    [SerializeField] float gravity = 0.0f;
    [SerializeField] float jumpHeight = 6.0f;

    public GameObject creationTool;
    public Button evolutionBtn;
    public Toggle onlyLeaderTgl;
    public Slider firstParentProbSldr;
    public Slider mutationProbSldr;
    public Slider mutationLevelSldr;
    public Button newPopulationGenerateBtn;

    public GameObject carInfoPanel;
    public GameObject genInfoPanel;
    public GameObject selInfoPanel;
    TextMeshProUGUI carInfo;
    TextMeshProUGUI genInfo;
    TextMeshProUGUI selInfo;

    Vector2 currentMouseDelta;
    Vector2 currentMouseDeltaVelocity;
    
    CharacterController controller;
    Vector2 currentDir;
    Vector2 currentDirVelocity;
    Vector2 velocity;
    private CreationTool creatorScript;

    private List<int> selectedCarsId = new List<int>();

    float velocityY;
    bool isGrounded;
    float cameraCap;

    int generation = 1;

    void Awake()
    {
        creatorScript = creationTool.GetComponent<CreationTool>();
        newPopulationGenerateBtn.onClick.AddListener(EvolutionBtnPress);
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();

        carInfoPanel.SetActive(false);
        carInfo = carInfoPanel.GetComponent<TextMeshProUGUI>();
        genInfo = genInfoPanel.GetComponent<TextMeshProUGUI>();
        selInfo = selInfoPanel.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        UpdateMouse();
        UpdateMove();

        genInfo.text = "Generation: " + generation;
        selInfo.text = "Selected: " + String.Join(',', selectedCarsId);

        // Debug.Log("Only leaders: " + onlyLeaderTgl.isOn.ToString());
        // Debug.Log("First parent prob: " + firstParentProbSldr.value.ToString());
        // Debug.Log("Mutation prob: " + mutationProbSldr.value.ToString());
        // Debug.Log("Mutation level: " + mutationLevelSldr.value.ToString());
    }

    void EvolutionBtnPress()
    {
        Debug.Log("Evolution");
        creatorScript.Evolution(selectedCarsId, onlyLeaderTgl.isOn, firstParentProbSldr.value, mutationProbSldr.value, mutationLevelSldr.value);
        creatorScript.NewStart();
        selectedCarsId.Clear();
        generation++;
    }

    void UpdateMouse()
    {
        if (Input.GetMouseButton(2)){
            Cursor.visible = false;
            carInfoPanel.SetActive(false);
            
            Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
            cameraCap -= currentMouseDelta.y * mouseSensitivity;
            cameraCap = Mathf.Clamp(cameraCap, -90.0f, 90.0f);
            playerCamera.localEulerAngles = Vector3.right * cameraCap;
            transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
        }
        else
        {
            Cursor.visible = true;

            LookForGameObject();
        }
    }

    void UpdateMove()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, ground);
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDir.Normalize();
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);
        velocityY += gravity * 2.0f * Time.deltaTime;
        Vector3 velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * speed + Vector3.up * velocityY;
        controller.Move(velocity * Time.deltaTime);

        if (isGrounded && Input.GetButtonDown("Jump")) //GetKey(KeyCode.Space)) //GetButtonDown("Space"))
        {
            // Debug.Log("Jump!");
            velocityY = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }
        if (isGrounded! && controller.velocity.y < -1.0f)
        {
            velocityY = -8.0f;
        }
    }

    private void LookForGameObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, detectedCursor))
        {
            CarController selected_car = hit.collider.gameObject.GetComponent<CarController>();
            if (selected_car)
            {
                carInfoPanel.SetActive(true);
                carInfoPanel.transform.position = Input.mousePosition + new Vector3(0, 50, 0);
                carInfo.text = "Car " + selected_car.Id;
                if (Input.GetMouseButton(0) && !selectedCarsId.Contains(selected_car.Id))
                {
                    selectedCarsId.Add(selected_car.Id);
                }
            }
            else
            {
                carInfoPanel.SetActive(false);
            }
        }
        else
        {
            carInfoPanel.SetActive(false);
        }
    }
}
