using UnityEngine;

// attach this to every body part prefab (eye, ear, nose, mouth)
[RequireComponent(typeof(Rigidbody))]
public class BodyPiece : MonoBehaviour
{
    public bool IsHeld { get; private set; }
    public bool IsAttached { get; private set; }

    public Vector3 Center { get { return col.bounds.center; } }

    private Rigidbody rb;
    private Collider col;
    private Transform originalParent;

    private Vector3 lastPosition;
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

        // make the piece follow the hand
        transform.SetParent(holdPoint, true);

        // move the piece so its center is exactly on the hold point
        transform.position += holdPoint.position - col.bounds.center;

        lastPosition = transform.position;
        handVelocity = Vector3.zero;
    }
    void Update()
    {
        if (!IsHeld) return;

        Vector3 currentVelocity = (transform.position - lastPosition) / Time.deltaTime;

        // mix with the previous value so it is not so jumpy
        handVelocity = Vector3.Lerp(handVelocity, currentVelocity, 0.5f);

        lastPosition = transform.position;
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