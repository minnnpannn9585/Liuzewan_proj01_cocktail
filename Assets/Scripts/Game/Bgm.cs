using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Bgm : MonoBehaviour
{
    public static Bgm Instance { get; private set; }

    private AudioSource _audioSource;

    [Header("BGM Clips")]
    [Tooltip("游戏开始时播放的背景音乐")]
    public AudioClip startBgm;
    [Tooltip("过程动画结束后切换的游戏背景音乐")]
    public AudioClip gameBgm;

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

    private void Start()
    {
        if (_audioSource != null && startBgm != null)
        {
            _audioSource.clip = startBgm;
            _audioSource.loop = true;
            _audioSource.Play();
        }
    }

    /// <summary>
    /// 在过程动画结束后调用此方法切换背景音乐
    /// </summary>
    public void SwitchToGameBgm()
    {
        if (_audioSource != null && gameBgm != null && _audioSource.clip != gameBgm)
        {
            _audioSource.clip = gameBgm;
            _audioSource.Play();
        }
    }
}