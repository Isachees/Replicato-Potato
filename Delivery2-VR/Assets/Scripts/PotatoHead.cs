using UnityEngine;
using UnityEngine.InputSystem;

public class PotatoHead : MonoBehaviour
{
    public static PotatoHead Instance { get; private set; }

    [Header("snap settings")]

    public float snapDistance = 0.3f;
    public float surfaceOffset = 0f;
    public bool alignToSurface = false;

    [Header("reset settings")]
    public bool launchOnReset = true;
    public float resetLaunchForce = 15f;
    public float resetLaunchTorque = 5f;
    [Range(0f, 1f)] public float resetLaunchUpward = 0.05f;
    [Range(0f, 0.5f)] public float resetLaunchSpread = 0.1f;
    public bool resetLaunchAtPlayer = false;

    private Collider[] potatoColliders;

    void Awake()
    {
        Instance = this;

        Collider[] all = GetComponentsInChildren<Collider>();
        var list = new System.Collections.Generic.List<Collider>();
        foreach (Collider c in all)
        {
            if (c.isTrigger) continue;
            if (c.GetComponentInParent<BodyPiece>() != null) continue;
            list.Add(c);
        }
        potatoColliders = list.ToArray();
    }

    public bool TrySnap(BodyPiece piece)
    {
        Vector3 piecePos = piece.Center;

        float bestDistance = float.MaxValue;
        RaycastHit bestHit = default;
        bool found = false;

        foreach (Collider c in potatoColliders)
        {
            Vector3 center = c.bounds.center;
            Vector3 dir = piecePos - center;
            if (dir.sqrMagnitude < 0.0001f) dir = Vector3.up;
            dir.Normalize();

            Vector3 origin = center + dir * 10f;
            if (!c.Raycast(new Ray(origin, -dir), out RaycastHit hit, 20f)) continue;

            float surfaceDist = (hit.point - center).magnitude;
            float pieceDist = (piecePos - center).magnitude;

            float dist = pieceDist <= surfaceDist ? 0f : pieceDist - surfaceDist;

            if (dist < bestDistance)
            {
                bestDistance = dist;
                bestHit = hit;
                found = true;
            }
        }

        if (!found || bestDistance > snapDistance) return false;

        Vector3 targetCenter = bestHit.point + bestHit.normal * surfaceOffset;

        Quaternion rot = piece.transform.rotation;
        if (alignToSurface)
        {
            rot = Quaternion.FromToRotation(piece.transform.up, bestHit.normal) * rot;
        }

        piece.AttachTo(transform, targetCenter, rot);
        return true;
    }

    public void ResetPotato()
    {
        BodyPiece[] pieces = FindObjectsByType<BodyPiece>();
        foreach (BodyPiece p in pieces)
        {
            if (p.IsHeld) continue;
            if (!p.IsAttached) continue;

            if (launchOnReset)
            {
                Vector3 dir;
                Camera cam = Camera.main;

                if (resetLaunchAtPlayer && cam != null)
                {
                    dir = cam.transform.position - p.Center;
                }
                else
                {
                    dir = p.Center - transform.position;
                    dir.y = 0f;
                }

                if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;

                dir = dir.normalized + Vector3.up * resetLaunchUpward + Random.insideUnitSphere * resetLaunchSpread;
                p.Eject(dir.normalized * resetLaunchForce, Random.insideUnitSphere * resetLaunchTorque);
            }
            else
            {
                p.ResetToStart();
            }
        }
    }
}