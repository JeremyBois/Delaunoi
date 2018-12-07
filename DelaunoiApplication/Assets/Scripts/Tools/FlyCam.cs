using UnityEngine;
using System.Collections;


/// <summary>
/// A Unity Scene Camera clone. Priceless for debugging.
/// </summary>
public class FlyCam : MonoBehaviour
{

// PUBLIC
    // Rotate camera
    public float rotationSensitivity = 90.0f;

    // Zoom
    public float zoomSensitivity = 230.0f;

    // Move world
    public float middleMouseSensibility = 5.0f;

    // Move camera
    public float  acceleration = 25.0f;
    public float  normalMoveSpeed = 10.0f;


// PRIVATE
    // Move camera
    private float currentSpeed;

    // Rotation
    public float rotationX = 0.0f;
    public float rotationY = 0.0f;

    // Move world
    private Vector3 previousMousePos;
    private Vector3 currentMousePos;
    private float intensityX;
    private float intensityY;


    void Start()
    {
        currentSpeed = normalMoveSpeed;

        // Get init value from transform
        rotationX = transform.rotation.eulerAngles.y;
        rotationY = transform.rotation.eulerAngles.x;
    }

    void Update()
    {
        float delta = Time.deltaTime;

        // Zoom
        Camera.main.fieldOfView -= Input.GetAxis("Mouse ScrollWheel") *
                                   zoomSensitivity * delta;




        // Lock / Unlock mouse
        if (Input.GetKeyDown(KeyCode.End))
        {
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked
                                ? CursorLockMode.None
                                : CursorLockMode.Locked;
        }



        // Move world with middle click
        if (Input.GetMouseButtonDown(2))
        {
            previousMousePos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(2))
        {
            currentMousePos = Input.mousePosition;
            intensityX = currentMousePos.x - previousMousePos.x;
            intensityY = currentMousePos.y - previousMousePos.y;
            transform.position -= middleMouseSensibility * transform.right * intensityX * delta;
            transform.position -= middleMouseSensibility * transform.up * intensityY * delta;

            previousMousePos = currentMousePos;
        }




        // Only get mouvement inputs if right click pressed
        if (!Input.GetMouseButton(1))
        {
            // Reinit
            currentSpeed = normalMoveSpeed;
            return;
        }




        // Rotation
        rotationX += Input.GetAxis("Mouse X") * rotationSensitivity * delta;
        rotationY += Input.GetAxis("Mouse Y") * rotationSensitivity * delta;
        rotationY = Mathf.Clamp(rotationY, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(rotationY, Vector3.left);




        // Movement
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            currentSpeed += acceleration * delta;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            currentSpeed -= acceleration * delta;
        }
        transform.position += transform.forward * currentSpeed * Input.GetAxis("Vertical") * delta;
        transform.position += transform.right * currentSpeed * Input.GetAxis("Horizontal") * delta;

        if (Input.GetKey(KeyCode.Q)) { transform.position += transform.up * currentSpeed * delta; }
        if (Input.GetKey(KeyCode.E)) { transform.position -= transform.up * currentSpeed * delta; }
    }
}
