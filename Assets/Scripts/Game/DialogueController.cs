using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI - Speaker A")]
    [SerializeField] private GameObject dialoguePanelA;
    [SerializeField] private TextMeshProUGUI dialogueTextA;

    [Header("UI - Speaker B")]
    [SerializeField] private GameObject dialoguePanelB;
    [SerializeField] private TextMeshProUGUI dialogueTextB;

    [Header("Dialogue Lines Format")]
    [Tooltip("Use prefix like: 'A:xxx' or 'B:xxx'. If missing prefix, defaults to A.")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    [Header("Typewriter")]
    [Tooltip("Characters per second.")]
    [Min(1f)]
    [SerializeField] private float charsPerSecond = 45f;

    private int _index;
    private Action _onFinished;

    private Coroutine _typingCoroutine;
    private bool _isTyping;
    private bool _currentIsSpeakerB;
    private string _currentContent = string.Empty;

    public string[] GetConfiguredLinesCopy()
    {
        return lines == null ? Array.Empty<string>() : (string[])lines.Clone();
    }

    private void Awake()
    {
        Hide();
    }

    public void PlayConfigured(Action onFinished)
    {
        Play(GetConfiguredLinesCopy(), onFinished);
    }

    public void Play(string[] dialogueLines, Action onFinished)
    {
        lines = dialogueLines ?? Array.Empty<string>();
        _onFinished = onFinished;
        _index = 0;

        if (lines.Length == 0)
        {
            Finish();
            return;
        }

        Show();
        RenderCurrentLine();
    }

    // Play dialogue lines from external caller (UIManager), e.g. drink1/drink2 lines.
    public void PlayLines(string[] dialogueLines, Action onFinished)
    {
        Play(dialogueLines, onFinished);
    }

    private void Update()
    {
        if (!IsAnyPanelActive())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            OnClickAdvance();
        }
    }

    private void OnClickAdvance()
    {
        // If still typing -> complete this line instantly
        if (_isTyping)
        {
            CompleteCurrentLineInstant();
            return;
        }

        // Otherwise go next line
        Next();
    }

    private bool IsAnyPanelActive()
    {
        bool a = dialoguePanelA != null && dialoguePanelA.activeSelf;
        bool b = dialoguePanelB != null && dialoguePanelB.activeSelf;
        return a || b;
    }

    private void Next()
    {
        _index++;
        if (_index >= lines.Length)
        {
            Finish();
            return;
        }

        RenderCurrentLine();
    }

    private void RenderCurrentLine()
    {
        StopTypingIfNeeded();

        string raw = lines[_index] ?? string.Empty;
        _currentIsSpeakerB = TryParseSpeakerB(raw, out _currentContent);

        if (_currentIsSpeakerB)
        {
            SetPanelActive(dialoguePanelA, false);
            SetPanelActive(dialoguePanelB, true);
            SetText(dialogueTextB, string.Empty);
        }
        else
        {
            SetPanelActive(dialoguePanelB, false);
            SetPanelActive(dialoguePanelA, true);
            SetText(dialogueTextA, string.Empty);
        }

        _typingCoroutine = StartCoroutine(TypeCurrentLine());
    }

    private IEnumerator TypeCurrentLine()
    {
        _isTyping = true;

        TextMeshProUGUI target = _currentIsSpeakerB ? dialogueTextB : dialogueTextA;
        if (target == null)
        {
            _isTyping = false;
            yield break;
        }

        string text = _currentContent ?? string.Empty;
        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        // Type progressively
        for (int i = 1; i <= text.Length; i++)
        {
            target.text = text.Substring(0, i);
            yield return new WaitForSeconds(delay);
        }

        _isTyping = false;
        _typingCoroutine = null;
    }

    private void CompleteCurrentLineInstant()
    {
        StopTypingIfNeeded();

        TextMeshProUGUI target = _currentIsSpeakerB ? dialogueTextB : dialogueTextA;
        SetText(target, _currentContent);

        _isTyping = false;
    }

    private void StopTypingIfNeeded()
    {
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }
        _isTyping = false;
    }

    private static void SetText(TextMeshProUGUI target, string value)
    {
        if (target != null)
            target.text = value ?? string.Empty;
    }

    // Returns true if this line should be spoken by B. Removes prefix from returned content.
    private static bool TryParseSpeakerB(string raw, out string content)
    {
        raw = raw?.Trim() ?? string.Empty;

        // B:xxxx / b:xxxx
        if (raw.Length >= 2 && (raw[0] == 'B' || raw[0] == 'b') && raw[1] == ':')
        {
            content = raw.Substring(2).TrimStart();
            return true;
        }

        // A:xxxx / a:xxxx
        if (raw.Length >= 2 && (raw[0] == 'A' || raw[0] == 'a') && raw[1] == ':')
        {
            content = raw.Substring(2).TrimStart();
            return false;
        }

        // default speaker A
        content = raw;
        return false;
    }

    private static void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }

    private void Finish()
    {
        StopTypingIfNeeded();
        Hide();
        _onFinished?.Invoke();
        _onFinished = null;
    }

    public void Show()
    {
        if (dialoguePanelA != null) dialoguePanelA.SetActive(true);
        if (dialoguePanelB != null) dialoguePanelB.SetActive(false);
    }

    public void Hide()
    {
        StopTypingIfNeeded();
        if (dialoguePanelA != null) dialoguePanelA.SetActive(false);
        if (dialoguePanelB != null) dialoguePanelB.SetActive(false);
    }
}