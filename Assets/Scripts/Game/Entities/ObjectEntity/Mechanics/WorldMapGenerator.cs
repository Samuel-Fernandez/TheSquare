using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using UnityEditor;

public class WorldMapGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public string fileName = "WorldMap";
    public int pixelsPerTile = 8; // Résolution de chaque tuile dans l'image finale
    public float subPixelSampling = 1.0f; // 1.0 = échantillonnage normal, 2.0 = 4x plus de détails

    [Header("Mode de détail avancé")]
    public bool useDetailedSampling = false; // Active l'échantillonnage multi-points
    public bool antiAliasing = true; // Lissage des bords
    public bool captureSubPixelObjects = false; // Capture les objets plus petits qu'une tuile

    [Header("Objets à inclure")]
    public bool includeTilemaps = true;
    public bool includeSpriteRenderers = true;
    public LayerMask layersToInclude = -1; // Quels layers inclure

    [Header("Mode de couleur")]
    public bool useRealColors = true; // Si vrai, utilise les vraies couleurs
    public float colorSaturation = 1.2f; // Augmente la saturation pour plus de contraste
    public float colorBrightness = 1.0f; // Ajuste la luminosité

    [Header("Couleurs de fallback")]
    public Color defaultSpriteColor = Color.magenta;
    public Color wallColor = Color.black;
    public Color floorColor = Color.white;
    public Color ceilingColor = Color.gray;
    public Color emptyColor = Color.clear;

    [Header("Tilemaps et SpriteRenderers détectés")]
    public Tilemap[] tilemaps;
    public SpriteRenderer[] spriteRenderers;

    [ContextMenu("Auto-détecter les Tilemaps")]
    public void AutoDetectTilemaps()
    {
        tilemaps = FindObjectsOfType<Tilemap>();
        Debug.Log($"Trouvé {tilemaps.Length} tilemaps dans la scène");
    }

    [ContextMenu("Auto-détecter les SpriteRenderers")]
    public void AutoDetectSpriteRenderers()
    {
        spriteRenderers = FindObjectsOfType<SpriteRenderer>();
        Debug.Log($"Trouvé {spriteRenderers.Length} sprite renderers dans la scène");
    }

    [ContextMenu("Auto-détecter tout")]
    public void AutoDetectAll()
    {
        AutoDetectTilemaps();
        AutoDetectSpriteRenderers();
    }

    [ContextMenu("Générer la carte PNG")]

    public void GenerateMap()
    {
        if ((includeTilemaps && (tilemaps == null || tilemaps.Length == 0)) &&
            (includeSpriteRenderers && (spriteRenderers == null || spriteRenderers.Length == 0)))
        {
            Debug.LogError("Aucun objet assigné ! Utilisez 'Auto-détecter' ou assignez manuellement.");
            return;
        }

        // Calculer les limites de tous les objets
        BoundsInt bounds = GetCombinedBounds();

        if (bounds.size.x <= 0 || bounds.size.y <= 0)
        {
            Debug.LogError("Aucun objet trouvé dans la zone !");
            return;
        }

        // Calculer la résolution finale avec le facteur de détail
        float detailFactor = useDetailedSampling ? subPixelSampling : 1.0f;
        int finalPixelsPerTile = Mathf.RoundToInt(pixelsPerTile * detailFactor);

        // Créer la texture
        int width = bounds.size.x * finalPixelsPerTile;
        int height = bounds.size.y * finalPixelsPerTile;
        Texture2D texture = new Texture2D(width, height);

        // Remplir avec la couleur vide
        Color[] pixels = new Color[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = emptyColor;
        }

        Debug.Log($"Génération de la carte {width}x{height} avec {finalPixelsPerTile} pixels par tuile...");

        // Parcourir avec plus de précision
        if (useDetailedSampling)
        {
            GenerateDetailedMap(pixels, bounds, width, height, finalPixelsPerTile);
        }
        else
        {
            GenerateBasicMap(pixels, bounds, width, finalPixelsPerTile);
        }

        texture.SetPixels(pixels);
        texture.Apply();

        // Appliquer l'anti-aliasing si demandé
        if (antiAliasing && useDetailedSampling)
        {
            ApplyAntiAliasing(texture);
        }

        // Rotation 180°
        texture = RotateTexture180(texture);

        // Sauvegarder
        SaveTexture(texture);

        Debug.Log($"Carte générée avec succès : {width}x{height} pixels");
    }

    private Texture2D RotateTexture180(Texture2D original)
    {
        int width = original.width;
        int height = original.height;
        Texture2D rotated = new Texture2D(width, height);

        Color[] originalPixels = original.GetPixels();
        Color[] rotatedPixels = new Color[originalPixels.Length];

        // Étape 1 : Rotation 180°
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = y * width + x;
                int rotatedIndex = (height - 1 - y) * width + (width - 1 - x);
                rotatedPixels[rotatedIndex] = originalPixels[originalIndex];
            }
        }

        // Étape 2 : Flip horizontal (gauche <-> droite)
        Color[] finalPixels = new Color[rotatedPixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int srcIndex = y * width + x;
                int dstIndex = y * width + (width - 1 - x);
                finalPixels[dstIndex] = rotatedPixels[srcIndex];
            }
        }

        rotated.SetPixels(finalPixels);
        rotated.Apply();
        return rotated;
    }



    private void GenerateBasicMap(Color[] pixels, BoundsInt bounds, int imageWidth, int finalPixelsPerTile)
    {
        // Méthode originale, rapide
        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3 worldPos = new Vector3(x + 0.5f, y + 0.5f, 0);
                Color pixelColor = GetPixelColor(worldPos, new Vector3Int(x, y, 0));

                if (pixelColor != emptyColor)
                {
                    DrawTilePixels(pixels, x - bounds.xMin, y - bounds.yMin, bounds.size.x, pixelColor, imageWidth, finalPixelsPerTile);
                }
            }
        }
    }

    private void GenerateDetailedMap(Color[] pixels, BoundsInt bounds, int imageWidth, int imageHeight, int finalPixelsPerTile)
    {
        // Méthode détaillée : échantillonne chaque pixel individuellement
        float worldPixelSize = 1.0f / finalPixelsPerTile;

        for (int px = 0; px < imageWidth; px++)
        {
            for (int py = 0; py < imageHeight; py++)
            {
                // Calculer la position mondiale de ce pixel
                float worldX = bounds.xMin + (px * worldPixelSize) + (worldPixelSize * 0.5f);
                float worldY = bounds.yMin + (py * worldPixelSize) + (worldPixelSize * 0.5f);

                Vector3 worldPos = new Vector3(worldX, worldY, 0);
                Vector3Int gridPos = new Vector3Int(Mathf.FloorToInt(worldX), Mathf.FloorToInt(worldY), 0);

                Color pixelColor = emptyColor;

                if (captureSubPixelObjects)
                {
                    // Échantillonnage multiple pour capturer les petits objets
                    pixelColor = GetDetailedPixelColor(worldPos, gridPos, worldPixelSize);
                }
                else
                {
                    pixelColor = GetPixelColor(worldPos, gridPos);
                }

                // Inverser Y pour Unity
                int index = ((imageHeight - 1 - py) * imageWidth) + px;
                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = pixelColor;
                }
            }

            // Afficher le progrès
            if (px % 100 == 0)
            {
                float progress = (float)px / imageWidth;
                if (UnityEditor.EditorUtility.DisplayCancelableProgressBar("Génération détaillée", $"Pixel {px}/{imageWidth}", progress))
                {
                    UnityEditor.EditorUtility.ClearProgressBar();
                    return;
                }
            }
        }

        UnityEditor.EditorUtility.ClearProgressBar();
    }

    private Color GetDetailedPixelColor(Vector3 centerPos, Vector3Int gridPos, float pixelSize)
    {
        // Échantillonnage 3x3 pour capturer plus de détails
        Vector3[] samplePoints = {
            centerPos,
            centerPos + new Vector3(-pixelSize * 0.3f, -pixelSize * 0.3f, 0),
            centerPos + new Vector3(pixelSize * 0.3f, -pixelSize * 0.3f, 0),
            centerPos + new Vector3(-pixelSize * 0.3f, pixelSize * 0.3f, 0),
            centerPos + new Vector3(pixelSize * 0.3f, pixelSize * 0.3f, 0),
        };

        Color finalColor = emptyColor;
        float totalAlpha = 0;
        Color mixedColor = Color.clear;

        foreach (Vector3 samplePos in samplePoints)
        {
            Color sampleColor = GetPixelColor(samplePos, gridPos);
            if (sampleColor.a > 0.1f)
            {
                mixedColor += sampleColor * sampleColor.a;
                totalAlpha += sampleColor.a;
            }
        }

        if (totalAlpha > 0)
        {
            finalColor = mixedColor / totalAlpha;
            finalColor.a = Mathf.Clamp01(totalAlpha / samplePoints.Length);
        }

        return finalColor;
    }

    private void ApplyAntiAliasing(Texture2D texture)
    {
        Color[] pixels = texture.GetPixels();
        Color[] newPixels = new Color[pixels.Length];
        int width = texture.width;
        int height = texture.height;

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int index = y * width + x;

                // Moyenner avec les pixels adjacents
                Color center = pixels[index];
                Color avg = (
                    pixels[index - 1] +     // gauche
                    pixels[index + 1] +     // droite
                    pixels[index - width] + // haut
                    pixels[index + width] + // bas
                    center * 2              // centre avec plus de poids
                ) / 6.0f;

                // Mélanger légèrement
                newPixels[index] = Color.Lerp(center, avg, 0.3f);
            }
        }

        // Copier les bords
        for (int x = 0; x < width; x++)
        {
            newPixels[x] = pixels[x]; // bas
            newPixels[(height - 1) * width + x] = pixels[(height - 1) * width + x]; // haut
        }
        for (int y = 0; y < height; y++)
        {
            newPixels[y * width] = pixels[y * width]; // gauche
            newPixels[y * width + (width - 1)] = pixels[y * width + (width - 1)]; // droite
        }

        texture.SetPixels(newPixels);
        texture.Apply();
    }

    private BoundsInt GetCombinedBounds()
    {
        Bounds worldBounds = new Bounds();
        bool hasBounds = false;

        // Ajouter les bounds des tilemaps
        if (includeTilemaps && tilemaps != null)
        {
            foreach (Tilemap tilemap in tilemaps)
            {
                if (tilemap.cellBounds.size.x > 0 && tilemap.cellBounds.size.y > 0)
                {
                    Bounds tilemapBounds = new Bounds();
                    tilemapBounds.SetMinMax(
                        tilemap.cellBounds.min,
                        tilemap.cellBounds.max
                    );

                    if (!hasBounds)
                    {
                        worldBounds = tilemapBounds;
                        hasBounds = true;
                    }
                    else
                    {
                        worldBounds.Encapsulate(tilemapBounds);
                    }
                }
            }
        }

        // Ajouter les bounds des sprite renderers
        if (includeSpriteRenderers && spriteRenderers != null)
        {
            foreach (SpriteRenderer sr in spriteRenderers)
            {
                if (sr != null && sr.sprite != null)
                {
                    // Vérifier le layer
                    if ((layersToInclude.value & (1 << sr.gameObject.layer)) == 0)
                        continue;

                    Vector3 pos = sr.transform.position;
                    Vector3Int gridPos = new Vector3Int(
                        Mathf.FloorToInt(pos.x),
                        Mathf.FloorToInt(pos.y),
                        0
                    );

                    if (!hasBounds)
                    {
                        worldBounds = new Bounds(gridPos, Vector3.one);
                        hasBounds = true;
                    }
                    else
                    {
                        worldBounds.Encapsulate(gridPos);
                    }
                }
            }
        }

        if (!hasBounds) return new BoundsInt();

        return new BoundsInt(
            Mathf.FloorToInt(worldBounds.min.x),
            Mathf.FloorToInt(worldBounds.min.y),
            0,
            Mathf.CeilToInt(worldBounds.size.x),
            Mathf.CeilToInt(worldBounds.size.y),
            1
        );
    }

    private Color GetPixelColor(Vector3 worldPos, Vector3Int gridPos)
    {
        Color finalColor = emptyColor;

        // 1. Vérifier les sprite renderers (priorité haute car ils sont souvent au-dessus)
        if (includeSpriteRenderers && spriteRenderers != null)
        {
            // Trier par ordre Z (les plus hauts en premier)
            System.Array.Sort(spriteRenderers, (a, b) =>
            {
                if (a == null && b == null) return 0;
                if (a == null) return 1;
                if (b == null) return -1;
                return b.transform.position.z.CompareTo(a.transform.position.z);
            });

            foreach (SpriteRenderer sr in spriteRenderers)
            {
                if (sr == null || sr.sprite == null) continue;

                // Vérifier le layer
                if ((layersToInclude.value & (1 << sr.gameObject.layer)) == 0)
                    continue;

                // Vérifier si le sprite contient cette position
                if (SpriteContainsPoint(sr, worldPos))
                {
                    Color spriteColor = GetSpriteRendererColor(sr);
                    if (spriteColor.a > 0.1f) // Pas transparent
                    {
                        finalColor = spriteColor;
                        break; // Premier sprite trouvé (le plus haut)
                    }
                }
            }
        }

        // 2. Si pas de sprite trouvé, vérifier les tilemaps
        if (finalColor == emptyColor && includeTilemaps && tilemaps != null)
        {
            finalColor = GetTileColor(gridPos);
        }

        return finalColor;
    }

    private bool SpriteContainsPoint(SpriteRenderer sr, Vector3 worldPos)
    {
        Vector3 localPos = sr.transform.InverseTransformPoint(worldPos);
        Bounds spriteBounds = sr.sprite.bounds;

        return spriteBounds.Contains(localPos);
    }

    private Color GetSpriteRendererColor(SpriteRenderer sr)
    {
        if (sr == null || sr.sprite == null) return emptyColor;

        Color spriteColor = emptyColor;

        if (useRealColors)
        {
            // Récupérer la couleur moyenne du sprite
            spriteColor = GetSpriteAverageColor(sr.sprite);

            // Multiplier par la couleur du renderer
            if (spriteColor != Color.clear)
            {
                spriteColor *= sr.color;
                spriteColor = EnhanceColor(spriteColor);
            }
        }

        // Fallback
        if (spriteColor == Color.clear || spriteColor.a < 0.1f)
        {
            spriteColor = sr.color != Color.white ? sr.color : defaultSpriteColor;
        }

        return spriteColor;
    }

    private Color GetTileColor(Vector3Int position)
    {
        foreach (Tilemap tilemap in tilemaps)
        {
            TileBase tile = tilemap.GetTile(position);
            if (tile != null)
            {
                if (useRealColors)
                {
                    Color realColor = GetRealTileColor(tilemap, tile, position);
                    if (realColor != Color.clear)
                    {
                        return EnhanceColor(realColor);
                    }
                }

                // Fallback sur les couleurs prédéfinies
                return DetermineTileColor(tilemap.name, tile.name);
            }
        }
        return emptyColor;
    }

    private Color GetRealTileColor(Tilemap tilemap, TileBase tileBase, Vector3Int position)
    {
        // Méthode 1 : Couleur du tilemap renderer
        TilemapRenderer renderer = tilemap.GetComponent<TilemapRenderer>();
        if (renderer != null && renderer.material != null && renderer.material.HasProperty("_Color"))
        {
            Color materialColor = renderer.material.GetColor("_Color");
            if (materialColor != Color.white) // Si ce n'est pas la couleur par défaut
            {
                return materialColor;
            }
        }

        // Méthode 2 : Couleur de la tuile elle-même
        Color tileColor = tilemap.GetColor(position);
        if (tileColor != Color.white)
        {
            return tileColor;
        }

        // Méthode 3 : Si c'est un Tile (pas un RuleTile), récupérer le sprite
        if (tileBase is Tile tile && tile.sprite != null)
        {
            return GetSpriteAverageColor(tile.sprite);
        }

        // Méthode 4 : Pour les RuleTiles ou autres, essayer de récupérer le sprite par réflection
        System.Type tileType = tileBase.GetType();
        var spriteProperty = tileType.GetProperty("sprite");
        if (spriteProperty != null)
        {
            Sprite sprite = spriteProperty.GetValue(tileBase) as Sprite;
            if (sprite != null)
            {
                return GetSpriteAverageColor(sprite);
            }
        }

        return Color.clear;
    }

    private Color GetSpriteAverageColor(Sprite sprite)
    {
        if (sprite == null || sprite.texture == null) return Color.clear;

        try
        {
            // Créer une texture temporaire lisible
            Texture2D readableTexture = GetReadableTexture(sprite.texture);
            if (readableTexture == null) return Color.clear;

            // Récupérer les pixels de la zone du sprite
            Rect rect = sprite.textureRect;
            int startX = Mathf.FloorToInt(rect.x);
            int startY = Mathf.FloorToInt(rect.y);
            int width = Mathf.FloorToInt(rect.width);
            int height = Mathf.FloorToInt(rect.height);

            Color[] pixels = readableTexture.GetPixels(startX, startY, width, height);

            // Calculer la couleur moyenne (en ignorant les pixels transparents)
            float r = 0, g = 0, b = 0, a = 0;
            int validPixels = 0;

            foreach (Color pixel in pixels)
            {
                if (pixel.a > 0.1f) // Ignorer les pixels presque transparents
                {
                    r += pixel.r * pixel.a; // Pondérer par l'alpha
                    g += pixel.g * pixel.a;
                    b += pixel.b * pixel.a;
                    a += pixel.a;
                    validPixels++;
                }
            }

            if (validPixels > 0)
            {
                return new Color(r / validPixels, g / validPixels, b / validPixels, a / validPixels);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Impossible de lire la texture du sprite {sprite.name}: {e.Message}");
        }

        return Color.clear;
    }

    private Texture2D GetReadableTexture(Texture2D original)
    {
        if (original == null) return null;

        // Si la texture est déjà lisible
        try
        {
            original.GetPixel(0, 0);
            return original;
        }
        catch
        {
            // La texture n'est pas lisible, créer une copie
        }

        // Créer une RenderTexture temporaire
        RenderTexture renderTexture = RenderTexture.GetTemporary(original.width, original.height);
        Graphics.Blit(original, renderTexture);

        // Lire depuis la RenderTexture
        RenderTexture.active = renderTexture;
        Texture2D readableTexture = new Texture2D(original.width, original.height);
        readableTexture.ReadPixels(new Rect(0, 0, original.width, original.height), 0, 0);
        readableTexture.Apply();

        // Nettoyer
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTexture;
    }

    private Color EnhanceColor(Color color)
    {
        // Ajuster la saturation
        Color.RGBToHSV(color, out float h, out float s, out float v);
        s = Mathf.Clamp01(s * colorSaturation);
        v = Mathf.Clamp01(v * colorBrightness);

        Color enhancedColor = Color.HSVToRGB(h, s, v);
        enhancedColor.a = color.a;

        return enhancedColor;
    }

    private Color DetermineTileColor(string tilemapName, string tileName)
    {
        string name = (tilemapName + " " + tileName).ToLower();

        if (name.Contains("wall") || name.Contains("mur"))
            return wallColor;
        else if (name.Contains("ceiling") || name.Contains("plafond") || name.Contains("toit"))
            return ceilingColor;
        else if (name.Contains("floor") || name.Contains("sol") || name.Contains("ground"))
            return floorColor;

        // Par défaut, considérer comme un mur si pas identifié
        return wallColor;
    }

    private void DrawTilePixels(Color[] pixels, int tileX, int tileY, int mapWidth, Color color, int imageWidth, int finalPixelsPerTile)
    {
        for (int px = 0; px < finalPixelsPerTile; px++)
        {
            for (int py = 0; py < finalPixelsPerTile; py++)
            {
                int x = tileX * finalPixelsPerTile + px;
                int y = tileY * finalPixelsPerTile + py;

                // Unity utilise un système de coordonnées inversé pour les textures
                int index = ((pixels.Length / imageWidth - 1 - y) * imageWidth) + x;
                if (index >= 0 && index < pixels.Length)
                {
                    pixels[index] = color;
                }
            }
        }
    }

    private void SaveTexture(Texture2D texture)
    {
        byte[] bytes = texture.EncodeToPNG();

#if UNITY_EDITOR
        string path = $"Assets/{fileName}_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}.png";
        File.WriteAllBytes(path, bytes);
        AssetDatabase.Refresh();
        Debug.Log($"Carte sauvegardée : {path}");
#else
        string path = Path.Combine(Application.persistentDataPath, $"{fileName}_{UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}.png");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"Carte sauvegardée : {path}");
#endif

        DestroyImmediate(texture);
    }
}