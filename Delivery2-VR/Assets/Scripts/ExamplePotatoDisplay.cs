using System.Collections;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


public class ExamplePotatoDisplay : MonoBehaviour
{
    public static ExamplePotatoDisplay Instance { get; private set; }

    [Header("Prefabs")]
    public GameObject potatoPrefab;
    public GameObject[] eyePrefabs;
    public GameObject[] nosePrefabs;
    public GameObject[] mouthPrefabs;
    public GameObject[] earPrefabs;

    [Header("Montaje de la patata")]
    public bool randomizeOnStart = true;
    public bool twoEars = true;
    public bool mirrorLeftEar = true;
    // estas alturas son respecto al tamaño de la patata que si no se descuadra (fracciones)
    [Range(0f, 1f)] public float eyesHeight = 0.75f;
    [Range(0f, 1f)] public float noseHeight = 0.47f;
    [Range(0f, 1f)] public float mouthHeight = 0.2f;
    [Range(0f, 1f)] public float earsHeight = 0.7f;
    // giro de las orejas hacia la cara 
    [Range(0f, 90f)] public float earsForwardAngle = 90f;
    public float surfaceGap = 0.002f;

    [Header("Pantalla")]
    public Renderer screenRenderer;          
    public Vector2 screenSize = new Vector2(1.6f, 1.2f);
    public Material screenMaterial;          
    public Material frameMaterial;           
    public int textureHeight = 512;
    public Color backgroundColor = new Color(0.12f, 0.14f, 0.18f);

    [Header("Camara")]
    public Vector3 studioPosition = new Vector3(0f, -20f, 0f);
    public float cameraFov = 30f;
    public float cameraYaw = 20f;
    public float cameraPitch = 8f;
    public float framingMargin = 1.25f;
    public float studioLightIntensity = 1.5f;

    [Header("Rotar patatilla")]
    public float rotationSpeed = 30f;

    [Header("Pruebas i debuging")]
    public bool randomizeWithKey = true;     // esto con la tecla T

    public int CurrentEye { get; private set; } = -1;
    public int CurrentNose { get; private set; } = -1;
    public int CurrentMouth { get; private set; } = -1;
    public int CurrentEar { get; private set; } = -1;

    private GameObject studioRoot;
    private Transform pivot;
    private Transform potatoVisual;
    private Transform partsRoot;
    private Bounds potatoBounds;
    private Camera studioCamera;
    private RenderTexture renderTexture;
    private Coroutine renderRoutine;

    void Awake()
    {
        Instance = this;
        BuildStudio();
        if (screenRenderer == null) CreateScreenGeometry(screenMaterial, frameMaterial);
        AssignTextureToScreen();
    }

    void Start()
    {
        if (randomizeOnStart) Randomize();
        else SetParts(0, 0, 0, 0);
    }

    void Update()
    {
        if (pivot != null && rotationSpeed != 0f)
        {
            pivot.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);
        }

#if ENABLE_INPUT_SYSTEM
        if (randomizeWithKey && Keyboard.current != null && Keyboard.current.tKey.wasPressedThisFrame)
        {
            Randomize();
        }
#endif
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (studioCamera != null) studioCamera.targetTexture = null;
        if (renderTexture != null) { renderTexture.Release(); Destroy(renderTexture); }
        if (studioRoot != null) Destroy(studioRoot);
    }


    // Cambia todas las piezas al azar (evita repetir exactamente la combinación actual).
    [ContextMenu("Randomize")]
    public void Randomize()
    {
        int combos = Count(eyePrefabs) * Count(nosePrefabs) * Count(mouthPrefabs) * Count(earPrefabs);
        int eye, nose, mouth, ear;
        int tries = 0;
        do
        {
            eye = RandomIndex(eyePrefabs);
            nose = RandomIndex(nosePrefabs);
            mouth = RandomIndex(mouthPrefabs);
            ear = RandomIndex(earPrefabs);
            tries++;
        }
        while (combos > 1 && tries < 20 &&
               eye == CurrentEye && nose == CurrentNose && mouth == CurrentMouth && ear == CurrentEar);

        SetParts(eye, nose, mouth, ear);
    }

    
    public void SetParts(int eye, int nose, int mouth, int ear)
    {
        if (partsRoot == null) return;

        for (int i = partsRoot.childCount - 1; i >= 0; i--)
        {
            GameObject old = partsRoot.GetChild(i).gameObject;
            old.SetActive(false);
            Destroy(old);
        }

        CurrentEye = eye; CurrentNose = nose; CurrentMouth = mouth; CurrentEar = ear;
        Bounds b = potatoBounds;

        PlaceFront(Get(eyePrefabs, eye), eyesHeight);
        PlaceFront(Get(nosePrefabs, nose), noseHeight);
        PlaceFront(Get(mouthPrefabs, mouth), mouthHeight);

        GameObject earPrefab = Get(earPrefabs, ear);
        if (earPrefab != null)
        {
            float y = b.min.y + earsHeight * b.size.y;
            PlacePiece(earPrefab, new Vector3(b.max.x, y, b.center.z), Vector3.right, false, earsForwardAngle);
            if (twoEars)
                PlacePiece(earPrefab, new Vector3(b.min.x, y, b.center.z), Vector3.left, mirrorLeftEar, -earsForwardAngle);
        }

        RequestRender();
    }

    // montaje de la potato

    void PlaceFront(GameObject prefab, float heightFraction)
    {
        if (prefab == null) return;
        Bounds b = potatoBounds;
        Vector3 point = new Vector3(b.center.x, b.min.y + heightFraction * b.size.y, b.min.z);
        PlacePiece(prefab, point, Vector3.back, false);
    }


    void PlacePiece(GameObject prefab, Vector3 surfacePoint, Vector3 outwardNormal, bool mirror, float extraYaw = 0f)
    {
        GameObject go = CreateVisual(prefab, partsRoot, prefab.name);
        if (go == null) return;

        Bounds pb = go.GetComponent<MeshFilter>().sharedMesh.bounds;
        Quaternion rot = Quaternion.Euler(0f, extraYaw, 0f) * Quaternion.LookRotation(-outwardNormal, Vector3.up);
        Vector3 scale = mirror ? new Vector3(-1f, 1f, 1f) : Vector3.one;
        Vector3 backCenter = new Vector3(pb.center.x, pb.center.y, pb.max.z);

        go.transform.localRotation = rot;
        go.transform.localScale = scale;
        Vector3 pos = surfacePoint + outwardNormal * surfaceGap
                      - rot * Vector3.Scale(scale, backCenter);

        
        float innermost = float.MaxValue;
        for (int i = 0; i < 8; i++)
        {
            Vector3 corner = new Vector3(
                (i & 1) == 0 ? pb.min.x : pb.max.x,
                (i & 2) == 0 ? pb.min.y : pb.max.y,
                (i & 4) == 0 ? pb.min.z : pb.max.z);
            Vector3 c = pos + rot * Vector3.Scale(scale, corner);
            innermost = Mathf.Min(innermost, Vector3.Dot(c - surfacePoint, outwardNormal));
        }
        pos += outwardNormal * (surfaceGap - innermost);

        go.transform.localPosition = pos;
    }

    // copia solo la malla y los materiales de un prefab sin física ni scripts
    GameObject CreateVisual(GameObject prefab, Transform parent, string name)
    {
        if (prefab == null) return null;
        MeshFilter mf = prefab.GetComponentInChildren<MeshFilter>(true);
        MeshRenderer mr = prefab.GetComponentInChildren<MeshRenderer>(true);
        if (mf == null || mr == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("ExamplePotatoDisplay: el prefab " + prefab.name + " no tiene malla.", this);
            return null;
        }

        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<MeshFilter>().sharedMesh = mf.sharedMesh;
        MeshRenderer r = go.AddComponent<MeshRenderer>();
        r.sharedMaterials = mr.sharedMaterials;
        // el suelo de la sala queda encima del estudio: sin esto le hace sombrita
        r.receiveShadows = false;
        r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        return go;
    }

    // estudio

    void BuildStudio()
    {
        studioRoot = new GameObject("ExamplePotato_Studio");
        studioRoot.transform.position = studioPosition;

        pivot = new GameObject("Pivot").transform;
        pivot.SetParent(studioRoot.transform, false);

        GameObject potato = CreateVisual(potatoPrefab, pivot, "Potato");
        if (potato == null)
        {
            Debug.LogError("ExamplePotatoDisplay: falta asignar el prefab Potato.", this);
            potatoVisual = new GameObject("Potato").transform;
            potatoVisual.SetParent(pivot, false);
            potatoBounds = new Bounds(Vector3.zero, Vector3.one);
        }
        else
        {
            potatoVisual = potato.transform;
            potatoBounds = potato.GetComponent<MeshFilter>().sharedMesh.bounds;
        }
        // centrar la patata en el pivote para que gire sobre sí misma
        potatoVisual.localPosition = -potatoBounds.center;

        partsRoot = new GameObject("Parts").transform;
        partsRoot.SetParent(potatoVisual, false);

        // textura
        float aspect = Mathf.Max(0.1f, screenSize.x / screenSize.y);
        int h = Mathf.Max(64, textureHeight);
        renderTexture = new RenderTexture(Mathf.RoundToInt(h * aspect), h, 24);
        renderTexture.antiAliasing = 2;
        renderTexture.name = "ExamplePotatoRT";
        renderTexture.Create();

        
        float halfH = 0.5f * Mathf.Max(potatoBounds.size.y, (potatoBounds.size.x + 0.4f) / aspect) * framingMargin;
        float dist = halfH / Mathf.Tan(cameraFov * 0.5f * Mathf.Deg2Rad) + potatoBounds.extents.z;
        Quaternion view = Quaternion.Euler(cameraPitch, cameraYaw, 0f);

        GameObject camGo = new GameObject("StudioCamera");
        camGo.transform.SetParent(studioRoot.transform, false);
        camGo.transform.position = pivot.position + view * new Vector3(0f, 0f, -dist);
        camGo.transform.LookAt(pivot.position);

        studioCamera = camGo.AddComponent<Camera>();
        studioCamera.stereoTargetEye = StereoTargetEyeMask.None;   // no es una cámara VR
        studioCamera.targetTexture = renderTexture;
        studioCamera.clearFlags = CameraClearFlags.SolidColor;
        studioCamera.backgroundColor = backgroundColor;
        studioCamera.fieldOfView = cameraFov;
        studioCamera.nearClipPlane = 0.05f;
        studioCamera.farClipPlane = dist * 4f;
        studioCamera.allowHDR = false;
        studioCamera.depth = -10;
        studioCamera.enabled = rotationSpeed != 0f;  

        // luz de relleno del estudio 
        GameObject lightGo = new GameObject("StudioLight");
        lightGo.transform.SetParent(studioRoot.transform, false);
        lightGo.transform.position = camGo.transform.position + Vector3.up * 1.5f;
        Light l = lightGo.AddComponent<Light>();
        l.type = LightType.Point;
        l.range = dist * 3f;
        l.intensity = studioLightIntensity;
        l.shadows = LightShadows.None;
    }

    void RequestRender()
    {
        if (studioCamera == null || rotationSpeed != 0f) return;   // si gira ya renderiza siempre
        if (!isActiveAndEnabled) return;
        if (renderRoutine != null) StopCoroutine(renderRoutine);
        renderRoutine = StartCoroutine(RenderOnce());
    }

    IEnumerator RenderOnce()
    {
        studioCamera.enabled = true;
        yield return null;            
        yield return null;
        studioCamera.enabled = false;
        renderRoutine = null;
    }

    //pantalla

    
    public void CreateScreenGeometry(Material screenMat, Material frameMat)
    {
        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
        screen.name = "Screen";
        screen.transform.SetParent(transform, false);
        screen.transform.localScale = new Vector3(screenSize.x, screenSize.y, 1f);
        RemoveCollider(screen);

        GameObject frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.name = "Frame";
        frame.transform.SetParent(transform, false);
        frame.transform.localPosition = new Vector3(0f, 0f, 0.03f);
        frame.transform.localScale = new Vector3(screenSize.x + 0.1f, screenSize.y + 0.1f, 0.04f);
        RemoveCollider(frame);

        screenRenderer = screen.GetComponent<Renderer>();
        if (screenMat != null) screenRenderer.sharedMaterial = screenMat;
        if (frameMat != null) frame.GetComponent<Renderer>().sharedMaterial = frameMat;
        else if (Application.isPlaying) frame.GetComponent<Renderer>().material.color = new Color(0.08f, 0.08f, 0.09f);
    }

    void AssignTextureToScreen()
    {
        if (screenRenderer == null || renderTexture == null) return;

        Material mat;
        if (screenMaterial != null)
        {
            mat = new Material(screenMaterial);   
        }
        else
        {    
            Shader s = Shader.Find("Universal Render Pipeline/Unlit");
            if (s == null) s = Shader.Find("Unlit/Texture");
            mat = new Material(s);
        }
        screenRenderer.material = mat;

        mat.mainTexture = renderTexture;
        if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", renderTexture);
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
    }

    static void RemoveCollider(GameObject go)
    {
        Collider c = go.GetComponent<Collider>();
        if (c == null) return;
        if (Application.isPlaying) Destroy(c);
        else DestroyImmediate(c);
    }

   

    static int Count(GameObject[] arr)
    {
        return (arr == null || arr.Length == 0) ? 1 : arr.Length;
    }

    static int RandomIndex(GameObject[] arr)
    {
        return (arr == null || arr.Length == 0) ? -1 : Random.Range(0, arr.Length);
    }

    static GameObject Get(GameObject[] arr, int index)
    {
        if (arr == null || index < 0 || index >= arr.Length) return null;
        return arr[index];
    }
    public struct PotatoParts
    {
        public string eye, nose, mouth, ear;
        public bool twoEars;

        public override string ToString() =>
            $"Ojos: {eye ?? "-"}, Nariz: {nose ?? "-"}, Boca: {mouth ?? "-"}, Orejas: {ear ?? "-"}{(ear != null && twoEars ? " (x2)" : "")}";
    }

    // Devuelve los nombres de los prefabs colocados 
    public PotatoParts GetCurrentParts()
    {
        return new PotatoParts
        {
            eye = NameOf(eyePrefabs, CurrentEye),
            nose = NameOf(nosePrefabs, CurrentNose),
            mouth = NameOf(mouthPrefabs, CurrentMouth),
            ear = NameOf(earPrefabs, CurrentEar),
            twoEars = twoEars
        };
    }

    static string NameOf(GameObject[] arr, int index)
    {
        GameObject p = Get(arr, index);
        return p != null ? p.name : null;
    }
}


