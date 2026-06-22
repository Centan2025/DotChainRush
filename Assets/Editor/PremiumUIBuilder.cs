using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PremiumUIBuilder : EditorWindow
{
    [MenuItem("Tools/Build Premium Canvas UI")]
    public static void BuildUI()
    {
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("UIManager bulunamadi!");
            EditorUtility.DisplayDialog("Hata", "UIManager bulunamadi!", "Tamam");
            return;
        }

        Undo.RecordObject(uiManager, "Build Premium Canvas UI");

        // ---- DEEP CLEAN ----
        GameObject[] allGOs = FindObjectsByType<GameObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (GameObject go in allGOs)
        {
            if (go == null || go == uiManager.gameObject) continue;
            string n = go.name.ToLower();
            if (n.Contains("canvas") || n.Contains("ui canvas") ||
                n.Contains("scoretext") || n.Contains("timertext") ||
                n.Contains("combofeedback") || n.Contains("flashscreen") ||
                n.Contains("headerpanel") || n.Contains("bottomhud") ||
                n.Contains("dangerbanner") || n.Contains("dangertext") ||
                n.Contains("popupbox") || n.Contains("timerprogresscircle") ||
                n.Contains("leveltext") || n.Contains("goalcard") ||
                n.Contains("fevercard") || n.Contains("upnextcard") ||
                n.Contains("gameover") || n.Contains("eventsystem"))
            {
                if (go.scene.name != null)
                    Undo.DestroyObjectImmediate(go);
            }
        }

        // ---- CANVAS ----
        GameObject canvasGo = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(390, 844);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        Undo.RegisterCreatedObjectUndo(canvasGo, "Create UI Canvas");

        // EventSystem
        if (FindFirstObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject esGo = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            Undo.RegisterCreatedObjectUndo(esGo, "Create EventSystem");
        }

        // ---- FONTS ----
        TMP_FontAsset bold = LoadFont("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Roboto-Bold SDF.asset");
        TMP_FontAsset arcade = LoadFont("Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Anton SDF.asset");
        if (bold == null) bold = LoadFont("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (arcade == null) arcade = bold;

        Sprite circle = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Circle.png");

        var F = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

        // Reference colors
        Color C_DARK = new Color(0.04f, 0.02f, 0.08f, 1f);       // Fully opaque dark bg
        Color C_GOLD = new Color(1f, 0.82f, 0f);
        Color C_ORANGE = new Color(1f, 0.6f, 0f);
        Color C_PURPLE = new Color(0.85f, 0.15f, 1f);
        Color C_CYAN = new Color(0.36f, 0.94f, 1f);
        Color C_PINK = new Color(1f, 0.25f, 0.85f);
        Color C_GREEN = new Color(0.2f, 1f, 0.3f);
        Color C_CARD = new Color(0.06f, 0.04f, 0.12f, 0.95f);

        // Notch safe area offset - push content below camera cutout
        float NOTCH = 40f;

        // ============================================================
        //  FLASH SCREEN
        // ============================================================
        Image flashImg = Img("FlashScreenImage", canvasGo, V(0,0), V(1,1), V(0.5f,0.5f), Vz, Color.clear);
        flashImg.raycastTarget = false;
        Set(uiManager, "flashScreenImage", flashImg, F);

        // ============================================================
        //  HEADER PANEL - 200px tall, FULLY OPAQUE
        // ============================================================
        Image headerBg = Img("HeaderPanel", canvasGo, V(0,1), V(1,1), V(0.5f,1), V(0, 200), C_DARK);
        headerBg.rectTransform.anchoredPosition = Vz;
        GameObject H = headerBg.gameObject;

        // Neon bottom line
        Img("HeaderLine", H, V(0,0), V(1,0), V(0.5f,0), V(0, 2), new Color(C_PURPLE.r, C_PURPLE.g, C_PURPLE.b, 0.4f));

        // ---- PAUSE BUTTON (top-left, below notch) ----
        Image pauseBg = Img("PauseBtn", H, V(0,1), V(0,1), V(0,1), V(36, 36), new Color(0.12f, 0.08f, 0.18f, 0.9f));
        pauseBg.rectTransform.anchoredPosition = V(14, -(NOTCH + 4));
        pauseBg.gameObject.AddComponent<Button>();
        Txt("PauseTxt", pauseBg.gameObject, Vz, V(1,1), Vz, "II", 16, Color.white, arcade);

        // ---- BEST SCORE (small gold, top center) ----
        TextMeshProUGUI bestTxt = Txt("BestScoreText", H, V(0.5f,1), V(0.5f,1), V(0, -(NOTCH + 2)),
            "BEST: 0", 11, C_GOLD, bold, V(180, 16));
        Set(uiManager, "bestScoreText", bestTxt, F);

        // ---- MAIN SCORE (large white, centered) ----
        TextMeshProUGUI scoreTxt = Txt("ScoreText", H, V(0.5f,1), V(0.5f,1), V(0, -(NOTCH + 20)),
            "000000", 44, Color.white, arcade, V(300, 52));
        Set(uiManager, "scoreText", scoreTxt, F);

        // ---- COMBO xN (orange, below score) ----
        Txt("ComboLabel", H, V(0.5f,1), V(0.5f,1), V(0, -(NOTCH + 72)),
            "COMBO x1", 16, C_ORANGE, arcade, V(180, 24));

        // ---- COIN DISPLAY (top-right pill) ----
        Image coinBg = Img("CoinDisplay", H, V(1,1), V(1,1), V(1,1), V(82, 26), new Color(0.15f, 0.12f, 0.22f, 0.9f));
        coinBg.rectTransform.anchoredPosition = V(-10, -(NOTCH + 2));
        Txt("CoinTxt", coinBg.gameObject, Vz, V(1,1), Vz, "0", 13, C_GOLD, bold);

        // ---- TIMER RING (right side, donut style) ----
        // Outer dark circle
        Image timerOuter = Img("TimerRing", H, V(1,1), V(1,1), V(1,0.5f), V(64, 64), new Color(0.05f, 0.03f, 0.1f, 1f));
        timerOuter.rectTransform.anchoredPosition = V(-12, -(NOTCH + 55));
        if (circle != null) timerOuter.sprite = circle;
        GameObject TR = timerOuter.gameObject;

        // Purple ring fill (radial 360)
        Image timerFill = Img("TimerFill", TR, V(0.06f,0.06f), V(0.94f,0.94f), V(0.5f,0.5f), Vz, C_PURPLE);
        if (circle != null) timerFill.sprite = circle;
        timerFill.type = Image.Type.Filled;
        timerFill.fillMethod = Image.FillMethod.Radial360;
        timerFill.fillOrigin = (int)Image.Origin360.Top;
        timerFill.fillAmount = 0.8f;
        Set(uiManager, "timerProgressCircle", timerFill, F);

        // Inner dark circle (makes the donut hole)
        Image timerInner = Img("TimerInner", TR, V(0.18f,0.18f), V(0.82f,0.82f), V(0.5f,0.5f), Vz, new Color(0.05f, 0.03f, 0.1f, 1f));
        if (circle != null) timerInner.sprite = circle;

        // "TIME" label
        Txt("TimeLabel", TR, V(0.5f,1), V(0.5f,1), V(0, -6), "TIME", 7, C_PURPLE, bold, V(40, 10));

        // Timer value
        TextMeshProUGUI timerTxt = Txt("TimerText", TR, V(0.5f,0.45f), V(0.5f,0.45f), Vz,
            "0:00", 17, Color.white, arcade, V(50, 24));
        Set(uiManager, "timerText", timerTxt, F);

        // ---- FEVER ICON (left side, below pause, small dark circle) ----
        // Solid dark square that looks like a badge
        Image feverBg = Img("FeverIcon", H, V(0,1), V(0,1), V(0,1), V(40, 40), new Color(0.1f, 0.07f, 0.16f, 0.95f));
        feverBg.rectTransform.anchoredPosition = V(14, -(NOTCH + 48));

        // Lightning bolt text
        Txt("BoltTxt", feverBg.gameObject, Vz, V(1,1), V(0, 2), "Z", 20, Color.yellow, arcade);

        // "FEVER" label - Placed directly below the Fever Icon
        Txt("FeverLabel", H, V(0,1), V(0,1), V(12, -(NOTCH + 92)), "FEVER", 8, C_GOLD, bold, V(44, 12));

        // Mini fever progress squares (Segmented/Mini Bar) - Placed directly under the Fever Icon and FEVER Label
        Image miniBar = Img("FeverMiniBar", H, V(0,1), V(0,1), V(0,1), V(40, 6), new Color(0.2f, 0.15f, 0.3f, 0.8f));
        miniBar.rectTransform.anchoredPosition = V(14, -(NOTCH + 104));

        // ============================================================
        //  DANGER ZONE BANNER
        // ============================================================
        Image dangerBg = Img("DangerBanner", canvasGo, V(0,1), V(1,1), V(0.5f,1), V(0, 26), new Color(0.9f, 0.05f, 0.05f, 0.9f));
        dangerBg.rectTransform.anchoredPosition = V(0, -200);
        Set(uiManager, "dangerBanner", dangerBg.gameObject, F);

        TextMeshProUGUI dangerTxt = Txt("DangerText", dangerBg.gameObject, Vz, V(1,1), Vz,
            "DANGER ZONE", 12, Color.white, arcade);
        Set(uiManager, "dangerText", dangerTxt, F);
        dangerBg.gameObject.SetActive(false);

        // ============================================================
        //  FLOATING COMBO FEEDBACK (Positioned directly under score/header and above play area)
        // ============================================================
        GameObject comboGo = new GameObject("ComboContainer", typeof(RectTransform));
        comboGo.transform.SetParent(canvasGo.transform, false);
        RectTransform comboRt = comboGo.GetComponent<RectTransform>();
        comboRt.anchorMin = V(0.5f, 1f); // Anchor to top-center of the screen
        comboRt.anchorMax = V(0.5f, 1f);
        comboRt.pivot = V(0.5f, 1f);
        comboRt.anchoredPosition = V(0, -205f); // Positioned right under the 200px Header panel
        comboRt.sizeDelta = V(300, 90);
        Set(uiManager, "comboContainer", comboGo, F);

        TextMeshProUGUI cTitle = Txt("ComboTitleText", comboGo, V(0.5f,1f), V(0.5f,1f), V(0, -10f),
            "12 CHAIN!", 26, C_PINK, arcade, V(280, 30));
        Set(uiManager, "comboTitleText", cTitle, F);

        TextMeshProUGUI cVal = Txt("ComboValueText", comboGo, V(0.5f,1f), V(0.5f,1f), V(0, -40f),
            "+24,000", 20, C_GREEN, arcade, V(280, 26));
        Set(uiManager, "comboMultiplierText", cVal, F);

        TextMeshProUGUI cChain = Txt("ComboChainText", comboGo, V(0.5f,1f), V(0.5f,1f), V(0, -66f),
            "", 11, C_CYAN, bold, V(280, 16));
        Set(uiManager, "comboChainText", cChain, F);

        comboGo.SetActive(false);

        // ============================================================
        //  BOTTOM HUD (3 compact cards)
        // ============================================================
        GameObject bottomGo = new GameObject("BottomHUDPanel", typeof(RectTransform));
        bottomGo.transform.SetParent(canvasGo.transform, false);
        RectTransform bottomRt = bottomGo.GetComponent<RectTransform>();
        bottomRt.anchorMin = V(0, 0);
        bottomRt.anchorMax = V(1, 0);
        bottomRt.pivot = V(0.5f, 0);
        bottomRt.anchoredPosition = V(0, 6);
        bottomRt.sizeDelta = V(-14, 68);

        HorizontalLayoutGroup hlg = bottomGo.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 5;
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.childControlHeight = true;
        hlg.childControlWidth = true;
        hlg.padding = new RectOffset(4, 4, 3, 3);

        // --- GOAL ---
        GameObject goalCard = Card(bottomGo, "GoalCard", C_CARD);
        CardTitle(goalCard, "GOAL", new Color(0.6f, 0.4f, 1f), arcade);
        TextMeshProUGUI goalTxt = CardBody(goalCard, "HEDEF: 0", bold);
        Set(uiManager, "goalText", goalTxt, F);

        // --- UP NEXT ---
        GameObject nextCard = Card(bottomGo, "UpNextCard", C_CARD);
        CardTitle(nextCard, "UP NEXT", C_CYAN, arcade);

        // Dot previews
        GameObject dotsRow = new GameObject("DotsRow", typeof(RectTransform));
        dotsRow.transform.SetParent(nextCard.transform, false);
        RectTransform dRt = dotsRow.GetComponent<RectTransform>();
        dRt.anchorMin = V(0.5f, 0.5f);
        dRt.anchorMax = V(0.5f, 0.5f);
        dRt.anchoredPosition = V(0, -5);
        dRt.sizeDelta = V(80, 14);
        HorizontalLayoutGroup dh = dotsRow.AddComponent<HorizontalLayoutGroup>();
        dh.spacing = 3;
        dh.childAlignment = TextAnchor.MiddleCenter;
        dh.childControlWidth = false;
        dh.childControlHeight = false;

        Color[] dotC = { new Color(0.2f,0.4f,1f), Color.red, C_ORANGE, Color.magenta, Color.yellow };
        foreach (Color c in dotC)
        {
            GameObject d = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            d.transform.SetParent(dotsRow.transform, false);
            d.GetComponent<RectTransform>().sizeDelta = V(12, 12);
            Image di = d.GetComponent<Image>();
            di.sprite = circle;
            di.color = c;
        }

        // --- FEVER MODE (hidden) ---
        GameObject feverCard = Card(bottomGo, "FeverCard", C_CARD);
        CardTitle(feverCard, "FEVER MODE", C_ORANGE, arcade);
        Set(uiManager, "feverPanel", feverCard, F);

        TextMeshProUGUI fm = Txt("FeverMult", feverCard, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 4),
            "x10", 18, Color.yellow, arcade, V(60, 22));
        Set(uiManager, "feverMultiplierText", fm, F);

        Image fbBg = Img("FeverBarBg", feverCard, V(0.5f,0.5f), V(0.5f,0.5f), V(0.5f,0.5f), V(55, 5), new Color(0.2f,0.15f,0.25f));
        fbBg.rectTransform.anchoredPosition = V(0, -8);
        Image fbFill = Img("FeverBar", fbBg.gameObject, Vz, V(1,1), V(0.5f,0.5f), Vz, C_ORANGE);
        fbFill.type = Image.Type.Filled;
        fbFill.fillMethod = Image.FillMethod.Horizontal;
        fbFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        Set(uiManager, "feverProgressBar", fbFill, F);

        TextMeshProUGUI ft = Txt("FeverTime", feverCard, V(0.5f,0.5f), V(0.5f,0.5f), V(0, -18),
            "0.0 SEC", 8, Color.white, bold, V(60, 12));
        Set(uiManager, "feverTimeLeftText", ft, F);
        feverCard.SetActive(false);

        // ============================================================
        //  LEVEL TEXT (hidden)
        // ============================================================
        GameObject lvlH = new GameObject("LevelTextHidden", typeof(RectTransform));
        lvlH.transform.SetParent(canvasGo.transform, false);
        lvlH.SetActive(false);
        TextMeshProUGUI lvlTxt = lvlH.AddComponent<TextMeshProUGUI>();
        lvlTxt.text = "";
        Set(uiManager, "levelText", lvlTxt, F);

        // ============================================================
        //  GAME OVER PANEL
        // ============================================================
        Image goBg = Img("GameOverPanel", canvasGo, Vz, V(1,1), V(0.5f,0.5f), Vz, new Color(0.03f,0.02f,0.06f,0.96f));
        GameObject GO = goBg.gameObject;

        Txt("GameOverText", GO, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 260),
            "OYUN BITTI", 56, new Color(1f,0.15f,0.15f), arcade, V(340, 70));
        Txt("HighScoreTextVal", GO, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 170),
            "HIGH SCORE: 0", 28, C_GOLD, arcade, V(340, 42));
        Txt("ScoreTextVal", GO, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 90),
            "SKOR: 0", 40, Color.white, arcade, V(340, 52));
        Txt("BestComboTextVal", GO, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 15),
            "EN IYI COMBO: 0 TOP", 26, C_CYAN, arcade, V(340, 38));
        Txt("ImprovementTextVal", GO, V(0.5f,0.5f), V(0.5f,0.5f), V(0, -50),
            "+0%", 22, Color.green, bold, V(340, 32));

        Image restBg = Img("RestartButton", GO, V(0.5f,0.5f), V(0.5f,0.5f), V(0.5f,0.5f), V(250, 65), new Color(0.1f,0.62f,0.4f));
        restBg.rectTransform.anchoredPosition = V(0, -150);
        Button restBtn = restBg.gameObject.AddComponent<Button>();
        Txt("RestartTxt", restBg.gameObject, Vz, V(1,1), Vz, "TEKRAR OYNA", 24, Color.white, arcade);

        GO.SetActive(false);
        Set(uiManager, "gameOverPanel", GO, F);

        GameManager gm = FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            SerializedObject so = new SerializedObject(gm);
            so.FindProperty("restartButton").objectReferenceValue = restBtn;
            so.ApplyModifiedProperties();
        }

        // ============================================================
        //  LEVEL UP OVERLAY
        // ============================================================
        Image luBg = Img("LevelUpOverlay", canvasGo, Vz, V(1,1), V(0.5f,0.5f), Vz, new Color(0.03f,0.02f,0.06f,0.96f));
        GameObject LU = luBg.gameObject;
        Set(uiManager, "levelUpOverlay", LU, F);

        Image popBg = Img("PopupBox", LU, V(0.5f,0.5f), V(0.5f,0.5f), V(0.5f,0.5f), V(300, 340), new Color(0.1f,0.06f,0.16f,1f));

        Txt("LvlTitle", popBg.gameObject, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 110),
            "SEVIYE TAMAMLANDI!", 22, Color.yellow, arcade, V(260, 40));

        TextMeshProUGUI luSub = Txt("LvlSub", popBg.gameObject, V(0.5f,0.5f), V(0.5f,0.5f), V(0, 65),
            "SEVIYE 2", 20, Color.white, bold, V(260, 30));
        Set(uiManager, "unlockedLevelTitleText", luSub, F);

        TextMeshProUGUI luDesc = Txt("LvlDesc", popBg.gameObject, V(0.5f,0.5f), V(0.5f,0.5f), V(0, -10),
            "Hazirlanin!", 14, new Color(0.82f,0.82f,0.82f), bold, V(240, 80));
        Set(uiManager, "levelPreviewText", luDesc, F);

        Image contBg = Img("ContinueBtn", popBg.gameObject, V(0.5f,0.5f), V(0.5f,0.5f), V(0.5f,0.5f), V(170, 46), C_CYAN);
        contBg.rectTransform.anchoredPosition = V(0, -100);
        Button contBtn = contBg.gameObject.AddComponent<Button>();
        Txt("ContTxt", contBg.gameObject, Vz, V(1,1), Vz, "DEVAM ET", 16, Color.black, arcade);
        Set(uiManager, "levelUpContinueButton", contBtn, F);

        LU.SetActive(false);

        // Compatibility
        Set(uiManager, "comboFeedbackText", scoreTxt, F);

        EditorUtility.SetDirty(uiManager);
        EditorUtility.SetDirty(canvasGo);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
            UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());

        Debug.Log("Premium Canvas UI kuruldu!");
        EditorUtility.DisplayDialog("Basarili",
            "Premium Canvas UI kuruldu!\n\n" +
            "- Header: 200px, notch-safe, tam opak\n" +
            "- Timer: Donut halka efekti\n" +
            "- Alt kartlar: 68px kompakt\n" +
            "- Toplar kartlarin ustunde", "Harika");
    }

    // ======= HELPERS =======
    static Vector2 V(float x, float y) => new Vector2(x, y);
    static Vector2 Vz => Vector2.zero;

    static TMP_FontAsset LoadFont(string p) => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(p);

    static void Set(UIManager m, string f, object v, System.Reflection.BindingFlags fl)
    {
        var fi = m.GetType().GetField(f, fl);
        if (fi != null) fi.SetValue(m, v);
    }

    static Image Img(string name, GameObject parent, Vector2 amin, Vector2 amax, Vector2 pivot, Vector2 size, Color color)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        rt.pivot = pivot;
        rt.sizeDelta = size;
        Image img = go.GetComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI Txt(string name, GameObject parent, Vector2 amin, Vector2 amax, Vector2 pos,
        string text, float size, Color color, TMP_FontAsset font, Vector2 dim = default)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent.transform, false);
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.anchorMin = amin;
        rt.anchorMax = amax;
        if (dim.x > 0 || dim.y > 0)
            rt.sizeDelta = dim;
        else
            rt.sizeDelta = Vz;
        rt.anchoredPosition = pos;
        TextMeshProUGUI t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.color = color;
        if (font != null) t.font = font;
        t.alignment = TextAlignmentOptions.Center;
        t.enableWordWrapping = true;
        t.overflowMode = TextOverflowModes.Truncate;
        t.raycastTarget = false;
        return t;
    }

    static GameObject Card(GameObject parent, string name, Color bg)
    {
        GameObject c = new GameObject(name, typeof(RectTransform), typeof(Image));
        c.transform.SetParent(parent.transform, false);
        c.GetComponent<Image>().color = bg;
        return c;
    }

    static void CardTitle(GameObject card, string title, Color color, TMP_FontAsset font)
    {
        TextMeshProUGUI t = Txt(title + "Title", card, V(0,1), V(1,1), V(0, -3), title, 9, color, font, V(0, 14));
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = V(0, 1);
        rt.anchorMax = V(1, 1);
        rt.pivot = V(0.5f, 1);
        rt.sizeDelta = V(0, 14);
    }

    static TextMeshProUGUI CardBody(GameObject card, string text, TMP_FontAsset font)
    {
        TextMeshProUGUI t = Txt("Body", card, Vz, V(1,1), V(0, -6), text, 10, Color.white, font);
        RectTransform rt = t.GetComponent<RectTransform>();
        rt.anchorMin = Vz;
        rt.anchorMax = V(1, 1);
        rt.sizeDelta = V(-6, -16);
        return t;
    }
}
