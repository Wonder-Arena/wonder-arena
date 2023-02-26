using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class AudioManager : MonoBehaviour
{
    // Audio source and clip references
    private AudioSource audioSource;
    private AudioClip currentClip;

    // Music tracks for each scene
    [SerializeField] AudioClip starterMusic;
    [SerializeField] AudioClip basicMusic;
    [SerializeField] AudioClip fightMusic;
    [SerializeField] AudioClip endMusic;

    public static AudioManager Instance { get; set; }
    private void Awake()
    {
        // Make our Instance only for and for all scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        // Get the audio source component and set the current clip
        audioSource = GetComponent<AudioSource>();
        currentClip = audioSource.clip;
    }

    private void OnEnable()
    {
        // Listen for scene changes
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Stop listening for scene changes
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Change the music based on scene name
        switch (scene.name)
        {
            case "ConnectingWallet":
                SetMusic(starterMusic);
                break;
            case "MainMenu":
                SetMusic(basicMusic);
                break;
            case "FightScene":
                SetMusic(fightMusic);
                break;
            default:
                SetMusic(basicMusic);
                break;
        }
    }

    private void SetMusic(AudioClip clip)
    {
        // If the current clip is the same as the new clip, do nothing
        if (currentClip == clip) return;

        // Otherwise, set the new clip and play it
        audioSource.clip = clip;
        audioSource.Play();

        // Update the current clip reference
        currentClip = clip;
    }
}