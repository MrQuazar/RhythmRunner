using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class SequenceController : MonoBehaviour
{
    public Volume volume;
    public List<Cubemap> skyCubemaps;
    public List<float> transitionDurations;
    public List<float> fadeDuration;
    public float currentFadeDuration = 2f;
    public bool enableFade = true;

    public Animator animator;
    public AnimatorOverrideController overrideController;
    public List<AnimationClip> animationClips;

    public Camera mainCamera;
    public List<Vector3> cameraPositions;

    public List<float> followDistances;
    public List<float> followHeights;
    public List<float> rotateSpeed;
    private HDRISky hdriSky;
    private VisualEnvironment visualEnvironment;
    private float defaultExposure;
    private int currentIndex = 0;
    private SmoothFollow smoothFollow;
    private List<Vector3> sunPositions;
    private List<bool> attachSunFlags;
    public GameObject sunObject;
    private float originalSunIntensity = 1f;
    public Transform modelParent;
    private GameObject currentModelInstance;


    [System.Serializable]
    public class Vector3Data
    {
        public float x, y, z;

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [System.Serializable]
    public class SkySequenceList
    {
        public List<SkySequenceData> sequences;
    }
    [System.Serializable]
    public class SkySequenceData
    {
        public string title;
        public List<string> skyCubemapNames;
        public List<Vector3Data> cameraPositions;
        public List<float> followDistances;
        public List<float> followHeights;
        public List<float> rotateSpeed;
        public List<float> fadeDuration;
        public List<float> transitionDurations;
        public List<string> animationClipNames;
        public List<Vector3Data> sunWorldPositions;
        public List<bool> attachSunToCamera;
        public string playerModelName;

    }

    void Start()
    {
        TextAsset jsonFile = Resources.Load<TextAsset>("song_config");
        if (jsonFile == null)
        {
            Debug.LogError("Could not find song_config.json in Resources.");
            return;
        }

        SkySequenceList sequenceList = JsonUtility.FromJson<SkySequenceList>(jsonFile.text);
        SkySequenceData selectedSequence = null;
        
        AudioSource cameraAudioSource = Camera.main.GetComponent<AudioSource>();
        if (cameraAudioSource != null && cameraAudioSource.clip != null)
        {
            string songTitle = cameraAudioSource.clip.name;
            selectedSequence = sequenceList.sequences.Find(seq => seq.title == songTitle);

            if (selectedSequence != null)
            {
                Debug.Log("Found sequence for: " + songTitle);
                // Proceed with using selectedSequence
            }
            else
            {
                Debug.LogWarning("No matching sequence found for song: " + songTitle);
                return;
            }
        }
        else
        {
            Debug.LogWarning("Camera AudioSource or AudioClip is missing.");
            return;
        }

        SkySequenceData jsonData = selectedSequence;

        // Instantiate player model from Resources
        if (!string.IsNullOrEmpty(jsonData.playerModelName))
        {
            GameObject modelPrefab = Resources.Load<GameObject>(jsonData.playerModelName);
            if (modelPrefab != null)
            {
                currentModelInstance = Instantiate(modelPrefab, modelParent.position, modelParent.rotation, modelParent);

                animator = currentModelInstance.GetComponent<Animator>();
                if (animator != null && overrideController != null)
                {
                    animator.runtimeAnimatorController = overrideController;
                }
            }
            else
            {
                Debug.LogError($"Player model '{jsonData.playerModelName}' not found in Resources.");
            }
        }

        sunPositions = new List<Vector3>();
        foreach (var pos in jsonData.sunWorldPositions)
            sunPositions.Add(pos.ToVector3());

        attachSunFlags = jsonData.attachSunToCamera;

        // Load cubemaps from Resources
        skyCubemaps = new List<Cubemap>();
        foreach (var name in jsonData.skyCubemapNames)
        {
            var cubemap = Resources.Load<Cubemap>(name);
            if (cubemap != null)
                skyCubemaps.Add(cubemap);
            else
                Debug.LogWarning($"Cubemap '{name}' not found in Resources.");
        }

        // Load camera positions
        cameraPositions = new List<Vector3>();
        foreach (var pos in jsonData.cameraPositions)
            cameraPositions.Add(pos.ToVector3());

        followDistances = jsonData.followDistances;
        followHeights = jsonData.followHeights;
        rotateSpeed = jsonData.rotateSpeed;
        transitionDurations = jsonData.transitionDurations;
        fadeDuration = jsonData.fadeDuration;
        // Load animation clips from Resources
        animationClips = new List<AnimationClip>();
        foreach (var clipName in jsonData.animationClipNames)
        {
            var clip = Resources.Load<AnimationClip>(clipName);
            if (clip != null)
                animationClips.Add(clip);
            else
                Debug.LogWarning($"Animation clip '{clipName}' not found in Resources.");
        }

        // Setup SmoothFollow
        if (mainCamera != null)
            smoothFollow = mainCamera.GetComponent<SmoothFollow>();
        if (sunObject != null)
        {
            Transform target = FindDeepChild(currentModelInstance.transform, "Target:Head");
            if (target != null)
            {
                smoothFollow.SetTarget(target);
            }
            else
            {
                Debug.LogWarning("Target:Head not found in model.");
            }
            smoothFollow.SetSunObject(sunObject);
            Light sunLight = sunObject.GetComponent<Light>();
            if (sunLight != null)
            {
                originalSunIntensity = sunLight.intensity;
            }
        }
        if (animator != null && overrideController != null)
            animator.runtimeAnimatorController = overrideController;

        if (volume.profile.TryGet(out hdriSky) && volume.profile.TryGet(out visualEnvironment))
        {
            visualEnvironment.skyType.value = (int)SkyType.HDRI;
            defaultExposure = hdriSky.exposure.value;
            StartCoroutine(PlaySkySequence());
        }
        else
        {
            Debug.LogError("Sky or VisualEnvironment overrides not found in the Volume profile.");
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform result = FindDeepChild(child, name);
            if (result != null)
                return result;
        }
        return null;
    }

    IEnumerator PlaySkySequence()
    {
        while (currentIndex < skyCubemaps.Count)
        {
            if (enableFade && currentIndex > 0)
                yield return FadeSky(0); // Fade out

            // Sky change
            hdriSky.hdriSky.value = skyCubemaps[currentIndex];
            currentFadeDuration = fadeDuration[currentIndex];
            ForceSkyRefresh();

            // Camera transform update
            if (mainCamera != null)
            {
                mainCamera.transform.position = cameraPositions[currentIndex];
                if (sunObject != null && smoothFollow != null)
                {
                    smoothFollow.SetFollowDistance(followDistances[currentIndex]);
                    smoothFollow.SetHeight(followHeights[currentIndex]);
                    smoothFollow.SetRotateSpeed(rotateSpeed[currentIndex]);
                    smoothFollow.SetSunWorldPosition(sunPositions[currentIndex]);
                    smoothFollow.AttachSunToCamera(attachSunFlags[currentIndex]);
                }
            }

            // Animation override update
            if (overrideController != null && currentIndex < animationClips.Count)
            {
                var clipToPlay = animationClips[currentIndex];
                var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrideController.GetOverrides(overrides);

                for (int i = 0; i < overrides.Count; i++)
                {
                    if (overrides[i].Key.name == "idle")
                    {
                        overrides[i] = new KeyValuePair<AnimationClip, AnimationClip>(overrides[i].Key, clipToPlay);
                    }
                }

                overrideController.ApplyOverrides(overrides);

            }

            if (enableFade)
                yield return FadeSky(defaultExposure); // Fade in

            yield return new WaitForSeconds(transitionDurations[currentIndex]);
            currentIndex++;
        }
    }

    void ForceSkyRefresh()
    {
        int skyType = visualEnvironment.skyType.value;

        visualEnvironment.skyType.overrideState = false;
        visualEnvironment.skyType.value = 0;
        visualEnvironment.skyType.overrideState = true;
        visualEnvironment.skyType.value = skyType;
    }

    IEnumerator FadeSky(float targetExposure)
    {
        float startExposure = hdriSky.exposure.value;
        float elapsed = 0f;

        // Disable sun light
        Light sunLight = sunObject?.GetComponent<Light>();
        float previousSunIntensity = originalSunIntensity;

        if (sunLight != null)
            sunLight.intensity = 0f;

        while (elapsed < currentFadeDuration)
        {
            elapsed += Time.deltaTime;
            hdriSky.exposure.value = Mathf.Lerp(startExposure, targetExposure, elapsed / currentFadeDuration);
            yield return null;
        }

        hdriSky.exposure.value = targetExposure;

        // Restore sun light
        if (sunLight != null)
            sunLight.intensity = previousSunIntensity;
    }

}
