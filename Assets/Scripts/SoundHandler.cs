using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundHandler : MonoBehaviour
{
    [SerializeField]
    private Image _soundImage;

    [SerializeField]
    private Sprite _activeSoundSprite, _inactiveSoundSprite;

    private void Start()
    {
        bool sound = (PlayerPrefs.HasKey(Constants.Data.SETTINGS_SOUND) ?
           PlayerPrefs.GetInt(Constants.Data.SETTINGS_SOUND) : 1) == 1;
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;

        AudioManager.Instance.AddButtonSound();
    }

    public void ToggleSound()
    {
        bool sound = (PlayerPrefs.HasKey(Constants.Data.SETTINGS_SOUND) 
            ? PlayerPrefs.GetInt(Constants.Data.SETTINGS_SOUND) : 1) == 1;
        sound = !sound;
        PlayerPrefs.SetInt(Constants.Data.SETTINGS_SOUND, sound ? 1 : 0);
        _soundImage.sprite = sound ? _activeSoundSprite : _inactiveSoundSprite;
        AudioManager.Instance.ToggleSound();
    }
}
