using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float speed = 10.0f;        // Vitesse de déplacement horizontal
    public float verticalSpeed = 5.0f; // Vitesse de déplacement vertical
    public float sensitivity = 1.0f;   // Sensibilité de la souris
    public float zoomSpeed = 2.0f;     // Vitesse de zoom

    private Vector3 moveDirection;
    private Camera camera;

    void Start()
    {
        camera = GetComponent<Camera>();
    }

    void Update()
    {
        moveDirection = Vector3.zero;

        // Déplacement horizontal avec ZQSD
        if (Input.GetKey(KeyCode.W)) moveDirection += transform.forward;
        if (Input.GetKey(KeyCode.S)) moveDirection -= transform.forward;
        if (Input.GetKey(KeyCode.A)) moveDirection -= transform.right;
        if (Input.GetKey(KeyCode.D)) moveDirection += transform.right;

        // Déplacement vertical avec Espace et A/shift
        if (Input.GetKey(KeyCode.Space)) moveDirection += Vector3.up;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.Q)) moveDirection += Vector3.down;

        // Appliquer le déplacement
        Vector3 horizontalMove = new Vector3(moveDirection.x, 0, moveDirection.z) * speed;
        Vector3 verticalMove = new Vector3(0, moveDirection.y, 0) * verticalSpeed;
        transform.Translate((horizontalMove + verticalMove) * Time.deltaTime, Space.World);

        // Rotation avec clic droit
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            transform.Rotate(Vector3.up * mouseX * sensitivity, Space.World);
            transform.Rotate(Vector3.left * mouseY * sensitivity, Space.Self);
        }

        // Zoom avec la molette
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        camera.fieldOfView -= scroll * zoomSpeed;
        camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, 10f, 80f);
    }
}
