
public interface IInteract
{
    /// <summary>
    /// 상호작용 로직
    /// </summary>
    /// <param name="player">플레이어 정보 넘겨줌</param>
    void Interact(Player player);
    
    /// <summary>
    ///  사용 가능한 지 확인
    /// </summary>
    /// <param name="player">플레이어 정보 넘겨줌</param>
    /// <returns></returns>
    bool CanInteract(Player player);
}