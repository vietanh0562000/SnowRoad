using System;
using FalconGames._GamePlay.Scripts.Level.Signals;
using Mono.Cecil;
using PuzzleGames;
using Zenject;

public class StepsViewModel : ViewModel
{
    public int StartStepsCount { get; private set; }

    public ObservableProperty<int> StepsLeft { get; private set; } = new ObservableProperty<int>();
    public ObservableProperty<int> StepsForBonus { get; private set; } = new ObservableProperty<int>();
    
    public DelegateCommand MoveToPreviousStepCommand { get; private set; }

    private bool isBonusReceivedEarlier;

    private StepsRecorder _stepsRecorder;
    private IslandsUpdater _islandsUpdater;
    
    private  SignalBus _signalBus;

    public StepsViewModel(LevelSettings levelSettings, IslandsUpdater islandsUpdater, StepsRecorder stepsRecorder, bool isBonusReceivedEarlier, SignalBus signalBus) {
        _stepsRecorder = stepsRecorder;

        InitCommands();

        _islandsUpdater = islandsUpdater;

        this.isBonusReceivedEarlier = isBonusReceivedEarlier;

        StartStepsCount = StepsLeft.Value = levelSettings.Steps;
        StepsForBonus.Value = levelSettings.StepsForBonus;
        _signalBus = signalBus;
    }

    [Zenject.Inject]
    private void InitSignals(Zenject.SignalBus signalBus){
        signalBus.Subscribe<IslandUpdatingSignal>(OnIslandUpdating);
    }

    private void InitCommands(){
        MoveToPreviousStepCommand = new DelegateCommand(OnMovedToPreviousStep, _stepsRecorder.CanMoveToPrevStep);
        _stepsRecorder.StepRecorded += MoveToPreviousStepCommand.InvokeCanExecuteChanged;
    }

    private void OnIslandUpdating(){
        _stepsRecorder.RecordStep();
        StepsLeft.Value--;

        if (StepsLeft == 0)
        {
            _islandsUpdater.IsIslandsUpdatingAllowed = false;
            _signalBus.Fire(new LevelFailedSignal());
        }
           
    }

    private void OnMovedToPreviousStep(){
        StepsLeft.Value++;
        _stepsRecorder.MoveToPreviousStep();
        _islandsUpdater.IsIslandsUpdatingAllowed = true;
        _islandsUpdater.ExternalUpdateStarted(_stepsRecorder.IslandAnimationDuration);
        MoveToPreviousStepCommand.InvokeCanExecuteChanged();
    }

    public bool IsBonusReceived(){
        if(isBonusReceivedEarlier)
            return true;

        return StartStepsCount - StepsLeft == StepsForBonus;
    }
}
