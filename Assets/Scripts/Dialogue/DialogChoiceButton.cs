using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public sealed class DialogueChoiceButton : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text choiceText;
    [SerializeField] private Button button;
    [SerializeField] private Image backgroundImage;

    [Header("Default Visuals")]
    [SerializeField] private bool applyDefaultVisualStyle = true;
    [SerializeField] private Color normalColor = new(0.12f, 0.14f, 0.18f, 1f);
    [SerializeField] private Color highlightedColor = new(0.22f, 0.25f, 0.32f, 1f);
    [SerializeField] private Color pressedColor = new(0.08f, 0.09f, 0.12f, 1f);
    [SerializeField] private Color selectedColor = new(0.22f, 0.25f, 0.32f, 1f);
    [SerializeField] private Color disabledColor = new(0.25f, 0.25f, 0.25f, 0.45f);
    [SerializeField] private Color textColor = Color.white;

    private int choiceIndex;
    private Action<int> onClicked;

    private void Awake()
    {
        CacheReferences();
        ApplyVisualStyle();
    }

    private void OnValidate()
    {
        CacheReferences();
        ApplyVisualStyle();
    }

    public void Initialize(string text, int index, Action<int> clickCallback)
    {
        CacheReferences();
        ApplyVisualStyle();

        choiceIndex = index;
        onClicked = clickCallback;

        if (choiceText != null)
            choiceText.text = text;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(HandleClicked);
    }

    private void HandleClicked()
    {
        onClicked?.Invoke(choiceIndex);
    }

    private void CacheReferences()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        if (choiceText == null)
            choiceText = GetComponentInChildren<TMP_Text>(true);
    }

    private void ApplyVisualStyle()
    {
        if (!applyDefaultVisualStyle)
            return;

        if (backgroundImage != null)
            backgroundImage.color = normalColor;

        if (choiceText != null)
            choiceText.color = textColor;

        if (button == null)
            return;

        button.targetGraphic = backgroundImage;

        ColorBlock colors = button.colors;
        colors.normalColor = normalColor;
        colors.highlightedColor = highlightedColor;
        colors.pressedColor = pressedColor;
        colors.selectedColor = selectedColor;
        colors.disabledColor = disabledColor;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
    }
}