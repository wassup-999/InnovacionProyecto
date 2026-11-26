using UnityEngine;

[DisallowMultipleComponent]
public sealed class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [Header("Asigna tus propios audios desde el Inspector")]
    [SerializeField] private AudioClip ambientLoop;
    [SerializeField] private AudioClip uiClick;
    [Range(0f, 1f)] [SerializeField] private float masterVolume = .75f;
    private AudioSource musicSource;
    private AudioSource effectsSource;

    public float MasterVolume
    {
        get => masterVolume;
        set { masterVolume = Mathf.Clamp01(value); AudioListener.volume = masterVolume; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.clip = ambientLoop;
        effectsSource = gameObject.AddComponent<AudioSource>();
        effectsSource.playOnAwake = false;
        effectsSource.spatialBlend = 0f;
        masterVolume = PlayerPrefs.GetFloat("MuseumVolume", masterVolume);
        AudioListener.volume = masterVolume;
        if (ambientLoop != null) musicSource.Play();
    }

    public void PlayAmbient(AudioClip clip, bool loop = true)
    {
        musicSource.clip = clip;
        musicSource.loop = loop;
        if (clip == null) musicSource.Stop(); else musicSource.Play();
    }

    public void PlayEffect(AudioClip clip)
    {
        if (clip != null) effectsSource.PlayOneShot(clip);
    }

    public void PlayUIClick()
    {
        if (uiClick != null) PlayEffect(uiClick);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
