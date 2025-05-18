using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ProcessCar : MonoBehaviour
{
    public GameObject carPrefab;
    public Button spawnButton;
    public TMP_InputField Input_ArrivalTime;
    public TMP_InputField Input_ProcessName;
    public TMP_InputField Input_BurstTime;
    public TMP_InputField Input_Priority;
    public float carSpacing = 550f;
    public Vector3 baseSpawnPosition = new Vector3(0f, 1f, 0f);

    private int carCount = 0;

    public static List<CarData> spawnedCars = new List<CarData>();

    void Start()
    {
        if (spawnButton != null)
            spawnButton.onClick.AddListener(SpawnCar);
    }

    void SpawnCar()
    {
        // Parse times
        if (!float.TryParse(Input_ArrivalTime.text, out float arrivalTime))
        {
            Debug.LogWarning("Invalid arrival time!");
            return;
        }
        if (!float.TryParse(Input_BurstTime.text, out float burstTime))
        {
            Debug.LogWarning("Invalid burst time!");
            return;
        }
        if (!float.TryParse(Input_Priority.text, out float priority))
        {
            Debug.LogWarning("Invalid priority!");
            return;
        }
        if (string.IsNullOrWhiteSpace(Input_ProcessName.text))
        {
            Debug.LogWarning("Invalid name!");
            return;
        }

        Vector3 spawnPos = new Vector3(carCount * carSpacing, baseSpawnPosition.y, baseSpawnPosition.z);
        GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
        car.transform.localScale = new Vector3(500f, 500f, 500f);

        // ✅ Set tag for camera to find it
        car.tag = "Car";  // Make sure this tag exists in Unity Editor!

        // Store data
        CarData carData = new CarData
        {
            carObject = car,
            arrivalTime = arrivalTime,
            burstTime = burstTime,
            priority = priority,
            isRunning = false,
            name = Input_ProcessName.text,
        };

        spawnedCars.Add(carData);
        carCount++;
    }

    public void SpawnCarsFromList(ProcessDefinitionList list)
    {
        foreach (var pd in list.processes)
        {
            Vector3 spawnPos = new Vector3(carCount * carSpacing, baseSpawnPosition.y, baseSpawnPosition.z);
            GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            car.transform.localScale = new Vector3(500f, 500f, 500f);
            car.tag = "Car";

            var data = new CarData
            {
                carObject = car,
                arrivalTime = pd.arrivalTime,
                burstTime = pd.burstTime,
                priority = pd.priority,
                isRunning = false,
                name = pd.processName
            };
            spawnedCars.Add(data);
            carCount++;
        }
    }
}

public class CarData
{
    public GameObject carObject;
    public float arrivalTime;
    public float burstTime;
    public float priority;
    public bool isRunning;
    public string name;
}
