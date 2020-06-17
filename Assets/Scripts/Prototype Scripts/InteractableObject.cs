using System.Collections;
using System.Collections.Generic;
using System.Reflection;
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

    [SerializeField] private GameObject highlighterBasis;
    private GameObject highlighterInstance;
    private Renderer highlighterRenderer;
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

    public void ShowHighlighter(bool state)
    {
        highlighterInstance.SetActive(state);
    }

    public void ResetHighlighter()
    {
        highlighterInstance.transform.parent = highlighterBasis.transform;
        highlighterInstance.transform.localPosition = Vector3.zero;
        highlighterInstance.transform.localRotation = highlighterBasis.transform.localRotation;
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
        GrabbableObject grabbableComponent = highlighterInstance.GetComponent<GrabbableObject>();
        if (grabbableComponent != null)
        {
            grabbableComponent.DetachForceAppliers();
            Destroy(grabbableComponent);
        }

        BoxDragSFX boxSFXComponent = highlighterInstance.GetComponent<BoxDragSFX>();
        if (boxSFXComponent != null) boxSFXComponent.ToggleThis(false);

        //Setup trigger check
        Collider[] colliders = highlighterInstance.GetComponents<Collider>();
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
        highlighterInstance.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
        highlighterInstance.tag = "Outline";

        //Hide highlighter
        highlighterInstance.SetActive(false);
    }
}
