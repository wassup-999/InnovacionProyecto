using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

/// <summary>
/// Controla el menú de inicio construido con UI Toolkit.
/// Se encarga de la navegación, el panel de ayuda y la transición
/// hacia la escena del museo.
/// </summary>
[RequireComponent(typeof(UIDocument))]
public class StartMenuController : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Nombre exacto de la escena que se carga al comenzar (debe estar en Build Settings).")]
    [SerializeField] private string nextSceneName = "Npc";

    [Tooltip("Duración del fundido a negro antes de cargar la escena.")]
    [SerializeField] private float fadeDuration = 0.45f;

    private UIDocument document;
    private VisualElement screen;
    private VisualElement helpOverlay;

    private Button startButton;
    private Button helpButton;
    private Button helpCloseButton;
    private Button quitButton;

    private bool isTransitioning;

    private const string HiddenClass = "hidden";
    private const string FadeOutClass = "fade-out";

    private void Awake()
    {
        document = GetComponent<UIDocument>();
    }

    private void OnDisable()
    {
        UnbindUI();
    }

    private void Start()
    {
        // El UIDocument crea su árbol en OnEnable; enlazamos en Start para
        // garantizar que rootVisualElement ya existe.
        BindUI();

        // Foco inicial para navegación con teclado/gamepad.
        startButton?.Focus();
    }

    private void BindUI()
    {
        if (document == null)
        {
            Debug.LogError("[StartMenu] Falta el componente UIDocument.", this);
            return;
        }

        VisualElement root = document.rootVisualElement;
        if (root == null)
        {
            Debug.LogError("[StartMenu] UIDocument sin rootVisualElement. Revisa el PanelSettings y el UXML asignados.", this);
            return;
        }

        // El root del UIDocument no se estira por defecto (crece solo hasta su
        // contenido): sin esto el menú se ancla arriba y deja una banda sin
        // cubrir en 16:9. Lo forzamos a ocupar todo el panel.
        root.style.position = Position.Absolute;
        root.style.left = 0f;
        root.style.top = 0f;
        root.style.right = 0f;
        root.style.bottom = 0f;

        screen = root.Q<VisualElement>("screen");
        helpOverlay = root.Q<VisualElement>("help-overlay");

        startButton = root.Q<Button>("start-button");
        helpButton = root.Q<Button>("help-button");
        helpCloseButton = root.Q<Button>("help-close-button");
        quitButton = root.Q<Button>("quit-button");

        if (startButton != null) startButton.clicked += OnStartClicked;
        if (helpButton != null) helpButton.clicked += OpenHelp;
        if (helpCloseButton != null) helpCloseButton.clicked += CloseHelp;
        if (quitButton != null) quitButton.clicked += OnQuitClicked;

        // Menú: cursor libre y visible.
        UnityEngine.Cursor.lockState = CursorLockMode.None;
        UnityEngine.Cursor.visible = true;
    }

    private void UnbindUI()
    {
        if (startButton != null) startButton.clicked -= OnStartClicked;
        if (helpButton != null) helpButton.clicked -= OpenHelp;
        if (helpCloseButton != null) helpCloseButton.clicked -= CloseHelp;
        if (quitButton != null) quitButton.clicked -= OnQuitClicked;
    }

    private void Update()
    {
        if (isTransitioning) return;

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame &&
            helpOverlay != null &&
            !helpOverlay.ClassListContains(HiddenClass))
        {
            CloseHelp();
        }
    }

    private void OpenHelp()
    {
        helpOverlay?.RemoveFromClassList(HiddenClass);
        helpCloseButton?.Focus();
    }

    private void CloseHelp()
    {
        helpOverlay?.AddToClassList(HiddenClass);
        startButton?.Focus();
    }

    private void OnStartClicked()
    {
        if (isTransitioning) return;
        StartCoroutine(LoadTourRoutine());
    }

    private IEnumerator LoadTourRoutine()
    {
        isTransitioning = true;

        if (startButton != null) startButton.SetEnabled(false);
        if (helpButton != null) helpButton.SetEnabled(false);
        if (quitButton != null) quitButton.SetEnabled(false);

        screen?.AddToClassList(FadeOutClass);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogError("[StartMenu] No hay escena configurada en 'nextSceneName'.", this);
            isTransitioning = false;
            screen?.RemoveFromClassList(FadeOutClass);
            yield break;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
