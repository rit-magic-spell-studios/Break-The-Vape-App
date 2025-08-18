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
    [SerializeField] protected Transform objectContainer;
    [SerializeField] private RenderTexture tutorialVisual;
    [SerializeField, TextArea] private string tutorialText;

    protected Label scoreLabel;
    protected Label finalScoreLabel;
    protected Label totalScoreLabel;

    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;
    protected VisualElement playGoalInfoScreen;

    protected VisualElement gameTutorialPopup;

    public GameSessionData GameSessionData { get; private set; }
    public bool IsPlayingGame => (CurrentScreen == gameScreen && CurrentPopup == null);

    protected override void Awake( ) {
        base.Awake( );

        gameTutorialPopup = ui.Q<VisualElement>("GameTutorialPopup");
        gameScreen = ui.Q<VisualElement>("GameScreen");
        pauseScreen = ui.Q<VisualElement>("PauseScreen");
        winScreen = ui.Q<VisualElement>("WinScreen");
        playGoalInfoScreen = ui.Q<VisualElement>("PlayGoalInfoScreen");

        ui.Q<Button>("ResumeButton").clicked += ( ) => { DisplayScreen(gameScreen); };
        ui.Q<Button>("QuitButton").clicked += ( ) => { GoToScene("MainMenu"); };
        ui.Q<Button>("HomeButton").clicked += ( ) => { GoToScene("MainMenu"); };
        ui.Q<Button>("PlayAgainButton").clicked += ( ) => { GoToScene(SceneManager.GetActiveScene( ).name); };
        ui.Q<Button>("PauseButton").clicked += ( ) => { DisplayScreen(pauseScreen); };
        ui.Q<Button>("PlayButton").clicked += ( ) => { HideCurrentPopup( ); };
        ui.Q<Button>("HowToPlayButton").clicked += ( ) => { DisplayBasicPopup(gameTutorialPopup); };

        scoreLabel = ui.Q<Label>("ScoreLabel");
        finalScoreLabel = ui.Q<Label>("FinalScoreLabel");
        totalScoreLabel = ui.Q<Label>("TotalScoreLabel");

        ui.Q<Button>("PlayGoalInfoButton").clicked += ( ) => DisplayScreen(playGoalInfoScreen);
        ui.Q<Button>("PlayGoalInfoBackButton").clicked += ( ) => DisplayScreen(winScreen);

        // Set tutorial popup information
        ui.Q<VisualElement>("TutorialVisual").style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tutorialVisual));
        ui.Q<Label>("TutorialLabel").text = tutorialText;
        ui.Q<Label>("TitleLabel").text = name;

        // Create a new game session data entry for this game
        GameSessionData = new GameSessionData(name, DataManager.AppSessionData.RITchCode, DataManager.AppSessionData.TotalPointsEarned);
        DataManager.AppSessionData.OnTotalTimeSecondsChange += ( ) => {
            float secondsRemaining = Mathf.Max(0, playGoalSeconds - DataManager.AppSessionData.TotalTimeSeconds);
            string timerString = string.Format("{0:0}:{1:00}", (int) secondsRemaining / 60, (int) secondsRemaining % 60);
            ui.Q<Label>("RadialProgressBarLabel").text = timerString;
            ui.Q<RadialProgress>("RadialProgressBar").Progress = DataManager.AppSessionData.TotalTimeSeconds / playGoalSeconds * 100f;
        };
        GameSessionData.OnPointsEarnedChange += ( ) => {
            scoreLabel.text = $"Score: <b>{GameSessionData.PointsEarned} pts</b>";
            finalScoreLabel.text = $"+ {GameSessionData.PointsEarned} pts";
            totalScoreLabel.text = $"{GameSessionData.TotalPointsEarned} pts";
        };
        GameSessionData.InvokeAllDelegates( );
    }

    protected override void Start( ) {
        base.Start( );
        DisplayScreen(gameScreen);
        DisplayBasicPopup(gameTutorialPopup, checkForAnimations: false);
    }

    protected override void Update( ) {
        base.Update( );
        GameSessionData.TotalTimeSecondsValue += Time.deltaTime;
    }

    protected override void GoToScene(string sceneName) {
        DataManager.Instance.UploadSessionData(GameSessionData);
        DataManager.AppSessionData.TotalPointsEarnedValue = GameSessionData.TotalPointsEarned;
        base.GoToScene(sceneName);
    }

    /// <summary>
    /// Add points scored within this game
    /// </summary>
    /// <param name="points">The amount of points to add</param>
    public void AddPoints(Vector3 position, int points) {
        GameSessionData.PointsEarnedValue += points;
        SpawnPointsPopup(position, points);
    }

    public void WinGame( ) {
        DelayAction(( ) => {
            DisplayScreen(winScreen, onHalfway: ( ) => { Destroy(objectContainer.gameObject); });
        }, gameWinDelaySeconds);
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
