using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerRopeGrab : MonoBehaviour
{
    [Header("Rope Settings")]
    public LayerMask ropeLayer;
    public float swingForce = 15f;
    public float jumpOffForce = 12f;
    public float climbSpeed = 3f;

    private Rigidbody2D rb;
    private HingeJoint2D ropeJoint;
    private bool isSwinging = false;
    private HeroKnight playerMovement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerMovement = GetComponent<HeroKnight>();
    }

    void Update()
    {
        if (isSwinging)
        {
            HandleSwinging();
        }
    }

    /// <summary>
    /// Detects contact with a rope trigger and processes user input to grab it.
    /// </summary>
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isSwinging && ((1 << collision.gameObject.layer) & ropeLayer) != 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                AttachToRope(collision.attachedRigidbody);
            }
        }
    }

    /// <summary>
    /// Connects the player to the rope using a HingeJoint2D.
    /// </summary>
    private void AttachToRope(Rigidbody2D ropeLink)
    {
        isSwinging = true;

        if (playerMovement != null) playerMovement.enabled = false;

        ropeJoint = gameObject.AddComponent<HingeJoint2D>();
        ropeJoint.connectedBody = ropeLink;
        ropeJoint.autoConfigureConnectedAnchor = false;
        ropeJoint.connectedAnchor = Vector2.zero;

        ropeJoint.anchor = new Vector2(0, 1f);
    }

    /// <summary>
    /// Processes input for swinging, climbing, and detaching from the rope.
    /// </summary>
    private void HandleSwinging()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        rb.AddForce(Vector2.right * horizontalInput * swingForce);

        float verticalInput = Input.GetAxisRaw("Vertical");
        if (verticalInput != 0)
        {
            ropeJoint.anchor += new Vector2(0, -verticalInput * climbSpeed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            DetachFromRope();
            rb.linearVelocity = new Vector2(horizontalInput * swingForce * 0.5f, jumpOffForce);
        }
    }

    /// <summary>
    /// Detaches the player from the rope and restores movement capabilities.
    /// </summary>
    private void DetachFromRope()
    {
        isSwinging = false;
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
            rb.linearVelocity = Vector2.zero;
        }
        if (ropeJoint != null)
        {
            Destroy(ropeJoint);
        }
    }
}