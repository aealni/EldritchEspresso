using UnityEngine;

/// <summary>
/// Manages background music playback. Persists across scenes.
/// </summary>
public class BackgroundMusicManager : MonoBehaviour
{
    [Header("Music Settings")]
    [SerializeField] private AudioClip backgroundMusic;
    [SerializeField] private float volume = 0.5f;
    [SerializeField] private bool playOnAwake = true;
    
    private AudioSource audioSource;
    private static BackgroundMusicManager instance;
    
    private void Awake()
    {
        // Singleton pattern - ensure only one music manager exists
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        audioSource.clip = backgroundMusic;
        audioSource.volume = volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        
        if (playOnAwake && backgroundMusic != null)
        {
            Play();
        }
    }
    
    /// <summary>
    /// Play the background music
    /// </summary>
    public void Play()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }
    
    /// <summary>
    /// Stop the background music
    /// </summary>
    public void Stop()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
    
    /// <summary>
    /// Pause the background music
    /// </summary>
    public void Pause()
    {
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }
    
    /// <summary>
    /// Resume the background music
    /// </summary>
    public void Resume()
    {
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }
    }
    
    /// <summary>
    /// Set the volume of the background music
    /// </summary>
    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
    
    /// <summary>
    /// Change the background music track
    /// </summary>
    public void ChangeMusic(AudioClip newClip)
    {
        if (audioSource != null && newClip != null)
        {
            bool wasPlaying = audioSource.isPlaying;
            audioSource.Stop();
            audioSource.clip = newClip;
            backgroundMusic = newClip;
            
            if (wasPlaying)
            {
                audioSource.Play();
            }
        }
    }
}
