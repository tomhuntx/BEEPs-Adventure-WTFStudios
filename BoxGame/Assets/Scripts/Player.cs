using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(FPSController))]
public class Player : MonoBehaviour
{
    public static Player Instance;
    private FPSController controller;

    [Header("Box Handling Properties")]
    [Tooltip("Strength of impulse force applied upon throwing.")]
    [SerializeField] private float throwForce = 30.0f;

    [Tooltip("The position of the grabbed object in this transform's local space.")]
    [SerializeField] private Vector3 objectOffset;

    [Tooltip("How far can the player can reach boxes and interaction.")]
    [SerializeField] private float interactionDistance = 3.0f;
    
    
    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        controller = this.GetComponent<FPSController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
