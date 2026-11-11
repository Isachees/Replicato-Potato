using UnityEngine;

// attach this to every body part prefab (eye, ear, nose, mouth)
[RequireComponent(typeof(Rigidbody))]
public class BodyPiece : MonoBehaviour
{
    public float grabSpeed = 50f;

    public bool IsHeld { get; private set; }
    public bool IsAttached { get; private set; }

    public Vector3 Center { get { return col.bounds.center; } }

    private Rigidbody rb;
    private Collider col;
    private Transform originalParent;

    private Transform hold;
    private Vector3 targetLocalPosition;

    private Vector3 lastHoldPosition;
    private Vector3 handVelocity;

    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalParent = transform.parent;

        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    // called when a controller grabs the piece
    public void Grab(Transform holdPoint)
    {
        IsHeld = true;
        IsAttached = false;

        // stop all movement and turn off physics while it is in the hand
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // where the piece must end up: its center exactly on the hold point
        Vector3 targetWorldPosition = transform.position + (holdPoint.position - col.bounds.center);

        // follow the hand, but stay where we are (Update moves us to the target)
        transform.SetParent(holdPoint, true);
        hold = holdPoint;
        targetLocalPosition = holdPoint.InverseTransformPoint(targetWorldPosition);

        lastHoldPosition = holdPoint.position;
        handVelocity = Vector3.zero;
    }

    void Update()
    {
        if (!IsHeld) return;

        // fly towards the hand (does nothing once it has arrived)
        transform.localPosition = Vector3.MoveTowards(
            transform.localPosition, targetLocalPosition, grabSpeed * Time.deltaTime);

        // measure the speed of the hand, not of the piece,
        // so the flight does not count as a throw
        Vector3 currentVelocity = (hold.position - lastHoldPosition) / Time.deltaTime;

        // mix with the previous value so it is not so jumpy
        handVelocity = Vector3.Lerp(handVelocity, currentVelocity, 0.5f);

        lastHoldPosition = hold.position;
    }

    // called when the controller lets go of the piece
    public void Release()
    {
        IsHeld = false;

        // go back to the old parent and let physics take over (it falls)
        transform.SetParent(originalParent, true);
        rb.isKinematic = false;

        // throw it with the speed the hand had
        rb.linearVelocity = handVelocity;

        if (PotatoHead.Instance != null)
        {
            PotatoHead.Instance.TrySnap(this);
        }
    }

    public void AttachTo(Transform parent, Vector3 targetCenter, Quaternion rotation)
    {
        transform.rotation = rotation;

        transform.position += targetCenter - col.bounds.center;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        transform.SetParent(parent, true);

        IsAttached = true;
    }

    public void ResetToStart()
    {
        IsHeld = false;
        IsAttached = false;

        transform.SetParent(originalParent, false);
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;

        rb.isKinematic = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}