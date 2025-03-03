using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField]
    private AudioSource _effectSource;

    [SerializeField]
    private AudioSource _musicSource; // New AudioSource for music

    [SerializeField]
    private AudioClip _clickSound;

    [SerializeField]
    private AudioClip _backgroundMusic; // New AudioClip for background music

    private bool isSoundMuted;
    private bool IsSoundMuted
    {
        get
        {
            isSoundMuted = (PlayerPrefs.HasKey(Constants.Data.SETTINGS_SOUND)
                ? PlayerPrefs.GetInt(Constants.Data.SETTINGS_SOUND) : 1) == 0;
            return isSoundMuted;
        }
        set
        {
            isSoundMuted = value;
            PlayerPrefs.SetInt(Constants.Data.SETTINGS_SOUND, isSoundMuted ? 0 : 1);
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        PlayerPrefs.SetInt(Constants.Data.SETTINGS_SOUND, IsSoundMuted ? 0 : 1);
        _effectSource.mute = IsSoundMuted;
        _musicSource.mute = IsSoundMuted; // Mute music if sound is muted

        PlayMusic(_backgroundMusic); // Start playing background music
    }

    public void AddButtonSound()
    {
        var buttons = FindObjectsOfType<Button>(true);
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.AddListener(() =>
            {
                PlaySound(_clickSound);
            });
        }
    }

    public void PlaySound(AudioClip clip)
    {
        _effectSource.PlayOneShot(clip);
    }

    public void ToggleSound()
    {
        _effectSource.mute = IsSoundMuted;
        _musicSource.mute = IsSoundMuted; // Mute/unmute music
    }

    public void PlayMusic(AudioClip clip)
    {
        if (_musicSource.clip == clip) return; // Avoid restarting the same music
        _musicSource.clip = clip;
        _musicSource.loop = true; // Loop the music
        _musicSource.Play();
    }

    public void PauseMusic()
    {
        _musicSource.Pause();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
    }
}
