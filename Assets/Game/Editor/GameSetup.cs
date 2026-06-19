using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameSetup
{
    [MenuItem("Tools/Add Neon Background Only")]
    public static void AddBackgroundOnly()
    {
        // First, check if there's an old UI GameBackground and destroy it
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            Transform oldUI = canvas.transform.Find("GameBackground");
            if (oldUI != null) Object.DestroyImmediate(oldUI.gameObject);
        }

        GameObject bgGO = GameObject.Find("GameBackground");
        if (bgGO == null)
        {
            bgGO = new GameObject("GameBackground");
        }
        bgGO.transform.SetParent(null);
        bgGO.transform.position = new Vector3(0f, 0f, 0f);

        SpriteRenderer bgSR = bgGO.GetComponent<SpriteRenderer>();
        if (bgSR == null) bgSR = bgGO.AddComponent<SpriteRenderer>();

        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/game_background.png");
        if (bgSprite != null)
        {
            bgSR.sprite = bgSprite;
            bgSR.color = new Color(1f, 1f, 1f, 0.5f); // 50% opacity in world space
        }
        bgSR.sortingOrder = -10; // Draw behind gameplay dots (dots are at 0)

        // Scale to cover orthographic size 5 viewport (already large at 10.8x19.2 units)
        bgGO.transform.localScale = Vector3.one;

        NeonBackgroundAnimator animator = bgGO.GetComponent<NeonBackgroundAnimator>();
        if (animator == null) bgGO.AddComponent<NeonBackgroundAnimator>();

        Debug.Log("Successfully added Neon World Background!");
    }


    [MenuItem("Tools/Setup Dot Chain Rush")]
    public static void SetupGame()
    {
        // 0a. Create Texture Placeholders
        CreatePlaceholderTexture("Assets/Textures/pause_circle.png", new Color(0.82f, 0.74f, 1f)); // Light Purple
        
        // Build hollow gradient ring for timer
        BuildGradientRingTexture("Assets/Textures/timer.png");

        CreatePlaceholderTexture("Assets/Textures/bolt.png", new Color(1f, 0.84f, 0f)); // Gold/Yellow
        CreatePlaceholderTexture("Assets/Textures/rocket.png", new Color(1f, 0.45f, 0.35f)); // Coral Red
        CreatePlaceholderTexture("Assets/Textures/refresh.png", new Color(0.8f, 0.76f, 0.85f)); // Light gray-purple
        CreatePlaceholderTexture("Assets/Textures/settings.png", new Color(0.8f, 0.76f, 0.85f));
        CreatePlaceholderTexture("Assets/Textures/auto_fix_high.png", new Color(0.82f, 0.74f, 1f));
        CreatePlaceholderTexture("Assets/Textures/emoji_events.png", new Color(0.49f, 0.96f, 1f));
        CreatePlaceholderTexture("Assets/Textures/star.png", new Color(1f, 1f, 1f, 0.8f)); // White star

        // 0. Clean up leftover dots and stray colliders saved in the scene hierarchy from previous runs
        Dot[] leftoverDots = Object.FindObjectsByType<Dot>(FindObjectsSortMode.None);
        foreach (var dot in leftoverDots)
        {
            if (dot.gameObject.scene.name != null) // is in scene, not a prefab
            {
                Object.DestroyImmediate(dot.gameObject);
            }
        }

        Collider2D[] allSceneColliders = Object.FindObjectsByType<Collider2D>(FindObjectsSortMode.None);
        foreach (var col in allSceneColliders)
        {
            GameObject go = col.gameObject;
            if (go.scene.name != null && 
                go.name != "ScreenBoundaries" && 
                !go.name.Contains("Circle") && 
                !go.name.Contains("Prefab"))
            {
                Debug.LogWarning($"[Setup Cleanup] Destroying stray collider object in scene: {go.name}");
                Object.DestroyImmediate(go);
            }
        }

        // 1. Setup Folders
        string[] folders = {
            "Assets/Game/Scripts",
            "Assets/Game/Prefabs",
            "Assets/Game/UI",
            "Assets/Game/Effects",
            "Assets/Game/Materials",
            "Assets/Game/Audio",
            "Assets/Game/Scenes"
        };
        foreach (string folder in folders)
        {
            if (!System.IO.Directory.Exists(folder))
            {
                System.IO.Directory.CreateDirectory(folder);
            }
        }

        // 2. Generate Circle Texture (Base with crisp/sharp solid edge)
        string texturePath = "Assets/Game/UI/Circle.png";
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 63.5f;
                    float dy = y - 63.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    if (dist <= 61f)
                    {
                        if (dist <= 59f)
                        {
                            alpha = 1.0f;
                        }
                        else
                        {
                            alpha = Mathf.Clamp01(1f - (dist - 59f) / 2f); // sharp anti-aliased edge
                        }
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(texturePath, bytes);
            System.IO.File.WriteAllBytes("Assets/Circle.png", bytes);
            AssetDatabase.ImportAsset(texturePath);
            AssetDatabase.ImportAsset("Assets/Circle.png");
 
            TextureImporter importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false; // Disable mipmaps to prevent blurriness
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }

            TextureImporter importer2 = AssetImporter.GetAtPath("Assets/Circle.png") as TextureImporter;
            if (importer2 != null)
            {
                importer2.textureType = TextureImporterType.Sprite;
                importer2.spritePixelsPerUnit = 128f;
                importer2.filterMode = FilterMode.Bilinear;
                importer2.mipmapEnabled = false; // Disable mipmaps to prevent blurriness
                importer2.textureCompression = TextureImporterCompression.Uncompressed;
                importer2.SaveAndReimport();
            }
        }

        // 2b. Generate Highlight Texture (3D Specular & Shadows matching the sharp radius)
        string highlightPath = "Assets/Game/UI/Highlight.png";
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 63.5f;
                    float dy = y - 63.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    Color pixelColor = Color.clear;
                    if (dist <= 61f)
                    {
                        float rNorm = dist / 61f;
                        float nz = Mathf.Sqrt(1f - rNorm * rNorm);
                        float nx = dx / 61f;
                        float ny = dy / 61f;

                        // Specular reflection (white glow at top-left)
                        float spec = Mathf.Pow(Mathf.Max(0f, -0.4f * nx + 0.4f * ny + 0.82f * nz), 12f);

                        // Inner shadow/depth
                        float shadowFactor = Mathf.Pow(Mathf.Max(0f, 0.5f * nx - 0.5f * ny + 0.7f * (1f - nz)), 2f) * 0.4f;

                        if (spec > 0.05f)
                        {
                            pixelColor = new Color(1f, 1f, 1f, spec * 0.9f);
                        }
                        else if (shadowFactor > 0.02f)
                        {
                            pixelColor = new Color(0f, 0f, 0f, shadowFactor);
                        }
                    }
                    tex.SetPixel(x, y, pixelColor);
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(highlightPath, bytes);
            AssetDatabase.ImportAsset(highlightPath);

            TextureImporter importer = AssetImporter.GetAtPath(highlightPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false; // Disable mipmaps to prevent blurriness
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // 2c. Generate Special Ring Texture (Concentric Outline border with thick neon glow)
        string specialRingPath = "Assets/Game/UI/SpecialRing.png";
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 63.5f;
                    float dy = y - 63.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    // Glowing concentric ring with a wider falloff to simulate a glowing edge neon style
                    if (dist >= 46f && dist <= 62f)
                    {
                        float delta = Mathf.Abs(dist - 54f);
                        alpha = Mathf.Exp(-delta * delta / 22f); // Wider, softer exponential glow
                    }

                    // Pure White Color for dynamic tinting
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(specialRingPath, bytes);
            AssetDatabase.ImportAsset(specialRingPath);

            TextureImporter importer = AssetImporter.GetAtPath(specialRingPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false; // Disable mipmaps to prevent blurriness
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // 2d. Generate Smoke Glow Texture (Smooth high-fidelity clean radial glow halo)
        string smokeGlowPath = "Assets/Game/UI/SmokeGlow.png";
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 63.5f;
                    float dy = y - 63.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    if (dist <= 63.5f)
                    {
                        float t = dist / 63.5f;
                        alpha = Mathf.Pow(1.0f - t, 1.8f); // extremely clean, smooth radial falloff
                    }
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(smokeGlowPath, bytes);
            AssetDatabase.ImportAsset(smokeGlowPath);

            TextureImporter importer = AssetImporter.GetAtPath(smokeGlowPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Bilinear;
                importer.mipmapEnabled = false; // Disable mipmaps to prevent blurriness
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // 2e. Generate Line Texture (Futuristic Glowing Neon Laser Beam)
        string lineTexPath = "Assets/Game/UI/LineTexture.png";
        {
            Texture2D tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (int y = 0; y < 16; y++)
            {
                for (int x = 0; x < 16; x++)
                {
                    float dy = Mathf.Abs(y - 7.5f);
                    float alpha = 0f;
                    Color col = Color.white;

                    if (dy <= 1.5f)
                    {
                        // Hot white core
                        alpha = 1.0f;
                        col = Color.white;
                    }
                    else if (dy <= 7.5f)
                    {
                        // Soft glowing neon outline
                        float t = (dy - 1.5f) / 6.0f;
                        alpha = Mathf.Lerp(0.85f, 0.0f, t * t);
                        col = new Color(0.92f, 0.92f, 1.0f, alpha);
                    }
                    tex.SetPixel(x, y, new Color(col.r, col.g, col.b, alpha));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(lineTexPath, bytes);
            AssetDatabase.ImportAsset(lineTexPath);

            TextureImporter importer = AssetImporter.GetAtPath(lineTexPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.filterMode = FilterMode.Bilinear;
                importer.wrapMode = TextureWrapMode.Repeat;
                importer.SaveAndReimport();
            }
        }

        // 2f. Generate Rainbow Circle Texture (Multicolor rainbow wheel for special dots)
        string rainbowCirclePath = "Assets/Game/UI/RainbowCircle.png";
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 63.5f;
                    float dy = y - 63.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    if (dist <= 63.5f)
                    {
                        if (dist <= 46f)
                        {
                            alpha = 1.0f;
                        }
                        else
                        {
                            float t = (dist - 46f) / 17.5f;
                            alpha = Mathf.Lerp(1.0f, 0.0f, t * t);
                        }
                    }
                    
                    float angle = Mathf.Atan2(dy, dx);
                    float hue = (angle + Mathf.PI) / (2f * Mathf.PI);
                    Color rainbowColor = Color.HSVToRGB(hue, 0.9f, 1f);
                    tex.SetPixel(x, y, new Color(rainbowColor.r, rainbowColor.g, rainbowColor.b, alpha));
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(rainbowCirclePath, bytes);
            AssetDatabase.ImportAsset(rainbowCirclePath);
 
            TextureImporter importer = AssetImporter.GetAtPath(rainbowCirclePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        // 2g. Generate Obstacle Texture (Metallic grey bubble with steel rivets)
        string obstaclePath = "Assets/Game/UI/Obstacle.png";
        {
            Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    float dx = x - 63.5f;
                    float dy = y - 63.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = 0f;
                    Color col = Color.clear;
                    if (dist <= 63.5f)
                    {
                        if (dist <= 46f)
                        {
                            alpha = 1.0f;
                        }
                        else
                        {
                            float t = (dist - 46f) / 17.5f;
                            alpha = Mathf.Lerp(1.0f, 0.0f, t * t);
                        }

                        // Metallic dark steel base
                        float rNorm = dist / 46f;
                        float brightness = 0.22f + 0.14f * Mathf.Sin(dist * 0.35f) + (1f - rNorm) * 0.12f;
                        
                        // Circular steel rivet points
                        float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                        if (Mathf.Abs(dist - 30f) < 2.5f && (Mathf.RoundToInt(angle) % 45 == 0))
                        {
                            brightness += 0.35f;
                        }
                        col = new Color(brightness, brightness, brightness + 0.05f, alpha);
                    }
                    tex.SetPixel(x, y, col);
                }
            }
            tex.Apply();
            byte[] bytes = tex.EncodeToPNG();
            System.IO.File.WriteAllBytes(obstaclePath, bytes);
            AssetDatabase.ImportAsset(obstaclePath);
 
            TextureImporter importer = AssetImporter.GetAtPath(obstaclePath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128f;
                importer.filterMode = FilterMode.Bilinear;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.SaveAndReimport();
            }
        }

        Sprite circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        Sprite highlightSprite = AssetDatabase.LoadAssetAtPath<Sprite>(highlightPath);
        Sprite specialRingSprite = AssetDatabase.LoadAssetAtPath<Sprite>(specialRingPath);
        Sprite smokeGlowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(smokeGlowPath);
        Sprite rainbowSprite = AssetDatabase.LoadAssetAtPath<Sprite>(rainbowCirclePath);
        Sprite obstacleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(obstaclePath);

        // Create/Fetch Zero Friction Physics Material
        PhysicsMaterial2D physicsMat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Game/Materials/DotPhysicsMaterial.physicsMaterial2D");
        if (physicsMat == null)
        {
            physicsMat = new PhysicsMaterial2D("DotPhysicsMaterial");
            physicsMat.friction = 0f;
            physicsMat.bounciness = 0.15f;
            AssetDatabase.CreateAsset(physicsMat, "Assets/Game/Materials/DotPhysicsMaterial.physicsMaterial2D");
        }

        // 3. Create/Update Circle Prefabs
        UpdateCirclePrefab("Assets/Game/Prefabs/Circle.prefab", circleSprite, highlightSprite, specialRingSprite, smokeGlowSprite, rainbowSprite, obstacleSprite, physicsMat);
        UpdateCirclePrefab("Assets/Prefabs/Circle.prefab", circleSprite, highlightSprite, specialRingSprite, smokeGlowSprite, rainbowSprite, obstacleSprite, physicsMat);
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Game/Prefabs/Circle.prefab");

        // 3b. Create Particle Explosion Prefab
        string particlePrefabPath = "Assets/Game/Prefabs/Explosion.prefab";
        GameObject explosionPrefab = null;
        {
            GameObject tempGO = new GameObject("ExplosionPrefabTemp");
            ParticleSystem ps = tempGO.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.duration = 0.5f;
            main.loop = false;
            main.startSizeMultiplier = 1f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.32f); // Random size
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f); // Random lifetime
            main.startSpeed = new ParticleSystem.MinMaxCurve(8f, 13f); // Super fast initial velocity for explosive shockwave
            main.stopAction = ParticleSystemStopAction.Destroy;

            // Limit Velocity Over Lifetime: High damping/drag to slow particles down quickly (explosive release effect)
            var limitVelocity = ps.limitVelocityOverLifetime;
            limitVelocity.enabled = true;
            limitVelocity.dampen = 0.28f; // Strong braking factor
            limitVelocity.drag = new ParticleSystem.MinMaxCurve(2.5f); // High air resistance

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            emission.burstCount = 1;
            emission.SetBurst(0, new ParticleSystem.Burst(0f, 35)); // 35 glowing particles!

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.05f; // Extremely tight source radius for core shockwave point

            // Size Over Lifetime: Shrink to 0 for a neat pop effect
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Color Over Lifetime: Fade out particles gracefully
            var colorOverLifetime = ps.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            // Texture Sheet Animation module to assign the sprite
            var textureSheet = ps.textureSheetAnimation;
            textureSheet.enabled = true;
            textureSheet.mode = ParticleSystemAnimationMode.Sprites;
            textureSheet.AddSprite(circleSprite);

            ParticleSystemRenderer psr = tempGO.GetComponent<ParticleSystemRenderer>();
            if (psr != null)
            {
                psr.sharedMaterial = AssetDatabase.GetBuiltinExtraResource<Material>("Sprites-Default.mat");
                psr.renderMode = ParticleSystemRenderMode.Billboard;
            }

            explosionPrefab = PrefabUtility.SaveAsPrefabAsset(tempGO, particlePrefabPath);
            Object.DestroyImmediate(tempGO);
        }

        // 4. Setup Managers & Pools
        ColorManager colorManager = Object.FindAnyObjectByType<ColorManager>();
        if (colorManager == null)
        {
            GameObject colorManagerGO = new GameObject("ColorManager");
            colorManager = colorManagerGO.AddComponent<ColorManager>();
        }

        DifficultyManager difficultyManager = Object.FindAnyObjectByType<DifficultyManager>();
        if (difficultyManager == null)
        {
            GameObject diffGO = new GameObject("DifficultyManager");
            difficultyManager = diffGO.AddComponent<DifficultyManager>();
        }

        AudioManager audioManager = Object.FindAnyObjectByType<AudioManager>();
        if (audioManager == null)
        {
            GameObject audioGO = new GameObject("AudioManager");
            audioManager = audioGO.AddComponent<AudioManager>();
        }

        ObjectPool objectPool = Object.FindAnyObjectByType<ObjectPool>();
        if (objectPool == null)
        {
            GameObject poolGO = new GameObject("ObjectPool");
            objectPool = poolGO.AddComponent<ObjectPool>();
        }

        SerializedObject serializedPool = new SerializedObject(objectPool);
        serializedPool.FindProperty("prefab").objectReferenceValue = prefab;
        serializedPool.FindProperty("initialSize").intValue = 15;
        serializedPool.ApplyModifiedProperties();

        // Setup Screen Boundaries (U-shape: Left Wall, Bottom Floor, Right Wall)
        // Clean up any duplicate boundary colliders
        EdgeCollider2D[] allGrounds = Object.FindObjectsByType<EdgeCollider2D>(FindObjectsSortMode.None);
        foreach (var oldGround in allGrounds)
        {
            if (oldGround.gameObject.name != "ScreenBoundaries")
            {
                Object.DestroyImmediate(oldGround.gameObject);
            }
        }

        EdgeCollider2D ground = Object.FindAnyObjectByType<EdgeCollider2D>();
        if (ground == null)
        {
            GameObject groundGO = new GameObject("ScreenBoundaries");
            ground = groundGO.AddComponent<EdgeCollider2D>();
        }

        // Ensure ScreenBoundsAdaptor is attached to calculate boundaries dynamically at runtime
        ScreenBoundsAdaptor adaptor = ground.GetComponent<ScreenBoundsAdaptor>();
        if (adaptor == null)
        {
            adaptor = ground.gameObject.AddComponent<ScreenBoundsAdaptor>();
        }

        // Reset transform to prevent any position offset placing the boundaries in the center
        ground.transform.position = Vector3.zero;
        ground.transform.rotation = Quaternion.identity;
        ground.transform.localScale = Vector3.one;

        ground.points = new Vector2[] {
            new Vector2(-2.8f, 6f),     // Top-Left
            new Vector2(-2.8f, -4.5f),  // Bottom-Left
            new Vector2(2.8f, -4.5f),   // Bottom-Right
            new Vector2(2.8f, 6f)       // Top-Right
        };

        // 5. Setup Scene Camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camGO = new GameObject("Main Camera");
            cam = camGO.AddComponent<Camera>();
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 0f, -10f);
        }
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.backgroundColor = new Color(0.12f, 0.12f, 0.16f); // Dark background
        cam.clearFlags = CameraClearFlags.SolidColor;

        // 6. Setup Spawner
        DotSpawner spawner = Object.FindAnyObjectByType<DotSpawner>();
        if (spawner == null)
        {
            GameObject spawnerGO = new GameObject("DotSpawner");
            spawner = spawnerGO.AddComponent<DotSpawner>();
        }

        // 7. Setup Chain Controller
        ChainController chainController = Object.FindAnyObjectByType<ChainController>();
        if (chainController == null)
        {
            GameObject chainGO = new GameObject("ChainController");
            chainController = chainGO.AddComponent<ChainController>();
        }

        TouchInputProcessor inputProcessor = Object.FindAnyObjectByType<TouchInputProcessor>();
        if (inputProcessor == null)
        {
            GameObject inputGO = new GameObject("TouchInputProcessor");
            inputProcessor = inputGO.AddComponent<TouchInputProcessor>();
        }

        SerializedObject serializedInput = new SerializedObject(inputProcessor);
        serializedInput.FindProperty("dotLayerMask").intValue = 1 << 0; // Default layer
        serializedInput.ApplyModifiedProperties();

        LineRenderer lr = chainController.GetComponent<LineRenderer>();
        if (lr == null) lr = chainController.gameObject.AddComponent<LineRenderer>();
        Material lineMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Game/Materials/LineMaterial.mat");
        if (lineMat == null)
        {
            Shader shader = Shader.Find("Mobile/Particles/Additive");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Particles/Additive");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            lineMat = new Material(shader);
            AssetDatabase.CreateAsset(lineMat, "Assets/Game/Materials/LineMaterial.mat");
        }
        Texture2D lineTex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Game/UI/LineTexture.png");
        if (lineTex != null)
        {
            lineMat.mainTexture = lineTex;
        }
        lr.sharedMaterial = lineMat;
        lr.textureMode = LineTextureMode.Tile;

        SerializedObject serializedChain = new SerializedObject(chainController);
        serializedChain.FindProperty("dotLayerMask").intValue = 1 << 0; // Default layer
        serializedChain.ApplyModifiedProperties();

        // 8. Setup Canvas & UI — Find or create Canvas with guaranteed correct configuration
        Canvas canvas = null;
        Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        // Find our main "Canvas" by name
        foreach (Canvas c in allCanvases)
        {
            if (c.gameObject.name == "Canvas")
            {
                canvas = c;
                break;
            }
        }

        // Destroy any stray canvases that could overlay/block our UI
        foreach (Canvas c in allCanvases)
        {
            if (c != canvas)
            {
                Debug.LogWarning($"[Setup] Removing stray Canvas: {c.gameObject.name}");
                Object.DestroyImmediate(c.gameObject);
            }
        }

        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
        }

        // ALWAYS ensure proper Canvas configuration (critical fix for pre-existing Canvas)
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvas.gameObject.SetActive(true);

        // Remove any UIDocument component that could interfere with Canvas rendering
        var canvasUIDoc = canvas.GetComponent<UnityEngine.UIElements.UIDocument>();
        if (canvasUIDoc != null)
        {
            Debug.LogWarning("[Setup] Removing UIDocument from Canvas to fix rendering");
            Object.DestroyImmediate(canvasUIDoc);
        }

        // Ensure CanvasScaler is properly configured
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;

        // Ensure GraphicRaycaster for button interactions
        if (canvas.GetComponent<GraphicRaycaster>() == null)
            canvas.gameObject.AddComponent<GraphicRaycaster>();

        // Deep clean: Destroy all existing UI child objects under Canvas to clear historical elements
        System.Collections.Generic.List<Transform> childrenToDestroy = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            childrenToDestroy.Add(canvas.transform.GetChild(i));
        }
        foreach (Transform child in childrenToDestroy)
        {
            Object.DestroyImmediate(child.gameObject);
        }

        // EventSystem
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            eventSystem = esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // Rebuild Background Sprite (Neon wave background in world space)
        GameObject bgGO = GameObject.Find("GameBackground");
        if (bgGO == null) bgGO = new GameObject("GameBackground");
        bgGO.transform.SetParent(null);
        bgGO.transform.position = new Vector3(0f, 0f, 0f);

        SpriteRenderer bgSR = bgGO.GetComponent<SpriteRenderer>();
        if (bgSR == null) bgSR = bgGO.AddComponent<SpriteRenderer>();

        Sprite bgSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/game_background.png");
        if (bgSprite != null)
        {
            bgSR.sprite = bgSprite;
            bgSR.color = new Color(1f, 1f, 1f, 0.5f); // 50% opacity in world space
        }
        bgSR.sortingOrder = -10; // Draw behind gameplay dots
        bgGO.transform.localScale = Vector3.one;

        NeonBackgroundAnimator animator = bgGO.GetComponent<NeonBackgroundAnimator>();
        if (animator == null) bgGO.AddComponent<NeonBackgroundAnimator>();


        // Rebuild 1. Pause Button (Top Left)
        GameObject pauseGO = new GameObject("PauseButton");
        pauseGO.transform.SetParent(canvas.transform, false);
        Image pauseImg = pauseGO.AddComponent<Image>();
        pauseImg.color = new Color(0.07f, 0.06f, 0.11f, 0.75f); // Transparent dark background
        Button pauseButton = pauseGO.AddComponent<Button>();

        RectTransform pauseRect = pauseGO.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0f, 1f);
        pauseRect.anchorMax = new Vector2(0f, 1f);
        pauseRect.pivot = new Vector2(0f, 1f);
        pauseRect.anchoredPosition = new Vector2(50f, -60f);
        pauseRect.sizeDelta = new Vector2(110f, 110f);

        GameObject pauseTextGO = new GameObject("Text");
        pauseTextGO.transform.SetParent(pauseGO.transform, false);
        TextMeshProUGUI pauseText = pauseTextGO.AddComponent<TextMeshProUGUI>();
        pauseText.fontSize = 44;
        pauseText.text = "||";
        pauseText.color = Color.white;
        pauseText.alignment = TextAlignmentOptions.Center;
        RectTransform pauseTextRect = pauseTextGO.GetComponent<RectTransform>();
        pauseTextRect.anchorMin = Vector2.zero;
        pauseTextRect.anchorMax = Vector2.one;
        pauseTextRect.sizeDelta = Vector2.zero;

        // Rebuild 2. Center Scores Container
        GameObject centerScoresGO = new GameObject("CenterScoreContainer");
        centerScoresGO.transform.SetParent(canvas.transform, false);
        RectTransform centerScoresRect = centerScoresGO.AddComponent<RectTransform>();
        centerScoresRect.anchorMin = new Vector2(0.5f, 1f);
        centerScoresRect.anchorMax = new Vector2(0.5f, 1f);
        centerScoresRect.pivot = new Vector2(0.5f, 1f);
        centerScoresRect.anchoredPosition = new Vector2(0f, -40f);
        centerScoresRect.sizeDelta = new Vector2(500f, 220f);

        // Best Score Title row (Crown + BEST)
        GameObject bestTitleGO = new GameObject("BestTitleText");
        bestTitleGO.transform.SetParent(centerScoresGO.transform, false);
        TextMeshProUGUI bestTitle = bestTitleGO.AddComponent<TextMeshProUGUI>();
        bestTitle.fontSize = 24;
        bestTitle.text = "<sprite name=\"emoji_events\"> BEST";
        bestTitle.color = new Color(1f, 0.84f, 0f); // Bright Gold/Yellow
        bestTitle.alignment = TextAlignmentOptions.Center;
        RectTransform bestTitleRect = bestTitleGO.GetComponent<RectTransform>();
        bestTitleRect.anchorMin = new Vector2(0.5f, 1f);
        bestTitleRect.anchorMax = new Vector2(0.5f, 1f);
        bestTitleRect.pivot = new Vector2(0.5f, 1f);
        bestTitleRect.anchoredPosition = new Vector2(0f, 0f);
        bestTitleRect.sizeDelta = new Vector2(400f, 35f);

        // Best Score Value (Gold text)
        GameObject bestScoreGO = new GameObject("BestScoreText");
        bestScoreGO.transform.SetParent(centerScoresGO.transform, false);
        TextMeshProUGUI bestScoreText = bestScoreGO.AddComponent<TextMeshProUGUI>();
        bestScoreText.fontSize = 44;
        bestScoreText.fontStyle = FontStyles.Bold;
        bestScoreText.text = "0";
        bestScoreText.color = Color.white;
        bestScoreText.alignment = TextAlignmentOptions.Center;
        RectTransform bestScoreRect = bestScoreGO.GetComponent<RectTransform>();
        bestScoreRect.anchorMin = new Vector2(0.5f, 1f);
        bestScoreRect.anchorMax = new Vector2(0.5f, 1f);
        bestScoreRect.pivot = new Vector2(0.5f, 1f);
        bestScoreRect.anchoredPosition = new Vector2(0f, -30f);
        bestScoreRect.sizeDelta = new Vector2(400f, 50f);

        // Current Score Value (Huge white text)
        GameObject scoreGO = new GameObject("ScoreText");
        scoreGO.transform.SetParent(centerScoresGO.transform, false);
        TextMeshProUGUI scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
        scoreText.fontSize = 94;
        scoreText.fontStyle = FontStyles.Bold;
        scoreText.text = "0";
        scoreText.color = Color.white;
        scoreText.alignment = TextAlignmentOptions.Center;
        RectTransform scoreRect = scoreGO.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0.5f, 1f);
        scoreRect.anchorMax = new Vector2(0.5f, 1f);
        scoreRect.pivot = new Vector2(0.5f, 1f);
        scoreRect.anchoredPosition = new Vector2(0f, -110f);
        scoreRect.sizeDelta = new Vector2(480f, 110f);

        // Rebuild 3. Gold Coin Bar (Top Right)
        GameObject goldBarGO = new GameObject("GoldBarPanel");
        goldBarGO.transform.SetParent(canvas.transform, false);
        Image goldBarImg = goldBarGO.AddComponent<Image>();
        goldBarImg.color = new Color(0.07f, 0.06f, 0.11f, 0.75f); // Semi-transparent container background

        RectTransform goldBarRect = goldBarGO.GetComponent<RectTransform>();
        goldBarRect.anchorMin = new Vector2(1f, 1f);
        goldBarRect.anchorMax = new Vector2(1f, 1f);
        goldBarRect.pivot = new Vector2(1f, 1f);
        goldBarRect.anchoredPosition = new Vector2(-50f, -60f);
        goldBarRect.sizeDelta = new Vector2(340f, 90f);

        // Inner Coin Circle
        GameObject coinIconGO = new GameObject("CoinIcon");
        coinIconGO.transform.SetParent(goldBarGO.transform, false);
        Image coinImg = coinIconGO.AddComponent<Image>();
        coinImg.color = new Color(1f, 0.73f, 0f); // Gold Yellow fill
        RectTransform coinIconRect = coinIconGO.GetComponent<RectTransform>();
        coinIconRect.anchorMin = new Vector2(0f, 0.5f);
        coinIconRect.anchorMax = new Vector2(0f, 0.5f);
        coinIconRect.pivot = new Vector2(0f, 0.5f);
        coinIconRect.anchoredPosition = new Vector2(8f, 0f);
        coinIconRect.sizeDelta = new Vector2(74f, 74f);

        // Coin Icon symbol ($ sign)
        GameObject coinSymbolGO = new GameObject("Symbol");
        coinSymbolGO.transform.SetParent(coinIconGO.transform, false);
        TextMeshProUGUI coinSym = coinSymbolGO.AddComponent<TextMeshProUGUI>();
        coinSym.fontSize = 42;
        coinSym.fontStyle = FontStyles.Bold;
        coinSym.text = "$";
        coinSym.color = Color.white;
        coinSym.alignment = TextAlignmentOptions.Center;
        RectTransform coinSymRect = coinSymbolGO.GetComponent<RectTransform>();
        coinSymRect.anchorMin = Vector2.zero;
        coinSymRect.anchorMax = Vector2.one;
        coinSymRect.sizeDelta = Vector2.zero;

        // Gold Amount Text
        GameObject goldAmountGO = new GameObject("GoldAmountText");
        goldAmountGO.transform.SetParent(goldBarGO.transform, false);
        TextMeshProUGUI goldText = goldAmountGO.AddComponent<TextMeshProUGUI>();
        goldText.fontSize = 34;
        goldText.text = "12,450";
        goldText.color = Color.white;
        goldText.alignment = TextAlignmentOptions.Center;
        RectTransform goldAmountRect = goldAmountGO.GetComponent<RectTransform>();
        goldAmountRect.anchorMin = new Vector2(0.24f, 0f);
        goldAmountRect.anchorMax = new Vector2(0.76f, 1f);
        goldAmountRect.offsetMin = Vector2.zero;
        goldAmountRect.offsetMax = Vector2.zero;

        // Gold Add Plus Button (Rightmost inside container)
        GameObject addGoldGO = new GameObject("AddGoldButton");
        addGoldGO.transform.SetParent(goldBarGO.transform, false);
        Image addGoldImg = addGoldGO.AddComponent<Image>();
        addGoldImg.color = new Color(0.05f, 0.55f, 0.36f); // Slick emerald green button
        Button addGoldBtn = addGoldGO.AddComponent<Button>();
        RectTransform addGoldRect = addGoldGO.GetComponent<RectTransform>();
        addGoldRect.anchorMin = new Vector2(1f, 0.5f);
        addGoldRect.anchorMax = new Vector2(1f, 0.5f);
        addGoldRect.pivot = new Vector2(1f, 0.5f);
        addGoldRect.anchoredPosition = new Vector2(-8f, 0f);
        addGoldRect.sizeDelta = new Vector2(70f, 70f);

        GameObject addGoldPlusGO = new GameObject("Plus");
        addGoldPlusGO.transform.SetParent(addGoldGO.transform, false);
        TextMeshProUGUI plusText = addGoldPlusGO.AddComponent<TextMeshProUGUI>();
        plusText.fontSize = 40;
        plusText.text = "+";
        plusText.color = Color.white;
        plusText.alignment = TextAlignmentOptions.Center;
        RectTransform plusTextRect = addGoldPlusGO.GetComponent<RectTransform>();
        plusTextRect.anchorMin = Vector2.zero;
        plusTextRect.anchorMax = Vector2.one;
        plusTextRect.sizeDelta = Vector2.zero;

        // Rebuild 4. Circular Timer (Far Right under Top Row)
        GameObject timerCircleGO = new GameObject("TimerCircleContainer");
        timerCircleGO.transform.SetParent(canvas.transform, false);
        RectTransform timerCircleRect = timerCircleGO.AddComponent<RectTransform>();
        timerCircleRect.anchorMin = new Vector2(1f, 1f);
        timerCircleRect.anchorMax = new Vector2(1f, 1f);
        timerCircleRect.pivot = new Vector2(1f, 1f);
        timerCircleRect.anchoredPosition = new Vector2(-50f, -195f);
        timerCircleRect.sizeDelta = new Vector2(160f, 160f);

        // Circular background track (a very thin low-alpha ring as backdrop)
        GameObject timerTrackGO = new GameObject("Track");
        timerTrackGO.transform.SetParent(timerCircleGO.transform, false);
        Image trackImg = timerTrackGO.AddComponent<Image>();
        trackImg.color = new Color(0.12f, 0.08f, 0.22f, 0.35f); // Low alpha purple outline track
        Sprite timerPngSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/timer.png");
        if (timerPngSprite != null) trackImg.sprite = timerPngSprite;
        else if (circleSprite != null) trackImg.sprite = circleSprite;
        RectTransform timerTrackRect = timerTrackGO.GetComponent<RectTransform>();
        timerTrackRect.anchorMin = Vector2.zero;
        timerTrackRect.anchorMax = Vector2.one;
        timerTrackRect.sizeDelta = Vector2.zero;

        // Circular filled progress ring (Fades/depletes dynamically)
        GameObject timerRingGO = new GameObject("ProgressRing");
        timerRingGO.transform.SetParent(timerCircleGO.transform, false);
        Image timerProgressCircle = timerRingGO.AddComponent<Image>();
        timerProgressCircle.color = Color.white; // Keep white to let the baked neon gradient show through
        if (timerPngSprite != null) timerProgressCircle.sprite = timerPngSprite;
        else if (circleSprite != null) timerProgressCircle.sprite = circleSprite;
        timerProgressCircle.type = Image.Type.Filled;
        timerProgressCircle.fillMethod = Image.FillMethod.Radial360;
        timerProgressCircle.fillOrigin = (int)Image.Origin360.Top;
        timerProgressCircle.fillAmount = 1f;
        RectTransform timerRingRect = timerRingGO.GetComponent<RectTransform>();
        timerRingRect.anchorMin = new Vector2(0.05f, 0.05f);
        timerRingRect.anchorMax = new Vector2(0.95f, 0.95f);
        timerRingRect.sizeDelta = Vector2.zero;



        // Inside layout (TIME header & countdown value)
        GameObject timeHeaderGO = new GameObject("TimeHeader");
        timeHeaderGO.transform.SetParent(timerCircleGO.transform, false);
        TextMeshProUGUI timeHeader = timeHeaderGO.AddComponent<TextMeshProUGUI>();
        timeHeader.fontSize = 18;
        timeHeader.text = "TIME";
        timeHeader.color = new Color(0.9f, 0.1f, 0.85f); // Pink header
        timeHeader.alignment = TextAlignmentOptions.Center;
        RectTransform timeHeaderRect = timeHeaderGO.GetComponent<RectTransform>();
        timeHeaderRect.anchorMin = new Vector2(0.5f, 0.65f);
        timeHeaderRect.anchorMax = new Vector2(0.5f, 0.65f);
        timeHeaderRect.pivot = new Vector2(0.5f, 0.5f);
        timeHeaderRect.sizeDelta = new Vector2(120f, 30f);

        GameObject timerTextGO = new GameObject("TimerText");
        timerTextGO.transform.SetParent(timerCircleGO.transform, false);
        TextMeshProUGUI timerText = timerTextGO.AddComponent<TextMeshProUGUI>();
        timerText.fontSize = 38;
        timerText.fontStyle = FontStyles.Bold;
        timerText.text = "<b>1:28</b>";
        timerText.color = Color.white;
        timerText.alignment = TextAlignmentOptions.Center;
        timerText.overflowMode = TextOverflowModes.Overflow; // Prevent clip
        RectTransform timerTextRect = timerTextGO.GetComponent<RectTransform>();
        timerTextRect.anchorMin = new Vector2(0.5f, 0.4f);
        timerTextRect.anchorMax = new Vector2(0.5f, 0.4f);
        timerTextRect.pivot = new Vector2(0.5f, 0.5f);
        timerTextRect.sizeDelta = new Vector2(120f, 40f);

        // Screen Flash Image
        Image flashImageComp = null;
        GameObject flashGO = new GameObject("FlashScreenImage");
        flashGO.transform.SetParent(canvas.transform, false);
        flashImageComp = flashGO.AddComponent<Image>();
        flashImageComp.color = Color.clear;
        flashImageComp.raycastTarget = false;
        RectTransform flashRect = flashGO.GetComponent<RectTransform>();
        flashRect.anchorMin = Vector2.zero;
        flashRect.anchorMax = Vector2.one;
        flashRect.sizeDelta = Vector2.zero;

        // Combo Feedback Panel (Center)
        GameObject comboContainer = new GameObject("ComboContainer");
        comboContainer.transform.SetParent(canvas.transform, false);
        RectTransform comboContainerRect = comboContainer.AddComponent<RectTransform>();
        comboContainerRect.anchorMin = new Vector2(0.5f, 0.5f);
        comboContainerRect.anchorMax = new Vector2(0.5f, 0.5f);
        comboContainerRect.pivot = new Vector2(0.5f, 0.5f);
        comboContainerRect.anchoredPosition = new Vector2(0f, 100f);
        comboContainerRect.sizeDelta = new Vector2(600f, 250f);

        GameObject comboTitleGO = new GameObject("ComboTitleText");
        comboTitleGO.transform.SetParent(comboContainer.transform, false);
        RectTransform comboTitleRect = comboTitleGO.AddComponent<RectTransform>();
        TextMeshProUGUI comboTitleText = comboTitleGO.AddComponent<TextMeshProUGUI>();
        comboTitleText.fontSize = 38;
        comboTitleText.text = "COMBO";
        comboTitleText.color = new Color(1f, 0.64f, 0f);
        comboTitleText.alignment = TextAlignmentOptions.Center;
        comboTitleText.enableWordWrapping = false;
        comboTitleRect.anchorMin = new Vector2(0.5f, 0.8f);
        comboTitleRect.anchorMax = new Vector2(0.5f, 0.8f);
        comboTitleRect.pivot = new Vector2(0.5f, 0.5f);
        comboTitleRect.sizeDelta = new Vector2(500f, 50f);

        GameObject comboMultiplierGO = new GameObject("ComboMultiplierText");
        comboMultiplierGO.transform.SetParent(comboContainer.transform, false);
        RectTransform comboMultRect = comboMultiplierGO.AddComponent<RectTransform>();
        TextMeshProUGUI comboMultiplierText = comboMultiplierGO.AddComponent<TextMeshProUGUI>();
        comboMultiplierText.fontSize = 86;
        comboMultiplierText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        comboMultiplierText.text = "x4";
        comboMultiplierText.color = new Color(1f, 0.84f, 0f);
        comboMultiplierText.alignment = TextAlignmentOptions.Center;
        comboMultiplierText.enableWordWrapping = false;
        comboMultiplierText.enableAutoSizing = true;
        comboMultiplierText.fontSizeMin = 24;
        comboMultiplierText.fontSizeMax = 86;
        comboMultRect.anchorMin = new Vector2(0.5f, 0.45f);
        comboMultRect.anchorMax = new Vector2(0.5f, 0.45f);
        comboMultRect.pivot = new Vector2(0.5f, 0.5f);
        comboMultRect.sizeDelta = new Vector2(500f, 100f);

        GameObject comboChainGO = new GameObject("ComboChainText");
        comboChainGO.transform.SetParent(comboContainer.transform, false);
        RectTransform comboChainRect = comboChainGO.AddComponent<RectTransform>();
        TextMeshProUGUI comboChainText = comboChainGO.AddComponent<TextMeshProUGUI>();
        comboChainText.fontSize = 28;
        comboChainText.text = "12 CHAIN!";
        comboChainText.color = new Color(0.4f, 0.95f, 1f);
        comboChainText.alignment = TextAlignmentOptions.Center;
        comboChainText.enableWordWrapping = false;
        comboChainRect.anchorMin = new Vector2(0.5f, 0.15f);
        comboChainRect.anchorMax = new Vector2(0.5f, 0.15f);
        comboChainRect.pivot = new Vector2(0.5f, 0.5f);
        comboChainRect.sizeDelta = new Vector2(500f, 40f);

        comboContainer.SetActive(false);

        // Danger Zone Banner (Danger Strip)
        GameObject dangerBanner = new GameObject("DangerZoneBanner");
        dangerBanner.transform.SetParent(canvas.transform, false);
        Image dangerImg = dangerBanner.AddComponent<Image>();
        dangerImg.color = new Color(0.85f, 0.05f, 0.05f, 0.85f); // Red Banner Alert
        RectTransform dangerBannerRect = dangerBanner.GetComponent<RectTransform>();
        dangerBannerRect.anchorMin = new Vector2(0f, 0.35f);
        dangerBannerRect.anchorMax = new Vector2(1f, 0.35f);
        dangerBannerRect.pivot = new Vector2(0.5f, 0.5f);
        dangerBannerRect.anchoredPosition = new Vector2(0f, 0f);
        dangerBannerRect.sizeDelta = new Vector2(0f, 90f);

        GameObject dangerTextGO = new GameObject("DangerText");
        dangerTextGO.transform.SetParent(dangerBanner.transform, false);
        TextMeshProUGUI dangerText = dangerTextGO.AddComponent<TextMeshProUGUI>();
        dangerText.fontSize = 38;
        dangerText.fontStyle = FontStyles.Bold;
        dangerText.text = "▲ DANGER ZONE ▲";
        dangerText.color = Color.white;
        dangerText.alignment = TextAlignmentOptions.Center;
        RectTransform dangerTextRect = dangerTextGO.GetComponent<RectTransform>();
        dangerTextRect.anchorMin = Vector2.zero;
        dangerTextRect.anchorMax = Vector2.one;
        dangerTextRect.sizeDelta = Vector2.zero;

        dangerBanner.SetActive(false);

        // Rebuild 5. Fever Panel (Left Side under Pause)
        GameObject feverPanel = new GameObject("FeverPanelContainer");
        feverPanel.transform.SetParent(canvas.transform, false);
        RectTransform feverPanelRect = feverPanel.AddComponent<RectTransform>();
        feverPanelRect.anchorMin = new Vector2(0f, 1f);
        feverPanelRect.anchorMax = new Vector2(0f, 1f);
        feverPanelRect.pivot = new Vector2(0f, 1f);
        feverPanelRect.anchoredPosition = new Vector2(50f, -195f);
        feverPanelRect.sizeDelta = new Vector2(160f, 220f);

        // Fever circle/glow back
        GameObject feverIconGO = new GameObject("FeverIconBack");
        feverIconGO.transform.SetParent(feverPanel.transform, false);
        Image feverImgComp = feverIconGO.AddComponent<Image>();
        feverImgComp.color = Color.white; // Render bolt.png as-is
        Sprite boltPngSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Textures/bolt.png");
        if (boltPngSprite != null) feverImgComp.sprite = boltPngSprite;
        else if (circleSprite != null) feverImgComp.sprite = circleSprite;
        RectTransform feverIconRect = feverIconGO.GetComponent<RectTransform>();
        feverIconRect.anchorMin = new Vector2(0.5f, 0.65f);
        feverIconRect.anchorMax = new Vector2(0.5f, 0.65f);
        feverIconRect.pivot = new Vector2(0.5f, 0.5f);
        feverIconRect.sizeDelta = new Vector2(110f, 110f);

        // Fever subtitle
        GameObject feverTitleGO = new GameObject("FeverText");
        feverTitleGO.transform.SetParent(feverPanel.transform, false);
        TextMeshProUGUI feverTitle = feverTitleGO.AddComponent<TextMeshProUGUI>();
        feverTitle.fontSize = 22;
        feverTitle.fontStyle = FontStyles.Bold | FontStyles.Italic;
        feverTitle.text = "FEVER";
        feverTitle.color = new Color(1f, 0.25f, 0.85f); // Pink/Fuchsia matching the guide
        feverTitle.alignment = TextAlignmentOptions.Center;
        RectTransform feverTitleRect = feverTitleGO.GetComponent<RectTransform>();
        feverTitleRect.anchorMin = new Vector2(0.5f, 0.26f);
        feverTitleRect.anchorMax = new Vector2(0.5f, 0.26f);
        feverTitleRect.pivot = new Vector2(0.5f, 0.5f);
        feverTitleRect.sizeDelta = new Vector2(160f, 35f);

        // Fever horizontal progress bar
        GameObject feverTrackGO = new GameObject("ProgressBarTrack");
        feverTrackGO.transform.SetParent(feverPanel.transform, false);
        Image fTrackImg = feverTrackGO.AddComponent<Image>();
        fTrackImg.color = new Color(0.08f, 0.07f, 0.12f, 0.9f);
        if (circleSprite != null) fTrackImg.sprite = circleSprite; // soft boundaries
        RectTransform fTrackRect = feverTrackGO.GetComponent<RectTransform>();
        fTrackRect.anchorMin = new Vector2(0.05f, 0.05f);
        fTrackRect.anchorMax = new Vector2(0.95f, 0.05f);
        fTrackRect.pivot = new Vector2(0.5f, 0.5f);
        fTrackRect.anchoredPosition = new Vector2(0f, 0f);
        fTrackRect.sizeDelta = new Vector2(0f, 20f);

        GameObject feverBarGO = new GameObject("ProgressBarFill");
        feverBarGO.transform.SetParent(feverTrackGO.transform, false);
        Image feverProgressBar = feverBarGO.AddComponent<Image>();
        feverProgressBar.color = new Color(1f, 0.25f, 0.85f); // Gradient Pink/Fuchsia
        if (circleSprite != null) feverProgressBar.sprite = circleSprite;
        feverProgressBar.type = Image.Type.Filled;
        feverProgressBar.fillMethod = Image.FillMethod.Horizontal;
        feverProgressBar.fillAmount = 0f;
        RectTransform feverBarRect = feverBarGO.GetComponent<RectTransform>();
        feverBarRect.anchorMin = Vector2.zero;
        feverBarRect.anchorMax = Vector2.one;
        feverBarRect.sizeDelta = Vector2.zero;
        feverBarRect.sizeDelta = Vector2.zero;

        feverPanel.SetActive(true);

        // Bottom HUD Card 1: Goal
        GameObject goalCardGO = new GameObject("GoalHUDCard");
        goalCardGO.transform.SetParent(canvas.transform, false);
        Image goalCardImg = goalCardGO.AddComponent<Image>();
        goalCardImg.color = new Color(0.07f, 0.06f, 0.11f, 0.75f);
        RectTransform goalCardRect = goalCardGO.GetComponent<RectTransform>();
        goalCardRect.anchorMin = new Vector2(0.12f, 0.04f);
        goalCardRect.anchorMax = new Vector2(0.45f, 0.04f);
        goalCardRect.pivot = new Vector2(0.5f, 0f);
        goalCardRect.anchoredPosition = new Vector2(0f, 0f);
        goalCardRect.sizeDelta = new Vector2(0f, 130f);

        GameObject goalHeaderGO = new GameObject("Header");
        goalHeaderGO.transform.SetParent(goalCardGO.transform, false);
        TextMeshProUGUI goalHeader = goalHeaderGO.AddComponent<TextMeshProUGUI>();
        goalHeader.fontSize = 24;
        goalHeader.text = "HEDEF";
        goalHeader.color = new Color(231f / 255f, 223f / 255f, 240f / 255f, 0.45f);
        goalHeader.alignment = TextAlignmentOptions.Center;
        RectTransform goalHeaderRect = goalHeaderGO.GetComponent<RectTransform>();
        goalHeaderRect.anchorMin = new Vector2(0.5f, 0.75f);
        goalHeaderRect.anchorMax = new Vector2(0.5f, 0.75f);
        goalHeaderRect.pivot = new Vector2(0.5f, 0.5f);
        goalHeaderRect.sizeDelta = new Vector2(300f, 40f);

        GameObject goalTextGO = new GameObject("GoalText");
        goalTextGO.transform.SetParent(goalCardGO.transform, false);
        TextMeshProUGUI goalText = goalTextGO.AddComponent<TextMeshProUGUI>();
        goalText.fontSize = 44;
        goalText.fontStyle = FontStyles.Bold;
        goalText.text = "HEDEF: 1,000";
        goalText.color = new Color(1f, 0.62f, 0f); // Goal Neon Orange
        goalText.alignment = TextAlignmentOptions.Center;
        RectTransform goalTextRect = goalTextGO.GetComponent<RectTransform>();
        goalTextRect.anchorMin = new Vector2(0.5f, 0.35f);
        goalTextRect.anchorMax = new Vector2(0.5f, 0.35f);
        goalTextRect.pivot = new Vector2(0.5f, 0.5f);
        goalTextRect.sizeDelta = new Vector2(300f, 60f);

        // Bottom HUD Card 2: Level
        GameObject levelCardGO = new GameObject("LevelHUDCard");
        levelCardGO.transform.SetParent(canvas.transform, false);
        Image levelCardImg = levelCardGO.AddComponent<Image>();
        levelCardImg.color = new Color(0.07f, 0.06f, 0.11f, 0.75f);
        RectTransform levelCardRect = levelCardGO.GetComponent<RectTransform>();
        levelCardRect.anchorMin = new Vector2(0.55f, 0.04f);
        levelCardRect.anchorMax = new Vector2(0.88f, 0.04f);
        levelCardRect.pivot = new Vector2(0.5f, 0f);
        levelCardRect.anchoredPosition = new Vector2(0f, 0f);
        levelCardRect.sizeDelta = new Vector2(0f, 130f);

        GameObject levelHeaderGO = new GameObject("Header");
        levelHeaderGO.transform.SetParent(levelCardGO.transform, false);
        TextMeshProUGUI levelHeader = levelHeaderGO.AddComponent<TextMeshProUGUI>();
        levelHeader.fontSize = 24;
        levelHeader.text = "LVL";
        levelHeader.color = new Color(231f / 255f, 223f / 255f, 240f / 255f, 0.45f);
        levelHeader.alignment = TextAlignmentOptions.Center;
        RectTransform levelHeaderRect = levelHeaderGO.GetComponent<RectTransform>();
        levelHeaderRect.anchorMin = new Vector2(0.5f, 0.75f);
        levelHeaderRect.anchorMax = new Vector2(0.5f, 0.75f);
        levelHeaderRect.pivot = new Vector2(0.5f, 0.5f);
        levelHeaderRect.sizeDelta = new Vector2(300f, 40f);

        GameObject levelTextGO = new GameObject("LevelText");
        levelTextGO.transform.SetParent(levelCardGO.transform, false);
        TextMeshProUGUI levelText = levelTextGO.AddComponent<TextMeshProUGUI>();
        levelText.fontSize = 44;
        levelText.fontStyle = FontStyles.Bold;
        levelText.text = "SEVİYE: 1";
        levelText.color = new Color(0f, 0.96f, 1f); // Level neon cyan
        levelText.alignment = TextAlignmentOptions.Center;
        RectTransform levelTextRect = levelTextGO.GetComponent<RectTransform>();
        levelTextRect.anchorMin = new Vector2(0.5f, 0.35f);
        levelTextRect.anchorMax = new Vector2(0.5f, 0.35f);
        levelTextRect.pivot = new Vector2(0.5f, 0.5f);
        levelTextRect.sizeDelta = new Vector2(300f, 60f);

        // GameOver Panel
        GameObject gameOverPanel = null;
        gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvas.transform, false);

        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0.05f, 0.04f, 0.08f, 0.96f); // Slick dark glassmorphic background

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        // 1. GameOver Header Text
        GameObject goTextGO = new GameObject("GameOverText");
        goTextGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI goText = goTextGO.AddComponent<TextMeshProUGUI>();
        goText.fontSize = 80;
        goText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        goText.text = "OYUN BİTTİ";
        goText.color = new Color(1f, 0.2f, 0.2f);
        goText.alignment = TextAlignmentOptions.Center;

        RectTransform goRect = goTextGO.GetComponent<RectTransform>();
        goRect.anchorMin = new Vector2(0.5f, 0.5f);
        goRect.anchorMax = new Vector2(0.5f, 0.5f);
        goRect.pivot = new Vector2(0.5f, 0.5f);
        goRect.anchoredPosition = new Vector2(0f, 320f);
        goRect.sizeDelta = new Vector2(800f, 120f);

        // 2. High Score Banner Text
        GameObject hsTextGO = new GameObject("HighScoreTextVal");
        hsTextGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI hsText = hsTextGO.AddComponent<TextMeshProUGUI>();
        hsText.fontSize = 46;
        hsText.text = "EN YÜKSEK SKOR: 12,450";
        hsText.color = Color.white;
        hsText.alignment = TextAlignmentOptions.Center;

        RectTransform hsRect = hsTextGO.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(0.5f, 0.5f);
        hsRect.anchorMax = new Vector2(0.5f, 0.5f);
        hsRect.pivot = new Vector2(0.5f, 0.5f);
        hsRect.anchoredPosition = new Vector2(0f, 180f);
        hsRect.sizeDelta = new Vector2(800f, 80f);

        // 3. Final score label
        GameObject fsTextGO = new GameObject("FinalScoreLabelText");
        fsTextGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI fsLabelText = fsTextGO.AddComponent<TextMeshProUGUI>();
        fsLabelText.fontSize = 32;
        fsLabelText.text = "ALINAN SKOR";
        fsLabelText.color = new Color(0.6f, 0.6f, 0.7f);
        fsLabelText.alignment = TextAlignmentOptions.Center;

        RectTransform fsLabelRect = fsTextGO.GetComponent<RectTransform>();
        fsLabelRect.anchorMin = new Vector2(0.5f, 0.5f);
        fsLabelRect.anchorMax = new Vector2(0.5f, 0.5f);
        fsLabelRect.pivot = new Vector2(0.5f, 0.5f);
        fsLabelRect.anchoredPosition = new Vector2(0f, 60f);
        fsLabelRect.sizeDelta = new Vector2(800f, 50f);

        // 4. Final score value
        GameObject fsValTextGO = new GameObject("ScoreTextVal");
        fsValTextGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI scoreTextVal = fsValTextGO.AddComponent<TextMeshProUGUI>();
        scoreTextVal.fontSize = 94;
        scoreTextVal.fontStyle = FontStyles.Bold;
        scoreTextVal.text = "4,210";
        scoreTextVal.color = Color.white;
        scoreTextVal.alignment = TextAlignmentOptions.Center;

        RectTransform fsValRect = fsValTextGO.GetComponent<RectTransform>();
        fsValRect.anchorMin = new Vector2(0.5f, 0.5f);
        fsValRect.anchorMax = new Vector2(0.5f, 0.5f);
        fsValRect.pivot = new Vector2(0.5f, 0.5f);
        fsValRect.anchoredPosition = new Vector2(0f, -20f);
        fsValRect.sizeDelta = new Vector2(800f, 120f);

        // 5. Improvement progress label
        GameObject impTextGO = new GameObject("ImprovementTextVal");
        impTextGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI impText = impTextGO.AddComponent<TextMeshProUGUI>();
        impText.fontSize = 32;
        impText.text = "+150 (SKORUNU GELİŞTİRDİN!)";
        impText.color = new Color(0.1f, 0.9f, 0.5f); // Neon green
        impText.alignment = TextAlignmentOptions.Center;

        RectTransform impRect = impTextGO.GetComponent<RectTransform>();
        impRect.anchorMin = new Vector2(0.5f, 0.5f);
        impRect.anchorMax = new Vector2(0.5f, 0.5f);
        impRect.pivot = new Vector2(0.5f, 0.5f);
        impRect.anchoredPosition = new Vector2(0f, -110f);
        impRect.sizeDelta = new Vector2(800f, 60f);

        // 6. Restart Button
        GameObject restartButtonGO = new GameObject("RestartButton");
        restartButtonGO.transform.SetParent(gameOverPanel.transform, false);
        Image restartBtnImg = restartButtonGO.AddComponent<Image>();
        restartBtnImg.color = new Color(0.9f, 0.1f, 0.85f); // Slick Neon pink button
        Button restartButton = restartButtonGO.AddComponent<Button>();

        RectTransform restartRect = restartButtonGO.GetComponent<RectTransform>();
        restartRect.anchorMin = new Vector2(0.5f, 0.5f);
        restartRect.anchorMax = new Vector2(0.5f, 0.5f);
        restartRect.pivot = new Vector2(0.5f, 0.5f);
        restartRect.anchoredPosition = new Vector2(0f, -240f);
        restartRect.sizeDelta = new Vector2(380f, 110f);

        GameObject restartTextGO = new GameObject("Text");
        restartTextGO.transform.SetParent(restartButtonGO.transform, false);
        TextMeshProUGUI restartText = restartTextGO.AddComponent<TextMeshProUGUI>();
        restartText.fontSize = 40;
        restartText.fontStyle = FontStyles.Bold;
        restartText.text = "TEKRAR OYNA";
        restartText.color = Color.white;
        restartText.alignment = TextAlignmentOptions.Center;
        RectTransform restartTextRect = restartTextGO.GetComponent<RectTransform>();
        restartTextRect.anchorMin = Vector2.zero;
        restartTextRect.anchorMax = Vector2.one;
        restartTextRect.sizeDelta = Vector2.zero;

        gameOverPanel.SetActive(false);

        // Level Up Overlay
        GameObject levelUpOverlay = new GameObject("LevelUpOverlay");
        levelUpOverlay.transform.SetParent(canvas.transform, false);
        Image levelUpBg = levelUpOverlay.AddComponent<Image>();
        levelUpBg.color = new Color(0.05f, 0.04f, 0.08f, 0.98f);
        RectTransform levelUpOverlayRect = levelUpOverlay.GetComponent<RectTransform>();
        levelUpOverlayRect.anchorMin = Vector2.zero;
        levelUpOverlayRect.anchorMax = Vector2.one;
        levelUpOverlayRect.sizeDelta = Vector2.zero;

        GameObject levelUpTitleGO = new GameObject("LevelUpTitleText");
        levelUpTitleGO.transform.SetParent(levelUpOverlay.transform, false);
        TextMeshProUGUI levelUpTitle = levelUpTitleGO.AddComponent<TextMeshProUGUI>();
        levelUpTitle.fontSize = 72;
        levelUpTitle.fontStyle = FontStyles.Bold | FontStyles.Italic;
        levelUpTitle.text = "SEVİYE ATLANDI!";
        levelUpTitle.color = new Color(1f, 0.84f, 0f);
        levelUpTitle.alignment = TextAlignmentOptions.Center;
        RectTransform levelUpTitleRect = levelUpTitleGO.GetComponent<RectTransform>();
        levelUpTitleRect.anchorMin = new Vector2(0.5f, 0.5f);
        levelUpTitleRect.anchorMax = new Vector2(0.5f, 0.5f);
        levelUpTitleRect.pivot = new Vector2(0.5f, 0.5f);
        levelUpTitleRect.anchoredPosition = new Vector2(0f, 300f);
        levelUpTitleRect.sizeDelta = new Vector2(800f, 100f);

        GameObject unlockedLevelTextGO = new GameObject("UnlockedLevelText");
        unlockedLevelTextGO.transform.SetParent(levelUpOverlay.transform, false);
        TextMeshProUGUI unlockedLevelTitleText = unlockedLevelTextGO.AddComponent<TextMeshProUGUI>();
        unlockedLevelTitleText.fontSize = 44;
        unlockedLevelTitleText.text = "SEVİYE 2";
        unlockedLevelTitleText.color = new Color(0f, 0.96f, 1f);
        unlockedLevelTitleText.alignment = TextAlignmentOptions.Center;
        RectTransform unlockedLevelRect = unlockedLevelTextGO.GetComponent<RectTransform>();
        unlockedLevelRect.anchorMin = new Vector2(0.5f, 0.5f);
        unlockedLevelRect.anchorMax = new Vector2(0.5f, 0.5f);
        unlockedLevelRect.pivot = new Vector2(0.5f, 0.5f);
        unlockedLevelRect.anchoredPosition = new Vector2(0f, 200f);
        unlockedLevelRect.sizeDelta = new Vector2(800f, 60f);

        GameObject levelPreviewGO = new GameObject("LevelPreviewText");
        levelPreviewGO.transform.SetParent(levelUpOverlay.transform, false);
        TextMeshProUGUI levelPreviewText = levelPreviewGO.AddComponent<TextMeshProUGUI>();
        levelPreviewText.fontSize = 32;
        levelPreviewText.text = "Preview Text";
        levelPreviewText.color = Color.white;
        levelPreviewText.alignment = TextAlignmentOptions.Center;
        RectTransform levelPreviewRect = levelPreviewGO.GetComponent<RectTransform>();
        levelPreviewRect.anchorMin = new Vector2(0.5f, 0.5f);
        levelPreviewRect.anchorMax = new Vector2(0.5f, 0.5f);
        levelPreviewRect.pivot = new Vector2(0.5f, 0.5f);
        levelPreviewRect.anchoredPosition = new Vector2(0f, 0f);
        levelPreviewRect.sizeDelta = new Vector2(800f, 180f);

        GameObject continueButtonGO = new GameObject("ContinueButton");
        continueButtonGO.transform.SetParent(levelUpOverlay.transform, false);
        Image continueBtnImg = continueButtonGO.AddComponent<Image>();
        continueBtnImg.color = new Color(0.05f, 0.55f, 0.36f);
        Button levelUpContinueButton = continueButtonGO.AddComponent<Button>();
        RectTransform continueBtnRect = continueButtonGO.GetComponent<RectTransform>();
        continueBtnRect.anchorMin = new Vector2(0.5f, 0.5f);
        continueBtnRect.anchorMax = new Vector2(0.5f, 0.5f);
        continueBtnRect.pivot = new Vector2(0.5f, 0.5f);
        continueBtnRect.anchoredPosition = new Vector2(0f, -220f);
        continueBtnRect.sizeDelta = new Vector2(360f, 100f);

        GameObject continueTextGO = new GameObject("Text");
        continueTextGO.transform.SetParent(continueButtonGO.transform, false);
        TextMeshProUGUI continueText = continueTextGO.AddComponent<TextMeshProUGUI>();
        continueText.fontSize = 38;
        continueText.fontStyle = FontStyles.Bold;
        continueText.text = "DEVAM ET";
        continueText.color = Color.white;
        continueText.alignment = TextAlignmentOptions.Center;
        RectTransform continueTextRect = continueTextGO.GetComponent<RectTransform>();
        continueTextRect.anchorMin = Vector2.zero;
        continueTextRect.anchorMax = Vector2.one;
        continueTextRect.sizeDelta = Vector2.zero;

        levelUpOverlay.SetActive(false);

        // 9. Setup Managers
        ScoreManager scoreManager = Object.FindAnyObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            GameObject smGO = new GameObject("ScoreManager");
            scoreManager = smGO.AddComponent<ScoreManager>();
        }

        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject uiManGO = new GameObject("UIManager");
            uiManager = uiManGO.AddComponent<UIManager>();
        }

        // Clean up UI Document from UIManager if it exists
        var uDocComp = uiManager.GetComponent<UnityEngine.UIElements.UIDocument>();
        if (uDocComp != null)
        {
            Object.DestroyImmediate(uDocComp);
        }

        SerializedObject serializedUI = new SerializedObject(uiManager);
        serializedUI.FindProperty("scoreText").objectReferenceValue = scoreText;
        serializedUI.FindProperty("bestScoreText").objectReferenceValue = bestScoreText;
        serializedUI.FindProperty("timerText").objectReferenceValue = timerText;
        serializedUI.FindProperty("timerProgressCircle").objectReferenceValue = timerProgressCircle;
        serializedUI.FindProperty("goldText").objectReferenceValue = goldText;
        serializedUI.FindProperty("pauseButton").objectReferenceValue = pauseButton;
        serializedUI.FindProperty("feverPanel").objectReferenceValue = feverPanel;
        serializedUI.FindProperty("feverProgressBar").objectReferenceValue = feverProgressBar;
        serializedUI.FindProperty("feverTimeLeftText").objectReferenceValue = null; // Do not overwrite main timerText
        serializedUI.FindProperty("comboContainer").objectReferenceValue = comboContainer;
        serializedUI.FindProperty("comboTitleText").objectReferenceValue = comboTitleText;
        serializedUI.FindProperty("comboMultiplierText").objectReferenceValue = comboMultiplierText;
        serializedUI.FindProperty("comboChainText").objectReferenceValue = comboChainText;
        serializedUI.FindProperty("dangerBanner").objectReferenceValue = dangerBanner;
        serializedUI.FindProperty("dangerText").objectReferenceValue = dangerText;
        serializedUI.FindProperty("flashScreenImage").objectReferenceValue = flashImageComp;
        serializedUI.FindProperty("goalText").objectReferenceValue = goalText;
        serializedUI.FindProperty("levelText").objectReferenceValue = levelText;
        serializedUI.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        serializedUI.FindProperty("levelUpOverlay").objectReferenceValue = levelUpOverlay;
        serializedUI.FindProperty("unlockedLevelTitleText").objectReferenceValue = unlockedLevelTitleText;
        serializedUI.FindProperty("levelPreviewText").objectReferenceValue = levelPreviewText;
        serializedUI.FindProperty("levelUpContinueButton").objectReferenceValue = levelUpContinueButton;
        serializedUI.ApplyModifiedProperties();

        GameManager manager = Object.FindAnyObjectByType<GameManager>();
        if (manager == null)
        {
            GameObject managerGO = new GameObject("GameManager");
            manager = managerGO.AddComponent<GameManager>();
        }

        SerializedObject serializedManager = new SerializedObject(manager);
        serializedManager.FindProperty("restartButton").objectReferenceValue = restartButton;
        serializedManager.ApplyModifiedProperties();

        // Setup ComboManager with explosion particle prefab
        ComboManager comboManager = Object.FindAnyObjectByType<ComboManager>();
        if (comboManager == null)
        {
            GameObject comboGO = new GameObject("ComboManager");
            comboManager = comboGO.AddComponent<ComboManager>();
        }

        SerializedObject serializedCombo = new SerializedObject(comboManager);
        serializedCombo.FindProperty("particlePrefab").objectReferenceValue = explosionPrefab;
        serializedCombo.ApplyModifiedProperties();

        // Save Scene & Assets
        AssetDatabase.SaveAssets();
        EditorUtility.SetDirty(cam);
        if (prefab != null) EditorUtility.SetDirty(prefab);
        if (explosionPrefab != null) EditorUtility.SetDirty(explosionPrefab);
        EditorUtility.SetDirty(colorManager);
        EditorUtility.SetDirty(difficultyManager);
        EditorUtility.SetDirty(audioManager);
        EditorUtility.SetDirty(objectPool);
        if (ground != null) EditorUtility.SetDirty(ground);
        EditorUtility.SetDirty(spawner);
        EditorUtility.SetDirty(chainController);
        if (inputProcessor != null) EditorUtility.SetDirty(inputProcessor);
        EditorUtility.SetDirty(canvas);
        EditorUtility.SetDirty(scoreManager);
        EditorUtility.SetDirty(uiManager);
        EditorUtility.SetDirty(manager);
        EditorUtility.SetDirty(comboManager);
        
        Debug.Log("Dot Chain Rush game setup complete with updated falling physics prefab, ObjectPool, Score, Combo & Audio Managers!");
        Debug.Log($"[Setup Diagnostic] Canvas children: {canvas.transform.childCount}, Canvas active: {canvas.gameObject.activeSelf}, RenderMode: {canvas.renderMode}");

        // Save the active scene to persist all hierarchy changes
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
        Debug.Log("[Setup] Scene saved successfully!");
    }

    private static void UpdateCirclePrefab(string prefabPath, Sprite circleSprite, Sprite highlightSprite, Sprite specialRingSprite, Sprite smokeGlowSprite, Sprite rainbowSprite, Sprite obstacleSprite, PhysicsMaterial2D physicsMat)
    {
        GameObject tempGO;
        bool exists = System.IO.File.Exists(prefabPath);
        if (exists)
        {
            GameObject original = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            tempGO = PrefabUtility.InstantiatePrefab(original) as GameObject;
        }
        else
        {
            tempGO = new GameObject("CirclePrefabTemp");
        }

        SpriteRenderer parentSR = tempGO.GetComponent<SpriteRenderer>();
        if (parentSR != null) Object.DestroyImmediate(parentSR);

        Transform visualCoreTransform = tempGO.transform.Find("VisualCore");
        GameObject visualCoreGO;
        if (visualCoreTransform == null)
        {
            visualCoreGO = new GameObject("VisualCore");
            visualCoreGO.transform.SetParent(tempGO.transform, false);
        }
        else
        {
            visualCoreGO = visualCoreTransform.gameObject;
        }

        SpriteRenderer sr = visualCoreGO.GetComponent<SpriteRenderer>();
        if (sr == null) sr = visualCoreGO.AddComponent<SpriteRenderer>();
        sr.sprite = circleSprite;
        sr.sortingOrder = 0; // Base circle is in front of smoke, behind highlight
        visualCoreGO.transform.localPosition = Vector3.zero;
        visualCoreGO.transform.localScale = Vector3.one;

        CircleCollider2D col = tempGO.GetComponent<CircleCollider2D>();
        if (col == null) col = tempGO.AddComponent<CircleCollider2D>();
        col.radius = 0.5f;
        col.isTrigger = false;
        col.sharedMaterial = physicsMat;

        Rigidbody2D rb = tempGO.GetComponent<Rigidbody2D>();
        if (rb == null) rb = tempGO.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; // Prevents tunneling through walls/floor

        Dot dot = tempGO.GetComponent<Dot>();
        if (dot == null) dot = tempGO.AddComponent<Dot>();

        SerializedObject soDot = new SerializedObject(dot);
        soDot.FindProperty("normalSprite").objectReferenceValue = circleSprite;
        soDot.FindProperty("specialSprite").objectReferenceValue = rainbowSprite;
        soDot.FindProperty("obstacleSprite").objectReferenceValue = obstacleSprite;
        soDot.ApplyModifiedProperties();

        // Create the highlight child using the highlightSprite (3D specular and shadow overlay)
        Transform highlightTransform = tempGO.transform.Find("Highlight");
        GameObject highlightGO;
        if (highlightTransform == null)
        {
            highlightGO = new GameObject("Highlight");
            highlightGO.transform.SetParent(tempGO.transform, false);
        }
        else
        {
            highlightGO = highlightTransform.gameObject;
        }

        SpriteRenderer highlightSR = highlightGO.GetComponent<SpriteRenderer>();
        if (highlightSR == null) highlightSR = highlightGO.AddComponent<SpriteRenderer>();
        highlightSR.sprite = highlightSprite;
        highlightSR.color = Color.white; // Render white highlights and black shadows as-is (untinted)
        highlightSR.sortingOrder = 1; // Render on top of base circle

        // Reset transform to cover the parent exactly
        highlightGO.transform.localPosition = Vector3.zero;
        highlightGO.transform.localScale = Vector3.one;

        // Create/Update the Smoke child using smokeGlowSprite (organic smoke halo outline rendered behind the bubble)
        Transform smokeTransform = tempGO.transform.Find("Smoke");
        GameObject smokeGO;
        if (smokeTransform == null)
        {
            smokeGO = new GameObject("Smoke");
            smokeGO.transform.SetParent(tempGO.transform, false);
        }
        else
        {
            smokeGO = smokeTransform.gameObject;
        }

        SpriteRenderer smokeSR = smokeGO.GetComponent<SpriteRenderer>();
        if (smokeSR == null) smokeSR = smokeGO.AddComponent<SpriteRenderer>();
        smokeSR.sprite = smokeGlowSprite;
        smokeSR.color = new Color(1f, 1f, 1f, 0.4f); // Slightly transparent white smoke (will be tinted by script if needed, or stays white/neon)
        smokeSR.sortingOrder = -2; // Render behind the base circle and connection line

        smokeGO.transform.localPosition = Vector3.zero;
        smokeGO.transform.localScale = Vector3.one * 1.05f; // Slightly larger than parent to bleed out as a smoke halo

        // Create/Update the SpecialRing child using specialRingSprite (rotating golden outline overlay for wild-cards)
        Transform specialRingTransform = tempGO.transform.Find("SpecialRing");
        GameObject specialRingGO;
        if (specialRingTransform == null)
        {
            specialRingGO = new GameObject("SpecialRing");
            specialRingGO.transform.SetParent(tempGO.transform, false);
        }
        else
        {
            specialRingGO = specialRingTransform.gameObject;
        }

        SpriteRenderer specialRingSR = specialRingGO.GetComponent<SpriteRenderer>();
        if (specialRingSR == null) specialRingSR = specialRingGO.AddComponent<SpriteRenderer>();
        specialRingSR.sprite = specialRingSprite;
        specialRingSR.color = Color.white; // Render golden color as-is
        specialRingSR.sortingOrder = 2; // Render on top of highlight

        specialRingGO.transform.localPosition = Vector3.zero;
        specialRingGO.transform.localScale = Vector3.one;
        specialRingGO.SetActive(false); // Disabled by default, enabled dynamically in Dot.cs

        PrefabUtility.SaveAsPrefabAsset(tempGO, prefabPath);
        Object.DestroyImmediate(tempGO);
    }

    private static void CreatePlaceholderTexture(string path, Color color)
    {
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        if (System.IO.File.Exists(path)) return;

        Texture2D tex = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dx = x - 31.5f;
                float dy = y - 31.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                // Simple circular icon shape
                if (dist <= 31.5f)
                {
                    float alpha = Mathf.Clamp01((31.5f - dist) / 1.5f);
                    tex.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha * color.a));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }
    }

    private static void BuildGradientRingTexture(string path)
    {
        string directory = System.IO.Path.GetDirectoryName(path);
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }

        // Delete old texture if it exists to overwrite it
        if (System.IO.File.Exists(path))
        {
            System.IO.File.Delete(path);
        }

        // High resolution hollow ring texture with baked gradient coloring (Yellow -> Orange -> Pink -> Purple)
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f - 0.5f;

        // Gradient color stops
        Color cYellow = new Color(1f, 0.82f, 0.12f);
        Color cOrange = new Color(0.98f, 0.38f, 0.08f);
        Color cPink = new Color(0.92f, 0.12f, 0.52f);
        Color cPurple = new Color(0.48f, 0.18f, 0.95f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Thin ring parameters (radius ~ 110 pixels out of 128 max radius)
                float outerRadius = 114f;
                float innerRadius = 100f;
                float thickness = (outerRadius - innerRadius) / 2f;
                float midRadius = innerRadius + thickness;

                float alpha = 0f;
                if (dist >= innerRadius - 2f && dist <= outerRadius + 2f)
                {
                    // Gaussian-like falloff across the thickness for a neon glowing hollow look
                    float distFromMid = Mathf.Abs(dist - midRadius);
                    alpha = Mathf.Exp(-(distFromMid * distFromMid) / (thickness * thickness * 0.35f));
                }

                if (alpha > 0.01f)
                {
                    // Bake gradient rotationally based on the angle
                    float angle = Mathf.Atan2(dy, dx); // -PI to PI
                    float t = (angle + Mathf.PI) / (2f * Mathf.PI); // 0 to 1

                    // Shift gradient alignment so yellow is at top (1:28 position)
                    t = Mathf.Repeat(t + 0.25f, 1f); 

                    // Interpolate gradient
                    Color pixelColor = Color.white;
                    if (t < 0.25f)
                        pixelColor = Color.Lerp(cPurple, cPink, t / 0.25f);
                    else if (t < 0.5f)
                        pixelColor = Color.Lerp(cPink, cOrange, (t - 0.25f) / 0.25f);
                    else if (t < 0.75f)
                        pixelColor = Color.Lerp(cOrange, cYellow, (t - 0.5f) / 0.25f);
                    else
                        pixelColor = Color.Lerp(cYellow, cPurple, (t - 0.75f) / 0.25f);

                    tex.SetPixel(x, y, new Color(pixelColor.r, pixelColor.g, pixelColor.b, alpha));
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, bytes);
        AssetDatabase.ImportAsset(path);

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 256f;
            importer.filterMode = FilterMode.Bilinear;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
    }
}
