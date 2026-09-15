using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text scoreText;
    public Text bestText;
    public Text comboText;
    public CanvasGroup comboGroup;
    public GameObject gameOverPanel;
    public Text finalText;
    public Text bestFinalText;
    public Text bestTitleText;

    Canvas canvas;
    Transform canvasTransform;
    SoundManager sfx;

    GameObject startMenuPanel;
    Button playButton;
    Text menuBestText;

    Button menuMusicButton;
    GameObject menuMusicOn;
    GameObject menuMusicOff;
    Button fieldMusicButton;
    GameObject fieldMusicOn;
    GameObject fieldMusicOff;

    GameObject pauseButton;
    GameObject pausePanel;
    Button resumeButton;
    Button restartFromPauseButton;
    Button menuFromPauseButton;

    Button restartFromGameOverButton;
    Button menuFromGameOverButton;

    public void Build(SoundManager sound)
    {
        sfx = sound;
        var canvasGo = new GameObject("UICanvas");
        canvasGo.transform.SetParent(transform, false);
        canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(440f, 800f);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();
        canvasTransform = canvasGo.transform;

        var esGo = new GameObject("EventSystem");
        esGo.transform.SetParent(transform, false);
        esGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        scoreText = MakeText("Score", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f), 54, Color.white, TextAnchor.MiddleCenter);
        bestText = MakeText("Best", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -210f), 22, new Color(1f, 1f, 1f, 0.75f), TextAnchor.MiddleCenter);
        scoreText.raycastTarget = false;
        bestText.raycastTarget = false;
        scoreText.gameObject.SetActive(false);
        bestText.gameObject.SetActive(false);

        comboText = MakeText("Combo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, 40, new Color(1f, 1f, 0.35f, 1f), TextAnchor.MiddleCenter);
        comboText.raycastTarget = false;
        comboText.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.6f);
        comboText.gameObject.SetActive(false);

        BuildStartMenu();
        BuildPauseButton();
        BuildFieldMusicButton();
        BuildPausePanel();
        BuildGameOverPanel();
        RefreshMusicIcon();
    }

    void BuildStartMenu()
    {
        startMenuPanel = new GameObject("StartMenu");
        startMenuPanel.transform.SetParent(canvasTransform, false);
        var bg = startMenuPanel.AddComponent<Image>();
        bg.color = new Color(0.06f, 0.10f, 0.16f, 0.72f);
        Stretch(bg.rectTransform);

        var title = MakeTextIn(bg.transform, "Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), 80, Color.white, TextAnchor.MiddleCenter);
        title.text = "Башня";
        title.gameObject.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.7f);
        MakeTextIn(bg.transform, "Subtitle", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 60f), 18, new Color(1f, 1f, 1f, 0.5f), TextAnchor.MiddleCenter).text = "Тапай, чтобы ставить";

        playButton = MakeButton(bg.transform, "PlayBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -30f), new Vector2(200f, 55f), new Color(0.18f, 0.72f, 0.42f), "ИГРАТЬ", 30, Color.white);

        menuBestText = MakeTextIn(bg.transform, "MenuBest", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -110f), 22, new Color(1f, 1f, 1f, 0.65f), TextAnchor.MiddleCenter);

        menuMusicButton = MakeMusicIconButton(bg.transform, "MusicBtn", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -56f), out menuMusicOn, out menuMusicOff);
        menuMusicButton.onClick.AddListener(ToggleMusic);
    }

    void BuildPauseButton()
    {
        pauseButton = new GameObject("PauseBtn");
        pauseButton.transform.SetParent(canvasTransform, false);
        var img = pauseButton.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.18f);
        var rt = pauseButton.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(48f, 48f);
        rt.anchoredPosition = new Vector2(44f, -50f);
        var btn = pauseButton.AddComponent<Button>();
        btn.targetGraphic = img;
        var txtGo = new GameObject("Icon");
        txtGo.transform.SetParent(pauseButton.transform, false);
        var txt = txtGo.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 22;
        txt.color = new Color(1f, 1f, 1f, 0.85f);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = "||";
        txt.raycastTarget = false;
        var txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        pauseButton.SetActive(false);
    }

    void BuildFieldMusicButton()
    {
        fieldMusicButton = MakeMusicIconButton(canvasTransform, "MusicFieldBtn", new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-36f, -50f), out fieldMusicOn, out fieldMusicOff);
        fieldMusicButton.onClick.AddListener(ToggleMusic);
        fieldMusicButton.gameObject.SetActive(false);
    }

    void BuildPausePanel()
    {
        pausePanel = new GameObject("PausePanel");
        pausePanel.transform.SetParent(canvasTransform, false);
        var bg = pausePanel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(bg.rectTransform);
        pausePanel.SetActive(false);

        MakeTextIn(bg.transform, "Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 130f), 52, Color.white, TextAnchor.MiddleCenter).text = "ПАУЗА";

        resumeButton = MakeButton(bg.transform, "ResumeBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 10f), new Vector2(200f, 48f), new Color(0.2f, 0.55f, 0.75f), "ПРОДОЛЖИТЬ", 24, Color.white);
        restartFromPauseButton = MakeButton(bg.transform, "RestartBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -55f), new Vector2(200f, 48f), new Color(0.65f, 0.35f, 0.2f), "ЗАНОВО", 24, Color.white);
        menuFromPauseButton = MakeButton(bg.transform, "MenuBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(200f, 48f), new Color(0.35f, 0.35f, 0.38f), "МЕНЮ", 24, Color.white);
    }

    void BuildGameOverPanel()
    {
        gameOverPanel = new GameObject("GameOver");
        gameOverPanel.transform.SetParent(canvasTransform, false);
        var panelImg = gameOverPanel.AddComponent<Image>();
        panelImg.color = new Color(0f, 0f, 0f, 0.72f);
        Stretch(panelImg.rectTransform);
        panelImg.raycastTarget = false;
        gameOverPanel.SetActive(false);

        bestTitleText = MakeTextIn(panelImg.transform, "Title", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), 44, Color.white, TextAnchor.MiddleCenter);
        finalText = MakeTextIn(panelImg.transform, "Final", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 30f), 90, Color.white, TextAnchor.MiddleCenter);
        bestFinalText = MakeTextIn(panelImg.transform, "BestFinal", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -50f), 30, new Color(1f, 1f, 1f, 0.8f), TextAnchor.MiddleCenter);

        restartFromGameOverButton = MakeButton(panelImg.transform, "RestartBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f), new Vector2(200f, 48f), new Color(0.18f, 0.72f, 0.42f), "ЗАНОВО", 24, Color.white);
        menuFromGameOverButton = MakeButton(panelImg.transform, "MenuBtn", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -185f), new Vector2(200f, 48f), new Color(0.35f, 0.35f, 0.38f), "МЕНЮ", 24, Color.white);
    }

    public void ShowMenu()
    {
        int best = PlayerPrefs.GetInt("StackBest", 0);
        menuBestText.text = best > 0 ? "РЕКОРД  " + best : "";
        startMenuPanel.SetActive(true);
        scoreText.gameObject.SetActive(false);
        bestText.gameObject.SetActive(false);
    }

    public void HideMenu()
    {
        startMenuPanel.SetActive(false);
        scoreText.gameObject.SetActive(true);
        bestText.gameObject.SetActive(true);
    }

    public void ShowPauseButton()
    {
        pauseButton.SetActive(true);
        if (fieldMusicButton != null) fieldMusicButton.gameObject.SetActive(true);
    }

    public void HidePauseButton()
    {
        pauseButton.SetActive(false);
        if (fieldMusicButton != null) fieldMusicButton.gameObject.SetActive(false);
    }

    public void ShowPause()
    {
        pausePanel.SetActive(true);
        pauseButton.SetActive(false);
        if (fieldMusicButton != null) fieldMusicButton.gameObject.SetActive(false);
    }

    public void HidePause()
    {
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);
        if (fieldMusicButton != null) fieldMusicButton.gameObject.SetActive(true);
    }

    public void WirePlayButtons(UnityEngine.Events.UnityAction onPlay, UnityEngine.Events.UnityAction onResume, UnityEngine.Events.UnityAction onRestartPause, UnityEngine.Events.UnityAction onMenuPause, UnityEngine.Events.UnityAction onRestartOver, UnityEngine.Events.UnityAction onMenuOver, UnityEngine.Events.UnityAction onPause)
    {
        playButton.onClick.AddListener(onPlay);
        resumeButton.onClick.AddListener(onResume);
        restartFromPauseButton.onClick.AddListener(onRestartPause);
        menuFromPauseButton.onClick.AddListener(onMenuPause);
        restartFromGameOverButton.onClick.AddListener(onRestartOver);
        menuFromGameOverButton.onClick.AddListener(onMenuOver);
        pauseButton.GetComponent<Button>().onClick.AddListener(onPause);
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
        bestText.text = "РЕКОРД " + PlayerPrefs.GetInt("StackBest", 0);
    }

    public void ShowCombo(int streak)
    {
        StopAllCoroutines();
        comboText.gameObject.SetActive(true);
        comboText.text = "ИДЕАЛЬНО x" + streak;
        comboGroup = comboText.GetComponent<CanvasGroup>();
        if (comboGroup == null) comboGroup = comboText.gameObject.AddComponent<CanvasGroup>();
        comboGroup.alpha = 1f;
        StartCoroutine(ComboAnim());
    }

    IEnumerator ComboAnim()
    {
        float t = 0f;
        Vector3 baseScale = Vector3.one * 0.5f;
        while (t < 0.8f)
        {
            t += Time.deltaTime;
            float k = t / 0.8f;
            comboText.transform.localScale = Vector3.Lerp(baseScale * 1.6f, baseScale, Mathf.Clamp01(k * 4f));
            comboGroup.alpha = 1f - Mathf.Clamp01((k - 0.45f) / 0.55f);
            yield return null;
        }
        comboText.gameObject.SetActive(false);
    }

    public void ShowGameOver(int score, int best)
    {
        bestTitleText.text = "ИГРА\nОКОНЧЕНА";
        finalText.text = score.ToString();
        bestFinalText.text = "РЕКОРД " + best;
        gameOverPanel.SetActive(true);
    }

    public void HideGameOver()
    {
        gameOverPanel.SetActive(false);
    }

    Text MakeText(string name, Vector2 aMin, Vector2 aMax, Vector2 pos, int size, Color color, TextAnchor align)
    {
        return MakeTextIn(canvasTransform, name, aMin, aMax, pos, size, color, align);
    }

    Text MakeTextIn(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, int size, Color color, TextAnchor align)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = size;
        t.color = color;
        t.alignment = align;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        var rt = t.rectTransform;
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(0f, 0f);
        return t;
    }

    Button MakeButton(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, Vector2 size, Color bgColor, string label, int fontSize, Color textColor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = bgColor;
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txt = txtGo.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.color = textColor;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        var txtRt = txtGo.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.sizeDelta = Vector2.zero;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        txt.text = label;
        return btn;
    }

    void ToggleMusic()
    {
        if (sfx == null) return;
        sfx.SetMusicOn(!sfx.IsMusicOn);
        RefreshMusicIcon();
    }

    void RefreshMusicIcon()
    {
        bool on = sfx != null && sfx.IsMusicOn;
        if (menuMusicOn != null) menuMusicOn.SetActive(on);
        if (menuMusicOff != null) menuMusicOff.SetActive(!on);
        if (fieldMusicOn != null) fieldMusicOn.SetActive(on);
        if (fieldMusicOff != null) fieldMusicOff.SetActive(!on);
    }

    Button MakeMusicIconButton(Transform parent, string name, Vector2 aMin, Vector2 aMax, Vector2 pos, out GameObject onIcon, out GameObject offIcon)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.27f, 0.3f, 0.34f, 0.85f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = aMin;
        rt.anchorMax = aMax;
        rt.sizeDelta = new Vector2(44f, 44f);
        rt.anchoredPosition = pos;

        onIcon = new GameObject("On");
        onIcon.transform.SetParent(go.transform, false);
        var onImg = onIcon.AddComponent<Image>();
        onImg.color = new Color(0f, 0f, 0f, 0f);
        onImg.raycastTarget = false;
        Stretch(onIcon.GetComponent<RectTransform>());
        BuildNoteIcon(onIcon.transform);

        offIcon = new GameObject("Off");
        offIcon.transform.SetParent(go.transform, false);
        var offImg = offIcon.AddComponent<Image>();
        offImg.color = new Color(0f, 0f, 0f, 0f);
        offImg.raycastTarget = false;
        Stretch(offIcon.GetComponent<RectTransform>());
        BuildNoteIcon(offIcon.transform);
        AddBar(offIcon.transform, Vector2.zero, new Vector2(4f, 40f), 45f, new Color(1f, 0.35f, 0.35f, 1f));
        return btn;
    }

    void BuildNoteIcon(Transform parent)
    {
        var white = new Color(1f, 1f, 1f, 0.92f);
        AddBar(parent, new Vector2(-3f, -7f), new Vector2(10f, 6f), 25f, white);
        AddBar(parent, new Vector2(9f, -7f), new Vector2(10f, 6f), 25f, white);
        AddBar(parent, new Vector2(1f, 1f), new Vector2(3.5f, 20f), 0f, white);
        AddBar(parent, new Vector2(13f, 1f), new Vector2(3.5f, 20f), 0f, white);
        AddBar(parent, new Vector2(7f, 10f), new Vector2(15f, 5f), 0f, white);
    }

    static GameObject AddBar(Transform parent, Vector2 pos, Vector2 size, float rot, Color c)
    {
        var go = new GameObject("Bar");
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = c;
        img.raycastTarget = false;
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = pos;
        if (rot != 0f) rt.localRotation = Quaternion.Euler(0f, 0f, rot);
        return go;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
