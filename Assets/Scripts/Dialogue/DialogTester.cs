using UnityEngine;

public sealed class DialogueUITester : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private DialogueUI dialogueUI;

    [Header("Test Portraits")]
    [SerializeField] private Sprite playerPortrait;
    [SerializeField] private Sprite kimPortrait;

    [Header("Options")]
    [SerializeField] private bool showOnStart = true;

    private void Start()
    {
        if (showOnStart)
            ShowOpeningNode();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
            ShowOpeningNode();

        if (Input.GetKeyDown(KeyCode.Escape))
            dialogueUI.Hide();
    }

    private void ShowOpeningNode()
    {
        dialogueUI.Show(
            speakerName: "Kim Woo-bin",
            body: "We finally made it to the night market. What Korean food do you want to eat first?",
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: DialoguePortraitSide.Right,
            choiceTexts: new[]
            {
                "Bibimbap sounds good.",
                "I want tteokbokki.",
                "Korean BBQ. Obviously.",
                "I am not that hungry."
            },
            onChoiceSelected: HandleOpeningChoice
        );
    }

    private void HandleOpeningChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                ShowBibimbapNode();
                break;

            case 1:
                ShowTteokbokkiNode();
                break;

            case 2:
                ShowKoreanBbqNode();
                break;

            case 3:
                ShowNotHungryNode();
                break;

            default:
                Debug.LogWarning($"Unhandled opening choice index: {choiceIndex}");
                break;
        }
    }

    private void ShowBibimbapNode()
    {
        dialogueUI.Show(
            speakerName: "Kim Woo-bin",
            body: "Good choice. Rice, vegetables, egg, gochujang... balanced and safe. You are playing this like a strategist.",
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: DialoguePortraitSide.Right,
            choiceTexts: new[]
            {
                "Add extra gochujang.",
                "Keep it mild.",
                "Actually, maybe BBQ."
            },
            onChoiceSelected: HandleBibimbapChoice
        );
    }

    private void HandleBibimbapChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                ShowPlayerLine(
                    "Extra gochujang. I want the real version.",
                    "Kim Woo-bin",
                    "Respect. But do not blame me when you start sweating halfway through."
                );
                break;

            case 1:
                ShowPlayerLine(
                    "Mild is fine. I want to survive dinner.",
                    "Kim Woo-bin",
                    "Reasonable. A peaceful bowl of bibimbap it is."
                );
                break;

            case 2:
                ShowKoreanBbqNode();
                break;
        }
    }

    private void ShowTteokbokkiNode()
    {
        dialogueUI.Show(
            speakerName: "Kim Woo-bin",
            body: "Tteokbokki? Spicy rice cakes at night is dangerous confidence.",
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: DialoguePortraitSide.Right,
            choiceTexts: new[]
            {
                "I can handle spice.",
                "Maybe with fish cakes too.",
                "Wait, how spicy is it?"
            },
            onChoiceSelected: HandleTteokbokkiChoice
        );
    }

    private void HandleTteokbokkiChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                ShowPlayerLine(
                    "I can handle spice.",
                    "Kim Woo-bin",
                    "That is what everyone says before the second bite."
                );
                break;

            case 1:
                ShowPlayerLine(
                    "With fish cakes too.",
                    "Kim Woo-bin",
                    "Now you are ordering correctly."
                );
                break;

            case 2:
                dialogueUI.Show(
                    speakerName: "Kim Woo-bin",
                    body: "Spicy enough to make you reconsider your life, but not enough to ruin the night.",
                    leftPortrait: playerPortrait,
                    rightPortrait: kimPortrait,
                    activePortraitSide: DialoguePortraitSide.Right,
                    choiceTexts: new[]
                    {
                        "Okay, tteokbokki.",
                        "Never mind. Bibimbap."
                    },
                    onChoiceSelected: index =>
                    {
                        if (index == 0)
                            ShowEnding("Kim Woo-bin", "Then tteokbokki it is. Brave choice.");
                        else
                            ShowBibimbapNode();
                    }
                );
                break;
        }
    }

    private void ShowKoreanBbqNode()
    {
        dialogueUI.Show(
            speakerName: "Kim Woo-bin",
            body: "Korean BBQ is the correct answer if you are hungry. Pork belly, beef short rib, lettuce wraps, kimchi, garlic.",
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: DialoguePortraitSide.Right,
            choiceTexts: new[]
            {
                "Pork belly.",
                "Beef short rib.",
                "Let you order for us."
            },
            onChoiceSelected: HandleKoreanBbqChoice
        );
    }

    private void HandleKoreanBbqChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                ShowEnding("Kim Woo-bin", "Samgyeopsal. Classic. I knew you had taste.");
                break;

            case 1:
                ShowEnding("Kim Woo-bin", "Galbi. Expensive choice. I respect it.");
                break;

            case 2:
                ShowEnding("Kim Woo-bin", "Smart. I will order enough food to make this look like a celebration.");
                break;
        }
    }

    private void ShowNotHungryNode()
    {
        dialogueUI.Show(
            speakerName: "Kim Woo-bin",
            body: "Not hungry? That is suspicious. There has to be something you are hungry for.",
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: DialoguePortraitSide.Right,
            choiceTexts: new[]
            {
                "Maybe just hotteok.",
                "Okay, fine. BBQ.",
                "I am hungry for you Kim Ssi"
            },
            onChoiceSelected: HandleNotHungryChoice
        );
    }

    private void HandleNotHungryChoice(int choiceIndex)
    {
        switch (choiceIndex)
        {
            case 0:
                ShowEnding("Kim Woo-bin", "Hotteok is acceptable. Sweet pancakes count as dinner if nobody argues.");
                break;

            case 1:
                ShowKoreanBbqNode();
                break;

            case 2:
                ShowEnding("Kim Woo-bin", "Holy Wocao uwu");
                break;
        }
    }

    private void ShowPlayerLine(string playerBody, string nextSpeakerName, string nextBody)
    {
        dialogueUI.Show(
            speakerName: "Player",
            body: playerBody,
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: DialoguePortraitSide.Left,
            choiceTexts: new[]
            {
                "Continue."
            },
            onChoiceSelected: _ => ShowEnding(nextSpeakerName, nextBody)
        );
    }

    private void ShowEnding(string speakerName, string body)
    {
        dialogueUI.Show(
            speakerName: speakerName,
            body: body,
            leftPortrait: playerPortrait,
            rightPortrait: kimPortrait,
            activePortraitSide: speakerName == "Player" ? DialoguePortraitSide.Left : DialoguePortraitSide.Right,
            choiceTexts: new[]
            {
                "End conversation.",
                "Start over."
            },
            onChoiceSelected: choiceIndex =>
            {
                if (choiceIndex == 0)
                    dialogueUI.Hide();
                else
                    ShowOpeningNode();
            }
        );
    }
}