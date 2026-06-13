using UnityEngine;

public class CameraFollow : MonoBehaviour
{
  /// <summary>
  /// Redirects the camera to follow a new target. Called by CharacterManager on swap.
  /// Resets the smoothing velocity so the camera doesn't drift from the old target's momentum.
  /// </summary>
  public void SetTarget(Transform newTarget)
  {
    target           = newTarget;
    currentVelocity  = Vector3.zero; // Reset SmoothDamp velocity to prevent drift
  }
  [SerializeField] private Transform target; // The player's transform
  [SerializeField] private float smoothSpeed = 0.125f; // How smoothly the camera follows
  [SerializeField] private Vector3 offset; // Offset from the player (e.g., to see above/behind)
  [SerializeField] private float deadZoneX = 1f; // Horizontal dead zone
  [SerializeField] private float deadZoneY = 1f; // Vertical dead zone

  private Vector3 currentVelocity = Vector3.zero;

  void LateUpdate()
  {
    if (target == null)
    {
      Debug.LogWarning("CameraFollow: Target is not assigned!");
      return;
    }

    Vector3 targetPosition = target.position + offset;
    Vector3 currentPosition = transform.position;

    // Calculate the difference between camera and target position
    float deltaX = targetPosition.x - currentPosition.x;
    float deltaY = targetPosition.y - currentPosition.y;

    // Check horizontal dead zone
    if (Mathf.Abs(deltaX) > deadZoneX)
    {
      // If outside dead zone, calculate new X position
      targetPosition.x = currentPosition.x + deltaX - Mathf.Sign(deltaX) * deadZoneX;
    }
    else
    {
      // If inside dead zone, keep current X position
      targetPosition.x = currentPosition.x;
    }

    // Check vertical dead zone
    if (Mathf.Abs(deltaY) > deadZoneY)
    {
      // If outside dead zone, calculate new Y position
      targetPosition.y = currentPosition.y + deltaY - Mathf.Sign(deltaY) * deadZoneY;
    }
    else
    {
      // If inside dead zone, keep current Y position
      targetPosition.y = currentPosition.y;
    }

    // Maintain the Z offset calculated from the target and offset
    // targetPosition.z is already correct.

    // Smoothly move the camera towards the target position
    transform.position = Vector3.SmoothDamp(currentPosition, targetPosition, ref currentVelocity, smoothSpeed);
  }
}
