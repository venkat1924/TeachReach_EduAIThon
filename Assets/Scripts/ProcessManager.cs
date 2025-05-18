using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;
public class ProcessManager : MonoBehaviour
{
    [Header("Scene References")]
    public Transform finishLine;
    public Button startRaceButton;
    public GameObject ProcessInputPanel;  // Drag your UI panel GameObject here in the Inspector
    public GameObject ChatBot;
    public GameObject StartRaceButton;


    public GameObject ganttChartPanel;
    public GameObject ganttBarPrefab;
    public TextMeshProUGUI ganttTextBox;

    private float globalTime = 0f;
    private bool raceStarted = false;

    private Dictionary<CarData, float> remainingTime = new Dictionary<CarData, float>();
    private Dictionary<CarData, float> secondAccumulator = new Dictionary<CarData, float>();

    private CarData currentRunningCar = null;
    private float lastSwitchTime = 0f;
    private bool allFinished = false;

    private List<GanttEntry> ganttLog = new List<GanttEntry>();

    private class GanttEntry
    {
        public string processName;
        public float startTime;
        public float endTime;
    }

    void Start()
    {
        if (startRaceButton != null)
            startRaceButton.onClick.AddListener(OnStartRace);
    }

    public void OnStartRace()
    {
        globalTime = 0f;
        raceStarted = true;
        currentRunningCar = null;
        lastSwitchTime = 0f;
        allFinished = false;

        if (ProcessInputPanel != null)
            ProcessInputPanel.SetActive(false);

        if (ChatBot != null)
            ChatBot.SetActive(false);

       
        StartRaceButton.SetActive(false);

        remainingTime.Clear();
        secondAccumulator.Clear();
        ganttLog.Clear();

        foreach (Transform child in ganttChartPanel.transform)
            Destroy(child.gameObject);

        int i = 0;
        foreach (var car in ProcessCar.spawnedCars)
        {
            car.isRunning = false;
            car.carObject.transform.position = new Vector3(
                car.carObject.transform.position.x,
                car.carObject.transform.position.y,
                0f
            );
        }
    }


    void Update()
    {
        if (!raceStarted || allFinished)
            return;

        globalTime += Time.deltaTime;

        // Step 1: check which cars should start
        foreach (var car in ProcessCar.spawnedCars)
        {
            if (!car.isRunning && globalTime >= car.arrivalTime)
            {
                car.isRunning = true;

                if (!remainingTime.ContainsKey(car))
                    remainingTime[car] = car.burstTime;

                if (!secondAccumulator.ContainsKey(car))
                    secondAccumulator[car] = 0f;
            }
        }

        // Step 2: Find highest-priority car that's running
        var eligibleCars = ProcessCar.spawnedCars
            .Where(c => c.isRunning && remainingTime[c] > 0f)
            .OrderBy(c => c.priority)
            .ToList();

        if (eligibleCars.Count > 0)
        {
            var newCar = eligibleCars[0];

            if (currentRunningCar != newCar)
            {
                if (currentRunningCar != null)
                {
                    ganttLog.Add(new GanttEntry
                    {
                        processName = currentRunningCar.name, // use this instead of currentRunningCar.name
                        startTime = lastSwitchTime,
                        endTime = globalTime
                    });

                }

                currentRunningCar = newCar;
                lastSwitchTime = globalTime;
            }

            float dt = Time.deltaTime;
            float remT = remainingTime[currentRunningCar];
            float distLeft = finishLine.position.z - currentRunningCar.carObject.transform.position.z;

            if (dt >= remT)
            {
                currentRunningCar.carObject.transform.Translate(0f, 0f, distLeft, Space.World);
                remainingTime[currentRunningCar] = 0f;
                currentRunningCar.burstTime = 0;
                currentRunningCar.isRunning = false;

                ganttLog.Add(new GanttEntry
                {
                    processName = currentRunningCar.name,
                    startTime = lastSwitchTime,
                    endTime = globalTime
                });

                currentRunningCar = null;
            }
            else
            {
                float speed = distLeft / remT;
                currentRunningCar.carObject.transform.Translate(0f, 0f, speed * dt, Space.World);
                remainingTime[currentRunningCar] -= dt;

                secondAccumulator[currentRunningCar] += dt;
                if (secondAccumulator[currentRunningCar] >= 1f)
                {
                    int secs = Mathf.FloorToInt(secondAccumulator[currentRunningCar]);
                    currentRunningCar.burstTime = Mathf.Max(0, currentRunningCar.burstTime - secs);
                    secondAccumulator[currentRunningCar] -= secs;
                }
            }
        }

        // Step 3: Check if all cars are finished
        if (ProcessCar.spawnedCars.All(c => remainingTime.ContainsKey(c) && remainingTime[c] <= 0f))
        {
            if (currentRunningCar != null)
            {
                ganttLog.Add(new GanttEntry
                {
                    processName = currentRunningCar.name,
                    startTime = lastSwitchTime,
                    endTime = globalTime
                });
                currentRunningCar = null;
            }

            allFinished = true;
            raceStarted = false;
            GenerateGanttChart();
        }
    }

    private void GenerateGanttChart()
    {
        if (ganttTextBox == null)
        {
            Debug.LogWarning("ganttTextBox not assigned!");
            return;
        }

        float totalDuration = ganttLog.Max(e => e.endTime);
        int chartWidth = 50; // Width of the text chart

        Dictionary<string, char[]> chartLines = new Dictionary<string, char[]>();

        foreach (var entry in ganttLog)
        {
            if (!chartLines.ContainsKey(entry.processName))
                chartLines[entry.processName] = Enumerable.Repeat('=', chartWidth).ToArray();

            int startIdx = Mathf.FloorToInt((entry.startTime / totalDuration) * chartWidth);
            int endIdx = Mathf.CeilToInt((entry.endTime / totalDuration) * chartWidth);

            for (int i = startIdx; i < endIdx-1 && i < chartWidth; i++)
                chartLines[entry.processName][i] = '#';
        }

        // Build the display text
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        foreach (var kvp in chartLines)
        {
            sb.AppendLine($"{kvp.Key.PadRight(10)}: {new string(kvp.Value)}");
        }

        ganttTextBox.text = sb.ToString();
        Debug.Log("Text-based Gantt Chart:\n" + sb.ToString());

    Debug.Log("Gantt Chart Generated:");
        foreach (var entry in ganttLog)
            Debug.Log($"{entry.processName}: {entry.startTime:F2}s to {entry.endTime:F2}s");
    }

}
