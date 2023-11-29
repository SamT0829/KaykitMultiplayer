using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private Dictionary<string, AudioClip> _audioClipTable = new();

    private AudioSource ambientSource;
    private AudioSource musicSource;
    private AudioSource fxSource;
    private AudioSource playerSource;
    private AudioSource voiceSource;

    public static AudioManager Instance;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ambientSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();
        fxSource = gameObject.AddComponent<AudioSource>();
        playerSource = gameObject.AddComponent<AudioSource>();
        voiceSource = gameObject.AddComponent<AudioSource>();

        SetAudioClipFromResource();
    }

    private void SetAudioClipFromResource()
    {
        // AudioClip t = (AudioClip)AssetDatabase.LoadAssetAtPath("Assets/Audio/AudioName", typeof(AudioClip));
        // AudioClip clip = Resources.Load("AudioName") as AudioClip;
        var guids = AssetDatabase.FindAssets("t:AudioClip", new string[] { "Assets/Audio" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AudioClip audio = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            _audioClipTable.Add(audio.name, audio);
        }
    }

    public void SetAudioClip(string audioName, AudioClip audioClip)
    {
        if (!_audioClipTable.ContainsKey(audioName))
        {
            _audioClipTable.Add(audioName, audioClip);
        }
        else
        {
            _audioClipTable[audioName] = audioClip;
        }
    }

    public void PlayAmbientAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            ambientSource.Stop();
            ambientSource.clip = audioClip;
            ambientSource.Play();
        }
    }

    public void StopAmbientAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            ambientSource.Stop();
        }
    }


    public void PlayMusicAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            musicSource.Stop();
            musicSource.clip = audioClip;
            musicSource.Play();
        }
    }

    public void StopMusicAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            musicSource.Stop();
        }
    }

    public void PlayPlayerAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            playerSource.Stop();
            playerSource.clip = audioClip;
            playerSource.Play();
        }
    }

    public void StopPlayerAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            playerSource.Stop();
        }
    }

    public void PlayFXAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            fxSource.Stop();
            fxSource.clip = audioClip;
            fxSource.Play();
        }
    }

    public void StopFXAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            fxSource.Stop();
        }
    }

    public void PlayVoiceAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            voiceSource.Stop();
            voiceSource.clip = audioClip;
            voiceSource.Play();
        }
    }

    public void StopVoiceAudio(string audioName)
    {
        if (_audioClipTable.TryGetValue(audioName, out AudioClip audioClip))
        {
            voiceSource.Stop();
        }
    }
}
