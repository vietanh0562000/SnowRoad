public class HintViewModel : ViewModel
{
    public event System.Action StepChanged;

    public ICommand MoveToPreviousStepCommand { get; private set; }
    public ICommand MoveToNextStepCommand { get; private set; }

    public ICommand ActivateHintCommand { get; private set; }
    public ICommand DeactivateHintCommand { get; private set; }

    public int CurrentStep => _hintSystem.CurrentStepIndex + 1;
    public int StepsCount => _hintSystem.StepsCount;

    public bool IsAdViewed {
        get => _data.IsAdViewed;
        set => _data.IsAdViewed = value;
    }

    private HintSystem _hintSystem;

    private SaveSystem<Data> _saveSystem;
    private Data _data = new Data();

    public HintViewModel(HintSystem hintSystem){
        _saveSystem = new SaveSystem<Data>();
        _data = _saveSystem.LoadData() as Data;

        _hintSystem = hintSystem;
        StepChanged += UpdateStepCommandsCanExecute;

        InitCommands();
    }

    private void InitCommands(){
        MoveToPreviousStepCommand = new DelegateCommand(
            () => {
                _hintSystem.PreviousStep();
                StepChanged?.Invoke();
            },
            () => _hintSystem.CanMoveToPreviousStep());
        
        MoveToNextStepCommand = new DelegateCommand(
            () => {
                _hintSystem.NextStep();
                StepChanged?.Invoke();
            },
            () => _hintSystem.CanMoveToNextStep());
        
        ActivateHintCommand = new DelegateCommand(_hintSystem.ActivateHint);
        DeactivateHintCommand = new DelegateCommand(_hintSystem.DeactivateHint);
    }

    private void UpdateStepCommandsCanExecute(){
        MoveToNextStepCommand.InvokeCanExecuteChanged();
        MoveToPreviousStepCommand.InvokeCanExecuteChanged();
    }

    public void ViewAd(System.Action onComplete){
        //TODO: Minus player resourse
        onComplete?.Invoke();
    }

    public async System.Threading.Tasks.Task<bool> IsInternetReachable() => await AdsManager.IsInternetReachable();

    public class Data : ISaveable
    {
        public bool IsAdViewed;
        
        public string FileName => "hint_data";
    }
}