﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Used to store model presets.
/// </summary>
[System.Serializable]
public struct ModelStates
{
    /// <summary>
    /// [BACK-END] Float variable which can be used for anything.
    /// </summary>
    [Tooltip("Assigned object health based on percentage.")]
    [Range(1, 100)]
    public float assignedValue;

    public Mesh mesh;
    public Material material;
}


[RequireComponent(typeof(Rigidbody))]
public class DestructibleObject : MonoBehaviour
{
    #region Exposed Variables
    [Header("Destructible Object Properties")]
    [Tooltip("How much damage it can take before breaking.")]
    [SerializeField] private float durability = 10.0f;
    private float originalDurability = 0;

    [Tooltip("Tick this to ignore durability check.")]
    [SerializeField] private bool isInvincible = false;

    [Tooltip("How much force it can take before registering damage.")]
    [SerializeField] private float forceMagnitudeThreshold = 1.5f;

    [Header("Other Properties")]
    [SerializeField] GameObject objectDestroyPrefab;

    [Tooltip("Presets when changing visuals upon damage.")]
    [SerializeField] List<ModelStates> modelPresets = new List<ModelStates>();
    #endregion

    #region Hidden Variables
    private Rigidbody rb;
    private MeshFilter mesh;
    private MeshRenderer mrenderer;
    #endregion;

    #region Events
    [Header("Events")]
    public UnityEvent OnImpactDamage;
    public UnityEvent OnImpactGeneral;
    public UnityEvent OnObjectDestroy;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        mesh = this.GetComponent<MeshFilter>();
		mrenderer = this.GetComponent<MeshRenderer>();
        originalDurability = durability;

        //sort contents to descending based on their assigned values
        modelPresets.Sort(delegate (ModelStates a, ModelStates b)
        {
            return b.assignedValue.CompareTo(a.assignedValue);
        });
    }

    private void OnCollisionEnter(Collision collision)
    {
        OnImpactGeneral.Invoke();

        if (!isInvincible)
            CheckDurability(Vector3.Magnitude(rb.velocity));
    }

    private void OnCollisionStay(Collision collision)
    {
        //print(collision.transform);
    }


    #region Private Methods
    /// <summary>
    /// Calculates how much force is absorbed on impact.
    /// </summary>
    /// <param name="magnitude">The object's rigibody magnitude.</param>
    private void CheckDurability(float magnitude)
    {
        int numDamageCycle = Mathf.FloorToInt(magnitude / forceMagnitudeThreshold);

        if (numDamageCycle > 0)
        {
            ApplyDamage(magnitude * numDamageCycle);
        }
    }
    
    /// <summary>
    /// Instanciates the explosion prefab before destroying this game object.
    /// </summary>
    private void DestroyObject()
    {
        OnObjectDestroy.Invoke();
        //uncomment this after implementing the animations prefab
        //Instantiate(objectDestroyPrefab, this.transform.position, this.transform.rotation);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Changes this game object's look.
    /// </summary>
    private void ChangeModelState(ModelStates preset)
    {
        //can inject code for instanciating destruction transition prefab here
        mesh.mesh = preset.mesh;
		mrenderer.material = preset.material;
    }
    #endregion

    #region Public Methods
    public void ApplyDamage(float damage)
    {
        if (!isInvincible)
        {
            durability -= damage;
            if (durability > 0)
            {
                foreach (ModelStates preset in modelPresets)
                {
                    if (preset.assignedValue <= (durability / originalDurability) * 100)
                    {
                        OnImpactDamage.Invoke();
                        ChangeModelState(preset);
                        break;
                    }
                }
            }
            else
            {
                DestroyObject();
            }
        }
    }
    #endregion
}