using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(FPSController))]
public class Player : MonoBehaviour
{
    public static Player Instance;    
    public Graphic crosshair;

    #region Exposed Variables
    [Header("Box Handling Properties")]
    [SerializeField] private float punchDamage = 3.0f;

    [Tooltip("Strength of impulse force applied upon throwing.")]
    [SerializeField] private float throwForce = 20.0f;

	[Tooltip("The position of the grabbed object in this transform's local space.")]
    [SerializeField] private Vector3 objectOffset;

    [Tooltip("How far can the player can reach boxes and interaction.")]
    [SerializeField] private float interactionDistance = 4f;

    [Tooltip("The area around the player where box placement should be ignored.")]
    [SerializeField] private float boxPlacementDeadzone = 1f;

    [SerializeField] private Material materialHighlight;
    #endregion

    #region Hidden Variables
    //outline
    private GameObject boxOutline;
    private BoxPlacementChecker outlineCollider;
    private Color originalOutlineColor;
    private Renderer outlineRenderer;

    //highlight
    private GameObject boxHighlight;

    private GameObject currentBox;
    private FPSController controller;
    private RaycastHit hitInfo;
    private bool isRaycastHit = false;
    #endregion


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
        isRaycastHit = DoRaycast(out hitInfo);

        //Only adjust crosshair position if deadzone exist
        if (boxPlacementDeadzone > 0) ManageCrosshair();

        if (currentBox != null)
        {
            ShowBoxPlacement();

            if (outlineCollider.isPlacable)
            {
                if (Input.GetButtonDown("Place Box") && boxOutline.activeSelf) PlaceBox();                
            }

            if (Input.GetButtonDown("Punch")) ThrowBox();
        }
        else
        {
            if (Input.GetButtonDown("Grab Box")) GrabBox();
            if (Input.GetButtonDown("Punch")) PunchBox();
            HighlightTarget();
        }

        //Debug raycast
        //if (isRaycastHit) Debug.DrawLine(controller.MainCam.transform.position, hitInfo.point, Color.green);
        //else              Debug.DrawRay(controller.MainCam.transform.position, controller.MainCam.transform.forward, Color.red);
    }


    #region Private Methods
    /// <summary>
    /// Set the targeted box's parent to this transform then instanciates an outline.
    /// </summary>
    private void GrabBox()
    {
        if (isRaycastHit)
        {
            if (hitInfo.transform.tag == "Box")
            {
                if (boxHighlight != null)
                {
                    Destroy(boxHighlight);
                }

                currentBox = hitInfo.transform.gameObject;

                //Clone grabbed box for outline setup
                boxOutline = Instantiate(currentBox);

                //Disable physics and collision
                currentBox.GetComponent<Rigidbody>().isKinematic = true;
                currentBox.GetComponent<Collider>().enabled = false;
                
                //Set grabbed box as child of main cam
                currentBox.transform.parent = controller.MainCam.transform;
                currentBox.transform.localPosition = Vector3.zero + objectOffset;
                currentBox.transform.rotation = controller.MainCam.transform.rotation;                

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

    /// <summary>
    /// Locks on other boxes sides else just sits on top of the currently targeted surface.
    /// </summary>
    private void ShowBoxPlacement()
    {
        if (isRaycastHit)
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
                    boxOutline.transform.position = hitInfo.point + hitInfo.normal / 2;
                    boxOutline.transform.rotation = transform.rotation;
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

    /// <summary>
    /// Instanciates a copy of the current box target then changes its color for highlight effect.
    /// </summary>
    private void HighlightTarget()
    {
        if (isRaycastHit)
        {
            if (hitInfo.transform.tag == "Box")
            {
                if (boxHighlight == null)
                {                
                    boxHighlight = Instantiate(hitInfo.transform.gameObject);
                    
                    //Remove physics and colliders
                    Destroy(boxHighlight.GetComponent<Collider>());
                    Destroy(boxHighlight.GetComponent<Box>());
                    Destroy(boxHighlight.GetComponent<Rigidbody>());

                    //Retain mesh and replace material
                    Renderer renderer = boxHighlight.GetComponent<Renderer>();
                    renderer.material = materialHighlight;

                    //Disable shadows
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    renderer.receiveShadows = false;

                    //Slight size offset just to prevent "z-fighting"
                    boxHighlight.transform.localScale += new Vector3(0.0001f, 0.0001f, 0.0001f);
                }
                else
                {
                    boxHighlight.transform.position = hitInfo.transform.position;
                    boxHighlight.transform.rotation = hitInfo.transform.rotation;
                }
            }
            else
            {
                if (boxHighlight != null)
                {
                    Destroy(boxHighlight);
                }
            }
        }
        else
        {
            if (boxHighlight != null)
            {
                Destroy(boxHighlight);
            }
        }
    }

    /// <summary>
    /// Grabbed box copies the outline transform before placement.
    /// </summary>
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

    /// <summary>
    /// Applies offset position before throwing.
    /// </summary>
    private void ThrowBox()
    {
        if (isRaycastHit && Vector3.Distance(hitInfo.point, this.transform.position) >= 2.5 ||
            !isRaycastHit)
        {
            Rigidbody boxRB = currentBox.GetComponent<Rigidbody>();
            Vector3 newPos = controller.MainCam.transform.localPosition;
            newPos.z = objectOffset.z;
            currentBox.transform.localPosition = newPos;
            currentBox.transform.parent = null;
            currentBox.layer = LayerMask.NameToLayer("Default");
            currentBox.GetComponent<Collider>().enabled = true;
            boxRB.isKinematic = false;
            boxRB.AddForce(controller.MainCam.transform.forward * throwForce, ForceMode.Impulse);
            currentBox = null;
            Destroy(boxOutline);
        }
    }

    /// <summary>
    /// Applies impulse force on boxes.
    /// </summary>
    private void PunchBox()
    {
        if (isRaycastHit)
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
        if (isRaycastHit && 
            Vector3.Distance(hitInfo.point, this.transform.position) >= boxPlacementDeadzone)
        {
            crosshair.transform.position = controller.MainCam.WorldToScreenPoint(hitInfo.point);
        }
        else
        {
            crosshair.transform.position = controller.MainCam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f, 0.5f));
        }
    }

    /// <summary>
    /// Do a raycast from the player cam.
    /// </summary>
    /// <param name="hitInfo">Raycast output</param>
    /// <returns>Returns true if the raycast hits a collider.</returns>
    private bool DoRaycast (out RaycastHit hitInfo)
    {
        return Physics.Raycast(controller.MainCam.transform.position, controller.MainCam.transform.forward, out hitInfo, interactionDistance);
    }
    #endregion
}
