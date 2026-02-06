using UnityEngine;

public class Background : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FitToScreen();
    }

    void FitToScreen()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) return;

        float width = sr.sprite.bounds.size.x;
        float height = sr.sprite.bounds.size.y;

        float worldScreenHeight = Camera.main.orthographicSize * 2f;
        float worldScreenWidth = worldScreenHeight / Screen.height * Screen.width;

        Vector3 xWidth = transform.localScale;
        xWidth.x = worldScreenWidth / width;
        xWidth.y = worldScreenHeight / height;

        float maxScale = Mathf.Max(xWidth.x, xWidth.y);
        transform.localScale = new Vector3(maxScale, maxScale, 1f);
    }
}
