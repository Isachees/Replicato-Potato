using System.Collections;
using UnityEngine;

// attach this to the controller anchors (same object as ControllerGrabber)
public class TeleportPointer : MonoBehaviour
{
    [Header("input")]
    public OVRInput.Controller controller = OVRInput.Controller.RTouch;

    [Header("teleport settings")]
    public float maxDistance = 15f;
    public float fadeTime = 0.25f;

    // shared by both controllers so they cannot teleport at the same time
    private static bool isTeleporting;

    private Transform rig;
    private Transform head;
    private ScreenFader fader;
    private ControllerGrabber grabber;

    void Start()
    {
        // the camera rig is the root we are going to move
        OVRCameraRig cameraRig = GetComponentInParent<OVRCameraRig>();
        rig = cameraRig.transform;
        head = cameraRig.centerEyeAnchor;

        // get the fader or create it if it does not exist
        fader = head.GetComponent<ScreenFader>();
        if (fader == null)
        {
            fader = head.gameObject.AddComponent<ScreenFader>();
        }

        grabber = GetComponent<ControllerGrabber>();
    }

    void Update()
    {
        if (isTeleporting) return;

        // do not teleport while a piece is in the hand
        if (grabber != null && grabber.IsHolding) return;

        // triggers must be detected because the points use a trigger collider
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit,
                maxDistance, ~0, QueryTriggerInteraction.Collide))
        {
            TeleportPoint point = hit.collider.GetComponentInParent<TeleportPoint>();

            if (point != null)
            {
                point.Hover();

                if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, controller))
                {
                    StartCoroutine(TeleportTo(point.transform.position));
                }
            }
        }
    }

    IEnumerator TeleportTo(Vector3 target)
    {
        isTeleporting = true;

        // fade out
        yield return fader.FadeTo(1f, fadeTime);

        // move the rig so the head ends up over the point
        // (the head is not always in the center of the rig, so we use the difference)
        Vector3 offset = target - head.position;
        offset.y = 0f;

        Vector3 newPos = rig.position + offset;
        newPos.y = target.y; // floor height of the point
        rig.position = newPos;

        // fade in
        yield return fader.FadeTo(0f, fadeTime);

        isTeleporting = false;
    }
}