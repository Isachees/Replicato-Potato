using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;


public class CheckButton : MonoBehaviour
{
    [Header("audio")]
    public AudioSource audioSource;
    public AudioClip winClip;
    public AudioClip failClip;

    [Header("boton")]
    public float cooldown = 0.5f;

    [Header("events")]
    public UnityEvent onFinished;

    private Animator animator;
    private float nextPressTime;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();

       
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    public void Press()
    {
        if (Time.time < nextPressTime) return;
        nextPressTime = Time.time + cooldown;

        if (animator != null) animator.SetTrigger("Press");

        ExamplePotatoDisplay display = ExamplePotatoDisplay.Instance;
        if (display == null)
        {
            Debug.LogWarning("CheckButton: ExamplePotatoDisplay.Instance es null");
            return;
        }

        ExamplePotatoDisplay.PotatoParts target = display.GetCurrentParts();

        List<BodyPiece> attached = new List<BodyPiece>();
        foreach (BodyPiece piece in FindObjectsByType<BodyPiece>(FindObjectsSortMode.None))
        {
            if (piece.IsAttached) attached.Add(piece);
        }

        int earsExpected = display.twoEars ? 2 : 1;

        List<string> failed = new List<string>();
        if (!CategoryOk(display.eyePrefabs, target.eye, attached, 1)) failed.Add("Ojos");
        if (!CategoryOk(display.nosePrefabs, target.nose, attached, 1)) failed.Add("Nariz");
        if (!CategoryOk(display.mouthPrefabs, target.mouth, attached, 1)) failed.Add("Boca");
        if (!CategoryOk(display.earPrefabs, target.ear, attached, earsExpected)) failed.Add("Orejas");

        bool win = failed.Count == 0;

        Debug.Log(win
            ? "CheckButton: CORRECTO"
            : "CheckButton: INCORRECTO, fallan: " + string.Join(", ", failed));

        AudioClip clip = win ? winClip : failClip;

        if (win) Debug.Log("CheckButton: sonaria el audio de VICTORIA");
        else Debug.Log("CheckButton: sonaria el audio de FALLO");

        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        else Debug.Log("CheckButton: no hay clip o AudioSource asignado (normal si aun no tienes audio)");
    }

    bool CategoryOk(GameObject[] prefabs, string targetName, List<BodyPiece> attached, int expectedCount)
    {
        if (prefabs == null || prefabs.Length == 0 || targetName == null) return true;

        HashSet<string> names = new HashSet<string>();
        foreach (GameObject p in prefabs)
        {
            if (p != null) names.Add(p.name);
        }

        int inCategory = 0;
        int correct = 0;
        foreach (BodyPiece piece in attached)
        {
            string clean = CleanName(piece.name);
            if (!names.Contains(clean)) continue;
            inCategory++;
            if (clean == targetName) correct++;
        }

        return inCategory == expectedCount && correct == expectedCount;
    }

    static string CleanName(string raw)
    {
        return Regex.Replace(raw, @"\s*\(.*?\)", "").Trim();
    }
}