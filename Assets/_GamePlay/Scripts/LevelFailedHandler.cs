using FalconGames._GamePlay.Scripts.Level.Signals;
using PuzzleGames;

public class LevelFailedHandler
{
    private BaseUI _baseUI;
    private BaseSoundsPlayer _baseSoundsPlayer;
    private BaseCamera _baseCamera;

    private int _levelNumber;
    private PlayerData _playerData;

    private StepsViewModel _stepsViewModel;
    private LevelsInfoProvider _levelsInfoProvider;
    
    [Zenject.Inject]
    private void Init(LevelsInfoProvider levelsInfoProvider, PlayerData playerData, StepsViewModel stepsViewModel) {
        _levelsInfoProvider = levelsInfoProvider;
        _stepsViewModel = stepsViewModel;
        _playerData = playerData;
    }

    [Zenject.Inject]
    private void InitBase(BaseUI baseUI, BaseSoundsPlayer baseSoundsPlayer, BaseCamera baseCamera){
        _baseUI = baseUI;
        _baseSoundsPlayer = baseSoundsPlayer;
        _baseCamera = baseCamera;
    }

    [Zenject.Inject]
    private void InitSignals(Zenject.SignalBus signalBus, int levelNumber){
        _levelNumber = levelNumber;

        signalBus.Subscribe<LevelFailedSignal>(OnLevelCompleted);
    }

    private void OnLevelCompleted(){
        IResource heart = ResourceType.Heart.Manager();
        heart.Subtract(1);
        _baseUI.LevelFailed();
    }
}