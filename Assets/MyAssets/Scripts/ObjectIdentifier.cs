using System.Collections;
using UnityEngine;
using TMPro;

public class ObjectIdentifier : MonoBehaviour
{
    public float rayLength = 100f;
    public LayerMask targetLayer;
    public TextMeshProUGUI objectNameDisplay;
    public TextMeshProUGUI messageDisplay;

    public GameObject cannonBallPrefab;
    public Transform cannonFront;
    public Transform yawCannon; 
    public Transform pitchCannon; 
    public Transform boxLidHinge; 

    private Camera playerCamera;
    private bool hasCannonBall = false;
    private bool isCannonReady = false;
    private bool isInAimingMode = false;
    private bool lidIsOpen = false;
    private bool isLidMoving = false;



    void Start()
    {
        playerCamera = Camera.main;
        if (objectNameDisplay == null || messageDisplay == null || cannonBallPrefab == null || cannonFront == null || yawCannon == null || pitchCannon == null || boxLidHinge == null)
        {
            Debug.LogError("haven't set reference in inspector");
        }
        else
        {
            objectNameDisplay.text = "";
            messageDisplay.text = "Go and get a ball";
        }
    }

    void Update()
    {
        RaycastHit hit;
        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out hit, rayLength, targetLayer))
        {
            objectNameDisplay.text = hit.collider.gameObject.name;

            if (!isInAimingMode && Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == "Box o' balls" && !hasCannonBall && !isLidMoving)
            {
                hasCannonBall = true;
                messageDisplay.text = "Click the cannon to load";
                StartCoroutine(ToggleLid(true));
            }
            else if (!isInAimingMode && Input.GetMouseButtonDown(0) && hit.collider.gameObject.name == "Cannon" && hasCannonBall)
            {
                isCannonReady = true;
                messageDisplay.text = "Press Z to aim";
            }
        }
        else
        {
            objectNameDisplay.text = "";
        }

        if (isCannonReady && Input.GetKeyDown(KeyCode.Z))
        {
            isInAimingMode = !isInAimingMode;
            messageDisplay.text = isInAimingMode ? "Use E/Q to yaw, T/G to pitch, Z to stop aiming" : "Press F to fire";
        }

        if (isInAimingMode)
        {
            AdjustCannonAiming();
        }

        if (!isInAimingMode && isCannonReady && Input.GetKeyDown(KeyCode.F))
        {
            FireCannonBall();
            isCannonReady = false;
            hasCannonBall = false;
            if (!isLidMoving) 
            {
                StartCoroutine(ToggleLid(false)); 
            }
            messageDisplay.text = "Go and get a ball";
        }
    }

    IEnumerator ToggleLid(bool open)
    {
        isLidMoving = true; 
        float targetRotation = open ? -45f : 0f; 
        float currentRotation = boxLidHinge.localEulerAngles.x;
        if (currentRotation > 180) currentRotation -= 360;

        while (open ? currentRotation > targetRotation : currentRotation < targetRotation)
        {
            currentRotation += (open ? -90 : 90) * Time.deltaTime; 
            currentRotation = Mathf.Clamp(currentRotation, -45, 0); 
            boxLidHinge.localEulerAngles = new Vector3(currentRotation, boxLidHinge.localEulerAngles.y, boxLidHinge.localEulerAngles.z);
            yield return null;
        }

        lidIsOpen = open;
        if (open)
        {
            yield return new WaitForSeconds(1); 
            StartCoroutine(ToggleLid(false)); 
        }
        isLidMoving = false; 
    }


    void AdjustCannonAiming()
    {
        if (Input.GetKey(KeyCode.E))
        {
            yawCannon.Rotate(Vector3.up * 20 * Time.deltaTime, Space.World); 
        }
        if (Input.GetKey(KeyCode.Q))
        {
            yawCannon.Rotate(Vector3.down * 20 * Time.deltaTime, Space.World); 
        }
        float pitchAngle = pitchCannon.localEulerAngles.x;
        if (pitchAngle > 180) pitchAngle -= 360; 

        if (Input.GetKey(KeyCode.G) && pitchAngle < 0)
        {
            pitchCannon.Rotate(Vector3.right * 20 * Time.deltaTime, Space.Self); 
        }
        if (Input.GetKey(KeyCode.T) && pitchAngle > -10)
        {
            pitchCannon.Rotate(Vector3.left * 20 * Time.deltaTime, Space.Self); 
        }
    }

    void FireCannonBall()
    {
        Instantiate(cannonBallPrefab, cannonFront.position, cannonFront.rotation).AddComponent<Rigidbody>().AddForce(cannonFront.forward * 1300);
    }
}
