using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class CarScript : MonoBehaviour
{
    [SerializeField] private float WheelRoationSpeed = 180;
    [SerializeField] private GameObject[] carWheels;
    [SerializeField] private GameLogic gameLogic;
    private bool _handledGameOver = false;
    private int _moving = 0;
    private int _moveTime = 0;
    private int _moveAmount = 0;
    private float _movingTimer = 0;
    private float _minMovement = 0;
    private float _maxMovement = 0;

    void Start()
    {
        gameLogic = GameObject.FindGameObjectWithTag("GameLogic").GetComponent<GameLogic>();
        // As some prefabs have different base transform position, use relative coordinates to set the threshold of min/max car movement.
        _minMovement = transform.position.z - 5;
        _maxMovement = transform.position.z + 5;
    }

    void Update()
    {
        if (!gameLogic.gameStarted) { return; }

        handleWheelRotation();

        if (_movingTimer < _moveTime && !gameLogic.gameOver)
        {
            _movingTimer += Time.deltaTime;
            handleCarMovement();
        }
        else if (!gameLogic.gameOver)
        {
            _movingTimer = 0;
            GetMovementValues();
        }
        
        if (gameLogic.gameOver) { handleGameOver(); }
    }

    void GetMovementValues()
    {
        _moving = UnityEngine.Random.Range(0, 2);
        _moveTime = UnityEngine.Random.Range(3, 6);
        _moveAmount = UnityEngine.Random.Range(3, 6);
        // Debug.Log($"Movement {gameObject.tag}: Direction - {_moving}, Amount - {_moveAmount}, Time - {_moveTime}.");
    }

    void handleWheelRotation()
    {
        foreach (GameObject wheel in carWheels)
        {
            // Each 2 full rotations reset to 0 to avoid getting bit values in position
            if (wheel.transform.localEulerAngles.x >= 720)
            {
                wheel.transform.eulerAngles = new Vector3(0, wheel.transform.eulerAngles.y, wheel.transform.eulerAngles.z);
            }
            wheel.transform.Rotate(Vector3.right * WheelRoationSpeed * Time.deltaTime);
        }
        // Gradually increment wheel rotation speed
        if (WheelRoationSpeed <= 3600) { WheelRoationSpeed += 180 * Time.deltaTime; }
    }

    void handleCarMovement()
    {
        if (transform.position.z <= _minMovement) { _moving = 1; } // Set to move forward if it reaches the limit of -5
        if (transform.position.z >= _maxMovement) { _moving = 0; } // Set to move backwards if it reaches the limit of 11

        if (_moving == 1) // Forward
        {
            transform.position = transform.position + (Vector3.forward * (_moveAmount / _moveTime) * Time.deltaTime);
        }
        else // Backwards
        {
            transform.position = transform.position + (Vector3.back * (_moveAmount / _moveTime) * Time.deltaTime);
        }
    }

    void handleGameOver()
    {
        if (_handledGameOver) { return; }

        //Debug.Log($"Current car: {gameObject.tag}, winner: {gameLogic.winner}");

        if (gameObject.CompareTag("PlayerCar")) // Finish logic for player car
        {
            if (gameLogic.winner == 0) // If win
            {
                transform.position = transform.position + (Vector3.forward * 120 * Time.deltaTime);
            }
            else // If lose
            {
                transform.position = transform.position + (Vector3.forward * 70 * Time.deltaTime);
            }
        }

        if (gameObject.CompareTag("AdversaryCar")) // Finish logic for adversary car
        {
            if (gameLogic.winner == 1) // If win
            {
                transform.position = transform.position + (Vector3.forward * 120 * Time.deltaTime);
            }
            else // If lose
            {
                transform.position = transform.position + (Vector3.forward * 70 * Time.deltaTime);
            }
        }

        if (gameObject.transform.position.z > 40) { _handledGameOver = true; } // Mark as handled when the car is out of screen
    }

}
