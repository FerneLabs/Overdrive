using System.Collections.Generic;
using UnityEngine;

public class RoadSpawnerScript : MonoBehaviour
{
    public static RoadSpawnerScript instance;

    void Awake() 
    {
        if (instance == null) 
        {
            instance = this;
        }
    }

    [SerializeField] private GameObject road;
    public float speed = 5;
    [SerializeField] private float acceleration = 5;
    public float maxSpeed = 180;
    [SerializeField] private float offCameraPoint = -270;

    private List<GameObject> _roadInstances = new List<GameObject>();
    private GameManager _gameManager;

    // Start is called before the first frame update
    void Start()
    {
        _gameManager = GameManager.instance;
        _roadInstances.Add(GameObject.FindGameObjectWithTag("InitialRoad")); // Add initial road to spawner so it can be controlled
    }

    // Update is called once per frame
    void Update()
    {
        if (!_gameManager.gameStarted || _gameManager.gameOver) { return; }

        // Handle road movement
        for (int i = _roadInstances.Count - 1; i > -1; i--) // Iterate in reverse to not mess up the list when removing the road
        {
            GameObject road = _roadInstances[i];
            road.transform.position = road.transform.position + (Vector3.back * speed * Time.deltaTime);

            if (road.transform.position.z <= offCameraPoint) // Delete when out of camera sight
            {
                Destroy(road);
                _roadInstances.RemoveAt(i);
            }
        }

        if (_roadInstances.Count > 0 && _roadInstances[_roadInstances.Count - 1])
        {
            if (_roadInstances[_roadInstances.Count - 1].transform.position.z < 3f) {
                // Debug.Log(_roadInstances[_roadInstances.Count - 1].transform.position.z);
                spawnRoad(transform.position);
            }
        }

        if (speed < maxSpeed)
        {
            speed += acceleration * Time.deltaTime;
        }
    }

    void spawnRoad(Vector3 position) 
    {
        GameObject roadPrefab = Instantiate(road, position, transform.rotation);
        _roadInstances.Add(roadPrefab);
        Debug.Log($"Spawned at speed: {speed}.");
    }

    public void ReduceSpeed(float reductionAmount)
    {
        speed = Mathf.Max(0, speed - reductionAmount); // Ensure speed doesn't go below 0
    }
}
