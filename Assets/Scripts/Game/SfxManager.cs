using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : MonoBehaviour
{
    public static SfxManager Instance { get; private set; }

    private AudioSource _audioSource;

    [Header("SFX Clips")]
    public AudioClip nextButtonClip; // Next按钮音效
    public AudioClip pouringClip;    // 倒酒音效 (Shake结束)
    public AudioClip errorClip;      // 错误提示音效
    public AudioClip iceDropClip;    // 冰块进杯音效 (选完杯子)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayNextButtonSfx()
    {
        if (nextButtonClip != null) _audioSource.PlayOneShot(nextButtonClip);
    }

    public void PlayPouringSfx()
    {
        if (pouringClip != null) _audioSource.PlayOneShot(pouringClip);
    }

    public void PlayErrorSfx()
    {
        if (errorClip != null) _audioSource.PlayOneShot(errorClip);
    }

    public void PlayIceDropSfx()
    {
        if (iceDropClip != null) _audioSource.PlayOneShot(iceDropClip);
    }
}