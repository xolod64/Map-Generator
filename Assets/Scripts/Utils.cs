using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Utils
{
    public static List<Vector2Int> GetPointsOnLine(int x0, int y0, int x1, int y1)
    {
        List<Vector2Int> points = new List<Vector2Int>();

        // Обчислюємо абсолютну відстань по осях
        int dx = Mathf.Abs(x1 - x0);
        int dy = Mathf.Abs(y1 - y0);

        // Визначаємо напрямок руху (+1 або -1)
        int sx = x0 < x1 ? 1 : -1;
        int sy = y0 < y1 ? 1 : -1;

        // Початкова помилка зсуву
        int err = dx - dy;

        while (true)
        {
            // Додаємо поточну точку в список
            points.Add(new Vector2Int(x0, y0));

            // Якщо дійшли до кінцевої точки — зупиняємось
            if (x0 == x1 && y0 == y1) break;

            int e2 = 2 * err;

            // Крок по осі X
            if (e2 > -dy)
            {
                err -= dy;
                x0 += sx;
            }

            // Крок по осі Y
            if (e2 < dx)
            {
                err += dx;
                y0 += sy;
            }
        }

        return points;
    }

    public static Vector2 GetCircumcenter(Vector2 a, Vector2 b, Vector2 c)
    {
        float d = 2f * (a.x * (b.y - c.y) + b.x * (c.y - a.y) + c.x * (a.y - b.y));

        if (Mathf.Abs(d) < 0.001f)
        {
            return (a + b + c) / 3f;
        }

        float aSq = a.x * a.x + a.y * a.y;
        float bSq = b.x * b.x + b.y * b.y;
        float cSq = c.x * c.x + c.y * c.y;

        float ux = (aSq * (b.y - c.y) + bSq * (c.y - a.y) + cSq * (a.y - b.y)) / d;
        float uy = (aSq * (c.x - b.x) + bSq * (a.x - c.x) + cSq * (b.x - a.x)) / d;

        return new Vector2(ux, uy);
    }

    public static float GetLineLength(int x1, int y1, int x2, int y2)
    {
        int dx = x2 - x1;
        int dy = y2 - y1;

        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float GetLineLength(Vector2Int pointA, Vector2Int pointB)
    {
        var (x1, y1) = pointA;
        var (x2, y2) = pointB;

        int dx = x2 - x1;
        int dy = y2 - y1;

        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float GetOppositeLegLength(float angleFloat, float legLength)
    {
        const float ToSixDegRadians = 0.104719755f;
        float angle = angleFloat * ToSixDegRadians;
        return legLength * MathF.Tan(angle);
    }

    public static void Deconstruct(this Vector2Int vector, out int x, out int y)
    {
        x = vector.x;
        y = vector.y;
    }

}