using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FPSController))]
public class Player : MonoBehaviour
{
    public static Player Instance;
    private FPSController controller;
    public Graphic crosshair;

    [Header("Box Handling Properties")]
    [SerializeField] private float punchDamage = 3.0f;

    [Tooltip("Strength of impulse force applied upon throwing.")]
    [SerializeField] private float throwForce = 30.0f;

	[Tooltip("The position of the grabbed object in this transform's local space.")]
    [SerializeField] private Vector3 objectOffset;

    [Tooltip("How far can the player can reach boxes and interaction.")]
    [SerializeField] private float interactionDistance = 3.0f;

    [Tooltip("The area around the player where box placement should be ignored.")]
    [SerializeField] private float boxPlacementDeadzone = 0.5f;

    private GameObject boxOutline;
    private BoxPlacementChecker outlineCollider;
    private Color originalOutlineColor;
    private Renderer outlineRenderer;
    private GameObject currentBox;
    
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
        if (currentBox != null)
        {
            ShowBoxPlacement();

            if (outlineCollider.isPlacable)
            {
                if (Input.GetButtonDown("Place Box")) PlaceBox();
                if (Input.GetButtonDown("Punch")) ThrowBox();
            }
        }
        else
        {
            if (Input.GetButtonDown("Grab Box")) GrabBox();
            if (Input.GetButtonDown("Punch")) PunchBox();
        }
        ManageCrosshair();
    }

    private void GrabBox()
    {
        RaycastHit hitInfo;
        if (DoRaycast(out hitInfo))
        {
            if (hitInfo.transform.tag == "Box")
            {
                currentBox = hitInfo.transform.gameObject;

                //Disable physics and collision
                currentBox.GetComponent<Rigidbody>().isKinematic = true;
                currentBox.GetComponent<Collider>().enabled = false;
                
                //Set grabbed box as child of main cam
                currentBox.transform.parent = controller.MainCam.transform;
                currentBox.transform.localPosition = Vector3.zero + objectOffset;
                currentBox.transform.rotation = Quaternion.identity;

                //Clone grabbed box for outline setup
                boxOutline = Instantiate(currentBox);

                //Put grabbed box in different layer mask to prevent clipping
                currentBox.layer = LayerMask.NameToLayer("Grabbed Object");

                //Remove physics and box component
                Destroy(boxOutline.GetComponent<Box>());
                Destroy(boxOutline.GetComponent<Rigidbody>());

                //Tweak collider and add collision checker
                boxOutline.layer = LayerMask.NameToLayer("Ignore Raycast"); //ignore raycast to prevent placement jittering
                BoxCollider collider = boxOutline.GetComponent<BoxCollider>();
                collider.size = new Vector3(0.9f, 0.9f, 0.9f);
                collider.isTrigger = true;
                collider.enabled = true;
                outlineCollider = boxOutline.AddComponent<BoxPlacementChecker>();

                //Set outline to semi-transparent
                outlineRenderer = boxOutline.GetComponent<Renderer>();
                RendererModeChanger.SetToTransparent(outlineRenderer);
                Color alpha = outlineRenderer.material.color;
                alpha.a = 0.5f;
                outlineRenderer.material.color = alpha;
                originalOutlineColor = outlineRenderer.material.color;

                //Remove shadows
                outlineRenderer.receiveShadows = false;
                outlineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                
                //Hide after setup
                boxOutline.SetActive(false);
            }
        }
    }

    private void ShowBoxPlacement()
    {
        RaycastHit hitInfo;
        if (DoRaycast(out hitInfo))
        {
            //if within deadzone, don't render the outline
            if (hitInfo.distance >= boxPlacementDeadzone)
            {
                boxOutline.SetActive(true);
                
                //Set outline color to red if not placable
                if (outlineCollider.isPlacable)
                {
                    outlineRenderer.material.color = originalOutlineColor;
                }
                else
                {
                    outlineRenderer.material.color = new Color(1, 0, 0, 0.5f);
                }

                //Box outline positioning and rotation
                if (hitInfo.transform.tag == "Box")
                {
                    boxOutline.transform.position = hitInfo.transform.position + hitInfo.normal;
                    boxOutline.transform.rotation = hitInfo.transform.rotation;
                }
                else if (hitInfo.transform.tag != "Player")
                {
                    boxOutline.transform.position = hitInfo.point + hitInfo.normal;
                    boxOutline.transform.rotation = Quaternion.identity;
                }
            }
            else
            {
                boxOutline.SetActive(false);
            }    
        }
        else
        {
            boxOutline.SetActive(false);
        }
    }

    private void PlaceBox()
    {
        currentBox.transform.parent = null;

        if (boxOutline.activeSelf)
        {
            currentBox.transform.position = boxOutline.transform.position;
            currentBox.transform.rotation = boxOutline.transform.rotation;
        }
        else
        {
            currentBox.transform.position = this.transform.position + this.transform.forward;
            currentBox.transform.rotation = Quaternion.identity;
        }

        //Revert Box state
        currentBox.layer = LayerMask.NameToLayer("Default");
        currentBox.GetComponent<Collider>().enabled = true;
        currentBox.GetComponent<Rigidbody>().isKinematic = false;
        currentBox = null;
        Destroy(boxOutline);
    }

    private void ThrowBox()
    {
        Rigidbody boxRB = currentBox.GetComponent<Rigidbody>();
        currentBox.transform.parent = null;
        currentBox.layer = LayerMask.NameToLayer("Default");
        currentBox.GetComponent<Collider>().enabled = true;
        boxRB.isKinematic = false;
        boxRB.AddForce(controller.MainCam.transform.forward * throwForce, ForceMode.Impulse);
        currentBox = null;
        Destroy(boxOutline);
    }

    private void PunchBox()
    {
        RaycastHit hitInfo;
        if (DoRaycast(out hitInfo))
        {
            if (hitInfo.transform.tag == "Box")
            {
                hitInfo.transform.GetComponent<Box>().DamageBox(punchDamage);
                hitInfo.transform.GetComponent<Rigidbody>().AddForce(controller.MainCam.transform.forward * throwForce, ForceMode.Impulse);
            }
        }
    }

    /// <summary>
    /// Move crosshair onto raycast hit. If no hits or the hit point is too close, set position to center.
    /// </summary>
    private void ManageCrosshair()
    {
        RaycastHit hitInfo;
        if (DoRaycast(out hitInfo) && 
            Vector3.Distance(hitInfo.point, this.transform.position) >= boxPlacementDeadzone)
        {
            crosshair.transform.position = controller.MainCam.WorldToScreenPoint(hitInfo.point);
        }
        else
        {
            crosshair.transform.position = controller.MainCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    private bool DoRaycast (out RaycastHit hitInfo)
    {
        Vector3 origin = controller.MainCam.transform.position;
        origin.z += 0.5f;
        return Physics.Raycast(origin, controller.MainCam.transform.forward, out hitInfo, interactionDistance);
    }
}
