using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum DialoguePortraitSide
{
    None,
    Left,
    Right
}

public sealed class DialogueUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Text")]
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueBodyText;

    [Header("Portraits")]
    [SerializeField] private Image leftPortraitImage;
    [SerializeField] private Image rightPortraitImage;
    [SerializeField, Range(0f, 1f)] private float inactivePortraitAlpha = 0.45f;

    [Header("Choices")]
    [SerializeField] private Transform choiceContainer;
    [SerializeField] private DialogueChoiceButton choiceButtonPrefab;

    [Header("Continue Indicator")]
    [SerializeField] private GameObject continueIndicator;

    [Header("Portrait Dimming")]
    [SerializeField] private GameObject leftDimArea;
    [SerializeField] private GameObject rightDimArea;

    private readonly List<DialogueChoiceButton> spawnedChoiceButtons = new();

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        Hide();
    }

    public void Show(
        string speakerName,
        string body,
        Sprite leftPortrait,
        Sprite rightPortrait,
        DialoguePortraitSide activePortraitSide,
        IReadOnlyList<string> choiceTexts,
        Action<int> onChoiceSelected
        
    )
    {
        if (speakerNameText != null)
            speakerNameText.text = speakerName;

        if (dialogueBodyText != null)
            dialogueBodyText.text = body;

        SetPortrait(leftPortraitImage, leftPortrait, activePortraitSide == DialoguePortraitSide.Left);
        SetPortrait(rightPortraitImage, rightPortrait, activePortraitSide == DialoguePortraitSide.Right);
        SetDimAreas(activePortraitSide);
        RebuildChoices(choiceTexts, onChoiceSelected);

        bool hasChoices = choiceTexts != null && choiceTexts.Count > 0;

        if (continueIndicator != null)
            continueIndicator.SetActive(!hasChoices);

        SetVisible(true);
    }

    public void Hide()
    {
        ClearChoices();
        SetVisible(false);
    }
    private void SetDimAreas(DialoguePortraitSide activePortraitSide)
    {
        if (leftDimArea != null)
            leftDimArea.SetActive(activePortraitSide == DialoguePortraitSide.Right);

        if (rightDimArea != null)
            rightDimArea.SetActive(activePortraitSide == DialoguePortraitSide.Left);
    }
    private void SetVisible(bool visible)
    {
        if (canvasGroup == null)
        {
            gameObject.SetActive(visible);
            return;
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private void SetPortrait(Image portraitImage, Sprite portraitSprite, bool isActiveSpeaker)
    {
        if (portraitImage == null)
            return;

        if (portraitSprite == null)
        {
            portraitImage.gameObject.SetActive(false);
            return;
        }

        portraitImage.gameObject.SetActive(true);
        portraitImage.sprite = portraitSprite;

        Color color = portraitImage.color;
        color.a = isActiveSpeaker ? 1f : inactivePortraitAlpha;
        portraitImage.color = color;
    }

    private void RebuildChoices(IReadOnlyList<string> choiceTexts, Action<int> onChoiceSelected)
    {
        ClearChoices();

        if (choiceTexts == null || choiceTexts.Count == 0)
            return;

        if (choiceContainer == null)
        {
            Debug.LogError("DialogueUI is missing Choice Container reference.");
            return;
        }

        if (choiceButtonPrefab == null)
        {
            Debug.LogError("DialogueUI is missing Choice Button Prefab reference.");
            return;
        }

        for (int i = 0; i < choiceTexts.Count; i++)
        {
            DialogueChoiceButton button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.Initialize(choiceTexts[i], i, onChoiceSelected);
            spawnedChoiceButtons.Add(button);
        }
    }

    private void ClearChoices()
    {
        for (int i = spawnedChoiceButtons.Count - 1; i >= 0; i--)
        {
            if (spawnedChoiceButtons[i] != null)
                Destroy(spawnedChoiceButtons[i].gameObject);
        }

        spawnedChoiceButtons.Clear();
    }
}