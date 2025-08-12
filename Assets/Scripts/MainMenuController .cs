using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class MainMenuController : MonoBehaviour
{
    public UIDocument mainMenuUIDocument;
    public List<AudioClip> musicClips;
    public List<TextAsset> jsonFiles;
    public AudioSource menuAudioSource;

    private DropdownField songDropdown;
    private Slider volumeSlider;
    private Button playButton;
    private Button quitButton;
    private Button howToButton;

    private const string VolumeKey = "MusicVolume";
    private const string SongIndexKey = "SelectedSongIndex";
    private int selectedSongIndex = 0;

    void Start()
    {
        var root = mainMenuUIDocument.rootVisualElement;

        songDropdown = root.Q<DropdownField>("SongSelector");
        volumeSlider = root.Q<Slider>("VolumeSlider");
        playButton = root.Q<Button>("PlayButton");
        quitButton = root.Q<Button>("QuitButton");
        howToButton = root.Q<Button>("HowToButton");

        if (songDropdown == null)
        {
            Debug.LogError("Could not find DropdownField with name 'SongSelector' in UXML");
            return;
        }

        // Populate dropdown
        List<string> songNames = new List<string>();
        foreach (var clip in musicClips)
            songNames.Add(clip.name);

        songDropdown.choices = songNames;

        // Load prefs
        selectedSongIndex = PlayerPrefs.GetInt(SongIndexKey, 0);
        float volume = PlayerPrefs.GetFloat(VolumeKey, 1f);

        songDropdown.index = selectedSongIndex;
        volumeSlider.value = volume * 10;
        menuAudioSource.volume = volume;

        var fill = root.Q<VisualElement>("Fill");

        volumeSlider.RegisterCallback<GeometryChangedEvent>(_ =>
        {
            UpdateFill(fill, volumeSlider.value);
        });

        volumeSlider.RegisterValueChangedCallback(evt =>
        {
            UpdateFill(fill, evt.newValue);
        });

        // Event listeners
        songDropdown.RegisterValueChangedCallback(evt => OnSongChanged(songDropdown.index));
        volumeSlider.RegisterValueChangedCallback(evt => OnVolumeChanged(volumeSlider.value));
        playButton.clicked += OnPlayClicked;
        quitButton.clicked += OnQuitClicked;
        howToButton.clicked += OnHowToClicked;
    }

    private void UpdateFill(VisualElement fill, float value)
    {
        float ratio = (value - volumeSlider.lowValue) / (volumeSlider.highValue - volumeSlider.lowValue);
        fill.style.width = ratio * volumeSlider.resolvedStyle.width;
    }

    void OnSongChanged(int index)
    {
        selectedSongIndex = index;
        PlayerPrefs.SetInt(SongIndexKey, selectedSongIndex);
    }

    void OnVolumeChanged(float value)
    {
        PlayerPrefs.SetFloat(VolumeKey, value/10);
        if (menuAudioSource != null)
            menuAudioSource.volume = value/10;
    }


    void OnPlayClicked()
    {
        MusicData.selectedClip = musicClips[selectedSongIndex];
        MusicData.selectedJson = jsonFiles[selectedSongIndex];

        SceneManager.LoadScene("OutdoorsScene");
    }

    public void OnQuitClicked()
    {
        Application.Quit();
    }

    public void OnHowToClicked()
    {
        Application.Quit();
    }
}
