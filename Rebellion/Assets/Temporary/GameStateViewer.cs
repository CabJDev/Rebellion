using TMPro;
using UnityEngine;

public class GameStateViewer : MonoBehaviour
{
    [SerializeField]
    private TMP_Text clock;
    [SerializeField]
    private TMP_Text day;
    [SerializeField]
    private TMP_Text phase;

    public void SetTime(float currentStateLength) { clock.text = currentStateLength.ToString() + "s"; }
    public void SetDay(int currentDay) { day.text = "Day: " + currentDay.ToString(); }
    public void SetPhase(string currentPhase) { phase.text = currentPhase.ToString(); }
}
