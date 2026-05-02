using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM Clips")]
    [SerializeField] private AudioClip lobbyBGM;
    [SerializeField] private AudioClip gameBGM;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip buttonClickSFX;

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
        }
    }

    public void PlayLobbyBGM() => PlayBGM(lobbyBGM);
    public void PlayGameBGM() => PlayBGM(gameBGM);
    public void StopBGM() => bgmSource.Stop();

    public void PlayButtonClick() => sfxSource.PlayOneShot(buttonClickSFX);

    public void SetBGMVolume(float volume) => bgmSource.volume = Mathf.Clamp01(volume);
    public void SetSFXVolume(float volume) => sfxSource.volume = Mathf.Clamp01(volume);

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null || bgmSource.clip == clip) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }
}