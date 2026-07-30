using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    [Header("オフセット")]
    public Vector3 offset = new Vector3(0f, 15f, -10f);

    [Header("追従速度")]
    public float smoothSpeed = 10f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desired = target.position + offset;

        transform.position = Vector3.Lerp(
            transform.position,
            desired,
            smoothSpeed * Time.deltaTime);
    }
}