using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class InteractableObject : MonoBehaviour
{
    //CREATE FADER DEDICATED TO RENDERERS
    //CREATE BASE CLASS FOR FADERS

    public static Material highlighterMaterial;
    public static Color normalHighlightColor;
    public static Color invalidHighlightColor;
    public const float CONTACT_OFFSET = 0.2f;

    [SerializeField] private GameObject highlighterBasis;
    private GameObject highlighterInstance;
    private Renderer highlighterRenderer;
    private bool isParented;
    private readonly Vector3 HIGHLIGHTER_OFFSET_SIZE = new Vector3(0.0001f,
                                                                   0.0001f,
                                                                   0.0001f);

    public GameObject HighlighterInstance { get { return highlighterInstance; } 
                                            set { highlighterInstance = value; } }
    public GameObject HighlighterBasis { get { return highlighterBasis; }
                                         set { highlighterBasis = value; } }


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


    public void SetHighlighterInvalid(bool isInvalid)
    {
        if (isInvalid)
        {
            highlighterRenderer.material.color = invalidHighlightColor;
        }
        else
        {
            highlighterRenderer.material.color = normalHighlightColor;
        }
    }

    public void ShowHighlighter(bool state)
    {
        highlighterInstance.SetActive(state);
    }

    public void ResetHighlighter()
    {
        highlighterInstance.transform.parent = this.transform;
        highlighterInstance.transform.localPosition = Vector3.zero;
        highlighterInstance.transform.localRotation = Quaternion.identity;
        highlighterRenderer.material.color = normalHighlightColor;

    }

    public void SetupHighlighter(Transform parent)
    {
        //Setup transform
        isParented = true;
        highlighterInstance = Instantiate(parent.gameObject);
        highlighterInstance.transform.parent = this.transform;
        highlighterInstance.transform.localPosition = Vector3.zero;
        //GameObject highlighterModel = Instantiate(highlighterBasis);
        
        //highlighterModel.transform.parent = highlighterInstance.transform;
        //highlighterModel.transform.localPosition = Vector3.zero;
        highlighterInstance.transform.localScale = Vector3.one;
        highlighterInstance.transform.localScale += HIGHLIGHTER_OFFSET_SIZE; //Prevent z-fighting 


        //Setup rendering
        GameObject highlighterModel = highlighterInstance.GetComponent<InteractableObject>().highlighterBasis;
        highlighterRenderer = highlighterModel.GetComponent<Renderer>();
        highlighterRenderer.material = highlighterMaterial;
        highlighterRenderer.receiveShadows = false;
        highlighterRenderer.shadowCastingMode = ShadowCastingMode.Off;

        //Remove other components        
        RemoveComponents();

        //Setup trigger check
        Collider[] colliders = highlighterInstance.GetComponents<Collider>();

        //No colliders exist in the instance, copy colliders from this transform.
        //if (colliders.Length <= 0)
        //{
        //    Collider[] mainColliders = this.transform.GetComponents<Collider>();
        //    colliders = new Collider[mainColliders.Length];

        //    for (int i = 0; i < colliders.Length; i++)
        //    {
        //         colliders[i] = CopyOver.CopyComponent(mainColliders[i], highlighterInstance.transform.gameObject);
        //    }
        //}

        if (colliders.Length > 0)
        {
            foreach (Collider collider in colliders)
            {
                collider.isTrigger = true;

                //Adjust collider size
                var colliderType = collider.GetType();
                if (colliderType.Equals(typeof(BoxCollider)))
                {
                    collider.TryGetComponent(out BoxCollider col);
                    col.size -= new Vector3(0.1f, 0.1f, 0.1f);
                }
                else if (colliderType.Equals(typeof(SphereCollider)))
                {
                    collider.TryGetComponent(out SphereCollider col);
                    col.radius -= 0.1f;
                }
                else if (colliderType.Equals(typeof(CapsuleCollider)))
                {
                    collider.TryGetComponent(out CapsuleCollider col);
                    col.radius -= 0.1f;
                    col.height -= 0.1f;
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
            rb.isKinematic = true;
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

    public void SetupHighlighter()
    {
        //Setup transform
        highlighterInstance = Instantiate(highlighterBasis);
        highlighterInstance.transform.parent = highlighterBasis.transform;
        highlighterInstance.transform.localPosition = Vector3.zero;
        highlighterInstance.transform.localScale += HIGHLIGHTER_OFFSET_SIZE; //Prevent z-fighting        

        //Setup rendering
        highlighterRenderer = highlighterInstance.GetComponent<Renderer>();
        highlighterRenderer.material = highlighterMaterial;
        highlighterRenderer.receiveShadows = false;
        highlighterRenderer.shadowCastingMode = ShadowCastingMode.Off;

        //Remove other components   
        RemoveComponents();

        //Setup trigger check
        Collider[] colliders = highlighterInstance.GetComponents<Collider>();

        //No colliders exist in the instance, copy colliders from this transform.
        if (colliders.Length <= 0)
        {
            Collider[] mainColliders = this.transform.GetComponents<Collider>();
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

                //Adjust collider size
                var colliderType = collider.GetType();
                if (colliderType.Equals(typeof(BoxCollider)))
                {
                    collider.TryGetComponent(out BoxCollider col);
                    col.size -= new Vector3(0.1f, 0.1f, 0.1f);
                }
                else if (colliderType.Equals(typeof(SphereCollider)))
                {
                    collider.TryGetComponent(out SphereCollider col);
                    col.radius -= 0.1f;
                }
                else if (colliderType.Equals(typeof(CapsuleCollider)))
                {
                    collider.TryGetComponent(out CapsuleCollider col);
                    col.radius -= 0.1f;
                    col.height -= 0.1f;
                }
                else
                {
                    Debug.LogError(string.Format("{0} has a collider of {1} which is not supported by {2} " +
                                                 "and only accepts the following: Box, Sphere, and Capsule.",
                                                 this.transform, colliderType, this.name));
                }
            }

            Rigidbody rb = colliders[0].attachedRigidbody;
            rb.isKinematic = true;
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
                    rb.isKinematic = true;
                }
            }
        }
    }
}
