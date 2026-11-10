using UnityEngine;

public class LevelCompleteReporter : MonoBehaviour
{
    // Puente para usar desde el inspector (por ejemplo, en el evento On Completed del MatchingGame)
    public void ReportCompleted() => ProgressManager.Instance.CompleteCurrentLevel();
}