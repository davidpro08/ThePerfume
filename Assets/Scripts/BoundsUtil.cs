using UnityEngine;

public static class BoundsUtil
{
    public static bool TryCalcBoundsFromRoot(Transform root, out Bounds bounds, bool includeInactive = true)
    {
        bounds = new Bounds();

        var renderers = root.GetComponentsInChildren<Renderer>(includeInactive);
        bool inited = false;

        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (!inited)
            {
                bounds = r.bounds;
                inited = true;
            }
            else
            {
                bounds.Encapsulate(r.bounds);
            }
        }
        return inited;
    }
}
