using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public abstract class GameController : UIController {
    [Header("GameController")]
    [SerializeField] private GameObject confettiParticlePrefab;
    [SerializeField] private GameObject pointsPopupPrefab;
    [SerializeField] private RenderTexture tutorialVisual;
    [SerializeField, TextArea] private string tutorialText;

    protected Label scoreLabel;
    protected Label finalScoreLabel;
    protected Label totalScoreLabel;

    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;

    protected VisualElement gameSubscreen;
    protected VisualElement tutorialSubscreen;
    public GameSessionData GameSessionData { get; private set; }

    protected override void Awake( ) {
        base.Awake( );

        // Get all screens within the game
        // Each game should have the same types of screens, but more functionality will need to be added individually
        gameSubscreen = ui.Q<VisualElement>("GameSubscreen");
        tutorialSubscreen = ui.Q<VisualElement>("TutorialSubscreen");
        gameScreen = ui.Q<VisualElement>("GameScreen");
        pauseScreen = ui.Q<VisualElement>("PauseScreen");
        winScreen = ui.Q<VisualElement>("WinScreen");

        // Set the states of the screens and subscreens based on what game controller state is active
        screens[(int) UIState.TUTORIAL] = gameScreen;
        screens[(int) UIState.GAME] = gameScreen;
        screens[(int) UIState.PAUSE] = pauseScreen;
        screens[(int) UIState.WIN] = winScreen;

        subscreens[(int) UIState.TUTORIAL] = tutorialSubscreen;
        subscreens[(int) UIState.GAME] = gameSubscreen;

        // Set all common button functions
        // Each game should also have these buttons
        ui.Q<Button>("ResumeButton").clicked += ( ) => { UIControllerState = LastUIControllerState; };
        ui.Q<Button>("QuitButton").clicked += ( ) => { FadeToScene(0); };
        ui.Q<Button>("HomeButton").clicked += ( ) => { FadeToScene(0); };
        ui.Q<Button>("PlayAgainButton").clicked += ( ) => { FadeToScene(SceneManager.GetActiveScene( ).buildIndex); };
        ui.Q<Button>("PauseButton").clicked += ( ) => { UIControllerState = UIState.PAUSE; };
        ui.Q<Button>("PlayButton").clicked += ( ) => { UIControllerState = UIState.GAME; };
        ui.Q<Button>("HowToPlayButton").clicked += ( ) => { UIControllerState = UIState.TUTORIAL; };

        // Get references to other important UI elements
        scoreLabel = ui.Q<Label>("ScoreLabel");
        finalScoreLabel = ui.Q<Label>("FinalScoreLabel");
        totalScoreLabel = ui.Q<Label>("TotalScoreLabel");

        // Set tutorial information
        ui.Q<VisualElement>("TutorialVisual").style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tutorialVisual));
        ui.Q<Label>("TutorialLabel").text = tutorialText;
        ui.Q<Label>("TitleLabel").text = name;

        ui.Q<Label>("MotivationalMessage").text = MOTIV_MESSAGES[Random.Range(0, MOTIV_MESSAGES.Length)];

        // Create a new game session data entry for this game
        GameSessionData = new GameSessionData(name, DataManager.AppSessionData.RITchCode, DataManager.AppSessionData.TotalPointsEarned);
        GameSessionData.OnPointsEarnedChange += ( ) => {
            scoreLabel.text = $"Score: <b>{GameSessionData.PointsEarned} pts</b>";
            finalScoreLabel.text = $"{GameSessionData.PointsEarned} pts";
            totalScoreLabel.text = $"{GameSessionData.TotalPointsEarned} pts";
        };
        GameSessionData.InvokeAllDelegates( );
    }

    protected override void Start( ) {
        base.Start( );
        UIControllerState = UIState.TUTORIAL;
    }

    protected override void Update( ) {
        base.Update( );
        GameSessionData.TotalTimeSecondsValue += Time.deltaTime;
    }

    protected override void FadeToScene(int sceneBuildIndex) {
        DataManager.AppSessionData.TotalPointsEarnedValue = GameSessionData.TotalPointsEarned;
        DataManager.Instance.UploadSessionData(GameSessionData);
        base.FadeToScene(sceneBuildIndex);
    }

    protected override void UpdateSubscreens( ) {
        SetElementVisibility(gameSubscreen, UIControllerState == UIState.GAME);
        SetElementVisibility(tutorialSubscreen, UIControllerState == UIState.TUTORIAL);
    }

    /// <summary>
    /// Spawn a confetti particle system at a particular position
    /// </summary>
    /// <param name="position">The position to spawn the confetti at</param>
    public void SpawnConfettiParticles(Vector3 position) {
        Instantiate(confettiParticlePrefab, position, Quaternion.identity);
    }

    /// <summary>
    /// Spawn a points popup on the screen at a specific position
    /// </summary>
    /// <param name="position">The position to spawn the points popup at</param>
    /// <param name="points">The number of points to display on the popup</param>
    public void SpawnPointsPopup(Vector3 position, int points) {
        PointsPopup pointsPopup = Instantiate(pointsPopupPrefab, position, Quaternion.identity).GetComponent<PointsPopup>( );
        pointsPopup.Points = points;
    }
}
