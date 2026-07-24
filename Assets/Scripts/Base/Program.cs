using UnityEngine;

public class Program : Singleton<Program>
{
    public ProgramState State { get; private set; }

    private void Awake()
    {
        ItemHighlightManager.ClearAll();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        State = ProgramState.None;

        ResourceManager.ClearCache();
        DefDatabaseRegistry.InitDefs();
        MarkovChainWordGenerator.Init();

        // Hide all elements
        UI_MainMenu.Instance.gameObject.SetActive(false);
        GameUI.Instance.gameObject.SetActive(false);

        // Initial state
        GoToMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void EnterState(ProgramState newState)
    {
        if (newState == State) return;

        // Leave state
        ProgramState oldState = State;
        switch (State)
        {
            case ProgramState.MainMenu:
                UI_MainMenu.Instance.gameObject.SetActive(false);
                break;

            case ProgramState.Game:
                GameUI.Instance.gameObject.SetActive(false);
                break;
        }

        // Enter state
        State = newState;
        switch (State)
        {
            case ProgramState.MainMenu:
                UI_MainMenu.Instance.OnEnter();
                break;

            case ProgramState.Game:
                GameUI.Instance.gameObject.SetActive(true);
                break;

        }
    }

    public void GoToMainMenu()
    {
        EnterState(ProgramState.MainMenu);
    }
}
