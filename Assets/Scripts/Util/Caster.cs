public static class Caster
{
    /// <summary>
    /// 부모 타입을 자식 타입으로 안전하게 변환한다.
    /// 변환 실패 시 null 반환.
    /// </summary>
    public static T CastTo<T>(object obj) where T : class
    {
        return obj as T;
    }

    /// <summary>
    /// 부모 타입을 자식 타입으로 안전하게 변환하고, 성공 여부를 반환한다.
    /// </summary>
    public static bool TryCastTo<T>(object obj, out T result) where T : class
    {
        result = obj as T;
        return result != null;
    }
}