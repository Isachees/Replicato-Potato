using UnityEngine;

public class ButtonLabel : MonoBehaviour
{
    public string text = "TEXTO";
    public Color color = Color.white;
    public float heightOffset = 0.12f;   
    public float characterSize = 0.012f; 
    public int fontSize = 100;
    public bool keepUpright = true;      

    private TextMesh tm;
    private Collider col;
    private Camera cam;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoSetup()
    {
        foreach (CheckButton b in FindObjectsByType<CheckButton>())
            Add(b.gameObject, "VALIDAR", new Color(0.2f, 1f, 0.3f));

        foreach (ResetButton b in FindObjectsByType<ResetButton>())
        {
            if (b.type == 0) Add(b.gameObject, "RESETEAR", new Color(1f, 0.25f, 0.25f));
            else Add(b.gameObject, "REROLL", new Color(0.35f, 0.65f, 1f));
        }
    }

    static void Add(GameObject target, string text, Color color)
    {
        if (target.GetComponent<ButtonLabel>() != null) return;
        ButtonLabel l = target.AddComponent<ButtonLabel>();
        l.text = text;
        l.color = color;
    }

    void Start()
    {
        col = GetComponentInChildren<Collider>();

        GameObject go = new GameObject("Label_" + text);
        go.transform.SetParent(transform, true);

        tm = go.AddComponent<TextMesh>();
        tm.text = text;
        tm.anchor = TextAnchor.LowerCenter;
        tm.alignment = TextAlignment.Center;
        tm.fontStyle = FontStyle.Bold;
        tm.fontSize = fontSize;
        tm.characterSize = characterSize;
        tm.color = color;

        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tm.font = font;
        go.GetComponent<MeshRenderer>().material = font.material;
    }

    void LateUpdate()
    {
        if (tm == null) return;

        if (cam == null || !cam.isActiveAndEnabled) cam = Camera.main;
        if (cam == null) return;

        Vector3 pos;
        if (col != null)
        {
            Bounds b = col.bounds;
            pos = new Vector3(b.center.x, b.max.y + heightOffset, b.center.z);
        }
        else
        {
            pos = transform.position + Vector3.up * heightOffset;
        }
        tm.transform.position = pos;

        Vector3 dir = pos - cam.transform.position;
        if (keepUpright) dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            tm.transform.rotation = Quaternion.LookRotation(dir, Vector3.up);
    }
}