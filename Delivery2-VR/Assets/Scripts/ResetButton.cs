using UnityEngine;

public class ResetButton : MonoBehaviour
{
    public float cooldown = 0.25f;

    private Animator animator;
    private float nextPressTime;

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

        if (PotatoHead.Instance != null)
        {
            PotatoHead.Instance.ResetPotato();
        }
    }
}