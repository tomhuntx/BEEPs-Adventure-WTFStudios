using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour
{
    #region Static Variables
    public static Material highlighterMaterial;
    public static Color normalHighlightColor;
    public static Color invalidHighlightColor;
    private readonly Vector3 HIGHLIGHTER_OFFSET_SIZE = new Vector3(0.0001f,
                                                                   0.0001f,
                                                                   0.0001f);
    #endregion

    #region Other Variables
    [Tooltip("The game object that contains a renderer and will be make a highlighter out of.")]
    [SerializeField] private GameObject highlighterBasis;
    private GameObject highlighterInstance;
    private Renderer highlighterRenderer;
    private bool isParented;

	[Header("Hover Event")]
	public UnityEvent onObjectHover;
	public UnityEvent onObjectNotHover;

	/*
	[Header("Tooltips - Tutorial Only")]
	public GameObject TutorialManager;
	[Tooltip("Whether to display the base hover tooltip or not.")]
	public bool displayTooltip = false;
	[Tooltip("Display the tooltip repeatedly (false) or once (true).")]
	public bool displayOnce = false;
	[Tooltip("Text to display on the tooltip (Leave blank if display tooltip not selected).")]
	public string tooltipMessage = "";
	[Tooltip("Whether to display more tooltips when the first is triggered (for place/throw boxes).")]
	public bool displayTooltip2 = false;
	[Tooltip("Text to display after the first tooltip is activiated")]
	public string secondTooltipMessage = "";
	[Tooltip("Is the text finished displaying - leave false in the editor.")]
	public bool tooltipFinished = false;
	*/
	#endregion

	#region Accessors
	public GameObject HighlighterInstance { get { return highlighterInstance; } 
                                            set { highlighterInstance = value; } }
    public GameObject HighlighterBasis { get { return highlighterBasis; }
                                         set { highlighterBasis = value; } }
    #endregion

    private void Start()
    {
        if (highlighterBasis == null)
            highlighterBasis = this.gameObject;
    }

    private void Update()
    {
        if (highlighterInstance == null)
            SetupHighlighter();
    }

	#region Public Methods

	/// <summary>
	/// Changes the highlighter's color to invalid if set to true.
	/// </summary>
	/// <param name="isInvalid">If the highlighter's color will be set to invalid.</param>
	public void SetHighlighterInvalid(bool isInvalid)
    {
        if (isInvalid)
        {
            //highlighterRenderer.material.color = invalidHighlightColor;
            highlighterRenderer.material.SetColor("_highlighter_color", invalidHighlightColor);
        }
        else
        {
            //highlighterRenderer.material.color = normalHighlightColor;
            highlighterRenderer.material.SetColor("_highlighter_color", normalHighlightColor);
        }
    }

	/// <summary>
	/// Hide/shows the highlighter game object.
	/// </summary>
	/// <param name="state">Set to true to show, otherwise, hides it.</param>
	public void ShowHighlighter(bool state)
    {
		if (highlighterInstance != null)
		{
			highlighterInstance.SetActive(state);
		}

		// Hover Events - used with tutorial boxes
		if (state)
		{
			onObjectHover.Invoke();
		}
		else
		{
			onObjectNotHover.Invoke();
		}
	}

	/// <summary>
	/// Gets highligher state. Used with tutorial tooltips.
	/// </summary>
	/// <returns>If the highlighter is on/off (true/false)</returns>
	public bool GetHighlighter()
	{
		return highlighterInstance.activeSelf;
	}

	/// <summary>
	/// Resets the highlighter's transform values and material color.
	/// </summary>
	public void ResetHighlighter()
    {
        highlighterInstance.transform.parent = this.transform;
        highlighterInstance.transform.localPosition = Vector3.zero;
        highlighterInstance.transform.localRotation = Quaternion.identity;
        highlighterInstance.transform.localScale = Vector3.one;
        //highlighterRenderer.material.color = normalHighlightColor;
        highlighterRenderer.material.SetColor("_highlighter_color", normalHighlightColor);
    }

    /// <summary>
    /// Highlighter setup if the interactable object's model 
    /// is parented to this script's attached transform.
    /// </summary>
    /// <param name="parent">The parent transform of the highlighter's basis.</param>
    public void SetupHighlighter(Transform parent)
    {
        //Setup transform
        isParented = true;
        highlighterInstance = Instantiate(parent.gameObject);
        highlighterInstance.transform.parent = this.transform;
        highlighterInstance.transform.localRotation = Quaternion.identity;
        highlighterInstance.transform.localPosition = Vector3.zero;
        highlighterInstance.transform.localScale = Vector3.one;
        //highlighterInstance.transform.localScale += HIGHLIGHTER_OFFSET_SIZE; //Prevent z-fighting 


        //Setup rendering
        GameObject highlighterModel = highlighterInstance.GetComponent<InteractableObject>().highlighterBasis;
        highlighterRenderer = highlighterModel.GetComponent<Renderer>();
        highlighterRenderer.material = highlighterMaterial;
        highlighterRenderer.receiveShadows = false;
        highlighterRenderer.shadowCastingMode = ShadowCastingMode.Off;

        //Remove other components        
        RemoveComponents();

        //Setup trigger check
        Collider[] colliders = highlighterInstance.GetComponentsInChildren<Collider>();

        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;

                //Decrease collider's size slightly to avoid errors for placing objects
                var colliderType = collider.GetType();
                if (colliderType.Equals(typeof(BoxCollider)))
                {
                    collider.TryGetComponent(out BoxCollider col);
                    col.size = new Vector3(Mathf.Abs(col.size.x - 0.1f),
                                           Mathf.Abs(col.size.y - 0.1f),
                                           Mathf.Abs(col.size.z - 0.1f));
                }
                else if (colliderType.Equals(typeof(SphereCollider)))
                {
                    collider.TryGetComponent(out SphereCollider col);
                    col.radius = Mathf.Abs(col.radius - 0.1f);
                }
                else if (colliderType.Equals(typeof(CapsuleCollider)))
                {
                    collider.TryGetComponent(out CapsuleCollider col);
                    col.radius = Mathf.Abs(col.radius - 0.1f);
                    col.height = Mathf.Abs(col.height - 0.1f);
                }
                else
                {
                    Debug.LogError(string.Format("{0} has a collider of {1} which is not supported by {2} " +
                                                 "and only accepts the following: Box, Sphere, and Capsule.",
                                                 this.transform, colliderType, this.name));
                }
            }

            Rigidbody rb = colliders[0].attachedRigidbody;
            if (rb == null) rb = highlighterInstance.AddComponent<Rigidbody>();
            //rb.isKinematic = true;
        }
        else
        {
            Debug.LogError(string.Format("{0} did not find any colliders, " +
                                         "please either put a collider along with this transform or " +
                                         "the one where the highlighter is based at before playing!", this.name));
        }

        highlighterModel.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
        highlighterModel.tag = "Outline";

        highlighterInstance.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
        highlighterInstance.tag = "Outline";

        //Hide highlighter
        highlighterInstance.SetActive(false);

	}

    /// <summary>
    /// Highlighter setup if the highlighter's basis game object is 
    /// within the same transform as this script that is attached to.
    /// </summary>
    public void SetupHighlighter()
    {
        //Setup transform
        highlighterInstance = Instantiate(highlighterBasis);
        highlighterInstance.transform.parent = highlighterBasis.transform;
        highlighterInstance.transform.localRotation = Quaternion.identity;
        highlighterInstance.transform.localPosition = Vector3.zero;
        //highlighterInstance.transform.localScale += HIGHLIGHTER_OFFSET_SIZE; //Prevent z-fighting        

        //Setup rendering
        highlighterRenderer = highlighterInstance.GetComponent<Renderer>();
        highlighterRenderer.material = highlighterMaterial;
        highlighterRenderer.receiveShadows = false;
        highlighterRenderer.shadowCastingMode = ShadowCastingMode.Off;

        //Remove other components   
        RemoveComponents();

        //Setup trigger check
        Collider[] colliders = highlighterInstance.GetComponentsInChildren<Collider>();

        //No colliders exist in the instance, copy colliders from this transform.
        if (colliders.Length <= 0)
        {
            Collider[] mainColliders = this.transform.GetComponentsInChildren<Collider>();
            colliders = new Collider[mainColliders.Length];

            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i] = CopyOver.CopyComponent(mainColliders[i], highlighterInstance);
            }
        }

        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;

                //Decrease collider's size slightly to avoid errors for placing objects
                var colliderType = collider.GetType();
                if (colliderType.Equals(typeof(BoxCollider)))
                {
                    collider.TryGetComponent(out BoxCollider col);
                    col.size = new Vector3(Mathf.Abs(col.size.x - 0.1f),
                                           Mathf.Abs(col.size.y - 0.1f),
                                           Mathf.Abs(col.size.z - 0.1f));
                }
                else if (colliderType.Equals(typeof(SphereCollider)))
                {
                    collider.TryGetComponent(out SphereCollider col);
                    col.radius = Mathf.Abs(col.radius - 0.1f);
                }
                else if (colliderType.Equals(typeof(CapsuleCollider)))
                {
                    collider.TryGetComponent(out CapsuleCollider col);
                    col.radius = Mathf.Abs(col.radius - 0.1f);
                    col.height = Mathf.Abs(col.height - 0.1f);
                }
                else
                {
                    Debug.LogError(string.Format("{0} has a collider of {1} which is not supported by {2} " +
                                                 "and only accepts the following: Box, Sphere, and Capsule.",
                                                 this.transform, colliderType, this.name));
                }
            }

            Rigidbody rb = colliders[0].attachedRigidbody;
            //rb.isKinematic = true;
        }
        else
        {
            Debug.LogError(string.Format("{0} did not find any colliders, " +
                                         "please either put a collider along with this transform or " +
                                         "the one where the highlighter is based at before playing!", this.name));
        }

        highlighterInstance.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
        highlighterInstance.tag = "Outline";

        //Hide highlighter
        highlighterInstance.SetActive(false);
	}
    #endregion


    #region Private Methods
    /// <summary>
    /// Removes the highlighter's unnecessary components.
    /// </summary>
    private void RemoveComponents()
    {
        List<Component> components = highlighterInstance.GetComponents<Component>().ToList();

        //First run: Destroy component that relies with other components
        for (int i = 0; i < components.Count; i++)
        {
            if (components[i].GetType().Equals(typeof(GrabbableObject)))
            {
                GrabbableObject grabbable = components[i] as GrabbableObject;
                grabbable.DetachForceAppliers();
                Destroy(components[i]);
                break;
            }
        }

        //Second run: Destroy the rest of unecessary components
        for (int i = 0; i < components.Count; i++)
        {
            var component = components[i].GetType();
            bool isAccepted = component.Equals(typeof(GrabbableObjectPlacementChecker)) ||
                              component.Equals(typeof(SphereCollider)) ||
                              component.Equals(typeof(BoxCollider)) ||
                              component.Equals(typeof(CapsuleCollider)) ||
                              component.Equals(typeof(Transform)) ||
                              component.Equals(typeof(MeshRenderer)) ||
                              component.Equals(typeof(SkinnedMeshRenderer)) ||
                              component.Equals(typeof(MeshFilter)) ||
                              component.Equals(typeof(Rigidbody));

            if (!isAccepted)
            {
                Destroy(components[i]);
            }
            else
            {
                if (component.Equals(typeof(Rigidbody)))
                {
                    Rigidbody rb = components[i] as Rigidbody;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
                    rb.isKinematic = true;                    
                }
            }
        }
    }
    #endregion
}
