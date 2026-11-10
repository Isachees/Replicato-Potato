using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public float cooldown = 0.5f;

    private Animator animator;
    private float nextPressTime;

    public int type = 0;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void Press()
    {
        if (Time.time < nextPressTime) return;
        nextPressTime = Time.time + cooldown;

        if (animator != null)
        {
            animator.SetTrigger("Press");
        }

        if (type == 0)
        {
            if (PotatoHead.Instance != null) PotatoHead.Instance.ResetPotato();
        }
        else
        {
            if (ExamplePotatoDisplay.Instance != null) ExamplePotatoDisplay.Instance.Randomize();
        }
    }
}