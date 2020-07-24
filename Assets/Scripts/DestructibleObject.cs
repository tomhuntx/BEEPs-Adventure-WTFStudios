using System.Collections;
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

//[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SpawnerRemote))]
public class DestructibleObject : MonoBehaviour
{
    #region Exposed Variables
    [Header("Destructible Object Properties")]
    [Tooltip("Assign a value if the game object to be affected is a child of this transform. Otherwise, you can leave it blank/unassigned.")]
    [SerializeField] private GameObject targetGameObject;

    [Tooltip("How much damage it can take before breaking.")]
    [SerializeField] private float durability = 10.0f;
    private float originalDurability = 0;

    [Tooltip("Tick this to ignore durability check.")]
    [SerializeField] private bool isInvincible = false;

    [Tooltip("How much force it can take before registering damage.")]
    [SerializeField] private float forceMagnitudeThreshold = 1.5f;

    
    [Header("Other Properties")]
    [Tooltip("How much impact magnitude before triggering OnImpactGeneral unity event.")]
    [SerializeField] private float generalImpactThreshold = 0.55f;

    [Tooltip("Presets when changing visuals upon damage.")]
    [SerializeField] List<ModelStates> modelPresets = new List<ModelStates>();
    #endregion

    #region Hidden Variables
    private Rigidbody rb;
    private MeshFilter mesh;
    //private MeshRenderer mrenderer;
    private Collider[] colliders;
    private List<PersistentForceRigidbody> forceAppliers = new List<PersistentForceRigidbody>();
    #endregion;

    #region Events
    [Header("Destructible Object Events")]
    public UnityEvent onPlayerPunch;
    public UnityEvent onColliderEnter;
    public UnityEvent onImpactGeneral;
    public UnityEvent onImpactDamage;    
    public UnityEvent onObjectDestroy;
    #endregion

    public GameObject TargetGameObject { get { return targetGameObject; } }
    public Rigidbody RigidbodyComponent { get { return rb; } }
    public MeshFilter MeshFilterComponent { get { return mesh; } }
    //public MeshRenderer MeshRendererComponent { get { return mrenderer; } }
    public Collider[] ColliderComponents { get { return colliders; } }
    public Renderer RendererComponent { get; private set; }


    void Awake()
    {
        GameObject referenceGameObject = this.gameObject;

        //Target game object is assigned, assigning references according to that.
        if (targetGameObject != null) referenceGameObject = targetGameObject;
        else targetGameObject = referenceGameObject;        
        originalDurability = durability;

        //Get and set colliders
        colliders = this.GetComponentsInChildren<Collider>();
        if (colliders.Length <= 0)
            Debug.LogError(targetGameObject.gameObject +
                " Doesn't have a collider attached to it, please attach a collider before playing!");

        rb = colliders[0].attachedRigidbody;

        //sort contents to descending based on their assigned values
        modelPresets.Sort(delegate (ModelStates a, ModelStates b)
        {
            return b.assignedValue.CompareTo(a.assignedValue);
        });

        mesh = targetGameObject.GetComponent<MeshFilter>();
        //mrenderer = targetGameObject.GetComponent<MeshRenderer>();
        RendererComponent = targetGameObject.GetComponent<Renderer>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        onColliderEnter.Invoke();

		if (rb != null)
		{
			float impactMagnitude = Vector3.Magnitude(rb.velocity);
			if (impactMagnitude > generalImpactThreshold)
				onImpactGeneral.Invoke();

			if (!isInvincible)
				CheckDurability(impactMagnitude);

			//print(Time.time + "-" +this.transform + ":" + impactMagnitude);
		}
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
    /// Changes this game object's look.
    /// </summary>
    private void ChangeModelState(ModelStates preset)
    {
        //can inject code for instanciating destruction transition prefab here
        mesh.mesh = preset.mesh;
		RendererComponent.material = preset.material;
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Reduces durability and manages model changing.
    /// </summary>
    /// <param name="damage">Amount from durability will be taken away.</param>
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
                        onImpactDamage.Invoke();
                        ChangeModelState(preset);
                        break;
                    }
                }
			}
            else
            {
                onObjectDestroy.Invoke();
                Destroy(this.transform.gameObject);
            }
        }
		else
		{
			Debug.Log("C");
		}
    }

    /// <summary>
    /// Add reference for removing force applier later.
    /// </summary>
    /// <param name="source">The force applier that affects this rigidbody.</param>
    public void AddForceApplierReference(PersistentForceRigidbody source)
    {
        forceAppliers.Add(source);
    }

    /// <summary>
    /// Stop external force appliers. 
    /// </summary>
    public void DetachForceAppliers()
    {
        if (forceAppliers.Count > 0)
        {
            foreach (PersistentForceRigidbody source in forceAppliers)
            {
                source.RemoveReference(rb);
            }

            forceAppliers.Clear();
        }
    }
    #endregion
}
