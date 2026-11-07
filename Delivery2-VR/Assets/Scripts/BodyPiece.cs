using UnityEngine;

// attach this to every body part prefab (eye, ear, nose, mouth)
[RequireComponent(typeof(Rigidbody))]
public class BodyPiece : MonoBehaviour
{
    public bool IsHeld { get; private set; }

    private Rigidbody rb;
    private Collider col;
    private Transform originalParent;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        originalParent = transform.parent;
    }

    // called when a controller grabs the piece
    public void Grab(Transform holdPoint)
    {
        IsHeld = true;

        // stop all movement and turn off physics while it is in the hand
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        // make the piece follow the hand
        transform.SetParent(holdPoint, true);

        // move the piece so its center is exactly on the hold point
        transform.position += holdPoint.position - col.bounds.center;
    }

    // called when the controller lets go of the piece
    public void Release()
    {
        IsHeld = false;

        // go back to the old parent and let physics take over (it falls)
        transform.SetParent(originalParent, true);
        rb.isKinematic = false;
    }
}