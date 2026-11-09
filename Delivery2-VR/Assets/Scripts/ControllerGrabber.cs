using UnityEngine;

public class ControllerGrabber : MonoBehaviour
{
    [Header("input")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("grab settings")]
    public float maxDistance = 5f;
    public float holdDistance = 0.2f;

    [Header("aim line")]
    public bool showLine = true;
    public Color lineColor = Color.white;
    public Color hoverColor = Color.green;

    private Transform holdPoint;
    private LineRenderer line;
    private BodyPiece heldPiece;
    // true while a piece is in the hand (used by the teleport pointer)
    public bool IsHolding { get { return heldPiece != null; } }

    void Start()
    {
        // empty object in front of the controller where the piece will be held
        holdPoint = new GameObject("HoldPoint_" + controller).transform;
        holdPoint.SetParent(transform, false);
        holdPoint.localPosition = new Vector3(0f, 0f, holdDistance);

        // simple line to see where we are pointing
        line = gameObject.AddComponent<LineRenderer>();
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.positionCount = 2;
        line.startWidth = 0.005f;
        line.endWidth = 0.005f;
    }

    void Update()
    {
        if (heldPiece == null)
        {
            HandleAiming();
        }
        else
        {
            HandleHolding();
        }
    }

    // no piece in hand: point with the ray and wait for the trigger
    void HandleAiming()
    {
        Vector3 origin = transform.position;
        Vector3 end = origin + transform.forward * maxDistance;
        BodyPiece target = null;
        ResetButton button = null;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            target = hit.collider.GetComponentInParent<BodyPiece>();
            button = hit.collider.GetComponentInParent<ResetButton>();
        }

        DrawLine(origin, end, (target != null || button != null) ? hoverColor : lineColor);

        bool triggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);

        //button
        if (button != null && triggerDown)
        {
            button.Press();
            return;
        }

        if (target != null && !target.IsHeld && triggerDown)
        {
            heldPiece = target;
            heldPiece.Grab(holdPoint);
            line.enabled = false;
        }
    }

    // piece in hand: it follows the controller until the trigger is released
    void HandleHolding()
    {
        if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller))
        {
            heldPiece.Release();
            heldPiece = null;
        }
    }

    void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        line.enabled = showLine;
        line.startColor = color;
        line.endColor = color;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}