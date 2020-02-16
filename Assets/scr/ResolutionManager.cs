using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class ResolutionManager : MonoBehaviour
{
    private const string KEY_PREF_RESOLUTION = "prefKeyResolution";

    public TextMeshProUGUI resolutionText;
    public TextMeshProUGUI fullscreenText;

    private Resolution[] resolutions;

    private bool isFullscreen;

    private int currentResolutionIndex = 0;

    private void OnEnable()
    {
        resolutions = Screen.resolutions;
        currentResolutionIndex = GetCurrentResolutionIndex();
        resolutionText.text = resolutions[currentResolutionIndex].ToString();
        fullscreenText.text = Screen.fullScreen.ToString();

        isFullscreen = Screen.fullScreen;
    }

    private void SetAndApplyResolution(int newResolutionIndex)
    {
        currentResolutionIndex = newResolutionIndex;
        ApplyCurrentResolution();        
    }

    private void ApplyCurrentResolution()
    {
        ApplyResolution(resolutions[currentResolutionIndex]);
    }

    private void ApplyResolution(Resolution resolution)
    {
        SetResolutionText(resolution);
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt(KEY_PREF_RESOLUTION, currentResolutionIndex);
    }

    private void SetResolutionText(Resolution resolution)
    {
        resolutionText.text = resolution.ToString();
    }

    public void SetNextResolution()
    {
        currentResolutionIndex = GetNextWrappedIndex(resolutions, currentResolutionIndex);
        SetResolutionText(resolutions[currentResolutionIndex]);
    }

    public void SetPreviousResolution()
    {
        currentResolutionIndex = GetPreviousWrappedIndex(resolutions, currentResolutionIndex);
        SetResolutionText(resolutions[currentResolutionIndex]);
    }

    private int GetNextWrappedIndex<T>(IList<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        return (currentIndex + 1) % collection.Count;
    }

    private int GetPreviousWrappedIndex<T>(IList<T> collection, int currentIndex)
    {
        if (collection.Count < 1) return 0;
        if ((currentIndex - 1) < 0) return collection.Count - 1;
        return (currentIndex - 1) % collection.Count;
    }

    private int GetCurrentResolutionIndex()
    {
        int currentRes = PlayerPrefs.GetInt(KEY_PREF_RESOLUTION, 0);
        if (currentRes == 0)
        {
            currentRes = Screen.resolutions.ToList().IndexOf(Screen.currentResolution);
        }

        return currentRes;
    }

    public void ChangeFullscreen()
    {
        isFullscreen = !isFullscreen;
        fullscreenText.text = isFullscreen.ToString();
    }

    public void ApplyChanges()
    {
        SetAndApplyResolution(currentResolutionIndex);
        Screen.fullScreen = isFullscreen;
    }
}
