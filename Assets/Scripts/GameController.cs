using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using UnityEngine.Video;

public abstract class GameController : UIController {
    [Header("GameController")]
    [SerializeField] private GameObject confettiParticlePrefab;
    [SerializeField] private GameObject pointsPopupPrefab;
    [SerializeField] protected Transform objectContainer;
    [SerializeField] private VideoPlayer tutorialPlayer;
    [SerializeField] private RenderTexture tutorialVisual;
    [SerializeField, TextArea] private string tutorialText;

    protected VisualElement gameScreen;
    protected VisualElement pauseScreen;
    protected VisualElement winScreen;

    protected VisualElement playGoalInfoPopup;
    protected VisualElement gameTutorialPopup;

    private Button selectedCravingButton;

    public GameSessionData GameSessionData { get; private set; }
    public bool IsPlayingGame => (CurrentScreen == gameScreen && CurrentPopup == null);

    protected override void Awake( ) {
        base.Awake( );

        gameScreen = ui.Q<VisualElement>("GameScreen");
        ui.Q<Button>("PauseButton").clicked += ( ) => { DisplayScreen(pauseScreen); };
        ui.Q<Button>("HowToPlayButton").clicked += ( ) => { DisplayBasicPopup(gameTutorialPopup); };

        pauseScreen = ui.Q<VisualElement>("PauseScreen");
        ui.Q<Button>("ResumeButton").clicked += ( ) => { DisplayScreen(gameScreen); };
        ui.Q<Button>("QuitButton").clicked += ( ) => { GoToScene("MainMenu"); };

        winScreen = ui.Q<VisualElement>("WinScreen");
        ui.Q<Button>("HomeButton").clicked += ( ) => {
            if (selectedCravingButton != null && animatingVisualElements.Count == 0) {
                GoToScene("MainMenu");
            } else {
                FlashTextValidation(new List<Label>( ) { ui.Q<Label>("CraveQuestionLabel"), ui.Q<Label>("CraveSubtitleLabel") });
            }
        };
        ui.Q<Button>("PlayAgainButton").clicked += ( ) => {
            if (selectedCravingButton != null && animatingVisualElements.Count == 0) {
                GoToScene(SceneManager.GetActiveScene( ).name);
            } else {
                FlashTextValidation(new List<Label>( ) { ui.Q<Label>("CraveQuestionLabel"), ui.Q<Label>("CraveSubtitleLabel") });
            }
        };

        // Set up the craving intensity buttons
        List<Button> cravingIntensityButtons = ui.Q<VisualElement>("CravingIntensityButtons").Query<Button>( ).ToList( );
        for (int i = 0; i < cravingIntensityButtons.Count; i++) {
            cravingIntensityButtons[i].RegisterCallback<ClickEvent>((e) => {
                selectedCravingButton?.RemoveFromClassList("uofr-button-selected");
                selectedCravingButton = (Button) e.currentTarget;
                selectedCravingButton.AddToClassList("uofr-button-selected");
            });
        }

        popupOverlay.RegisterCallback<ClickEvent>((e) => {
            if ((VisualElement) e.target == popupOverlay) {
                HideCurrentPopup( );
            }
        });

        gameTutorialPopup = ui.Q<VisualElement>("GameTutorialPopup");
        ui.Q<Button>("PlayButton").clicked += ( ) => { HideCurrentPopup( ); };

        // Set tutorial popup information
        tutorialPlayer.url = Path.Combine(Application.streamingAssetsPath, name.Replace(" ", "") + "Tutorial.mp4");
        tutorialPlayer.time = 0;
        ui.Q<VisualElement>("TutorialVisual").style.backgroundImage = new StyleBackground(Background.FromRenderTexture(tutorialVisual));
        ui.Q<Label>("TutorialLabel").text = tutorialText;
        ui.Q<Label>("TitleLabel").text = name;

        playGoalInfoPopup = ui.Q<VisualElement>("PlayGoalInfoPopup");
        ui.Q<Button>("PlayGoalInfoButton").clicked += ( ) => DisplayBasicPopup(playGoalInfoPopup);
        ui.Q<Button>("PlayGoalInfoContinueButton").clicked += ( ) => HideCurrentPopup( );

        // Create a new game session data entry for this game
        GameSessionData = new GameSessionData(name, DataManager.AppSessionData.TotalPointsEarned);
        DataManager.AppSessionData.OnTotalTimeSecondsChange += ( ) => {
            float secondsRemaining = Mathf.Max(0, PLAY_GOAL_SECONDS - DataManager.AppSessionData.TotalTimeSeconds);
            string timerString = string.Format("{0:0}:{1:00}", (int) secondsRemaining / 60, (int) secondsRemaining % 60);
            ui.Q<Label>("RadialProgressBarLabel").text = timerString;
            ui.Q<RadialProgress>("RadialProgressBar").Progress = DataManager.AppSessionData.TotalTimeSeconds / PLAY_GOAL_SECONDS * 100f;
        };
        GameSessionData.OnPointsEarnedChange += ( ) => {
            ui.Q<Label>("ScoreLabel").text = $"Score: <b>{GameSessionData.PointsEarned:N0} pts</b>";
            ui.Q<Label>("FinalScoreLabel").text = $"+ {GameSessionData.PointsEarned:N0} pts";
            ui.Q<Label>("TotalScoreLabel").text = $"{GameSessionData.TotalPointsEarned:N0} pts";
        };
    }

    protected override void Start( ) {
        base.Start( );

        DisplayScreen(gameScreen);
        DisplayBasicPopup(gameTutorialPopup, checkForAnimations: false);

        GameSessionData.InvokeAllDelegates( );
    }

    protected override void Update( ) {
        base.Update( );
        GameSessionData.TotalTimeSecondsValue += Time.deltaTime;
    }

    protected override void GoToScene(string sceneName) {
        if (selectedCravingButton != null) {
            GameSessionData.CravingIntensity = int.Parse(selectedCravingButton.text);
        }

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
            SoundManager.Instance.PlaySoundEffect(SoundEffectType.WIN);
            SpawnConfettiParticles(Vector3.zero);
        }, GAME_WIN_DELAY_SECONDS / 2f);

        DelayAction(( ) => {
            DisplayScreen(winScreen, onHalfway: ( ) => { Destroy(objectContainer.gameObject); });
        }, GAME_WIN_DELAY_SECONDS);
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
