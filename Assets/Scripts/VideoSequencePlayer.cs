using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoSequencePlayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;        // Assign in Inspector
    public List<VideoClip> videoClips;     // Assign list of video clips
    public List<float> playDurations;      // Time each video plays

    private int currentIndex = 0;
    private bool isPlayingSequence = false;

    void Start()
    {
        if (videoPlayer == null || videoClips.Count == 0 || videoClips.Count != playDurations.Count)
        {
            Debug.LogError("VideoSequencePlayer setup error: Check that videoPlayer, videoClips, and playDurations are correctly assigned and match in size.");
            return;
        }

        videoPlayer.isLooping = true;
        StartCoroutine(PlayVideoSequence());
    }

    IEnumerator PlayVideoSequence()
    {
        isPlayingSequence = true;

        while (currentIndex < videoClips.Count)
        {
            videoPlayer.clip = videoClips[currentIndex];
            videoPlayer.Play();
            yield return new WaitForSeconds(playDurations[currentIndex]);
            currentIndex++;
        }

        videoPlayer.Stop();
        videoPlayer.enabled = false;

        isPlayingSequence = false;
    }
}
