using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    [Header("references")]
    public Renderer disc;
    public ParticleSystem particles;

    [Header("idle look")]
    public Color idleColor = new Color(0.2f, 0.8f, 1f);
    public float idleSpeed = 0.2f;

    [Header("hover look")]
    public Color hoverColor = new Color(1f, 0.8f, 0.2f);
    public float hoverSpeed = 0.7f;

    public float changeSpeed = 8f;

    private Material discMat;
    private Material particleMat;
    private ParticleSystem.VelocityOverLifetimeModule vel;

    private Color currentColor;
    private float currentSpeed;
    private int lastHoverFrame = -10;

    void Start()
    {
        // instances of the materials so each point can have its own color
        discMat = disc.material;
        particleMat = particles.GetComponent<ParticleSystemRenderer>().material;

        // particles go up using velocity over lifetime (world space, so always up)
        vel = particles.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(0f);
        vel.z = new ParticleSystem.MinMaxCurve(0f);

        currentColor = idleColor;
        currentSpeed = idleSpeed;
        ApplyLook();
    }

    // the pointer calls this every frame while it is aiming at this point
    public void Hover()
    {
        lastHoverFrame = Time.frameCount;
    }

    void Update()
    {
        // if nobody called Hover in the last frames, the point is not being aimed at
        bool hovered = Time.frameCount - lastHoverFrame <= 1;

        Color targetColor = hovered ? hoverColor : idleColor;
        float targetSpeed = hovered ? hoverSpeed : idleSpeed;

        // smooth change
        float t = Time.deltaTime * changeSpeed;
        currentColor = Color.Lerp(currentColor, targetColor, t);
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, t);

        ApplyLook();
    }

    void ApplyLook()
    {
        discMat.SetColor("_BaseColor", currentColor);
        particleMat.SetColor("_BaseColor", currentColor);
        vel.y = new ParticleSystem.MinMaxCurve(currentSpeed);
    }
}