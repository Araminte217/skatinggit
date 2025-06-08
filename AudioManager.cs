using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        [Range(0.1f, 3f)]
        public float pitch = 1f;
    }

    [SerializeField] private Sound[] sounds;
    [SerializeField] private AudioSource musicSource;

    private Dictionary<string, AudioSource> audioSources = new Dictionary<string, AudioSource>();

    void Awake()
    {
        // ����ģʽ
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

        // ��ʼ����Ч
        foreach (Sound sound in sounds)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.clip = sound.clip;
            source.volume = sound.volume;
            source.pitch = sound.pitch;

            audioSources.Add(sound.name, source);
        }
    }

    public void PlaySound(string name)
    {
        if (audioSources.ContainsKey(name))
        {
            audioSources[name].Play();
        }
        else
        {
            Debug.LogWarning($"Sound {name} not found in AudioManager");
        }
    }

    public void PlayMusic(AudioClip music)
    {
        if (musicSource != null && music != null)
        {
            musicSource.clip = music;
            musicSource.Play();
        }
    }
    public void SetPaused(bool paused)
    {
        // ��ͣ/�ָ���������
        if (musicSource != null)
        {
            if (paused)
            {
                // ������ڲ��ţ�����ͣ
                if (musicSource.isPlaying)
                {
                    musicSource.Pause();
                }
            }
            else
            {
                // �������ͣ����ָ�����
                if (!musicSource.isPlaying && musicSource.time > 0)
                {
                    musicSource.UnPause();
                }
            }
        }

        // ��ͣ/�ָ�������Ч
        foreach (var source in audioSources.Values)
        {
            if (paused)
            {
                // ������ڲ��ţ�����ͣ
                if (source.isPlaying)
                {
                    source.Pause();
                }
            }
            else
            {
                // �������ͣ����ָ�����
                if (!source.isPlaying && source.time > 0)
                {
                    source.UnPause();
                }
            }
        }

        // ȫ����Ƶ����(��ѡ)
        AudioListener.pause = paused;
    }
}