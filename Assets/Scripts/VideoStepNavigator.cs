using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UIElements;
using System;

public class VideoStepNavigatorUIToolkit : MonoBehaviour
{
    [Serializable]
    public class HowToStep
    {
        public VideoClip video1;
        public VideoClip video2;
        [TextArea] public string text1;
        [TextArea] public string text2;
    }

    public UIDocument uiDocument;
    public HowToStep[] steps;

    private VisualElement container;
    private VisualElement howToContainer;
    private VisualElement video1Container;
    private VisualElement video2Container;
    private Label infoText1;
    private Label infoText2;
    private Button prevButton;
    private Button nextButton;
    private Button howToButton;
    private Button closeButton;

    private VideoPlayer videoPlayer1;
    private VideoPlayer videoPlayer2;
    private RenderTexture renderTexture1;
    private RenderTexture renderTexture2;

    private int currentIndex = 0;

    private void Awake()
    {
        var root = uiDocument.rootVisualElement;

        // Main panels
        container = root.Q<VisualElement>("Container");
        howToContainer = root.Q<VisualElement>("HowToContainer");

        // Buttons
        prevButton = root.Q<Button>("PreviousButton");
        nextButton = root.Q<Button>("NextButton");
        howToButton = root.Q<Button>("HowToButton");
        closeButton = root.Q<Button>("CloseButton");

        // Video placeholders
        video1Container = root.Q<VisualElement>("Video1Container");
        video2Container = root.Q<VisualElement>("Video1Container2");

        // Info texts
        infoText1 = root.Q<Label>("InfoText1");
        infoText2 = root.Q<Label>("InfoText2");

        // Setup video players
        SetupVideoPlayer(out videoPlayer1, out renderTexture1, video1Container);
        SetupVideoPlayer(out videoPlayer2, out renderTexture2, video2Container);

        // Wire up buttons
        howToButton.clicked += OnHowToClicked;
        closeButton.clicked += OnHowToClosed;
        prevButton.clicked += OnPrevButtonClicked;
        nextButton.clicked += OnNextButtonClicked;

        // Initial panel states
        container.style.display = DisplayStyle.Flex;
        howToContainer.style.display = DisplayStyle.None;
    }

    private void SetupVideoPlayer(out VideoPlayer vp, out RenderTexture rt, VisualElement container)
    {
        GameObject vpObj = new GameObject(container.name + "_Player");
        vpObj.transform.SetParent(this.transform, false);

        vp = vpObj.AddComponent<VideoPlayer>();
        vp.isLooping = true;
        vp.renderMode = VideoRenderMode.RenderTexture;

        rt = new RenderTexture(640, 360, 0);
        vp.targetTexture = rt;

        var videoImage = new Image();
        videoImage.image = rt;
        videoImage.style.width = Length.Percent(100);
        videoImage.style.height = Length.Percent(100);
        container.Add(videoImage);
    }

    private void PlayStep(int index)
    {
        if (index < 0 || index >= steps.Length) return;

        var step = steps[index];

        // Video 1 handling
        if (step.video1 != null)
        {
            video1Container.style.display = DisplayStyle.Flex;
            videoPlayer1.clip = step.video1;
            videoPlayer1.Play();
        }
        else
        {
            video1Container.style.display = DisplayStyle.None;
            videoPlayer1.Stop();
        }

        // Video 2 handling
        if (step.video2 != null)
        {
            video2Container.style.display = DisplayStyle.Flex;
            videoPlayer2.clip = step.video2;
            videoPlayer2.Play();
        }
        else
        {
            video2Container.style.display = DisplayStyle.None;
            videoPlayer2.Stop();
        }

        // Update texts
        infoText1.text = step.text1 ?? string.Empty;
        infoText2.text = step.text2 ?? string.Empty;

        // Update button states
        prevButton.SetEnabled(index > 0);
        nextButton.SetEnabled(index < steps.Length - 1);
    }


    private void OnNextButtonClicked()
    {
        if (currentIndex < steps.Length - 1)
        {
            currentIndex++;
            PlayStep(currentIndex);
        }
    }

    private void OnPrevButtonClicked()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            PlayStep(currentIndex);
        }
    }

    private void OnHowToClicked()
    {
        container.style.display = DisplayStyle.None;
        howToContainer.style.display = DisplayStyle.Flex;

        currentIndex = 0;
        PlayStep(currentIndex);
    }

    private void OnHowToClosed()
    {
        videoPlayer1.Stop();
        videoPlayer2.Stop();

        container.style.display = DisplayStyle.Flex;
        howToContainer.style.display = DisplayStyle.None;
    }
}
