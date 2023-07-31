using UnityEngine;
using System.Collections;
 
/**
 * A script to have a moving camera working ike in the editor vue
*/
public class FlyCamera : MonoBehaviour {

// ################################################################################################################################################################################################################
// ################################################################################################## ATTRIBUTES ##################################################################################################
// ################################################################################################################################################################################################################

    /**
     * The camera's moving speed
    */
    [SerializeField] 
    public float navigationSpeed = 2.4f;

    /**
     * The camera's speed multiplier toogled when pressing shift
    */
    [SerializeField] 
    public float shiftMultiplier = 2f;

    /**
     * The camera's rotation sensitivity
    */
    [SerializeField] 
    public float sensitivity = 1.0f;

    /**
     * Moving by hand speed
    */
    [SerializeField] 
    public float panSensitivity = 0.5f;

    /**
     * The camera's zoom speed
    */
    [SerializeField] 
    public float mouseWheelZoomSpeed = 1.0f;

    /**
     * The unity camera
    */
    private Camera cam;

    /**
     * Anchor point to uptade camera's positon
    */
    private Vector3 anchorPoint;

    /**
     * Anchor point to uptade camera's rotation
    */
    private Quaternion anchorRot;

    /**
     * Boolean to tell if we're moving using the mouse
    */
    private bool isPanning;

    /**
     * x position using the mouse
    */
    private float pan_x;

    /**
     * y position using the mouse
    */
    private float pan_y;

    /**
     * Pan position
    */
    private Vector3 panComplete;


// ################################################################################################################################################################################################################
// ################################################################################################### METHODS ####################################################################################################
// ################################################################################################################################################################################################################

    /**
     * Instantiate the camera
    */
    public void Awake(){
        cam = GetComponent<Camera>();
    }

    /**
     * Update the camera position and angle
    */
    public void Update(){
        // move the camera using the mouse
        MousePanning();
        if(isPanning) return;
   
        // translate the camera using a,w,s,d,q,e
        Vector3 move = Vector3.zero;
        float speed = navigationSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.deltaTime * 9.1f;
        if(Input.GetKey(KeyCode.W))
            move += Vector3.forward * speed; // w = forward
        if(Input.GetKey(KeyCode.S))
            move -= Vector3.forward * speed; // s = backward
        if(Input.GetKey(KeyCode.D))
            move += Vector3.right * speed; // d = right
        if(Input.GetKey(KeyCode.A))
            move -= Vector3.right * speed; // a = left
        if(Input.GetKey(KeyCode.E))
            move += Vector3.up * speed; // e = up
        if(Input.GetKey(KeyCode.Q))
            move -= Vector3.up * speed; // q = down
        transform.Translate(move);

        // right click to rotate the camera
        if(Input.GetMouseButtonDown(1)) {
            anchorPoint = new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            anchorRot = transform.rotation;
        }
        if(Input.GetMouseButton(1)) {
            Quaternion rot = anchorRot;
            Vector3 dif = anchorPoint - new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            rot.eulerAngles += dif * sensitivity;
            transform.rotation = rot;
        }

        // zoom in or out
        MouseWheeling();
    }
 
    /**
     * Zoom using the mouse wheel
    */
    public void MouseWheeling(){
        float speed = 10*(mouseWheelZoomSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.deltaTime * 9.1f);
        
        // scroll down to zoom out
        Vector3 pos = transform.position;
        if (Input.GetAxis("Mouse ScrollWheel") < 0){
            pos = pos - (transform.forward*speed);
            transform.position = pos;
        }

        // scroll up to zoom in
        if (Input.GetAxis("Mouse ScrollWheel") > 0){
            pos = pos + (transform.forward*speed);
            transform.position = pos;
        }
    }
 
    /**
     * Translation using the mouse
    */
    public void MousePanning(){
        pan_x=-Input.GetAxis("Mouse X")*panSensitivity;
        pan_y=-Input.GetAxis("Mouse Y")*panSensitivity;
        panComplete = new Vector3(pan_x,pan_y,0);
   
        // press scroll button while moving the mouse to translate the camera
        if (Input.GetMouseButtonDown(2)){
            isPanning=true;
        }
   
        if (Input.GetMouseButtonUp(2)){
            isPanning=false;
        }
   
        if(isPanning){
            transform.Translate(panComplete);
        }
    }
 
}