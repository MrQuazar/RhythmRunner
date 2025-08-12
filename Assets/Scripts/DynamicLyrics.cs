using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DynamicLyrics : MonoBehaviour
{
    public TextAsset jsonFile;
    public AudioSource songAudioSource;
    public GameObject wordPrefab;

    public RectTransform canvasRectTransform;
    public TextMeshProUGUI streakText;
    public List<Sprite> streakSprites;
    public Image streakImage;
    public int score = 0;
    private Vector2[] spawnPositions;
    private GameObject[] activeWordsAtPoints;
    private int currentSpawnIndex = 0;
    private float canvasWidth = 0f;
    private float canvasHeight = 0f;
    public int lastHoveredWordCount = 0;
    public int streakCount = 0;
    private Coroutine streakResetCoroutine;
    private Coroutine trembleCoroutine;
    private Vector3 originalTextPos;
    private Vector3 originalImagePos;
    public float trembleIntensity = 5f;
    public float trembleFrequency = 0.05f;
    private Color[] colors = new Color[]
    {
    new Color(1f, 0.6f, 0.2f),   // Bright orange (eye-catching)
    new Color(1f, 0.2f, 0.8f),   // Vivid pink/magenta
    new Color(0.2f, 0.8f, 1f),   // Neon cyan/sky blue
    new Color(0.8f, 0.4f, 1f),   // Electric purple
    new Color(1f, 1f, 0.4f)      // Bright lemon yellow
    };


    private List<LyricWord> lyricsWords;

    private bool isPaused = false;

    public void PauseLyrics()
    {
        isPaused = true;

        if (songAudioSource != null && songAudioSource.isPlaying)
            songAudioSource.Pause();

        if (canvasRectTransform != null)
            canvasRectTransform.gameObject.SetActive(false);
    }

    public void ResumeLyrics()
    {
        isPaused = false;

        if (songAudioSource != null && !songAudioSource.isPlaying)
            songAudioSource.Play();

        if (canvasRectTransform != null)
            canvasRectTransform.gameObject.SetActive(true);
    }


    void Update()
    {
        if (isPaused) return;
    }

    public void loadJson()
    {
        canvasWidth = canvasRectTransform.rect.width;
        canvasHeight = canvasRectTransform.rect.height;
        spawnPositions = new Vector2[4];
        activeWordsAtPoints = new GameObject[spawnPositions.Length];

        lyricsWords = JsonUtility.FromJson<WordListWrapper>(WrapJson(jsonFile.text)).words;
        StartCoroutine(ShowLyrics());
    }

    public void AddScore()
    {
        score += 10;
    }

    public void StreakTextHandler()
    {
        streakCount += 1;

        if (streakCount > 2)
        {
            int spriteIndex = 0;

            if (streakCount <= 10)
            {
                spriteIndex = 0;
            }
            else if (streakCount <= 25)
            {
                spriteIndex = 1;
                score += streakCount;
            }
            else if (streakCount <= 50)
            {
                spriteIndex = 2;
                score += streakCount * 2;
            }
            else
            {
                spriteIndex = 3;
                score += streakCount * 3;
            }

            if (spriteIndex < streakSprites.Count)
            {
                streakImage.sprite = streakSprites[spriteIndex];
            }

            streakText.text = streakCount.ToString();

            if (trembleCoroutine == null)
            {
                originalTextPos = streakText.rectTransform.anchoredPosition;
                originalImagePos = streakImage.rectTransform.anchoredPosition;
                trembleCoroutine = StartCoroutine(TrembleEffect());
            }
        }
        else
        {
            ResetTremble();
        }
    }


    private IEnumerator ResetStreakTextAfterDelay(int savedLastHoveredWordCount)
    {
        int snapshot = lastHoveredWordCount;
        yield return new WaitForSeconds(3f);

        if (savedLastHoveredWordCount == snapshot)
        {
            streakText.text = "";
        }

        streakResetCoroutine = null;
    }

    IEnumerator ShowLyrics()
    {
        int wordIndex = 0;

        while (wordIndex < lyricsWords.Count)
        {
            var wordData = lyricsWords[wordIndex];
            float startTime = TimeToSeconds(wordData.start);

            while (songAudioSource.time < startTime)
                yield return null;

            int spawnPointIndex = currentSpawnIndex % spawnPositions.Length;

            float yPos = canvasHeight * 0.2f - UnityEngine.Random.Range(0, canvasHeight * 0.6f);

            for (int i = 0; i < 4; i++)
            {
                float x = -700 + 350 * i;
                if (i > 1) x += 350;
                spawnPositions[i] = new Vector2(x, yPos);
            }
            Vector2 spawnPos = spawnPositions[spawnPointIndex];

            if (activeWordsAtPoints[spawnPointIndex] != null)
            {
                if (activeWordsAtPoints[spawnPointIndex].GetComponentInChildren<TextMeshProUGUI>().color != Color.green)
                {
                    if (score >= 10)
                    {
                        score -= 10;

                    }
                    if (streakCount > 2)
                    {
                        handleStreakBreak();
                    }
                }
                Destroy(activeWordsAtPoints[spawnPointIndex]);
            }

            GameObject wordGO = Instantiate(wordPrefab, canvasRectTransform);
            wordGO.GetComponent<RectTransform>().anchoredPosition = spawnPos;

            TextMeshProUGUI textComponent = wordGO.GetComponentInChildren<TextMeshProUGUI>();
            textComponent.text = wordData.text;
            textComponent.color = colors[UnityEngine.Random.Range(0, colors.Length)];

            Button wordButton = wordGO.GetComponent<Button>();
            HoverHandler hoverHandler = wordGO.AddComponent<HoverHandler>();
            hoverHandler.wordNumber = wordIndex;
            hoverHandler.textComponent = textComponent;
            hoverHandler.lyricsManager = this;

            activeWordsAtPoints[spawnPointIndex] = wordGO;
            currentSpawnIndex++;
            wordIndex++;
        }

        yield return new WaitUntil(() => !songAudioSource.isPlaying);
    }

    float TimeToSeconds(string timestamp)
    {
        TimeSpan ts = TimeSpan.Parse("00:" + timestamp);
        return (float)ts.TotalSeconds;
    }

    public void handleStreakBreak()
    {
        streakCount = 0;
        streakText.text = "Streak lost!";
        if (streakResetCoroutine != null)
            StopCoroutine(streakResetCoroutine);
        streakResetCoroutine = StartCoroutine(ResetStreakTextAfterDelay(lastHoveredWordCount));
    }
    private string WrapJson(string jsonArray)
    {
        return "{ \"words\": " + jsonArray + "}";
    }

    private IEnumerator TrembleEffect()
    {
        while (true)
        {
            Vector3 randomOffset = new Vector3(
                 UnityEngine.Random.Range(-trembleIntensity, trembleIntensity),
                 UnityEngine.Random.Range(-trembleIntensity, trembleIntensity),
                0f);

            streakText.rectTransform.anchoredPosition = originalTextPos + randomOffset;
            streakImage.rectTransform.anchoredPosition = originalImagePos + randomOffset;

            yield return new WaitForSeconds(trembleFrequency);
        }
    }

    private void ResetTremble()
    {
        if (trembleCoroutine != null)
        {
            StopCoroutine(trembleCoroutine);
            trembleCoroutine = null;

            streakText.rectTransform.anchoredPosition = originalTextPos;
            streakImage.rectTransform.anchoredPosition = originalImagePos;
        }
    }

}
