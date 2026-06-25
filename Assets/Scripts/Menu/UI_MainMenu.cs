using ElectionTactics;
using UnityEngine;
using UnityEngine.UI;

public class UI_MainMenu : Singleton<UI_MainMenu>
{
    public Button PlayButton;
    public Button SettingsButton;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PlayButton.onClick.AddListener(Play_OnClick);
        SettingsButton.onClick.AddListener(Settings_OnClick);
    }

    /// <summary>
    /// Called every time the main menu is entered
    /// </summary>
    public void OnEnter()
    {
        gameObject.SetActive(true);
        CloudManager.Instance.SetDefaultCloudSettings();
        EncounterCamera.Instance.SetMainMenu();
        AudioManager.StartMusic();
    }

    private void Play_OnClick()
    {
        IntroSequenceManager.Instance.StartIntroSequence();
    }

    private void Settings_OnClick()
    {

    }
}
