using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Cameras;

public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 20.0f;
    public float zoomSpeed = 20.0f;
    public Vector3 nearPos;
    public Vector3 farPos;
    private Camera camera;
    private ProtectCameraFromWallClip antiClip;


    // Start is called before the first frame update
    void Start()
    {
        camera = this.GetComponentInChildren<Camera>();
        antiClip = this.GetComponent<ProtectCameraFromWallClip>();
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.Rotate(this.transform.up, Input.GetAxis("Mouse X") * rotationSpeed * SettingsMenu.currentMouseSensitivity * Time.deltaTime);

        float mouseScroll = Mathf.Abs(Input.mouseScrollDelta.y);
        Vector3 finalPos = Vector3.zero;
        
        //Scroll Up
        if (Input.mouseScrollDelta.y > 0) finalPos = nearPos;
        
        //Scroll Down
        else if (Input.mouseScrollDelta.y < 0) finalPos = farPos;
        
        
        antiClip.cameraOffset = Vector3.Lerp(antiClip.cameraOffset, finalPos, mouseScroll * zoomSpeed * Time.deltaTime);
    }
}
