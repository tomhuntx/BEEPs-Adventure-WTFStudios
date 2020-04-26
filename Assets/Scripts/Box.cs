using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ModelStates
{

    /// <summary>
    /// [BACK-END] Float variable which can be used for anything.
    /// </summary>
    [Tooltip("Assigned box health based on percentage.")]
    [Range(1, 100)]
    public float assignedValue;

    public Mesh mesh;
    public Material material;
}


[RequireComponent(typeof(Rigidbody))]
public class Box : MonoBehaviour
{
    private Rigidbody rb;
    private MeshFilter mesh;
    private MeshRenderer renderer;

    [Header("Box Properties")]
    [SerializeField] private float durability = 10.0f;
    private float originalDurability = 0;

    [Tooltip("Tick this to ignore durability check.")]
    [SerializeField] private bool isInvincible = false;

    [Tooltip("How much force it can take before registering damage.")]
    [SerializeField] private float forceMagnitudeThreshold = 1.5f;

    [Header("Other Properties")]
    [SerializeField] GameObject boxDestroyPrefab;

    [Tooltip("Presets when changing visuals upon damage.")]
    [SerializeField] List<ModelStates> modelPresets = new List<ModelStates>();

    // Start is called before the first frame update
    void Start()
    {
        rb = this.GetComponent<Rigidbody>();
        mesh = this.GetComponent<MeshFilter>();
        renderer = this.GetComponent<MeshRenderer>();
        originalDurability = durability;

        //sort contents to descending based on their assigned values
        modelPresets.Sort(delegate (ModelStates a, ModelStates b)
        {
            return b.assignedValue.CompareTo(a.assignedValue);
        });
    }

    private void OnCollisionEnter(Collision collision)
    {
        CheckDurability(Vector3.Magnitude(rb.velocity));
    }

    /// <summary>
    /// Calculates how much force is absorbed on impact.
    /// </summary>
    /// <param name="magnitude">The object's rigibody magnitude.</param>
    private void CheckDurability(float magnitude)
    {
        int numDamageCycle = Mathf.FloorToInt(magnitude / forceMagnitudeThreshold);

        if (numDamageCycle > 0)
        {
            DamageBox(magnitude * numDamageCycle);
        }
    }
    
    private void DestroyBox()
    {
        //uncomment this after implementing the animations prefab
        //Instantiate(boxDestroyPrefab, this.transform.position, this.transform.rotation);
        Destroy(this.gameObject);
    }

    private void ChangeModelState(ModelStates preset)
    {
        mesh.mesh = preset.mesh;
        renderer.material = preset.material;
    }

    public void DamageBox(float damage)
    {
        durability -= damage;
        if (durability > 0)
        {
            foreach (ModelStates preset in modelPresets)
            {
                if (preset.assignedValue <= (durability / originalDurability) * 100)
                {
                    ChangeModelState(preset);
                    break;
                }
            }
        }
        else
        {
            DestroyBox();
        }
    }
}
