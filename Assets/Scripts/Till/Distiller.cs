using UnityEngine;

public class Distiller : MonoBehaviour
{
    public string distillerID; // 붙어있는 인스턴스 구별을 위한 ID
    public DistillerState currentState; // 붙어있는 인스턴스 현재 상태
    public Collider2D interactCollider; // 상호작용할 콜라이더

    void Awake()
    {
        if (string.IsNullOrEmpty(distillerID))
        {
            distillerID = System.Guid.NewGuid().ToString(); // 고유 ID 생성
        }

        interactCollider = GetComponent<Collider2D>();
        if (interactCollider == null) enabled = false;
    }

    void Start()
    {
        TillDataManager.Instance.RegisterDistiller(this);
        LoadDistillerState();
    }

    public void SetDistillerData(DistillerState distillerState)
    {
        currentState = distillerState;
    }

    // 씬 이동 요청

    private void LoadDistillerState()
    {
        DistillerState loadedState = TillDataManager.Instance.GetDistillerState(distillerID);
        if (loadedState != null)
        {
            currentState = loadedState;
            Debug.Log($"[증류기 {distillerID}] 증류기 데이터 로드 완료");
        }
        else
        {
            currentState = new DistillerState();
            Debug.Log($"[증류기 {distillerID}] 증류기 초기 설정 완료");
        }
    }

    public void SaveDistillerState()
    {
        TillDataManager.Instance.UpdateDistillerState(distillerID, currentState);
    }
}
