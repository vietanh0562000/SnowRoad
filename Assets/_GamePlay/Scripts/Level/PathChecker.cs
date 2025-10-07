using System.Collections.Generic;

public class PathChecker
{
    private Island _startIsland;
    private List<Island> _islands;

    private Zenject.SignalBus _signalBus;

    [Zenject.Inject]
    private void Init(Zenject.SignalBus signalBus, IslandsProvider islandsProvider){
        _signalBus = signalBus;

        _islands = islandsProvider.Islands;
        _startIsland = _islands.Find(island => island.Type == Island.IslandType.Start);
    }

    public void CheckPath(){
        Island currentIsland = _startIsland;
        HashSet<Island> islandsWithoutEnergy = new HashSet<Island>(_islands);

        // Ensure start island stays energized
        if(currentIsland != null){
            islandsWithoutEnergy.Remove(currentIsland);
        }

        while(currentIsland != null){
            if(currentIsland.TryGetNextIsland(out Island nextIsland)){
                // If the next is Finish, consider path complete regardless of energy island flag
                if(nextIsland.Type == Island.IslandType.Finish){
                    islandsWithoutEnergy.Remove(nextIsland);
                    nextIsland.AcivateEnergy();
                    _signalBus.Fire<LevelCompletedSignal>();
                    break;
                }

                if(nextIsland.IsEnergyIsland == false)
                    break;
                
                if(Island.IsInputAndOutputCorrespond(nextIsland.GetInputDirection(), currentIsland.GetOutputDirection()) == false){
                    UnityEngine.Debug.Log($"Direction mismatch: current '{currentIsland.name}' out={currentIsland.GetOutputDirection()} -> next '{nextIsland.name}' in={nextIsland.GetInputDirection()}");
                    break;
                }

                islandsWithoutEnergy.Remove(nextIsland);
                nextIsland.AcivateEnergy();

                currentIsland = nextIsland;
            }
            else 
                break;
        }

        foreach(Island island in islandsWithoutEnergy)
            island.DeactivateEnergy();
    }
}
