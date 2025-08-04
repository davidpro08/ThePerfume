using UnityEngine;

public class ColletableItem : MonoBehaviour
{
    [Header("아이템 정보")]
    public ItemData cropData;
    public int quantity = 1;

    [Header("끌려오기 설정")]
    public float attractionRange = 1.0f; // 플레이어와 근접한 정도 (끌려오기 시작하는 거리)
    public float pickupDistance = 0.5f; // 플레이어가 획득하는 거리
    public float speed = 3f; // 끌려오는 속도
    private Transform playerTransform; // 플레이어 위치
    private bool isAttracted = false; // 끌려오는 중인지 판단

    void Update()
    {
        // 플레이어 감지 시작 및 끌어당기기 시작
        if (!isAttracted && playerTransform == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            // 플레이어 못 찾음 / 프레임 넘어가기
            return;
        }
        if (playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            // 끌려오기 시작
            if (!isAttracted && distanceToPlayer <= attractionRange)
            {
                isAttracted = true;
                Debug.Log($"{cropData.itemName} 끌려오기 시작");
            }

            // 끌려오는 방향 조정
            if (isAttracted)
            {
                Vector3 direction = (playerTransform.position - transform.position).normalized;
                transform.position += direction * speed * Time.deltaTime;

                // 아이템 획득 조건
                if (distanceToPlayer <= pickupDistance)
                {
                    TryPickup();
                }
            }
        }
    }

    // 아이템 획득 시도
    private void TryPickup()
    {
        InventoryManager inventoryManager = FindFirstObjectByType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.Log("인벤토리 매니저 없음");
            return;
        }
        if (inventoryManager.AddItem(cropData, quantity))
        {
            Debug.Log($"{cropData.itemName} {quantity}개 획득");
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"{cropData.itemName} 획득 못함");
            isAttracted = false;
        }
    }

    // 초기화, 수확할 때 호출
    public void Initialize(CropData data, int count)
    {
        cropData = data;
        quantity = count;
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Debug.Log($"[{name} colleableItem] Initialize 시도");
        if (sr != null && cropData.itemIcon != null)
        {
            sr.sprite = cropData.itemIcon;
        }
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }
}
