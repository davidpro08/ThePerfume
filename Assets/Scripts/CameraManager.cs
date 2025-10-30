using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour
{
    [Header("Follow Mode")]
    [SerializeField] float followSmoothTime = 0.15f;
    [SerializeField] float followZoom = 3.5f; // 줌
    [SerializeField] Vector2 followOffset = Vector2.zero; // 플레이어 기준 오프셋, 일단 혹시 모르니깐 뷰를 위해서 추가는 해놓음

    [Header("Overview Mode")]
    [SerializeField] Transform mapRoot;
    [SerializeField] float overviewPadding = 0.8f;

    [Header("Refs")]
    [SerializeField] Transform player;
    [SerializeField] string[] followScenes = { "lab", "NPC_house", "Village" };
    [SerializeField] string[] overviewScenes = { "bench", "distiller", "Mixture" };

    Bounds overviewBounds;
    Camera cam;
    Vector3 vel; // SmoothDamp 속도 벡터
    private static CameraManager instance;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        cam = GetComponent<Camera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void LateUpdate()
    {
        if (ShouldFollow()) DoFollow();
        else DoOverview();
    }

    // ===== 기능 =====
    void DoFollow()
    {
        Vector3 target = new Vector3(player.position.x + followOffset.x,
        player.position.y + followOffset.y,
        transform.position.z);

        transform.position = Vector3.SmoothDamp(transform.position, target, ref vel, followSmoothTime);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, followZoom, Time.deltaTime * 5f);
    }

    void DoOverview()
    {
        if (overviewBounds.size.sqrMagnitude > 0.0001f)
        {
            float size = OrthographicSizeToFitBounds(overviewBounds, cam.aspect) * overviewPadding;
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, size, Time.deltaTime * 5f);

            // 카메라 부드럽게
            Vector3 center = new Vector3(overviewBounds.center.x, overviewBounds.center.y, transform.position.z);
            transform.position = Vector3.SmoothDamp(transform.position, center, ref vel, followSmoothTime);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!player)
        {
#if UNITY_2023_1_OR_NEWER
            var p = FindAnyObjectByType<Player>();
#else
            var p = FindObjectOfType<Player>();
#endif
            if (p) player = p.transform;
        }
        if (!mapRoot)
        {
            var tagged = GameObject.FindWithTag("MapRoot");
            if (tagged) mapRoot = tagged.transform;
        }

        if (mapRoot && BoundsUtil.TryCalcBoundsFromRoot(mapRoot, out overviewBounds, true) == false)
        {
            overviewBounds = CalcBoundsFromAllSpriteRenderers();
        }
        else if (!mapRoot)
        {
            overviewBounds = CalcBoundsFromAllSpriteRenderers();
        }

        if (IsIn(scene.name, followScenes) && player != null)
        {
            Vector3 target = new Vector3(player.position.x + followOffset.x,
            player.position.y + followOffset.y,
            transform.position.z);
            transform.position = target;
            cam.orthographicSize = followZoom;
        }
        else SnapToOverview();
    }

    // ===== 보조 =====
    bool ShouldFollow()
    {
        if (player == null) return false;

        var sr = player.GetComponentInChildren<SpriteRenderer>();
        var col = player.GetComponent<Collider2D>();

        bool visiable = (sr == null || sr.enabled);
        bool collidable = (col == null || col.enabled);

        return visiable && collidable;
    }

    float OrthographicSizeToFitBounds(Bounds b, float aspect)
    {
        float sizeByHeight = b.extents.y;
        float sizeByWidth = b.extents.x / aspect;
        return Mathf.Max(sizeByHeight, sizeByWidth);
    }

    bool IsIn(string sceneName, string[] list)
    {
        for (int i = 0; i < list.Length; i++) if (sceneName == list[i]) return true;
        return false;
    }

    void SnapToOverview()
    {
        Vector3 center = new Vector3(overviewBounds.center.x, overviewBounds.center.y, transform.position.z);
        transform.position = center;

        float size = Mathf.Max(overviewBounds.extents.y, overviewBounds.extents.x / cam.aspect);
        cam.orthographicSize = size * overviewPadding;
    }

    Bounds CalcBoundsFromAllSpriteRenderers()
    {
        var srs = FindObjectsOfType<SpriteRenderer>(true);
        Bounds b = new Bounds();
        bool inited = false;
        foreach (var sr in srs)
        {
            if (!inited)
            {
                b = sr.bounds;
                inited = true;
            }
            else
            {
                b.Encapsulate(sr.bounds);
            }
        }
        return b;
    }
}
