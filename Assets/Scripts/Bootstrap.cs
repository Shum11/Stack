using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        var camGo = new GameObject("Camera");
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.44f, 0.68f, 0.87f, 1f);
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 150f;
        float aspect = Screen.width / (float)Screen.height;
        cam.orthographicSize = Mathf.Clamp(3.2f / aspect, 1.7f, 8f);
        camGo.AddComponent<AudioListener>();

        BuildSkyGradient(cam);

        var sfxGo = new GameObject("Sound");
        var sfx = sfxGo.AddComponent<SoundManager>();
        sfx.Build();

        var uiGo = new GameObject("UI");
        var ui = uiGo.AddComponent<UIManager>();
        ui.Build();

        var runnerGo = new GameObject("GameRunner");
        var runner = runnerGo.AddComponent<GameRunner>();
        runner.Setup(cam, ui, sfx);
    }

    static void BuildSkyGradient(Camera cam)
    {
        Color top = new Color(0.30f, 0.55f, 0.82f, 1f);
        Color mid = new Color(0.50f, 0.75f, 0.90f, 1f);
        Color bot = new Color(0.78f, 0.90f, 0.96f, 1f);

        var tex = new Texture2D(1, 256, TextureFormat.RGBA32, false, true);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color[256];
        for (int i = 0; i < 256; i++)
        {
            float t = i / 255f;
            px[i] = t < 0.5f ? Color.Lerp(bot, mid, t * 2f) : Color.Lerp(mid, top, (t - 0.5f) * 2f);
        }
        tex.SetPixels(px);
        tex.Apply(false, true);

        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "SkyGradient";
        go.transform.SetParent(cam.transform, false);
        go.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        go.transform.localPosition = new Vector3(0f, 0f, 45f);

        float visibleH = cam.orthographicSize * 2f;
        float visibleW = visibleH * Mathf.Max(1f, Screen.width / (float)Screen.height);
        go.transform.localScale = new Vector3(visibleW / 10f, visibleH / 10f, 1f);

        var coll = go.GetComponent<Collider>();
        if (coll != null) Destroy(coll);
        var mat = new Material(Shader.Find("Unlit/Texture"));
        mat.mainTexture = tex;
        go.GetComponent<Renderer>().material = mat;
    }
}