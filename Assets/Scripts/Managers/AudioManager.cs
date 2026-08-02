using UnityEngine;
using System.Collections.Generic;

namespace BehindTheScenesFootball.Managers
{
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject obj = new GameObject("AudioManager");
                    instance = obj.AddComponent<AudioManager>();
                    DontDestroyOnLoad(obj);
                }
                return instance;
            }
        }

        private AudioSource musicSource;
        private List<AudioClip> playlist = new List<AudioClip>();
        private int currentTrackIndex = -1;

        private const string MusicEnabledKey = "MusicEnabled";
        private const string MusicVolumeKey = "MusicVolume";

        private bool isMusicEnabled = true;
        private float musicVolume = 0.5f;

        public bool IsMusicEnabled => isMusicEnabled;
        public float MusicVolume => musicVolume;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = false; // We handle track ending manually in Update to change songs!
            musicSource.playOnAwake = false;

            LoadSettings();
            LoadAudioClips();

            if (isMusicEnabled)
            {
                PlayRandomTrack();
            }
        }

        private void Update()
        {
            // If music is enabled, but not playing, automatically shuffle to the next track
            if (isMusicEnabled && musicSource != null && !musicSource.isPlaying && playlist.Count > 0)
            {
                PlayRandomTrack();
            }
        }

        private void LoadSettings()
        {
            isMusicEnabled = PlayerPrefs.GetInt(MusicEnabledKey, 1) == 1;
            musicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, 0.5f);
            musicSource.volume = musicVolume;
            musicSource.mute = !isMusicEnabled;
        }

        private void LoadAudioClips()
        {
            // Dynamic load from Assets/Resources/Audio
            string[] trackNames = { "Highlight_Reel", "Eighties_Action", "Who_Likes_to_Party", "Take_the_Lead", "Future_Gladiator" };
            foreach (var track in trackNames)
            {
                AudioClip clip = Resources.Load<AudioClip>("Audio/" + track);
                if (clip != null)
                {
                    playlist.Add(clip);
                }
                else
                {
                    Debug.LogWarning("Audio clip not found in Resources/Audio: " + track);
                }
            }
        }

        public void SetMusicEnabled(bool enabled)
        {
            isMusicEnabled = enabled;
            PlayerPrefs.SetInt(MusicEnabledKey, enabled ? 1 : 0);
            PlayerPrefs.Save();

            musicSource.mute = !enabled;

            if (enabled)
            {
                if (!musicSource.isPlaying)
                {
                    PlayRandomTrack();
                }
            }
            else
            {
                musicSource.Stop();
            }
        }

        public void SetVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(MusicVolumeKey, musicVolume);
            PlayerPrefs.Save();

            musicSource.volume = musicVolume;
        }

        public void PlayRandomTrack()
        {
            if (playlist.Count == 0) return;

            int newIndex;
            if (playlist.Count == 1)
            {
                newIndex = 0;
            }
            else
            {
                do
                {
                    newIndex = Random.Range(0, playlist.Count);
                } while (newIndex == currentTrackIndex);
            }

            currentTrackIndex = newIndex;
            musicSource.clip = playlist[currentTrackIndex];
            musicSource.Play();
        }
    }
}
