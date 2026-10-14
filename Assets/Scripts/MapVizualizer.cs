using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class MapVizualiser : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void VisualizeMap(Map map)
    {
        if (map == null || map.heightmap == null)
        {
            Debug.LogWarning("Map or heightmap is null!");
            return;
        }

        int width = map.width;
        int height = map.height;
        float[,] heightmap = map.heightmap;

        // 1. Знаходимо мінімальну та максимальну висоту для автоматичної нормалізації
        float min = float.MaxValue;
        float max = float.MinValue;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float val = heightmap[x, y];
                if (val < min) min = val;
                if (val > max) max = val;
            }
        }

        float range = max - min;
        if (range <= 0.0001f) range = 1.0f; // Захист від ділення на нуль, якщо всі значення однакові

        // 2. Формуємо масив кольорів для текстури
        Color32[] pixels = new Color32[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Нормалізуємо висоту від 0 до 1
                float normalized = (heightmap[x, y] - min) / range;
                byte gray = (byte)(normalized * 255f);

                // Одномірний індекс для текстури (рядок за рядком)
                int index = y * width + x;
                pixels[index] = new Color32(gray, gray, gray, 255);
            }
        }

        // 3. Створюємо Texture2D та заливаємо пікселі
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        
        // FilterMode.Point залишає пікселі чіткими (без розмиття/фільтрації)
        texture.filterMode = FilterMode.Point; 
        texture.wrapMode = TextureWrapMode.Clamp;
        
        texture.SetPixels32(pixels);
        texture.Apply();

        // 4. Створюємо Sprite і призначаємо його на SpriteRenderer
        // Pivot виставлено по центру (0.5, 0.5)
        Sprite sprite = Sprite.Create(
            texture, 
            new Rect(0, 0, width, height), 
            new Vector2(0.5f, 0.5f), 
            100f // Pixels Per Unit
        );

        spriteRenderer.sprite = sprite;
    }
}