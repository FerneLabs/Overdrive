using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.PlasticSCM.Editor.WebApi;
using UnityEngine;

public class RoadSpawnerScript : MonoBehaviour
{
    [SerializeField] private GameLogic gameLogic;
    [SerializeField] private GameObject road;
    [SerializeField] private float speed = 20;
    [SerializeField] private float acceleration = 4;
    [SerializeField] private float maxSpeed = 60;
    [SerializeField] private float offCameraPoint = -270;

    private List<GameObject> _roadInstances = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic").GetComponent<GameLogic>();
        _roadInstances.Add(GameObject.FindGameObjectWithTag("InitialRoad")); // Add initial road to spawner so it can be controlled
        Debug.Log(_roadInstances.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameLogic.gameStarted || gameLogic.gameOver) { return; }

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
            if (_roadInstances[_roadInstances.Count - 1].transform.position.z < 1f) {
                Debug.Log(_roadInstances[_roadInstances.Count - 1].transform.position.z);
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
}
