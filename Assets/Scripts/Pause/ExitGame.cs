using UnityEngine;

public class ExitGame : MonoBehaviour
{
    public void Quit()
    {
        Application.Quit();
        Debug.Log("게임 종료 요청됨"); // 에디터에서는 종료되지 않으므로 디버깅용 출력
    }
}