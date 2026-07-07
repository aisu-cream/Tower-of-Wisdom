using UnityEngine;

public static class RectTransformExtensions
{
    public static void SetWidth(this RectTransform t, float width)
    {
        t.sizeDelta = new Vector2(x:width, y:t.rect.height);
    }
}