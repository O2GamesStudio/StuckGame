using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("AudioSource Pool Settings")]
    [SerializeField] int initialPoolSize = 10;
    [SerializeField] int maxPoolSize = 20;

    [Header("Game SFX Clips")]
    [SerializeField] AudioClip targetPointCompleteClip;
    [SerializeField] AudioClip stageCompleteClip;
    [SerializeField] AudioClip gameOverClip;
    [SerializeField] AudioClip buttonClickClip;
    [SerializeField] AudioClip uiClickClip;

    [Header("BGM Clips")]
    [SerializeField] AudioClip gameBGM;
    [SerializeField] AudioClip menuBGM;

    [Header("Sound Settings")]
    private bool isSoundEnabled = true;
    private bool isMusicEnabled = true;

    // AudioSource Pool
    private Queue<AudioSource> audioSourcePool = new Queue<AudioSource>();
    private List<AudioSource> activeAudioSources = new List<AudioSource>();
    private AudioSource bgmSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeSoundManager();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        Application.targetFrameRate = 60;
    }

    void InitializeSoundManager()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
        bgmSource = gameObject.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = 1f;
        bgmSource.playOnAwake = false;

        isSoundEnabled = PlayerPrefs.GetInt("IsSoundOn", 1) == 1;
        isMusicEnabled = PlayerPrefs.GetInt("IsMusicOn", 1) == 1;
    }

    AudioSource CreateNewAudioSource()
    {
        GameObject audioObj = new GameObject("PooledAudioSource");
        audioObj.transform.SetParent(transform);
        AudioSource audioSource = audioObj.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.volume = 1f;
        audioSourcePool.Enqueue(audioSource);
        return audioSource;
    }

    AudioSource GetAudioSource()
    {
        AudioSource audioSource;

        while (audioSourcePool.Count > 0)
        {
            audioSource = audioSourcePool.Dequeue();
            if (audioSource != null && !audioSource.isPlaying)
            {
                activeAudioSources.Add(audioSource);
                return audioSource;
            }
        }

        if (activeAudioSources.Count < maxPoolSize)
        {
            audioSource = CreateNewAudioSource();
            audioSourcePool.Dequeue(); // 방금 추가된 것을 빼냄
            activeAudioSources.Add(audioSource);
            return audioSource;
        }

        audioSource = activeAudioSources[0];
        audioSource.Stop();
        return audioSource;
    }

    void ReturnAudioSource(AudioSource audioSource)
    {
        if (audioSource == null) return;

        audioSource.clip = null;
        audioSource.Stop();
        activeAudioSources.Remove(audioSource);

        if (!audioSourcePool.Contains(audioSource))
        {
            audioSourcePool.Enqueue(audioSource);
        }
    }

    public void SetSoundEnabled(bool enabled)
    {
        isSoundEnabled = enabled;
        PlayerPrefs.SetInt("IsSoundOn", enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (!enabled)
        {
            StopAllSFX();
        }
    }

    public void SetMusicEnabled(bool enabled)
    {
        isMusicEnabled = enabled;
        PlayerPrefs.SetInt("IsMusicOn", enabled ? 1 : 0);
        PlayerPrefs.Save();

        if (bgmSource != null)
        {
            if (enabled)
            {
                if (!bgmSource.isPlaying && bgmSource.clip != null)
                    bgmSource.Play();
            }
            else
            {
                bgmSource.Pause();
            }
        }
    }

    public bool IsSoundEnabled() => isSoundEnabled;
    public bool IsMusicEnabled() => isMusicEnabled;

    // ==================== Game SFX Methods ====================

    /// <summary>
    /// 타겟 포인트 완료 시 재생
    /// </summary>
    public void PlayTargetPointCompleteSFX()
    {
        PlaySFX(targetPointCompleteClip);
    }

    /// <summary>
    /// 스테이지 완료 시 재생
    /// </summary>
    public void PlayStageCompleteSFX()
    {
        PlaySFX(stageCompleteClip);
    }

    /// <summary>
    /// 게임 오버 시 재생
    /// </summary>
    public void PlayGameOverSFX()
    {
        PlaySFX(gameOverClip);
    }

    /// <summary>
    /// 버튼 클릭 시 재생
    /// </summary>
    public void PlayButtonClickSFX()
    {
        PlaySFX(buttonClickClip);
    }

    /// <summary>
    /// UI 클릭 시 재생
    /// </summary>
    public void PlayUIClickSFX()
    {
        PlaySFX(uiClickClip);
    }

    // ==================== Generic SFX Methods ====================

    /// <summary>
    /// 일반적인 SFX 재생 (AudioClip 직접 전달)
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        if (!isSoundEnabled) return;

        if (clip == null)
        {
            Debug.LogWarning("재생할 AudioClip이 null입니다!");
            return;
        }

        AudioSource audioSource = GetAudioSource();
        audioSource.clip = clip;
        audioSource.volume = 1f;
        audioSource.Play();

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    /// <summary>
    /// OneShot 방식으로 SFX 재생 (여러 사운드 동시 재생 가능)
    /// </summary>
    public void PlayOneShotSFX(AudioClip clip)
    {
        if (!isSoundEnabled) return;
        if (clip == null) return;

        AudioSource audioSource = GetAudioSource();
        audioSource.PlayOneShot(clip, 1f);

        StartCoroutine(ReturnToPoolAfterPlay(audioSource, clip.length));
    }

    IEnumerator ReturnToPoolAfterPlay(AudioSource audioSource, float delay)
    {
        yield return new WaitForSeconds(delay + 0.1f);
        ReturnAudioSource(audioSource);
    }

    // ==================== BGM Methods ====================

    /// <summary>
    /// 게임 BGM 재생
    /// </summary>
    public void PlayGameBGM()
    {
        PlayBGM(gameBGM);
    }

    /// <summary>
    /// 메뉴 BGM 재생
    /// </summary>
    public void PlayMenuBGM()
    {
        PlayBGM(menuBGM);
    }

    /// <summary>
    /// BGM 재생 (일반)
    /// </summary>
    void PlayBGM(AudioClip clip)
    {
        if (!isMusicEnabled) return;
        if (clip == null) return;

        // 같은 곡이 이미 재생 중이면 무시
        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    /// <summary>
    /// BGM 정지
    /// </summary>
    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }

    /// <summary>
    /// BGM 일시정지
    /// </summary>
    public void PauseBGM()
    {
        if (bgmSource != null)
            bgmSource.Pause();
    }

    /// <summary>
    /// BGM 재개
    /// </summary>
    public void ResumeBGM()
    {
        if (bgmSource != null && isMusicEnabled)
            bgmSource.UnPause();
    }

    /// <summary>
    /// BGM 페이드 인/아웃
    /// </summary>
    public void FadeBGM(float targetVolume, float duration)
    {
        StartCoroutine(FadeBGMCoroutine(targetVolume, duration));
    }

    IEnumerator FadeBGMCoroutine(float targetVolume, float duration)
    {
        float startVolume = bgmSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            bgmSource.volume = Mathf.Lerp(startVolume, targetVolume, elapsed / duration);
            yield return null;
        }

        bgmSource.volume = targetVolume;
    }

    // ==================== Utility Methods ====================

    /// <summary>
    /// 모든 SFX 정지
    /// </summary>
    public void StopAllSFX()
    {
        foreach (AudioSource source in activeAudioSources)
        {
            if (source != null && source.isPlaying)
            {
                source.Stop();
            }
        }

        // 모든 활성 소스를 풀로 반환
        while (activeAudioSources.Count > 0)
        {
            ReturnAudioSource(activeAudioSources[0]);
        }
    }

    /// <summary>
    /// 풀 상태 디버그 출력
    /// </summary>
    public void PrintPoolStatus()
    {
        Debug.Log($"AudioSource 풀 상태 - 사용 가능: {audioSourcePool.Count}, 활성: {activeAudioSources.Count}");
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}