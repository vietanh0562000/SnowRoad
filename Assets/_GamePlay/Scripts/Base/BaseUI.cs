using UnityEngine;
using DG.Tweening;
using PuzzleGames;

public class BaseUI : MonoBehaviour, IPauseHandler
{
    [field:Header("> Hint UI")]
    [field:SerializeField] public HintUI HintUI { get; private set; }

    [Header("> Main UI")]
    [SerializeField] private CanvasGroup mainUI;
    [Space]
    [SerializeField] private StepsView stepsView;
    [SerializeField] private BindableAnimatedButton previousStepButton;

    [Header("> Panels")]
    [SerializeField] private CanvasGroup background;
    [Space]
    [SerializeField] private PanelAnimator levelCompletedPanel;
    [SerializeField] private PanelAnimator allLevelsCompletedPanel;
    [SerializeField] private PanelAnimator levelFailedPanel;
    [Space]
    [SerializeField] private PanelAnimator pausePanel;

    public const float PanelsAnimationDuration = 1f;

    [Header("> Bonus")]
    [SerializeField] private BonusReceivedView bonusReceivedView;

    private bool isLevelCompleted;

    private GuideSystem _guideSystem;
    private PauseManager _pauseManager;

    private StepsViewModel _stepsViewModel;
    
    private Timer _bonusReceivedViewTimer;
    private Zenject.SignalBus _signalBus;

    [Zenject.Inject]
    private void Init(Zenject.SignalBus signalBus, StepsViewModel stepsViewModel, GuideSystem guideSystem, PauseManager pauseManager){
        _signalBus = signalBus;
        _stepsViewModel = stepsViewModel;
        _guideSystem = guideSystem;
        _pauseManager = pauseManager;

        _pauseManager.Subscribe(this);

        mainUI.alpha = 0;
        
        previousStepButton.OnClick.AddListener(OnClickPreviousButton);
        previousStepButton.Interactable = _stepsViewModel.MoveToPreviousStepCommand.CanExecute();
        _stepsViewModel.MoveToPreviousStepCommand.CanExecuteChanged += () =>
        {
            previousStepButton.Interactable = _stepsViewModel.MoveToPreviousStepCommand.CanExecute();
        };
        stepsView.Init(signalBus, stepsViewModel);
    }

    private void OnClickPreviousButton()
    {
        IResource goldManager = ResourceType.Gold.Manager();
        if (goldManager.GetAmount() >= 10)
        {
            if (!_stepsViewModel.MoveToPreviousStepCommand.CanExecute())
            {
                UIToastManager.Instance.Show("Can't move to previous step");
            }
            goldManager.Subtract(10);
            _stepsViewModel.MoveToPreviousStepCommand.Execute();
        }
        else
        {
            UIToastManager.Instance.Show("Not enough gold");
        }
    }
    
    public void ShowMainUI() {
        mainUI.DOFade(1, 1f).SetEase(Ease.OutCubic).SetEase(Ease.OutCubic).SetDelay(0.5f);
    }
    
    private void Update() {
        if(isLevelCompleted || _guideSystem.IsGuideShowing)
            return;

        if(Input.GetKeyDown(KeyCode.Escape)){
            if(HintUI.HintOpened){
                HintUI.ClosePanel();
            }
            else if(_pauseManager.IsPaused){
                UnpauseGame();
            }
            else{
                PauseGame();
            }
        }
    }

    public void PauseGame(){
        pausePanel.OpenPanel();
        _pauseManager.SetPaused(true);
    }

    public void UnpauseGame(){
        pausePanel.ClosePanel(deactivateGameObject: true);
        _pauseManager.SetPaused(false);
    }

    public void RestartLevel()
    {
        IResource heart = ResourceType.Heart.Manager();
        if (heart.GetAmount() > 0)
        {
            _signalBus.Fire<ReloadLevelSignal>();
        }
        else
        {
            UIToastManager.Instance.Show("Not enough heart");
        }
        
    } 
    public void LoadMenu() => _signalBus.Fire<LoadMenuSignal>();
    public void LoadNextLevel() => _signalBus.Fire<LoadNextLevelSignal>();

    public void LevelCompleted(bool isLastLevelCompleted = false){
        isLevelCompleted = true;

        mainUI.DOFade(0, 0.2f).SetEase(Ease.OutCubic);
        ChangeBackgroundVisibility(true, PanelsAnimationDuration);

        PanelAnimator panel = isLastLevelCompleted ? allLevelsCompletedPanel : levelCompletedPanel;
        Timer.StartNew(this, PanelsAnimationDuration, () => panel.gameObject.SetActive(true));

        if(_stepsViewModel.IsBonusReceived()){
            bonusReceivedView.gameObject.SetActive(true);
            _bonusReceivedViewTimer = Timer.StartNew(this, PanelsAnimationDuration + 0.5f, () => bonusReceivedView.Show(_stepsViewModel.StepsForBonus));
        }
    }

    public void LevelFailed()
    {
        mainUI.DOFade(0, 0.2f).SetEase(Ease.OutCubic);
        ChangeBackgroundVisibility(true, PanelsAnimationDuration);
        Timer.StartNew(this, PanelsAnimationDuration, () => levelFailedPanel.gameObject.SetActive(true));
    }

    public void ChangeBackgroundVisibility(bool visible, float delay = 0){
        background.gameObject.SetActive(true);
        
        background.DOFade(visible ? 1 : 0, 0.25f).SetDelay(delay).OnComplete(() => {
            if(visible == false) 
                background.gameObject.SetActive(false);
        });
    }

    public void SetPaused(bool isPaused){
        ChangeBackgroundVisibility(isPaused);
    }

    private void OnDestroy() {
        if(_bonusReceivedViewTimer != null)
            _bonusReceivedViewTimer.Dispose();

        _pauseManager.Unsubscribe(this);
    }
}