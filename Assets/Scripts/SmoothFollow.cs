using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    public Transform target;              // The target the camera follows
    public float followDistance = 5f;     // Distance behind the target
    public float height = 2f;             // Height above the target
    public float smoothSpeed = 0.125f;    // Smoothing factor
    public float rotateSpeed = 0f;        // Orbit speed (degrees per second)

    public GameObject sunObject;          // Optional sun object
    public Vector3 sunWorldPosition;      // Static sun position in world space
    public bool attachSunToCamera = false;// If true, sun follows the camera

    private Vector3 velocity = Vector3.zero;
    private float currentAngle = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        if (rotateSpeed != 0f)
        {
            currentAngle += rotateSpeed * Time.deltaTime;
            currentAngle %= 360f; // Keep it within 0–360 range
        }
        else
        {
            // Reset camera behind the target when rotation is disabled
            currentAngle = target.eulerAngles.y;
        }

        // Calculate orbit direction around target based on current angle
        Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -followDistance);

        Vector3 desiredPosition = target.position + offset + Vector3.up * height;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
        transform.LookAt(target);

        // Handle sun object
        if (sunObject != null)
        {
            if (attachSunToCamera)
            {
                if (sunObject.transform.parent != transform)
                    sunObject.transform.SetParent(transform, false);
                    sunObject.transform.localPosition = new Vector3(0f, 0f, -5.5f);
            }
            else
            {
                if (sunObject.transform.parent != null)
                    sunObject.transform.SetParent(null);
                sunObject.transform.position = sunWorldPosition;
            }
        }
    }


    public void SetFollowDistance(float newDistance) => followDistance = newDistance;
    public void SetHeight(float newHeight) => height = newHeight;
    public void SetRotateSpeed(float newSpeed) => rotateSpeed = newSpeed;
    public void SetSunWorldPosition(Vector3 position) => sunWorldPosition = position;
    public void AttachSunToCamera(bool attach) => attachSunToCamera = attach;
    public void SetSunObject(GameObject sun) => sunObject = sun;
    public void SetTarget(Transform currentTarget) => target = currentTarget;
}
