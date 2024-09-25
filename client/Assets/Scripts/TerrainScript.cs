using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainScript : MonoBehaviour
{
    [SerializeField] private float initialSpeed = 1;
    [SerializeField] private float acceleration = 1;
    [SerializeField] private float maxSpeed = 15;
    [SerializeField] private GameLogic gameLogic;

    // Start is called before the first frame update
    void Start()
    {
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic").GetComponent<GameLogic>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameLogic.gameStarted) { return; }

        if (!gameLogic.gameOver) // Stop moving if race is over
        {
            transform.position = transform.position + (Vector3.back * initialSpeed * Time.deltaTime);
            if (initialSpeed <= maxSpeed) { initialSpeed += acceleration * Time.deltaTime; }
        }
    }
}