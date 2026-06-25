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
        CloudManager.Instance.OnEnterMainMenu();
        EncounterCamera.Instance.SetMainMenu();
    }

    private void Play_OnClick()
    {
        Game.Instance.StartNewGame();
    }

    private void Settings_OnClick()
    {

    }
}
