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
    public bool IsHolding { get { return heldPiece != null; } }

    void Start()
    {
        holdPoint = new GameObject("HoldPoint_" + controller).transform;
        holdPoint.SetParent(transform, false);
        holdPoint.localPosition = new Vector3(0f, 0f, holdDistance);

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

    void HandleAiming()
    {
        Vector3 origin = transform.position;
        Vector3 end = origin + transform.forward * maxDistance;
        BodyPiece target = null;
        ResetButton button = null;
        CheckButton checkButton = null;
        ButtonLabel label = null;

        if (Physics.Raycast(origin, transform.forward, out RaycastHit hit, maxDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            end = hit.point;
            target = hit.collider.GetComponentInParent<BodyPiece>();
            button = hit.collider.GetComponentInParent<ResetButton>();
            checkButton = hit.collider.GetComponentInParent<CheckButton>();
            label = hit.collider.GetComponentInParent<ButtonLabel>();
            //Debug.Log("Rayo toca: " + hit.collider.name + " | CheckButton: " + (checkButton != null) + " | ResetButton: " + (button != null));
        }

        DrawLine(origin, end, (target != null || button != null || checkButton != null) ? hoverColor : lineColor);
        if (label != null) label.Hover();

        bool triggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller);
        //if (triggerDown) Debug.Log("Gatillo " + controller + " | checkButton: " + (checkButton != null));

        if (checkButton != null && triggerDown)
        {
            checkButton.Press();
            return;
        }

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