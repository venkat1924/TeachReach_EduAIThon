using UnityEngine;

public class CameraSimulationFollow : MonoBehaviour
{
    public float zoomSpeed = 10f;
    public float minSize = 20f;
    public float maxSize = 100f;
    public string carTag = "Car";

    void LateUpdate()
    {
        GameObject[] cars = GameObject.FindGameObjectsWithTag(carTag);
        if (cars.Length == 0) return;

        // Compute bounding box around all cars
        Bounds bounds = new Bounds(cars[0].transform.position, Vector3.zero);
        foreach (GameObject car in cars)
        {
            bounds.Encapsulate(car.transform.position);
        }

        Vector3 center = bounds.center;

        // Set camera to look down from above, keep X and Y fixed, only Z moves
        transform.position = new Vector3(transform.position.x, transform.position.y, center.z);

        // Adjust zoom (orthographicSize) based on Z spread
        float size = bounds.size.z / 2f;
        Camera.main.orthographicSize = Mathf.Clamp(size, minSize, maxSize);
    }
}
