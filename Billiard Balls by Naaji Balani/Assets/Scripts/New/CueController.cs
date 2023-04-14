using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CueController : MonoBehaviour
{
    public GameObject cueObject;
    public float forceMultiplier;

    private Vector3 cueBallPosition;
    private Vector3 targetBallPosition;
    private Vector3 targetBallDirection;
    private bool isDragging;
    [SerializeField] LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer.positionCount = 2;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            cueBallPosition = transform.position;
            targetBallPosition = cueObject.transform.position;
            lineRenderer.enabled = true;
        }

        if (Input.GetMouseButton(0) && isDragging)
        {
            // Rotate the cueObject based on the user input
            float rotationX = Input.GetAxis("Mouse X");
            cueObject.transform.Rotate(new Vector3(0, -rotationX, 0));

            // Calculate the direction of the cue ball based on the rotation of the cueObject
            Vector3 cueBallDirection = cueObject.transform.forward;

            // Draw a line from the cue ball to the target ball
            lineRenderer.SetPosition(0, cueBallPosition);
            lineRenderer.SetPosition(1, targetBallPosition);

            // Cast a ray from the cue ball to the target ball
            RaycastHit hit;
            if (Physics.Raycast(cueBallPosition, cueBallDirection, out hit))
            {
                // Check if the ray hits a target ball
                if (hit.collider.tag == "Player")
                {
                    // Calculate the direction in which the target ball will move after collision
                    Vector3 contactPoint = hit.point;
                    Vector3 surfaceNormal = hit.normal;
                    targetBallDirection = Vector3.Reflect(cueBallDirection, surfaceNormal);

                    // Draw a line from the target ball showing where it will move after being hit
                    lineRenderer.SetPosition(1, contactPoint);
                    lineRenderer.positionCount = 3;
                    lineRenderer.SetPosition(2, contactPoint + targetBallDirection * 5f);
                }
                else
                {
                    // If the ray does not hit a target ball, just draw a line from the cue ball to the cursor position
                    lineRenderer.SetPosition(1, hit.point);
                }
            }
            else
            {
                // If the ray does not hit anything, just draw a line from the cue ball to the cursor position
                lineRenderer.SetPosition(1, cueBallPosition + cueBallDirection * 10f);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            isDragging = false;

            // Add a force to the cue ball in the direction of the cueObject
            Rigidbody rb = GetComponent<Rigidbody>();
            rb.AddForce(cueObject.transform.forward * forceMultiplier, ForceMode.Impulse);

            //lineRenderer.enabled = false;
        }
    }
}
