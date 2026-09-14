using System;
using System.Collections;
using System.Reflection;
using NightCourier.Delivery;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NightCourier.Prototype
{
    /// <summary>Localized start screen, vehicle selection, and an animated in-shift pause menu.</summary>
    public sealed class MainMenuController : MonoBehaviour
    {
        public enum MenuLanguage { English, Korean }

        private const string LanguagePreference = "NightCourier.MenuLanguage";
        private const float RevealDuration = 1.45f;
        private const float PauseDuration = .42f;
        private static readonly Color Ink = new Color(.025f, .035f, .055f, 1);
        private static readonly Color Paper = new Color(.94f, .96f, .98f, 1);
        private static readonly Color Muted = new Color(.62f, .69f, .76f, 1);
        private static readonly Color Cyan = new Color(.37f, .9f, .94f, 1);

        private DeliveryHud hud;
        private VehicleAppearance appearance;
        private VehiclePreviewRenderer vehiclePreview;
        private Font displayFont;
        private bool ownsDisplayFont;
        private Texture2D backdrop, panel, button, buttonHover, secondaryButton, selectedButton, hairline;
        private GUIStyle eyebrow, title, subtitle, body, primaryButton, secondaryButtonStyle, small;
        private bool stylesReady;
        private bool settingsOpen;
        private bool vehicleSelectionOpen;
        private bool pauseOpen;
        private bool pauseSettingsOpen;
        private float revealProgress;
        private float pauseProgress;
        private CourierVehicle stagedVehicle;
        private int stagedColor;

        public bool IsOpen { get; private set; } = true;
        public bool IsPaused => pauseOpen;
        public bool IsTransitioning { get; private set; }
        public bool IsVehicleSelectionOpen => vehicleSelectionOpen;
        public float PauseProgress => pauseProgress;
        public MenuLanguage Language { get; private set; }
        public string GraphicsQuality => "MEDIUM";
        public CourierVehicle SelectedVehicle => appearance == null ? stagedVehicle : appearance.SelectedVehicle;
        public int SelectedColor => appearance == null ? stagedColor : appearance.SelectedColor;

        private void Awake()
        {
            Language = (MenuLanguage)Mathf.Clamp(PlayerPrefs.GetInt(LanguagePreference, 0), 0, 1);
            Time.timeScale = 0;
            AudioListener.pause = true;
            AudioListener.volume = 1;
            Cursor.visible = true;
            CreateTextures();
            RefreshFont();
            vehiclePreview = gameObject.AddComponent<VehiclePreviewRenderer>();
        }

        public void Initialize(DeliveryHud deliveryHud, VehicleAppearance vehicleAppearance)
        {
            hud = deliveryHud;
            appearance = vehicleAppearance;
            if (hud != null) hud.enabled = false;
            if (appearance == null) return;
            stagedVehicle = appearance.SelectedVehicle;
            stagedColor = appearance.SelectedColor;
            vehiclePreview.Show(stagedVehicle, stagedColor);
        }

        private void Update()
        {
            if (Keyboard.current == null || IsTransitioning) return;
            if (IsOpen)
            {
                if (vehicleSelectionOpen)
                {
                    if (Keyboard.current.escapeKey.wasPressedThisFrame) vehicleSelectionOpen = false;
                    else if (Keyboard.current.digit1Key.wasPressedThisFrame) StageVehicle(CourierVehicle.Pickup);
                    else if (Keyboard.current.digit2Key.wasPressedThisFrame) StageVehicle(CourierVehicle.Sport);
                    else if (Keyboard.current.digit3Key.wasPressedThisFrame) StageVehicle(CourierVehicle.Van);
                    else if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                        ConfirmVehicleSelection();
                    return;
                }
                if (Keyboard.current.escapeKey.wasPressedThisFrame)
                {
                    settingsOpen = false;
                    vehicleSelectionOpen = false;
                }
                else if (!settingsOpen && !vehicleSelectionOpen && Keyboard.current.vKey.wasPressedThisFrame)
                    OpenVehicleSelection();
                else if (!settingsOpen && !vehicleSelectionOpen &&
                         (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
                    BeginShift();
                return;
            }
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                if (pauseOpen) BeginResume();
                else BeginPause();
            }
        }

        public void SetLanguage(MenuLanguage language)
        {
            if (Language == language) return;
            Language = language;
            PlayerPrefs.SetInt(LanguagePreference, (int)language);
            PlayerPrefs.Save();
            RefreshFont();
        }

        public void OpenVehicleSelection()
        {
            if (!IsOpen || appearance == null) return;
            stagedVehicle = appearance.SelectedVehicle;
            stagedColor = appearance.SelectedColor;
            settingsOpen = false;
            vehicleSelectionOpen = true;
            vehiclePreview.Show(stagedVehicle, stagedColor);
        }

        public void StageVehicle(CourierVehicle vehicle)
        {
            stagedVehicle = vehicle;
            vehiclePreview.Show(stagedVehicle, stagedColor);
        }

        public void StageColor(int colorIndex)
        {
            stagedColor = Mathf.Clamp(colorIndex, 0, VehicleAppearance.AvailableColors.Count - 1);
            vehiclePreview.Show(stagedVehicle, stagedColor);
        }

        public void ConfirmVehicleSelection()
        {
            appearance?.Select(stagedVehicle, stagedColor);
            vehicleSelectionOpen = false;
        }

        public void BeginShift(bool skipTransition = false)
        {
            if (!IsOpen || IsTransitioning) return;
            settingsOpen = false;
            vehicleSelectionOpen = false;
            IsTransitioning = true;
            AudioListener.pause = false;
            AudioListener.volume = 0;
            if (skipTransition) { CompleteReveal(); return; }
            StartCoroutine(RevealShift());
        }

        public void BeginPause(bool skipTransition = false)
        {
            if (IsOpen || pauseOpen || IsTransitioning) return;
            pauseOpen = true;
            pauseSettingsOpen = false;
            Time.timeScale = 0;
            Cursor.visible = true;
            if (skipTransition) { pauseProgress = 1; AudioListener.volume = .35f; return; }
            IsTransitioning = true;
            StartCoroutine(FadePause(true));
        }

        public void BeginResume(bool skipTransition = false)
        {
            if (!pauseOpen || IsTransitioning) return;
            pauseSettingsOpen = false;
            if (skipTransition) { CompleteResume(); return; }
            IsTransitioning = true;
            StartCoroutine(FadePause(false));
        }

        public void QuitGame()
        {
            Time.timeScale = 1;
            AudioListener.pause = false;
            AudioListener.volume = 1;
            Application.Quit();
            Type editor = Type.GetType("UnityEditor.EditorApplication,UnityEditor");
            editor?.GetProperty("isPlaying", BindingFlags.Public | BindingFlags.Static)?.SetValue(null, false);
        }

        private IEnumerator RevealShift()
        {
            float elapsed = 0;
            while (elapsed < RevealDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                revealProgress = Mathf.SmoothStep(0, 1, Mathf.Clamp01(elapsed / RevealDuration));
                AudioListener.volume = revealProgress;
                yield return null;
            }
            CompleteReveal();
        }

        private IEnumerator FadePause(bool opening)
        {
            float elapsed = 0;
            float from = pauseProgress;
            float to = opening ? 1 : 0;
            while (elapsed < PauseDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                pauseProgress = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, elapsed / PauseDuration));
                AudioListener.volume = Mathf.Lerp(1, .35f, pauseProgress);
                yield return null;
            }
            if (opening) { pauseProgress = 1; AudioListener.volume = .35f; IsTransitioning = false; }
            else CompleteResume();
        }

        private void CompleteReveal()
        {
            revealProgress = 1;
            AudioListener.volume = 1;
            Time.timeScale = 1;
            IsTransitioning = false;
            IsOpen = false;
            vehiclePreview.SetRendering(false);
            if (hud != null) hud.enabled = true;
        }

        private void CompleteResume()
        {
            pauseProgress = 0;
            pauseOpen = false;
            AudioListener.volume = 1;
            Time.timeScale = 1;
            IsTransitioning = false;
        }

        private void OnGUI()
        {
            if (!IsOpen && !pauseOpen && pauseProgress <= .001f) return;
            float scale = Mathf.Clamp(Mathf.Min(Screen.width / 1920f, Screen.height / 1080f), .72f, 2f);
            EnsureStyles(scale);
            GUI.depth = -1000;
            Color previousColor = GUI.color;
            if (IsOpen) DrawStartLayer(scale);
            else DrawPauseLayer(scale);
            GUI.enabled = true;
            GUI.color = previousColor;
        }

        private void DrawStartLayer(float scale)
        {
            GUI.color = new Color(1, 1, 1, 1 - revealProgress);
            GUI.enabled = !IsTransitioning;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), backdrop, ScaleMode.StretchToFill);
            float margin = 76 * scale;
            GUI.Label(new Rect(margin, 48 * scale, 620 * scale, 30 * scale), "N I G H T   C O U R I E R", eyebrow);
            GUI.DrawTexture(new Rect(margin, 88 * scale, 48 * scale, 2 * scale), hairline);
            if (vehicleSelectionOpen) DrawVehicleSelection(margin, scale);
            else if (settingsOpen) DrawSettings(margin, scale, () => settingsOpen = false);
            else DrawHome(margin, scale);
            GUI.Label(new Rect(margin, Screen.height - 55 * scale, 620 * scale, 24 * scale),
                "NIGHTCOURIER  /  URP PROTOTYPE  /  00:47", small);
        }

        private void DrawPauseLayer(float scale)
        {
            GUI.color = new Color(1, 1, 1, pauseProgress);
            GUI.enabled = !IsTransitioning;
            Fill(new Rect(0, 0, Screen.width, Screen.height), new Color(.015f, .025f, .04f, .92f));
            float width = Mathf.Min(560 * scale, Screen.width - 80 * scale);
            float margin = (Screen.width - width) * .5f;
            if (pauseSettingsOpen) DrawSettings(margin, scale, () => pauseSettingsOpen = false);
            else DrawPause(margin, width, scale);
        }

        private void DrawHome(float margin, float scale)
        {
            float width = Mathf.Min(620 * scale, Screen.width - margin * 2);
            float top = Mathf.Max(80 * scale, Screen.height * .18f);
            GUI.Label(new Rect(margin, top, width, 150 * scale), T("The city is waiting.", "도시가 기다립니다."), title);
            GUI.Label(new Rect(margin, top + 142 * scale, width, 74 * scale),
                T("A midnight delivery through rain, neon, and quiet streets.", "비와 네온, 고요한 거리를 달리는 한밤의 배달."), subtitle);
            Rect card = new Rect(margin, top + 250 * scale, width, 188 * scale);
            GUI.DrawTexture(card, panel);
            GUI.Label(Inset(card, 28, 24, 22, scale), T("TONIGHT'S ROUTE", "오늘의 경로"), small);
            GUI.Label(Inset(card, 28, 59, 32, scale), T("8 parcels  ·  2 cities  ·  50% charge", "소포 8개  ·  도시 2곳  ·  배터리 50%"), body);
            GUI.DrawTexture(new Rect(card.x + 28 * scale, card.y + 111 * scale, card.width - 56 * scale, 1), hairline);
            GUI.Label(Inset(card, 28, 133, 28, scale), VehicleName(SelectedVehicle) + "   /   " + T("SELECTED", "선택됨"), small);
            Rect start = new Rect(margin, top + 470 * scale, 220 * scale, 62 * scale);
            DrawButton(start, "START!", primaryButton, true, () => BeginShift());
            Rect choose = new Rect(start.xMax + 14 * scale, start.y, 210 * scale, start.height);
            DrawButton(choose, T("CHOOSE CAR", "차량 선택"), secondaryButtonStyle, false, OpenVehicleSelection);
            Rect settings = new Rect(choose.xMax + 14 * scale, start.y, 176 * scale, start.height);
            DrawButton(settings, T("SETTINGS", "설정"), secondaryButtonStyle, false, () => settingsOpen = true);
            GUI.Label(new Rect(margin, start.yMax + 18 * scale, 440 * scale, 28 * scale), T("Return to start  /  V to choose car", "Return 시작  /  V 차량 선택"), small);
        }

        private void DrawVehicleSelection(float margin, float scale)
        {
            float width = Mathf.Min(1120 * scale, Screen.width - margin * 2);
            float top = Mathf.Max(72 * scale, Screen.height * .14f);
            GUI.Label(new Rect(margin, top, width, 100 * scale), T("Choose your car.", "차량을 선택하세요."), title);
            GUI.Label(new Rect(margin, top + 96 * scale, width, 48 * scale), T("Three silhouettes. One night shift.", "세 가지 실루엣, 하나의 야간 근무."), subtitle);
            Rect controls = new Rect(margin, top + 170 * scale, 390 * scale, 420 * scale);
            GUI.DrawTexture(controls, panel);
            for (int i = 0; i < 3; i++)
            {
                int index = i;
                Rect option = new Rect(controls.x + 24 * scale, controls.y + (28 + i * 62) * scale, controls.width - 48 * scale, 50 * scale);
                DrawChoice(option, VehicleName((CourierVehicle)i), stagedVehicle == (CourierVehicle)i, () => StageVehicle((CourierVehicle)index));
            }
            GUI.Label(new Rect(controls.x + 24 * scale, controls.y + 230 * scale, 220 * scale, 22 * scale), T("EXTERIOR COLOR", "외장 색상"), small);
            for (int i = 0; i < VehicleAppearance.AvailableColors.Count; i++)
            {
                int index = i;
                Rect swatch = new Rect(controls.x + (24 + i * 62) * scale, controls.y + 268 * scale, 48 * scale, 48 * scale);
                Fill(swatch, VehicleAppearance.AvailableColors[i]);
                if (stagedColor == i) Fill(new Rect(swatch.x, swatch.yMax + 6 * scale, swatch.width, 3 * scale), Cyan);
                if (GUI.Button(swatch, GUIContent.none, GUIStyle.none)) StageColor(index);
            }
            GUI.Label(new Rect(controls.x + 24 * scale, controls.y + 342 * scale, controls.width - 48 * scale, 52 * scale), VehicleDescription(stagedVehicle), small);
            Rect preview = new Rect(controls.xMax + 18 * scale, controls.y, width - controls.width - 18 * scale, controls.height);
            GUI.DrawTexture(preview, vehiclePreview.Texture, ScaleMode.ScaleToFit, false);
            Rect select = new Rect(margin, controls.yMax + 22 * scale, 220 * scale, 58 * scale);
            DrawButton(select, T("SELECT", "선택"), primaryButton, true, ConfirmVehicleSelection);
            Rect back = new Rect(select.xMax + 14 * scale, select.y, 180 * scale, select.height);
            DrawButton(back, T("BACK", "뒤로"), secondaryButtonStyle, false, () => vehicleSelectionOpen = false);
        }

        private void DrawPause(float margin, float width, float scale)
        {
            float top = Mathf.Max(90 * scale, Screen.height * .2f);
            GUI.Label(new Rect(margin, top, width, 95 * scale), T("Paused", "일시정지"), title);
            GUI.Label(new Rect(margin, top + 88 * scale, width, 44 * scale), T("The rain can wait.", "비는 잠시 기다려 줍니다."), subtitle);
            Rect resume = new Rect(margin, top + 180 * scale, width, 62 * scale);
            DrawButton(resume, T("RESUME", "계속하기"), primaryButton, true, () => BeginResume());
            Rect settings = new Rect(margin, resume.yMax + 14 * scale, width, 58 * scale);
            DrawButton(settings, T("SETTINGS", "설정"), secondaryButtonStyle, false, () => pauseSettingsOpen = true);
            Rect quit = new Rect(margin, settings.yMax + 14 * scale, width, 58 * scale);
            DrawButton(quit, T("QUIT GAME", "게임 종료"), secondaryButtonStyle, false, QuitGame);
            GUI.Label(new Rect(margin, quit.yMax + 22 * scale, width, 24 * scale), T("Tab also resumes", "Tab 키로도 계속하기"), small);
        }

        private void DrawSettings(float margin, float scale, Action backAction)
        {
            float width = Mathf.Min(620 * scale, Screen.width - margin * 2);
            float top = Mathf.Max(80 * scale, Screen.height * .18f);
            GUI.Label(new Rect(margin, top, width, 104 * scale), T("Settings", "설정"), title);
            GUI.Label(new Rect(margin, top + 102 * scale, width, 54 * scale), T("Make the night yours.", "당신에게 맞는 밤을 준비하세요."), subtitle);
            Rect card = new Rect(margin, top + 190 * scale, width, 278 * scale);
            GUI.DrawTexture(card, panel);
            GUI.Label(Inset(card, 28, 25, 22, scale), T("LANGUAGE", "언어"), small);
            Rect english = new Rect(card.x + 28 * scale, card.y + 58 * scale, 162 * scale, 48 * scale);
            Rect korean = new Rect(english.xMax + 10 * scale, english.y, 162 * scale, english.height);
            DrawChoice(english, "ENGLISH", Language == MenuLanguage.English, () => SetLanguage(MenuLanguage.English));
            DrawChoice(korean, "한국어", Language == MenuLanguage.Korean, () => SetLanguage(MenuLanguage.Korean));
            GUI.DrawTexture(new Rect(card.x + 28 * scale, card.y + 137 * scale, card.width - 56 * scale, 1), hairline);
            GUI.Label(Inset(card, 28, 163, 22, scale), T("GRAPHICS", "그래픽"), small);
            GUI.Label(Inset(card, 28, 198, 30, scale), "MEDIUM", body);
            GUI.Label(new Rect(card.xMax - 164 * scale, card.y + 202 * scale, 136 * scale, 24 * scale), T("LOCKED FOR NOW", "현재 고정"), small);
            Rect back = new Rect(margin, top + 496 * scale, 200 * scale, 58 * scale);
            DrawButton(back, T("BACK", "뒤로"), secondaryButtonStyle, false, backAction);
        }

        private string VehicleName(CourierVehicle vehicle) => vehicle switch
        {
            CourierVehicle.Sport => T("TWO-SEAT SPORT", "2인승 스포츠"),
            CourierVehicle.Van => T("COURIER VAN", "택배 밴"),
            _ => T("ELECTRIC PICKUP", "전기 픽업")
        };

        private string VehicleDescription(CourierVehicle vehicle) => vehicle switch
        {
            CourierVehicle.Sport => T("320 km/h / 650 kW / 2-speed APEX E2", "320km/h / 650kW / 2단 APEX E2"),
            CourierVehicle.Van => T("160 km/h / stable heavy cargo tune", "160km/h / 안정적인 중량 화물 세팅"),
            _ => T("200 km/h / balanced AWD delivery tune", "200km/h / 균형형 AWD 배달 세팅")
        };

        private void DrawButton(Rect rect, string text, GUIStyle style, bool primary, Action action)
        {
            bool hover = rect.Contains(Event.current.mousePosition);
            GUI.DrawTexture(rect, primary ? (hover ? buttonHover : button) : secondaryButton);
            if (GUI.Button(rect, text, style)) action();
        }

        private void DrawChoice(Rect rect, string text, bool selected, Action action)
        {
            GUI.DrawTexture(rect, selected ? selectedButton : secondaryButton);
            if (GUI.Button(rect, text, selected ? primaryButton : secondaryButtonStyle)) action();
        }

        private static void Fill(Rect rect, Color color)
        {
            Color previous = GUI.color;
            GUI.color = new Color(color.r * previous.r, color.g * previous.g,
                color.b * previous.b, color.a * previous.a);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static Rect Inset(Rect rect, float x, float y, float height, float scale) =>
            new Rect(rect.x + x * scale, rect.y + y * scale, rect.width - x * 2 * scale, height * scale);

        private string T(string english, string korean) => Language == MenuLanguage.Korean ? korean : english;

        private void RefreshFont()
        {
            if (ownsDisplayFont && displayFont != null) Destroy(displayFont);
            ownsDisplayFont = false;
            displayFont = null;
            if (Language == MenuLanguage.Korean)
            {
                string[] preferred = { "Apple SD Gothic Neo", "Malgun Gothic", "Noto Sans CJK KR", "Arial Unicode MS" };
                string[] installed = Font.GetOSInstalledFontNames();
                foreach (string candidate in preferred)
                {
                    foreach (string available in installed)
                    {
                        if (!string.Equals(candidate, available, StringComparison.OrdinalIgnoreCase)) continue;
                        displayFont = Font.CreateDynamicFontFromOSFont(available, 48);
                        ownsDisplayFont = displayFont != null;
                        break;
                    }
                    if (displayFont != null) break;
                }
            }
            if (displayFont == null) displayFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stylesReady = false;
        }

        private void EnsureStyles(float scale)
        {
            if (stylesReady && Mathf.Abs(title.fontSize - Mathf.RoundToInt(68 * scale)) < 2) return;
            eyebrow = Style(16, FontStyle.Bold, Paper, scale);
            title = Style(68, FontStyle.Normal, Paper, scale); title.wordWrap = true;
            subtitle = Style(23, FontStyle.Normal, Muted, scale); subtitle.wordWrap = true;
            body = Style(21, FontStyle.Normal, Paper, scale);
            small = Style(14, FontStyle.Normal, Muted, scale); small.wordWrap = true;
            primaryButton = Style(16, FontStyle.Bold, Ink, scale); primaryButton.alignment = TextAnchor.MiddleCenter;
            secondaryButtonStyle = Style(16, FontStyle.Bold, Paper, scale); secondaryButtonStyle.alignment = TextAnchor.MiddleCenter;
            foreach (GUIStyle style in new[] { primaryButton, secondaryButtonStyle })
            {
                style.normal.background = null; style.hover.background = null;
                style.active.background = null; style.focused.background = null;
            }
            stylesReady = true;
        }

        private GUIStyle Style(int size, FontStyle fontStyle, Color color, float scale)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                font = displayFont, fontSize = Mathf.RoundToInt(size * scale), fontStyle = fontStyle,
                alignment = TextAnchor.UpperLeft, clipping = TextClipping.Clip
            };
            style.normal.textColor = color;
            return style;
        }

        private void CreateTextures()
        {
            backdrop = new Texture2D(1, 256, TextureFormat.RGBA32, false) { name = "Main menu midnight gradient" };
            for (int y = 0; y < backdrop.height; y++)
            {
                float t = y / 255f;
                backdrop.SetPixel(0, y, Color.Lerp(new Color(.018f, .027f, .045f, .94f), new Color(.035f, .075f, .095f, .84f), t));
            }
            backdrop.Apply();
            panel = Solid("Main menu glass", new Color(.09f, .13f, .17f, .88f));
            button = Solid("Main menu button", new Color(.72f, .94f, .95f, 1));
            buttonHover = Solid("Main menu button hover", new Color(.86f, .99f, 1, 1));
            secondaryButton = Solid("Main menu secondary", new Color(.16f, .22f, .28f, .96f));
            selectedButton = Solid("Main menu selected", new Color(.68f, .9f, .92f, 1));
            hairline = Solid("Main menu accent", Cyan);
        }

        private static Texture2D Solid(string label, Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = label };
            texture.SetPixel(0, 0, color); texture.Apply(); return texture;
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            Time.timeScale = 1;
            AudioListener.pause = false;
            AudioListener.volume = 1;
            if (ownsDisplayFont && displayFont != null) Destroy(displayFont);
            foreach (Texture2D texture in new[] { backdrop, panel, button, buttonHover, secondaryButton, selectedButton, hairline })
                if (texture != null) Destroy(texture);
        }
    }
}
