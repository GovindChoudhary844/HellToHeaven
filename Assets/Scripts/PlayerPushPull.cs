using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerPushPull : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float grabDistance = 1.5f;
    public float raycastOffsetY = 0.5f; 
    public LayerMask pushableLayer;
    public KeyCode grabKey = KeyCode.F;

    private Rigidbody2D rb;
    private FixedJoint2D fixedJoint;
    private bool isGrabbing = false;
    private GameObject grabbedObject;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>() ?? GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerPushPull: SpriteRenderer not found on player or children!");
        }

        fixedJoint = gameObject.AddComponent<FixedJoint2D>();
        fixedJoint.enabled = false;
    }

    void Update()
    {
        float facingDirection = spriteRenderer.flipX ? -1f : 1f;
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + raycastOffsetY);
        Vector2 rayDirection = new Vector2(facingDirection, 0);

        Debug.DrawRay(rayOrigin, rayDirection * grabDistance, Color.yellow);

        if (Input.GetKeyDown(grabKey) && !isGrabbing)
        {
            TryGrab(rayOrigin, rayDirection);
        }
        else if (Input.GetKeyUp(grabKey) && isGrabbing)
        {
            ReleaseObject();
        }
    }

    /// <summary>
    /// Attempts to raycast forward and grab a pushable object.
    /// </summary>
    private void TryGrab(Vector2 origin, Vector2 direction)
    {
        RaycastHit2D hit = Physics2D.Raycast(origin, direction, grabDistance, pushableLayer);

        if (hit.collider != null && hit.collider.GetComponent<Rigidbody2D>() != null)
        {
            isGrabbing = true;
            grabbedObject = hit.collider.gameObject;

            fixedJoint.connectedBody = grabbedObject.GetComponent<Rigidbody2D>();
            fixedJoint.enabled = true;
        }
    }

    /// <summary>
    /// Releases the currently grabbed object and disables the joint.
    /// </summary>
    private void ReleaseObject()
    {
        isGrabbing = false;
        fixedJoint.enabled = false;
        fixedJoint.connectedBody = null;
        grabbedObject = null;
    }
}