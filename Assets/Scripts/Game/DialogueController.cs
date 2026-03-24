using System;
using TMPro;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue Lines")]
    [TextArea(2, 4)]
    [SerializeField] private string[] lines;

    private int _index;
    private Action _onFinished;

    public string[] GetConfiguredLinesCopy()
    {
        return lines == null ? Array.Empty<string>() : (string[])lines.Clone();
    }

    private void Awake()
    {
        Hide();
    }

    // 播放 Inspector 中配置的对话
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

    private void Update()
    {
        if (dialoguePanel == null || !dialoguePanel.activeSelf)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Next();
        }
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
        if (dialogueText != null)
            dialogueText.text = lines[_index];
    }

    private void Finish()
    {
        Hide();
        _onFinished?.Invoke();
        _onFinished = null;
    }

    public void Show()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
    }

    public void Hide()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
    }
}