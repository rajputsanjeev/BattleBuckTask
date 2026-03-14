using BattleBuck.Core;
using BattleBuck.Match;
using BattleBuck.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BattleBuck.Editor
{
    /// <summary>
    /// Editor menu script to create the full match scene setup once.
    /// Use: BattleBuck > Setup Match Scene
    /// Creates: Camera, Lighting, Ground, and the entire UI hierarchy.
    /// </summary>
    public static class MatchSceneSetupEditor
    {
        // ── Menu Item ────────────────────────────────────────────────

        [MenuItem("BattleBuck/Setup Match Scene")]
        public static void SetupMatchScene()
        {
            MatchController matchController = Object.FindAnyObjectByType<MatchController>();
            if (matchController == null)
            {
                EditorUtility.DisplayDialog("BattleBuck",
                    "No MatchController found in the scene.\nPlease add a MatchController component first.", "OK");
                return;
            }

            MatchConfig config = matchController.Config;
            if (config == null)
            {
                EditorUtility.DisplayDialog("BattleBuck",
                    "MatchController has no MatchConfig assigned.", "OK");
                return;
            }

            // Check for existing setup
            bool hasExisting = GameObject.Find("MatchUI_Canvas") != null
                            || GameObject.Find("Main Camera") != null
                            || GameObject.Find("Directional Light") != null
                            || GameObject.Find("Ground") != null;

            if (hasExisting)
            {
                if (!EditorUtility.DisplayDialog("BattleBuck",
                    "Existing scene objects found.\nDo you want to delete and recreate?",
                    "Recreate", "Cancel"))
                {
                    return;
                }

                DestroyIfExists("MatchUI_Canvas");
                DestroyIfExists("Main Camera");
                DestroyIfExists("Directional Light");
                DestroyIfExists("Ground");
            }

            Undo.IncrementCurrentGroup();

            SetupCamera(config);
            SetupLighting();
            SetupGround(config);
            SetupUI(matchController);

            EditorUtility.DisplayDialog("BattleBuck",
                "Match scene setup complete!\n• Camera\n• Lighting\n• Ground\n• UI Canvas\n\nSave the scene to persist.", "OK");
        }

        // ── Camera ───────────────────────────────────────────────────

        private static void SetupCamera(MatchConfig config)
        {
            GameObject camObj = new GameObject("Main Camera");
            Undo.RegisterCreatedObjectUndo(camObj, "Create Camera");

            Camera cam = camObj.AddComponent<Camera>();
            camObj.tag = "MainCamera";
            camObj.AddComponent<AudioListener>();

            float groundSize = config.groundHalfSize;
            float camHeight = groundSize * 1.5f;
            cam.transform.position = new Vector3(0f, camHeight, -groundSize * 0.8f);
            cam.transform.rotation = Quaternion.Euler(55f, 0f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.08f, 0.09f, 0.14f);
            cam.fieldOfView = 60f;
        }

        // ── Lighting ─────────────────────────────────────────────────

        private static void SetupLighting()
        {
            GameObject lightObj = new GameObject("Directional Light");
            Undo.RegisterCreatedObjectUndo(lightObj, "Create Lighting");

            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(0.95f, 0.92f, 0.85f);
            light.intensity = 1.2f;
            lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            RenderSettings.ambientLight = new Color(0.15f, 0.15f, 0.2f);
        }

        // ── Ground ───────────────────────────────────────────────────

        private static void SetupGround(MatchConfig config)
        {
            float groundScale = config.groundHalfSize / 5f;
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            Undo.RegisterCreatedObjectUndo(ground, "Create Ground");

            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(groundScale, 1f, groundScale);
            ground.GetComponent<Renderer>().material.color = new Color(0.12f, 0.13f, 0.18f);
            Object.DestroyImmediate(ground.GetComponent<Collider>());
        }

        // ── UI ───────────────────────────────────────────────────────

        private static void SetupUI(MatchController matchController)
        {
            GameObject canvasObj = new GameObject("MatchUI_Canvas");
            Undo.RegisterCreatedObjectUndo(canvasObj, "Create Match UI");

            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            CreateTimerUI(canvasObj.transform);
            CreateLeaderboardUI(canvasObj.transform, matchController);
            CreateKillFeedUI(canvasObj.transform, matchController);
            CreateWinnerScreen(canvasObj.transform, matchController);

            Selection.activeGameObject = canvasObj;
        }

        // ── Timer ────────────────────────────────────────────────────

        private static void CreateTimerUI(Transform parent)
        {
            GameObject timerObj = CreateUIElement("TimerPanel", parent);
            RectTransform timerRect = timerObj.GetComponent<RectTransform>();
            timerRect.anchorMin = new Vector2(0.5f, 1f);
            timerRect.anchorMax = new Vector2(0.5f, 1f);
            timerRect.pivot = new Vector2(0.5f, 1f);
            timerRect.anchoredPosition = new Vector2(0f, -20f);
            timerRect.sizeDelta = new Vector2(250f, 70f);

            Image timerBg = timerObj.AddComponent<Image>();
            timerBg.color = new Color(0f, 0f, 0f, 0.6f);

            GameObject timerTextObj = CreateUIElement("TimerText", timerObj.transform);
            RectTransform textRect = timerTextObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            TextMeshProUGUI timerText = timerTextObj.AddComponent<TextMeshProUGUI>();
            timerText.text = "03:00";
            timerText.fontSize = 36f;
            timerText.alignment = TextAlignmentOptions.Center;
            timerText.color = Color.white;
            timerText.fontStyle = FontStyles.Bold;

            TimerUI timerUI = timerObj.AddComponent<TimerUI>();
            SetField(timerUI, "_timerText", timerText);
        }

        // ── Leaderboard ──────────────────────────────────────────────

        private static void CreateLeaderboardUI(Transform parent, MatchController matchController)
        {
            GameObject lbPanel = CreateUIElement("LeaderboardPanel", parent);
            RectTransform lbRect = lbPanel.GetComponent<RectTransform>();
            lbRect.anchorMin = new Vector2(0f, 0.3f);
            lbRect.anchorMax = new Vector2(0f, 1f);
            lbRect.pivot = new Vector2(0f, 1f);
            lbRect.anchoredPosition = new Vector2(20f, -100f);
            lbRect.sizeDelta = new Vector2(320f, 500f);

            Image lbBg = lbPanel.AddComponent<Image>();
            lbBg.color = new Color(0f, 0f, 0f, 0.4f);

            GameObject headerObj = CreateUIElement("Header", lbPanel.transform);
            RectTransform headerRect = headerObj.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.anchoredPosition = Vector2.zero;
            headerRect.sizeDelta = new Vector2(0f, 40f);

            TextMeshProUGUI headerText = headerObj.AddComponent<TextMeshProUGUI>();
            headerText.text = "LEADERBOARD";
            headerText.fontSize = 20f;
            headerText.alignment = TextAlignmentOptions.Center;
            headerText.color = new Color(0.9f, 0.85f, 0.55f);
            headerText.fontStyle = FontStyles.Bold;

            GameObject containerObj = CreateUIElement("EntryContainer", lbPanel.transform);
            RectTransform containerRect = containerObj.GetComponent<RectTransform>();
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;
            containerRect.offsetMin = new Vector2(5f, 5f);
            containerRect.offsetMax = new Vector2(-5f, -45f);

            VerticalLayoutGroup vlg = containerObj.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 3f;
            vlg.childControlHeight = true;
            vlg.childControlWidth = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            vlg.padding = new RectOffset(5, 5, 5, 5);

            GameObject entryPrefab = CreateLeaderboardEntryPrefab(lbPanel.transform);
            entryPrefab.SetActive(false);

            LeaderboardUI lbUI = lbPanel.AddComponent<LeaderboardUI>();
            SetField(lbUI, "_matchController", matchController);
            SetField(lbUI, "_entryPrefab", entryPrefab.GetComponent<LeaderboardEntry>());
            SetField(lbUI, "_entryContainer", containerRect);
        }

        private static GameObject CreateLeaderboardEntryPrefab(Transform parent)
        {
            GameObject entry = CreateUIElement("EntryPrefab", parent);
            entry.GetComponent<RectTransform>().sizeDelta = new Vector2(0f, 38f);

            Image entryBg = entry.AddComponent<Image>();
            entryBg.color = new Color(1f, 1f, 1f, 0.08f);

            HorizontalLayoutGroup hlg = entry.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 8f;
            hlg.childControlWidth = false;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = true;
            hlg.padding = new RectOffset(10, 10, 2, 2);

            // Rank
            GameObject rankObj = CreateUIElement("RankText", entry.transform);
            rankObj.GetComponent<RectTransform>().sizeDelta = new Vector2(40f, 0f);
            TextMeshProUGUI rankText = rankObj.AddComponent<TextMeshProUGUI>();
            rankText.text = "#1";
            rankText.fontSize = 16f;
            rankText.alignment = TextAlignmentOptions.MidlineLeft;
            rankText.color = new Color(0.7f, 0.7f, 0.7f);
            rankText.fontStyle = FontStyles.Bold;
            rankObj.AddComponent<LayoutElement>().preferredWidth = 40f;

            // Name
            GameObject nameObj = CreateUIElement("NameText", entry.transform);
            nameObj.GetComponent<RectTransform>().sizeDelta = new Vector2(160f, 0f);
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = "Player";
            nameText.fontSize = 16f;
            nameText.alignment = TextAlignmentOptions.MidlineLeft;
            nameText.color = Color.white;
            LayoutElement nameLE = nameObj.AddComponent<LayoutElement>();
            nameLE.preferredWidth = 160f;
            nameLE.flexibleWidth = 1f;

            // Score
            GameObject scoreObj = CreateUIElement("ScoreText", entry.transform);
            scoreObj.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 0f);
            TextMeshProUGUI scoreText = scoreObj.AddComponent<TextMeshProUGUI>();
            scoreText.text = "0";
            scoreText.fontSize = 18f;
            scoreText.alignment = TextAlignmentOptions.MidlineRight;
            scoreText.color = new Color(0.95f, 0.85f, 0.45f);
            scoreText.fontStyle = FontStyles.Bold;
            scoreObj.AddComponent<LayoutElement>().preferredWidth = 50f;

            LeaderboardEntry entryComp = entry.AddComponent<LeaderboardEntry>();
            SetField(entryComp, "_rankText", rankText);
            SetField(entryComp, "_nameText", nameText);
            SetField(entryComp, "_scoreText", scoreText);
            SetField(entryComp, "_background", entryBg);

            return entry;
        }

        // ── Kill Feed ────────────────────────────────────────────────

        private static void CreateKillFeedUI(Transform parent, MatchController matchController)
        {
            GameObject feedObj = CreateUIElement("KillFeed", parent);
            RectTransform feedRect = feedObj.GetComponent<RectTransform>();
            feedRect.anchorMin = new Vector2(0.5f, 0f);
            feedRect.anchorMax = new Vector2(0.5f, 0f);
            feedRect.pivot = new Vector2(0.5f, 0f);
            feedRect.anchoredPosition = new Vector2(0f, 30f);
            feedRect.sizeDelta = new Vector2(500f, 50f);

            TextMeshProUGUI feedText = feedObj.AddComponent<TextMeshProUGUI>();
            feedText.text = "";
            feedText.fontSize = 20f;
            feedText.alignment = TextAlignmentOptions.Center;
            feedText.color = new Color(1f, 0.6f, 0.6f);
            feedText.fontStyle = FontStyles.Italic;

            KillFeedUI killFeed = feedObj.AddComponent<KillFeedUI>();
            SetField(killFeed, "_matchController", matchController);
            SetField(killFeed, "_killFeedText", feedText);
        }

        // ── Winner Screen ────────────────────────────────────────────

        private static void CreateWinnerScreen(Transform parent, MatchController matchController)
        {
            GameObject panel = CreateUIElement("WinnerPanel", parent);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image panelBg = panel.AddComponent<Image>();
            panelBg.color = new Color(0.05f, 0.06f, 0.1f, 0.92f);

            GameObject winnerNameObj = CreateUIElement("WinnerName", panel.transform);
            RectTransform wnRect = winnerNameObj.GetComponent<RectTransform>();
            wnRect.anchorMin = new Vector2(0.5f, 0.5f);
            wnRect.anchorMax = new Vector2(0.5f, 0.5f);
            wnRect.pivot = new Vector2(0.5f, 0.5f);
            wnRect.anchoredPosition = new Vector2(0f, 50f);
            wnRect.sizeDelta = new Vector2(600f, 80f);

            TextMeshProUGUI winnerNameText = winnerNameObj.AddComponent<TextMeshProUGUI>();
            winnerNameText.text = "Winner";
            winnerNameText.fontSize = 48f;
            winnerNameText.alignment = TextAlignmentOptions.Center;
            winnerNameText.color = new Color(0.95f, 0.85f, 0.35f);
            winnerNameText.fontStyle = FontStyles.Bold;

            GameObject winnerScoreObj = CreateUIElement("WinnerScore", panel.transform);
            RectTransform wsRect = winnerScoreObj.GetComponent<RectTransform>();
            wsRect.anchorMin = new Vector2(0.5f, 0.5f);
            wsRect.anchorMax = new Vector2(0.5f, 0.5f);
            wsRect.pivot = new Vector2(0.5f, 0.5f);
            wsRect.anchoredPosition = new Vector2(0f, -20f);
            wsRect.sizeDelta = new Vector2(400f, 60f);

            TextMeshProUGUI winnerScoreText = winnerScoreObj.AddComponent<TextMeshProUGUI>();
            winnerScoreText.text = "";
            winnerScoreText.fontSize = 32f;
            winnerScoreText.alignment = TextAlignmentOptions.Center;
            winnerScoreText.color = Color.white;

            GameObject reasonObj = CreateUIElement("ReasonText", panel.transform);
            RectTransform rRect = reasonObj.GetComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.pivot = new Vector2(0.5f, 0.5f);
            rRect.anchoredPosition = new Vector2(0f, -80f);
            rRect.sizeDelta = new Vector2(400f, 40f);

            TextMeshProUGUI reasonText = reasonObj.AddComponent<TextMeshProUGUI>();
            reasonText.text = "";
            reasonText.fontSize = 22f;
            reasonText.alignment = TextAlignmentOptions.Center;
            reasonText.color = new Color(0.7f, 0.7f, 0.8f);
            reasonText.fontStyle = FontStyles.Italic;

            WinnerScreenUI winnerUI = panel.AddComponent<WinnerScreenUI>();
            SetField(winnerUI, "_matchController", matchController);
            SetField(winnerUI, "_winnerPanel", panel);
            SetField(winnerUI, "_winnerNameText", winnerNameText);
            SetField(winnerUI, "_winnerScoreText", winnerScoreText);
            SetField(winnerUI, "_reasonText", reasonText);
        }

        // ── Helpers ──────────────────────────────────────────────────

        private static void DestroyIfExists(string name)
        {
            GameObject obj = GameObject.Find(name);
            if (obj != null) Undo.DestroyObjectImmediate(obj);
        }

        private static GameObject CreateUIElement(string name, Transform parent)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();
            return obj;
        }

        private static void SetField<T>(Component component, string fieldName, T value)
        {
            var so = new SerializedObject(component);
            var prop = so.FindProperty(fieldName);
            if (prop != null)
            {
                if (value is Object unityObj)
                {
                    prop.objectReferenceValue = unityObj;
                }
                so.ApplyModifiedProperties();
            }
            else
            {
                var field = component.GetType().GetField(fieldName,
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(component, value);
                    EditorUtility.SetDirty(component);
                }
                else
                {
                    Debug.LogError($"Field '{fieldName}' not found on {component.GetType().Name}");
                }
            }
        }
    }
}
