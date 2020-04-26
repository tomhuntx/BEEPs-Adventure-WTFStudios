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

    private GameObject boxOutline;
    private Collider outlineCollider;
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
        if (Input.GetButtonDown("Grab Box")) GrabBox();
        if (currentBox != null)
        {
            ShowBoxPlacement();
            if (Input.GetButtonDown("Place Box")) PlaceBox();
            if (Input.GetButtonDown("Punch")) ThrowBox();
        }
    }

    private void GrabBox()
    {
        RaycastHit hitInfo;
        if (DoRaycast(out hitInfo))
        {
            //Debug.DrawLine(this.transform.position, controller.MainCam.transform.forward);
            if (hitInfo.transform.tag == "Box")
            {
                currentBox = hitInfo.transform.gameObject;

                currentBox.GetComponent<Rigidbody>().isKinematic = true;
                currentBox.GetComponent<Collider>().enabled = false;
                currentBox.transform.parent = controller.MainCam.transform;
                currentBox.transform.localPosition = Vector3.zero + objectOffset;
                currentBox.transform.rotation = Quaternion.identity;

                boxOutline = Instantiate(currentBox);
                Destroy(boxOutline.GetComponent<Collider>());
                //outlineCollider = boxOutline.GetComponent<Collider>();
                //outlineCollider.isTrigger = true;
                Renderer renderer = boxOutline.GetComponent<Renderer>();
                RendererModeChanger.SetToTransparent(renderer);
                Color alpha = renderer.material.color;
                alpha.a = 0.5f;
                renderer.material.color = alpha;
                renderer.receiveShadows = false;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                boxOutline.SetActive(false);
            }
        }
    }

    private void ShowBoxPlacement()
    {
        RaycastHit hitInfo;

        

        if (DoRaycast(out hitInfo))
        {
            boxOutline.SetActive(true);
            if (hitInfo.transform.tag == "Box")
            {
                boxOutline.transform.position = hitInfo.transform.position + hitInfo.normal;
                boxOutline.transform.rotation = Quaternion.Euler(hitInfo.normal) * hitInfo.transform.rotation;
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

    private void PlaceBox()
    {
        currentBox.transform.parent = null;
        currentBox.transform.position = boxOutline.transform.position;
        currentBox.transform.rotation = boxOutline.transform.rotation;
        currentBox.GetComponent<Collider>().enabled = true;
        currentBox.GetComponent<Rigidbody>().isKinematic = false;
        currentBox = null;
        Destroy(boxOutline);
    }

    private void ThrowBox()
    {
        Rigidbody boxRB = currentBox.GetComponent<Rigidbody>();
        currentBox.transform.parent = null;
        currentBox.GetComponent<Collider>().enabled = true;
        boxRB.isKinematic = false;
        boxRB.AddForce(controller.MainCam.transform.forward * throwForce, ForceMode.Impulse);
        currentBox = null;
        Destroy(boxOutline);
    }

    private void PunchBox()
    {

    }

    private bool DoRaycast (out RaycastHit hitInfo)
    {
        Vector3 origin = this.transform.position;
        origin.y += 0.5f;
        return Physics.Raycast(origin, controller.MainCam.transform.forward, out hitInfo, interactionDistance);
    }
}
