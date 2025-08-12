using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public UIDocument pauseMenuUIDocument;
    public UIDocument endGameMenuUIDocument;
    public List<AudioClip> songList;
    public List<TextAsset> jsonList;

    private AudioSource camAudio;
    private DynamicLyrics lyricsScript;
    private bool musicEndedHandled = false;
    private bool isPaused = false;
    private VisualElement pauseContainer;
    private Slider volumeSlider;
    private Button RetryButton;
    private Button RetryButton2;
    private Button ResumeButton;
    private Button PauseButton;
    private Button NextSongButton;
    private Button MainMenuButton;
    private Button NextSongButton2;
    private Button MainMenuButton2;
    private Label finalScore;

    private Label gameScore;
    private const string VolumeKey = "MusicVolume";

    void Start()
    {
        var volume = PlayerPrefs.GetFloat(VolumeKey);
        var root = pauseMenuUIDocument.rootVisualElement;
        var root2 = endGameMenuUIDocument.rootVisualElement;
        pauseContainer = root.Q<VisualElement>("Container");
        pauseContainer.style.display = DisplayStyle.None;
        endGameMenuUIDocument.rootVisualElement.style.display = DisplayStyle.None;
        volumeSlider = root.Q<Slider>("VolumeSlider");
        RetryButton = root2.Q<Button>("RetryButton");
        RetryButton2 = root.Q<Button>("RetryButton");
        ResumeButton = root.Q<Button>("ResumeButton");
        NextSongButton = root.Q<Button>("NextSongButton");
        MainMenuButton = root.Q<Button>("MainMenuButton");
        NextSongButton2 = root2.Q<Button>("NextSongButton");
        MainMenuButton2 = root2.Q<Button>("MainMenuButton");
        PauseButton = root.Q<Button>("PauseButton");
        gameScore = root.Q<Label>("ScoreText");
        finalScore = root2.Q<Label>("Score");
        Camera mainCam = Camera.main;
        if (mainCam != null && MusicData.selectedClip != null)
        {
            camAudio = mainCam.GetComponent<AudioSource>();
            if (camAudio == null)
                camAudio = mainCam.gameObject.AddComponent<AudioSource>();

            camAudio.clip = MusicData.selectedClip;
            camAudio.volume = volume;
            camAudio.loop = false;
            camAudio.Play();
        }

        volumeSlider.value = volume * 10;

        GameObject gm = GameObject.Find("GameManager");
        if (gm != null && MusicData.selectedJson != null)
        {
            lyricsScript = gm.GetComponent<DynamicLyrics>();
            if (lyricsScript != null)
            {
                lyricsScript.jsonFile = MusicData.selectedJson;
                lyricsScript.loadJson();
            }
        }

        // Event listeners

        var fill = root.Q<VisualElement>("Fill");

        volumeSlider.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            UpdateFill(fill, volumeSlider.value);
        });

        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            UpdateFill(fill, evt.newValue);
        });
        volumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(volumeSlider.value));
        RetryButton.clicked += Retry;
        RetryButton2.clicked += Retry;
        ResumeButton.clicked += ResumeGame;
        NextSongButton.clicked += PlayNext;
        MainMenuButton.clicked += LoadMainMenu;
        NextSongButton2.clicked += PlayNext;
        MainMenuButton2.clicked += LoadMainMenu;
        PauseButton.clicked += TogglePause;
    }

    void Update()
    {
        gameScore.text = "Score: "+ lyricsScript.score.ToString();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        if (camAudio != null && !camAudio.isPlaying && !musicEndedHandled && !isPaused && !musicEndedHandled)
        {
            musicEndedHandled = true;
            finalScore.text = "Score: " + lyricsScript.score.ToString();
            ShowPauseMenu();
        }
    }

    private void UpdateFill(VisualElement fill, float value)
    {
        float ratio = (value - volumeSlider.lowValue) / (volumeSlider.highValue - volumeSlider.lowValue);
        fill.style.width = ratio * volumeSlider.resolvedStyle.width;
    }
    void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value/10);
        if (camAudio != null)
            camAudio.volume = value/10;
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            ShowPauseMenu();
        PauseButton.SetEnabled(!isPaused);
    }

    void ShowPauseMenu()
    {
        if(!musicEndedHandled) isPaused = true;
        Time.timeScale = 0;

        if (camAudio != null)
            camAudio.Pause();

        if (lyricsScript != null)
            lyricsScript.PauseLyrics();

        if (!musicEndedHandled)
            pauseContainer.style.display = DisplayStyle.Flex;
        else
            endGameMenuUIDocument.rootVisualElement.style.display = DisplayStyle.Flex;

    }

    public void ResumeGame()
    {
        isPaused = false;
        PauseButton.SetEnabled(true);
        Time.timeScale = 1;

        if (camAudio != null)
            camAudio.Play();

        if (lyricsScript != null)
            lyricsScript.ResumeLyrics();

        if (!musicEndedHandled)
            pauseContainer.style.display = DisplayStyle.None;
        else
            endGameMenuUIDocument.rootVisualElement.style.display = DisplayStyle.None;
    }

    public void Retry()
    {
        ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PlayNext()
    {
        if (songList == null || songList.Count == 0) return;

        int currentIndex = songList.IndexOf(MusicData.selectedClip);
        var availableIndexes = Enumerable.Range(0, songList.Count).Where(i => i != currentIndex).ToList();
        if (availableIndexes.Count == 0) return;

        int nextIndex = availableIndexes[Random.Range(0, availableIndexes.Count)];

        MusicData.selectedClip = songList[nextIndex];
        MusicData.selectedJson = jsonList[nextIndex];

        ResumeGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void LoadMainMenu()
    {
        ResumeGame();
        SceneManager.LoadScene("MainMenu");
    }
}
