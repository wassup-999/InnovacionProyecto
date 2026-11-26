using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class MuseumRuntimeUI : MonoBehaviour
{
    public static MuseumRuntimeUI Instance { get; private set; }
    private UIDocument document;
    private VisualElement root, hud, prompt, infoOverlay, infoCard, pauseOverlay, settingsOverlay;
    private Label timerLabel, countLabel, promptLabel, infoKicker, infoTitle, infoMeta, infoBody;
    private Image infoImage;
    private float startTime;
    private bool hasStarted, isPaused, infoOpen, settingsOpen;
    private PaintData currentPaint;
    private StatueData currentStatue;
    private PlayerController player;
    private CameraController cameraController;
    private PlayerInteractor interactor;
    private Slider sensitivitySlider, volumeSlider;
    private float nextHudUpdate;

    private void Awake()
    {
        Instance = this;
        document = GetComponent<UIDocument>();
        if (document.panelSettings == null)
        {
            var settings = ScriptableObject.CreateInstance<PanelSettings>();
            settings.name = "Museum Runtime Panel Settings";
            settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
            settings.referenceResolution = new Vector2Int(1920, 1080);
            settings.screenMatchMode = PanelScreenMatchMode.MatchWidthOrHeight;
            settings.match = .5f;
            document.panelSettings = settings;
        }
        var gm = GameManager.Instance;
        if (gm != null) { player = gm.PlayerController; cameraController = gm.CameraController; interactor = gm.PlayerInteractor; }
    }

    private void Start()
    {
        startTime = Time.time;
        BuildInterface();
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (infoOpen) CloseInfo();
            else if (settingsOpen) ShowSettings(false);
            else TogglePause();
        }
        if (timerLabel != null && Time.unscaledTime >= nextHudUpdate)
        {
            var time = TimeSpan.FromSeconds(Mathf.Max(0f, Time.time - startTime));
            timerLabel.text = $"RECORRIDO     {time:hh\\:mm\\:ss}";
            countLabel.text = $"OBRAS  {MuseumSceneEnhancements.ViewedPaintCount:00} / {MuseumSceneEnhancements.TotalPaintCount:00}     ·     ESCULTURAS  {MuseumSceneEnhancements.ViewedStatueCount:00} / {MuseumSceneEnhancements.TotalStatueCount:00}";
            nextHudUpdate = Time.unscaledTime + 1f;
        }
    }

    public static void ShowPainting(PaintData data) { if (Instance != null) Instance.OpenPaint(data); }
    public static void ShowStatue(StatueData data) { if (Instance != null) Instance.OpenStatue(data); }
    public static void SetFocus(string message) { if (Instance != null) Instance.SetPrompt(message); }
    public static void HideInfo() { if (Instance != null) Instance.CloseInfo(); }

    private void BuildInterface()
    {
        root = document.rootVisualElement;
        root.Clear();
        root.style.position = Position.Absolute; root.style.left = 0; root.style.right = 0; root.style.top = 0; root.style.bottom = 0;
        root.style.unityFont = null;

        hud = new VisualElement();
        hud.style.position = Position.Absolute; hud.style.left = 28; hud.style.top = 24; hud.style.paddingLeft = 18; hud.style.paddingRight = 20; hud.style.paddingTop = 12; hud.style.paddingBottom = 12;
        hud.style.backgroundColor = new Color(.055f,.045f,.037f,.88f); hud.style.borderLeftWidth = 2; hud.style.borderLeftColor = new Color(.77f,.58f,.29f,1);
        timerLabel = Text("MUSEO · 00:00", 13, new Color(.92f,.79f,.52f), true); countLabel = Text("OBRAS 00 / 00", 11, new Color(.91f,.89f,.83f), false);
        hud.Add(timerLabel); hud.Add(countLabel); root.Add(hud);

        prompt = new VisualElement(); prompt.style.position = Position.Absolute; prompt.style.bottom = Length.Percent(10); prompt.style.left = Length.Percent(50); prompt.style.translate = new Translate(-50, 0);
        prompt.style.paddingLeft = 23; prompt.style.paddingRight = 23; prompt.style.paddingTop = 11; prompt.style.paddingBottom = 11;
        prompt.style.backgroundColor = new Color(.07f,.055f,.04f,.92f); prompt.style.borderTopWidth = 1; prompt.style.borderTopColor = new Color(.82f,.67f,.39f,1);
        promptLabel = Text("E  ·  EXAMINAR", 14, new Color(.98f,.90f,.72f), true); prompt.Add(promptLabel); prompt.style.display = DisplayStyle.None; root.Add(prompt);

        infoOverlay = Overlay();
        infoCard = Card();
        var imageColumn = new VisualElement(); imageColumn.style.width = Length.Percent(42); imageColumn.style.minHeight = 360; imageColumn.style.backgroundColor = new Color(.12f,.105f,.09f,1);
        infoImage = new Image(); infoImage.scaleMode = ScaleMode.ScaleToFit; infoImage.style.flexGrow = 1; imageColumn.Add(infoImage);
        var copy = new VisualElement(); copy.style.flexGrow = 1; copy.style.paddingLeft = 38; copy.style.paddingRight = 38; copy.style.paddingTop = 34; copy.style.paddingBottom = 30;
        infoKicker = Text("COLECCIÓN PERMANENTE", 11, new Color(.82f,.65f,.36f), true);
        infoTitle = Text("Título", 29, new Color(.96f,.92f,.84f), true); infoTitle.style.whiteSpace = WhiteSpace.Normal;
        infoMeta = Text("Autor", 15, new Color(.82f,.70f,.50f), false); infoMeta.style.marginTop = 8;
        var divider = new VisualElement(); divider.style.height = 1; divider.style.backgroundColor = new Color(.8f,.66f,.39f,.5f); divider.style.marginTop = 18; divider.style.marginBottom = 18;
        infoBody = Text("Descripción", 16, new Color(.87f,.85f,.80f), false); infoBody.style.whiteSpace = WhiteSpace.Normal; infoBody.style.flexGrow = 1;
        var close = Button("CERRAR   ·   ESC", CloseInfo, true); close.style.alignSelf = Align.FlexEnd; close.style.marginTop = 24;
        copy.Add(infoKicker); copy.Add(infoTitle); copy.Add(infoMeta); copy.Add(divider); copy.Add(infoBody); copy.Add(close);
        infoCard.style.flexDirection = FlexDirection.Row; infoCard.Add(imageColumn); infoCard.Add(copy); infoOverlay.Add(infoCard); root.Add(infoOverlay);

        pauseOverlay = Overlay(); var pauseCard = Card(); pauseCard.style.width = 480; pauseCard.style.paddingLeft = 42; pauseCard.style.paddingRight = 42; pauseCard.style.paddingTop = 36; pauseCard.style.paddingBottom = 36;
        pauseCard.Add(Text("RECORRIDO EN PAUSA", 26, new Color(.96f,.92f,.84f), true));
        pauseCard.Add(Text("Tómate un momento. El museo te espera.", 14, new Color(.76f,.71f,.63f), false));
        pauseCard.Add(Button("CONTINUAR RECORRIDO", TogglePause, true));
        pauseCard.Add(Button("CONFIGURACIÓN", () => ShowSettings(true), false));
        pauseCard.Add(Button("SALIR AL MENÚ", ExitToMenu, false));
        pauseOverlay.Add(pauseCard); root.Add(pauseOverlay); pauseOverlay.style.display = DisplayStyle.None;

        settingsOverlay = Overlay(); var settingsCard = Card(); settingsCard.style.width = 540; settingsCard.style.paddingLeft = 42; settingsCard.style.paddingRight = 42; settingsCard.style.paddingTop = 36; settingsCard.style.paddingBottom = 32;
        settingsCard.Add(Text("CONFIGURACIÓN", 25, new Color(.96f,.92f,.84f), true));
        sensitivitySlider = new Slider("Sensibilidad del ratón", 20f, 140f); sensitivitySlider.value = PlayerPrefs.GetFloat("MuseumMouseSensitivity", cameraController != null ? cameraController.mouseSensivity : 80f); sensitivitySlider.style.marginTop = 20; sensitivitySlider.style.marginBottom = 16;
        sensitivitySlider.RegisterValueChangedCallback(evt => { if (cameraController != null) cameraController.mouseSensivity = evt.newValue; PlayerPrefs.SetFloat("MuseumMouseSensitivity", evt.newValue); });
        volumeSlider = new Slider("Volumen general", 0f, 1f); volumeSlider.value = PlayerPrefs.GetFloat("MuseumVolume", AudioManager.Instance != null ? AudioManager.Instance.MasterVolume : AudioListener.volume);
        volumeSlider.RegisterValueChangedCallback(evt => { AudioListener.volume = evt.newValue; PlayerPrefs.SetFloat("MuseumVolume", evt.newValue); if (AudioManager.Instance != null) AudioManager.Instance.MasterVolume = evt.newValue; });
        settingsCard.Add(sensitivitySlider); settingsCard.Add(volumeSlider); settingsCard.Add(Button("VOLVER A PAUSA", () => ShowSettings(false), true));
        settingsOverlay.Add(settingsCard); root.Add(settingsOverlay); settingsOverlay.style.display = DisplayStyle.None;

        infoOverlay.style.display = DisplayStyle.None; SetInputEnabled(true); UnityEngine.Cursor.lockState = CursorLockMode.Locked; UnityEngine.Cursor.visible = false;
    }

    private void OpenPaint(PaintData data)
    {
        if (data == null) return;
        currentPaint = data; currentStatue = null;
        infoKicker.text = "OBRA · COLECCIÓN PERMANENTE"; infoTitle.text = data.PaintTitle; infoMeta.text = data.Author; infoBody.text = data.Description;
        infoImage.image = data.ArtworkImage != null ? data.ArtworkImage : (data.AuthorImage != null ? data.AuthorImage.texture : null);
        infoOverlay.style.display = DisplayStyle.Flex; infoOpen = true; SetInputEnabled(false); UnityEngine.Cursor.lockState = CursorLockMode.None; UnityEngine.Cursor.visible = true;
    }

    private void OpenStatue(StatueData data)
    {
        if (data == null) return;
        currentStatue = data; currentPaint = null;
        infoKicker.text = "ESCULTURA · COLECCIÓN PERMANENTE"; infoTitle.text = data.StatueName;
        infoMeta.text = string.IsNullOrWhiteSpace(data.CultureAndOrigin) ? data.Period : data.CultureAndOrigin + (string.IsNullOrWhiteSpace(data.Period) ? "" : "  ·  " + data.Period);
        infoBody.text = data.Description; infoImage.image = data.Image;
        infoOverlay.style.display = DisplayStyle.Flex; infoOpen = true; SetInputEnabled(false); UnityEngine.Cursor.lockState = CursorLockMode.None; UnityEngine.Cursor.visible = true;
    }

    private void CloseInfo()
    {
        if (!infoOpen) return;
        infoOverlay.style.display = DisplayStyle.None; infoOpen = false; SetInputEnabled(true); UnityEngine.Cursor.lockState = CursorLockMode.Locked; UnityEngine.Cursor.visible = false;
        currentPaint = null; currentStatue = null;
    }

    private void TogglePause()
    {
        if (infoOpen) { CloseInfo(); return; }
        isPaused = !isPaused; Time.timeScale = isPaused ? 0f : 1f;
        pauseOverlay.style.display = isPaused ? DisplayStyle.Flex : DisplayStyle.None;
        if (isPaused) { SetInputEnabled(false); UnityEngine.Cursor.lockState = CursorLockMode.None; UnityEngine.Cursor.visible = true; }
        else { SetInputEnabled(true); UnityEngine.Cursor.lockState = CursorLockMode.Locked; UnityEngine.Cursor.visible = false; }
    }

    private void ShowSettings(bool show)
    {
        settingsOpen = show; settingsOverlay.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        pauseOverlay.style.display = show ? DisplayStyle.None : (isPaused ? DisplayStyle.Flex : DisplayStyle.None);
    }

    private void ExitToMenu()
    {
        Time.timeScale = 1f; SetInputEnabled(true); SceneManager.LoadScene("StartMenu");
    }

    private void SetInputEnabled(bool enabled)
    {
        if (player == null && GameManager.Instance != null) player = GameManager.Instance.PlayerController;
        if (cameraController == null && GameManager.Instance != null) cameraController = GameManager.Instance.CameraController;
        if (interactor == null && GameManager.Instance != null) interactor = GameManager.Instance.PlayerInteractor;
        if (player != null) player.SetCanMove(enabled);
        if (cameraController != null) cameraController.enabled = enabled;
        if (interactor != null) interactor.enabled = enabled;
    }

    private void SetPrompt(string text)
    {
        if (prompt == null) return;
        bool visible = !string.IsNullOrEmpty(text) && !infoOpen && !isPaused;
        prompt.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        promptLabel.text = text;
    }

    private static VisualElement Overlay()
    {
        var e = new VisualElement(); e.style.position = Position.Absolute; e.style.left = 0; e.style.right = 0; e.style.top = 0; e.style.bottom = 0;
        e.style.alignItems = Align.Center; e.style.justifyContent = Justify.Center; e.style.backgroundColor = new Color(.025f,.022f,.018f,.83f); return e;
    }

    private static VisualElement Card()
    {
        var e = new VisualElement(); e.style.width = Length.Percent(82); e.style.maxWidth = 1050; e.style.maxHeight = Length.Percent(82);
        e.style.backgroundColor = new Color(.085f,.072f,.056f,1); e.style.borderTopWidth = 1; e.style.borderBottomWidth = 1; e.style.borderLeftWidth = 1; e.style.borderRightWidth = 1;
        var gold = new Color(.72f,.55f,.29f,1); e.style.borderTopColor = gold; e.style.borderBottomColor = gold; e.style.borderLeftColor = gold; e.style.borderRightColor = gold; return e;
    }

    private static Label Text(string value, int size, Color color, bool bold)
    {
        var label = new Label(value); label.style.fontSize = size; label.style.color = color; label.style.unityFontStyleAndWeight = bold ? FontStyle.Bold : FontStyle.Normal; label.style.marginBottom = 10; return label;
    }

    private static Button Button(string title, Action action, bool primary)
    {
        var button = new Button(() => { if (AudioManager.Instance != null) AudioManager.Instance.PlayUIClick(); action?.Invoke(); }) { text = title }; button.style.minHeight = 46; button.style.marginTop = 9; button.style.paddingLeft = 18; button.style.paddingRight = 18;
        button.style.borderTopWidth = 1; button.style.borderBottomWidth = 1; button.style.borderLeftWidth = 1; button.style.borderRightWidth = 1;
        Color gold = new Color(.78f,.62f,.34f,1); button.style.borderTopColor = gold; button.style.borderBottomColor = gold; button.style.borderLeftColor = gold; button.style.borderRightColor = gold;
        button.style.backgroundColor = primary ? gold : new Color(.11f,.095f,.076f,1); button.style.color = primary ? new Color(.1f,.075f,.045f) : new Color(.91f,.86f,.76f); return button;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (Time.timeScale == 0f) Time.timeScale = 1f;
    }
}

