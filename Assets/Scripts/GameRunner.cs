using System.Collections.Generic;
using UnityEngine;

public class GameRunner : MonoBehaviour
{
    public static GameRunner Instance;

    const float BlockSize = 1f;
    const float BaseSize = 3f;
    const float CameraDistance = 15f;
    const float CameraSmoothTime = 0.2f;
    const float MoveSpeedBase = 2.6f;

    enum GameState { Menu, Playing, Paused, GameOver }
    GameState state = GameState.Menu;

    Camera cam;
    UIManager ui;
    SoundManager sfx;
    Transform stackRoot;

    readonly List<PlacedBlock> stack = new List<PlacedBlock>();
    PlacedBlock top;

    GameObject mover;
    Vector3 moveDir;
    float moveT;
    float moveDuration;
    float levelY;
    bool axisX = true;
    int score;
    int streak;

    void Awake()
    {
        Instance = this;
    }

    public void Setup(Camera c, UIManager u, SoundManager s)
    {
        cam = c;
        ui = u;
        sfx = s;
        stackRoot = new GameObject("Stack").transform;
        ResetGame();
        ui.WirePlayButtons(StartGame, Resume, RestartFromPause, GoToMenu, RestartFromGameOver, GoToMenu, Pause);
        state = GameState.Menu;
        ui.ShowMenu();
    }

    void ResetGame()
    {
        camVel = Vector3.zero;
        foreach (Transform child in stackRoot) Destroy(child.gameObject);
        stack.Clear();
        score = 0;
        streak = 0;
        ui.HideGameOver();
        ui.SetScore(0);

        Vector3 basePos = new Vector3(0f, BaseSize * 0.5f, 0f);
        GameObject b = MakeCube(basePos, new Vector3(BaseSize, BaseSize, BaseSize), new Color(0.30f, 0.32f, 0.42f));
        top = new PlacedBlock(b, new Vector2(BaseSize * 0.5f, BaseSize * 0.5f), basePos);
        stack.Add(top);
        levelY = BaseSize;
        SpawnMover();
    }

    public void StartGame()
    {
        ui.HideMenu();
        ui.ShowPauseButton();
        state = GameState.Playing;
        sfx.SetMusic(true);
    }

    public void Pause()
    {
        if (state != GameState.Playing) return;
        state = GameState.Paused;
        ui.HidePauseButton();
        ui.ShowPause();
        sfx.SetMusic(false);
    }

    public void Resume()
    {
        if (state != GameState.Paused) return;
        state = GameState.Playing;
        ui.HidePause();
        ui.ShowPauseButton();
        sfx.SetMusic(true);
    }

    public void RestartFromPause()
    {
        if (state != GameState.Paused) return;
        ui.HidePause();
        ResetGame();
        state = GameState.Playing;
        ui.ShowPauseButton();
        sfx.SetMusic(true);
    }

    public void RestartFromGameOver()
    {
        if (state != GameState.GameOver) return;
        ResetGame();
        state = GameState.Playing;
        ui.ShowPauseButton();
        sfx.SetMusic(true);
    }

    public void GoToMenu()
    {
        ui.HidePause();
        ui.HideGameOver();
        ui.HidePauseButton();
        ResetGame();
        state = GameState.Menu;
        ui.ShowMenu();
        sfx.SetMusic(true);
    }

    void SpawnMover()
    {
        if (mover != null) Destroy(mover);
        axisX = !axisX;
        float ext = axisX ? top.half.x : top.half.y;
        Vector2 size = new Vector2(top.half.x * 2f, top.half.y * 2f);
        moveDir = axisX ? Vector3.right : Vector3.forward;

        mover = MakeCube(Vector3.zero, new Vector3(size.x, BlockSize, size.y), ColorFor(score));
        mover.name = "Mover";
        moveT = 0f;

        float travel = 4f * ext;
        moveDuration = travel / MoveSpeedBase;
        moveDuration *= Mathf.Max(0.5f, Mathf.Pow(0.982f, score));
        LevelPosition();
    }

    void LevelPosition()
    {
        if (mover == null) return;
        float ext = axisX ? top.half.x : top.half.y;
        Vector3 c = top.pos + moveDir * Mathf.Lerp(-2f * ext, 2f * ext, moveT);
        mover.transform.position = new Vector3(c.x, levelY + BlockSize * 0.5f, c.z);
    }

    void Update()
    {
        if (state != GameState.Playing) return;
        if (mover == null) return;

        if (Input.GetMouseButtonDown(0) && !IsTapOnUI())
        {
            Drop();
            return;
        }
        moveT += Time.deltaTime / moveDuration;
        if (moveT >= 1f)
        {
            moveT = 1f;
            Drop();
            return;
        }
        LevelPosition();
    }

    bool IsTapOnUI()
    {
        var es = UnityEngine.EventSystems.EventSystem.current;
        return es != null && es.IsPointerOverGameObject();
    }

    void Drop()
    {
        if (mover == null || state != GameState.Playing) return;

        float mcX = mover.transform.position.x;
        float mcZ = mover.transform.position.z;
        float tx = top.pos.x;
        float tz = top.pos.z;
        float thx = top.half.x;
        float thz = top.half.y;

        float loX = Mathf.Max(mcX - thx, tx - thx);
        float hiX = Mathf.Min(mcX + thx, tx + thx);
        float loZ = Mathf.Max(mcZ - thz, tz - thz);
        float hiZ = Mathf.Min(mcZ + thz, tz + thz);

        if (hiX - loX <= 0.001f || hiZ - loZ <= 0.001f)
        {
            Fail();
            return;
        }

        bool perfect = Mathf.Abs((hiX - loX) - thx * 2f) < 0.001f && Mathf.Abs((hiZ - loZ) - thz * 2f) < 0.001f;

        SpawnLeftovers(mcX, mcZ, thx, thz, loX, hiX, loZ, hiZ, ColorFor(score + 1));

        Vector3 nCenter = new Vector3((loX + hiX) * 0.5f, levelY + BlockSize * 0.5f, (loZ + hiZ) * 0.5f);
        GameObject nb = MakeCube(nCenter, new Vector3(hiX - loX, BlockSize, hiZ - loZ), ColorFor(score));
        Destroy(mover);
        mover = null;

        top = new PlacedBlock(nb, new Vector2((hiX - loX) * 0.5f, (hiZ - loZ) * 0.5f), nCenter);
        stack.Add(top);
        levelY += BlockSize;

        score += perfect ? 2 : 1;
        streak = perfect ? streak + 1 : 0;
        if (perfect)
        {
            sfx.PlayPerfect();
            ui.ShowCombo(streak);
        }
        else
        {
            sfx.PlayPlace();
        }
        ui.SetScore(score);
        SpawnMover();
    }

    void SpawnLeftovers(float mcX, float mcZ, float thx, float thz, float loX, float hiX, float loZ, float hiZ, Color col)
    {
        float left = loX - (mcX - thx);
        if (left > 0.001f)
            FallPiece(new Vector3(mcX - thx + left * 0.5f, levelY + BlockSize * 0.5f, mcZ), new Vector3(left, BlockSize, thz * 2f), col);
        float right = (mcX + thx) - hiX;
        if (right > 0.001f)
            FallPiece(new Vector3(hiX + right * 0.5f, levelY + BlockSize * 0.5f, mcZ), new Vector3(right, BlockSize, thz * 2f), col);
        float back = loZ - (mcZ - thz);
        if (back > 0.001f)
            FallPiece(new Vector3(mcX, levelY + BlockSize * 0.5f, mcZ - thz + back * 0.5f), new Vector3(thx * 2f, BlockSize, back), col);
        float front = (mcZ + thz) - hiZ;
        if (front > 0.001f)
            FallPiece(new Vector3(mcX, levelY + BlockSize * 0.5f, hiZ + front * 0.5f), new Vector3(thx * 2f, BlockSize, front), col);
    }

    void FallPiece(Vector3 pos, Vector3 size, Color col)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = "Falling";
        g.transform.position = pos;
        g.transform.localScale = size;
        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = col;
        g.GetComponent<Renderer>().material = mat;
        var fb = g.AddComponent<FallingBlock>();
        fb.velocity = new Vector3(Random.Range(-0.6f, 0.6f), -6.5f, Random.Range(-0.6f, 0.6f));
        fb.spin = Random.insideUnitSphere * 200f;
    }

    void Fail()
    {
        state = GameState.GameOver;
        streak = 0;
        if (mover != null)
        {
            FallPiece(mover.transform.position, mover.transform.localScale, ColorFor(score));
            Destroy(mover);
            mover = null;
        }

        int best = PlayerPrefs.GetInt("StackBest", 0);
        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt("StackBest", best);
            PlayerPrefs.Save();
        }
        sfx.PlayOver();
        sfx.SetMusic(false);
        ui.HidePauseButton();
        ui.ShowGameOver(score, best);
        ui.SetScore(score);
    }

    GameObject MakeCube(Vector3 pos, Vector3 scl, Color c)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.transform.SetParent(stackRoot, false);
        g.transform.position = pos;
        g.transform.localScale = scl;
        var coll = g.GetComponent<Collider>();
        if (coll != null) Destroy(coll);
        var mat = new Material(Shader.Find("Unlit/Color"));
        mat.color = c;
        g.GetComponent<Renderer>().material = mat;
        return g;
    }

    static Color ColorFor(int lvl)
    {
        float h = Mathf.Repeat(lvl * 0.11f, 1f);
        return Color.HSVToRGB(h, 0.62f, 0.96f);
    }

    Vector3 camVel;

    void LateUpdate()
    {
        if (cam == null) return;
        Vector3 target = new Vector3(0f, levelY + 1.2f, 0f);
        Vector3 dir = new Vector3(-0.55f, 1.05f, -0.55f).normalized;
        Vector3 wantPos = target + dir * CameraDistance;
        cam.transform.position = Vector3.SmoothDamp(cam.transform.position, wantPos, ref camVel, CameraSmoothTime);
        float rotK = 1f - Mathf.Exp(-14f * Time.deltaTime);
        cam.transform.rotation = Quaternion.Slerp(cam.transform.rotation, Quaternion.LookRotation(target - cam.transform.position, Vector3.up), rotK);
    }
}

public class PlacedBlock
{
    public GameObject go;
    public Vector2 half;
    public Vector3 pos;

    public PlacedBlock(GameObject g, Vector2 h, Vector3 p)
    {
        go = g;
        half = h;
        pos = p;
    }
}

public class FallingBlock : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 spin;
    float life = 1.6f;

    void Start()
    {
        var coll = GetComponent<Collider>();
        if (coll != null) Destroy(coll);
    }

    void Update()
    {
        velocity.y -= 9.8f * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
        transform.Rotate(spin * Time.deltaTime, Space.Self);
        life -= Time.deltaTime;
        if (life <= 0f) Destroy(gameObject);
    }
}
