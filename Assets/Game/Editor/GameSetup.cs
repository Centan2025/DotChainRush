using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameSetup
{
    [MenuItem("Tools/Setup Dot Chain Rush")]
    public static void SetupGame()
    {
        // 0a. Create Texture Placeholders
        CreatePlaceholderTexture("Assets/Textures/pause_circle.png", new Color(0.82f, 0.74f, 1f)); // Light Purple
        CreatePlaceholderTexture("Assets/Textures/timer.png", new Color(0.49f, 0.96f, 1f)); // Cyan
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

        // 8. Setup Canvas & UI
        Canvas canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasGO = new GameObject("Canvas");
            canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
        }

        // EventSystem
        EventSystem eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            GameObject esGO = new GameObject("EventSystem");
            eventSystem = esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        // Score Text
        TextMeshProUGUI scoreText = null;
        Transform scoreTextTransform = canvas.transform.Find("ScoreText");
        if (scoreTextTransform == null)
        {
            GameObject scoreGO = new GameObject("ScoreText");
            scoreGO.transform.SetParent(canvas.transform, false);
            scoreText = scoreGO.AddComponent<TextMeshProUGUI>();
            scoreText.fontSize = 64;
            scoreText.text = "Score: 0";
            scoreText.color = Color.white;
            scoreText.alignment = TextAlignmentOptions.Left;

            RectTransform rect = scoreGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(50f, -50f);
            rect.sizeDelta = new Vector2(400f, 100f);
        }
        else
        {
            scoreText = scoreTextTransform.GetComponent<TextMeshProUGUI>();
        }

        // Timer Text
        TextMeshProUGUI timerText = null;
        Transform timerTextTransform = canvas.transform.Find("TimerText");
        if (timerTextTransform == null)
        {
            GameObject timerGO = new GameObject("TimerText");
            timerGO.transform.SetParent(canvas.transform, false);
            timerText = timerGO.AddComponent<TextMeshProUGUI>();
            timerText.fontSize = 64;
            timerText.text = "Time: 60s";
            timerText.color = Color.white;
            timerText.alignment = TextAlignmentOptions.Right;

            RectTransform rect = timerGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-50f, -50f);
            rect.sizeDelta = new Vector2(400f, 100f);
        }
        else
        {
            timerText = timerTextTransform.GetComponent<TextMeshProUGUI>();
        }

        // Screen Flash Image
        Image flashImageComp = null;
        Transform flashTransform = canvas.transform.Find("FlashScreenImage");
        if (flashTransform == null)
        {
            GameObject flashGO = new GameObject("FlashScreenImage");
            flashGO.transform.SetParent(canvas.transform, false);
            flashImageComp = flashGO.AddComponent<Image>();
            flashImageComp.color = Color.clear;
            flashImageComp.raycastTarget = false;

            RectTransform rect = flashGO.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
        }
        else
        {
            flashImageComp = flashTransform.GetComponent<Image>();
        }

        // Combo Feedback Text
        TextMeshProUGUI comboFeedbackText = null;
        Transform comboTransform = canvas.transform.Find("ComboFeedbackText");
        if (comboTransform == null)
        {
            GameObject comboGO = new GameObject("ComboFeedbackText");
            comboGO.transform.SetParent(canvas.transform, false);
            comboFeedbackText = comboGO.AddComponent<TextMeshProUGUI>();
            comboFeedbackText.fontSize = 80;
            comboFeedbackText.text = "COMBO!";
            comboFeedbackText.color = Color.yellow;
            comboFeedbackText.alignment = TextAlignmentOptions.Center;
            comboFeedbackText.gameObject.SetActive(false);

            RectTransform rect = comboGO.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, 300f);
            rect.sizeDelta = new Vector2(800f, 200f);
        }
        else
        {
            comboFeedbackText = comboTransform.GetComponent<TextMeshProUGUI>();
        }

        // GameOver Panel
        GameObject gameOverPanel = null;
        Transform panelTransform = canvas.transform.Find("GameOverPanel");
        if (panelTransform != null)
        {
            Object.DestroyImmediate(panelTransform.gameObject);
        }

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
        hsText.fontSize = 42;
        hsText.fontStyle = FontStyles.Bold | FontStyles.Italic;
        hsText.text = "HIGH SCORE: 0";
        hsText.color = new Color(1f, 0.84f, 0f); // Gold
        hsText.alignment = TextAlignmentOptions.Center;

        RectTransform hsRect = hsTextGO.GetComponent<RectTransform>();
        hsRect.anchorMin = new Vector2(0.5f, 0.5f);
        hsRect.anchorMax = new Vector2(0.5f, 0.5f);
        hsRect.pivot = new Vector2(0.5f, 0.5f);
        hsRect.anchoredPosition = new Vector2(0f, 190f);
        hsRect.sizeDelta = new Vector2(800f, 80f);

        // 3. Final Score Text
        GameObject scoreTextValGO = new GameObject("ScoreTextVal");
        scoreTextValGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI scoreTextVal = scoreTextValGO.AddComponent<TextMeshProUGUI>();
        scoreTextVal.fontSize = 56;
        scoreTextVal.fontStyle = FontStyles.Bold;
        scoreTextVal.text = "SCORE: 0";
        scoreTextVal.color = Color.white;
        scoreTextVal.alignment = TextAlignmentOptions.Center;

        RectTransform scoreTextValRect = scoreTextValGO.GetComponent<RectTransform>();
        scoreTextValRect.anchorMin = new Vector2(0.5f, 0.5f);
        scoreTextValRect.anchorMax = new Vector2(0.5f, 0.5f);
        scoreTextValRect.pivot = new Vector2(0.5f, 0.5f);
        scoreTextValRect.anchoredPosition = new Vector2(0f, 90f);
        scoreTextValRect.sizeDelta = new Vector2(800f, 90f);

        // 4. Best Combo Text
        GameObject bestComboTextValGO = new GameObject("BestComboTextVal");
        bestComboTextValGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI bestComboTextVal = bestComboTextValGO.AddComponent<TextMeshProUGUI>();
        bestComboTextVal.fontSize = 44;
        bestComboTextVal.fontStyle = FontStyles.Italic;
        bestComboTextVal.text = "BEST COMBO: 0 DOT";
        bestComboTextVal.color = new Color(0.49f, 0.96f, 1f); // Cyan
        bestComboTextVal.alignment = TextAlignmentOptions.Center;

        RectTransform bestComboRect = bestComboTextValGO.GetComponent<RectTransform>();
        bestComboRect.anchorMin = new Vector2(0.5f, 0.5f);
        bestComboRect.anchorMax = new Vector2(0.5f, 0.5f);
        bestComboRect.pivot = new Vector2(0.5f, 0.5f);
        bestComboRect.anchoredPosition = new Vector2(0f, 0f);
        bestComboRect.sizeDelta = new Vector2(800f, 80f);

        // 5. Performance Improvement Percentage Text
        GameObject impTextValGO = new GameObject("ImprovementTextVal");
        impTextValGO.transform.SetParent(gameOverPanel.transform, false);
        TextMeshProUGUI impTextVal = impTextValGO.AddComponent<TextMeshProUGUI>();
        impTextVal.fontSize = 38;
        impTextVal.fontStyle = FontStyles.Bold;
        impTextVal.text = "+25% daha iyi oynadın!";
        impTextVal.color = Color.green;
        impTextVal.alignment = TextAlignmentOptions.Center;

        RectTransform impRect = impTextValGO.GetComponent<RectTransform>();
        impRect.anchorMin = new Vector2(0.5f, 0.5f);
        impRect.anchorMax = new Vector2(0.5f, 0.5f);
        impRect.pivot = new Vector2(0.5f, 0.5f);
        impRect.anchoredPosition = new Vector2(0f, -90f);
        impRect.sizeDelta = new Vector2(800f, 80f);

        // 6. Restart Button
        GameObject buttonGO = new GameObject("RestartButton");
        buttonGO.transform.SetParent(gameOverPanel.transform, false);
        Image btnImage = buttonGO.AddComponent<Image>();
        btnImage.color = new Color(0.12f, 0.64f, 0.44f); // Slick emerald green
        Button restartButton = buttonGO.AddComponent<Button>();

        RectTransform btnRect = buttonGO.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(0.5f, 0.5f);
        btnRect.anchorMax = new Vector2(0.5f, 0.5f);
        btnRect.pivot = new Vector2(0.5f, 0.5f);
        btnRect.anchoredPosition = new Vector2(0f, -230f);
        btnRect.sizeDelta = new Vector2(360f, 100f);

        // Button Text
        GameObject btnTextGO = new GameObject("Text");
        btnTextGO.transform.SetParent(buttonGO.transform, false);
        TextMeshProUGUI btnText = btnTextGO.AddComponent<TextMeshProUGUI>();
        btnText.fontSize = 38;
        btnText.fontStyle = FontStyles.Bold;
        btnText.text = "TEKRAR OYNA";
        btnText.color = Color.white;
        btnText.alignment = TextAlignmentOptions.Center;

        RectTransform btnTextRect = btnTextGO.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.sizeDelta = Vector2.zero;

        gameOverPanel.SetActive(false);

        // 9. Setup Managers
        ScoreManager scoreManager = Object.FindAnyObjectByType<ScoreManager>();
        if (scoreManager == null)
        {
            GameObject scoreManGO = new GameObject("ScoreManager");
            scoreManager = scoreManGO.AddComponent<ScoreManager>();
        }

        ComboManager comboManager = Object.FindAnyObjectByType<ComboManager>();
        if (comboManager == null)
        {
            GameObject comboManGO = new GameObject("ComboManager");
            comboManager = comboManGO.AddComponent<ComboManager>();
        }
        
        SerializedObject serializedCombo = new SerializedObject(comboManager);
        serializedCombo.FindProperty("particlePrefab").objectReferenceValue = explosionPrefab;
        serializedCombo.ApplyModifiedProperties();

        UIManager uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            GameObject uiManGO = new GameObject("UIManager");
            uiManager = uiManGO.AddComponent<UIManager>();
        }

        // Setup UI Document (UI Toolkit)
        UnityEngine.UIElements.UIDocument uiDoc = uiManager.GetComponent<UnityEngine.UIElements.UIDocument>();
        if (uiDoc == null)
        {
            uiDoc = uiManager.gameObject.AddComponent<UnityEngine.UIElements.UIDocument>();
        }
        
        // Setup Panel Settings
        UnityEngine.UIElements.PanelSettings panelSettings = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.PanelSettings>("Assets/Game/UI/GamePanelSettings.asset");
        if (panelSettings == null)
        {
            panelSettings = ScriptableObject.CreateInstance<UnityEngine.UIElements.PanelSettings>();
            AssetDatabase.CreateAsset(panelSettings, "Assets/Game/UI/GamePanelSettings.asset");
        }
        
        // Always enforce mobile CSS viewport resolution (390x844) to scale elements correctly
        panelSettings.scaleMode = UnityEngine.UIElements.PanelScaleMode.ScaleWithScreenSize;
        panelSettings.referenceResolution = new Vector2Int(390, 844);
        panelSettings.screenMatchMode = UnityEngine.UIElements.PanelScreenMatchMode.MatchWidthOrHeight;
        panelSettings.match = 0.5f;
        EditorUtility.SetDirty(panelSettings);
        
        uiDoc.panelSettings = panelSettings;

        UnityEngine.UIElements.VisualTreeAsset uxmlAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.UIElements.VisualTreeAsset>("Assets/Game/UI/GameUI.uxml");
        if (uxmlAsset != null)
        {
            uiDoc.visualTreeAsset = uxmlAsset;
        }

        SerializedObject serializedUI = new SerializedObject(uiManager);
        serializedUI.FindProperty("scoreText").objectReferenceValue = scoreText;
        serializedUI.FindProperty("timerText").objectReferenceValue = timerText;
        serializedUI.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        serializedUI.FindProperty("comboFeedbackText").objectReferenceValue = comboFeedbackText;
        serializedUI.FindProperty("flashScreenImage").objectReferenceValue = flashImageComp;
        serializedUI.FindProperty("uiDocument").objectReferenceValue = uiDoc;
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
        EditorUtility.SetDirty(comboManager);
        EditorUtility.SetDirty(uiManager);
        if (uiDoc != null) EditorUtility.SetDirty(uiDoc);
        EditorUtility.SetDirty(manager);
        
        Debug.Log("Dot Chain Rush game setup complete with updated falling physics prefab, ObjectPool, Score, Combo & Audio Managers!");
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
}
