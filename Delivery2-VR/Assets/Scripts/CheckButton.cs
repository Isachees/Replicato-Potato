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

    [Header("victoria")]
    public bool showConfetti = true;
    public ParticleSystem confetti; 
    public bool showWinText = true;
    public string winText = "¡CORRECTO!";
    public float winTextDuration = 3f;
    public float winTextDistance = 1.5f;

    [Header("fallo")]
    public bool launchPiecesOnFail = true;
    public float launchForce = 15f;
    public float launchTorque = 5f;
    [Range(0f, 1f)] public float launchUpward = 0.05f;  
    [Range(0f, 0.5f)] public float launchSpread = 0.1f; 
    public bool launchAtPlayer = false;                 

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
        foreach (BodyPiece piece in FindObjectsByType<BodyPiece>())
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

        AudioClip clip = win ? winClip : failClip;
        if (win) Debug.Log("CheckButton: sonaria el audio de VICTORIA");
        else Debug.Log("CheckButton: sonaria el audio de FALLO, fallan: " + string.Join(", ", failed));

        if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);

        if (win)
        {
            if (showConfetti) PlayConfetti();
            if (showWinText) SpawnWinText();
        }
        else if (launchPiecesOnFail)
        {
            foreach (BodyPiece piece in attached) LaunchPiece(piece);
        }

        onFinished.Invoke();
    }

    

    void LaunchPiece(BodyPiece piece)
    {
        Vector3 dir;
        Camera cam = Camera.main;

        if (launchAtPlayer && cam != null)
        {
            dir = cam.transform.position - piece.Center;
        }
        else
        {
            Vector3 origin = PotatoHead.Instance != null ? PotatoHead.Instance.transform.position : transform.position;
            dir = piece.Center - origin;
            dir.y = 0f;
        }

        if (dir.sqrMagnitude < 0.0001f) dir = transform.forward;

        dir = dir.normalized + Vector3.up * launchUpward + Random.insideUnitSphere * launchSpread;
        piece.Eject(dir.normalized * launchForce, Random.insideUnitSphere * launchTorque);
    }

    

    void PlayConfetti()
    {
        if (confetti == null)
        {
            Debug.LogWarning("CheckButton: no hay Particle System asignado en 'Confetti'");
            return;
        }

        confetti.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        confetti.Play();
    }

    

    void SpawnWinText()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        GameObject go = new GameObject("WinText");
        Vector3 pos = cam.transform.position + cam.transform.forward * winTextDistance + Vector3.up * 0.3f;
        go.transform.position = pos;
        go.transform.rotation = Quaternion.LookRotation(pos - cam.transform.position);

        TextMesh tm = go.AddComponent<TextMesh>();
        tm.text = winText;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.fontSize = 100;
        tm.characterSize = 0.03f;
        tm.color = Color.green;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        go.GetComponent<MeshRenderer>().material = font.material;

        Destroy(go, winTextDuration);
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