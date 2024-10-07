using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class CarScript : MonoBehaviour
{
    [SerializeField] private float WheelRoationSpeed = 180;
    [SerializeField] private GameObject[] carWheels;
    private GameManager _gameManager;
    private bool _handledGameOver = false;
    private float _moveTo = 0;
    private int _moveTime = 0;
    private float _movingTimer = 0;
    private float _minMovement = 0;
    private float _maxMovement = 0;
    private Vector3 _velocity = Vector3.zero;
    private List<int> scores = new List<int>();

    void Start()
    {
        _gameManager = GameManager.instance;
        // As some prefabs have different base transform position, use relative coordinates to set the threshold of min/max car movement.
        _minMovement = transform.position.z - 3;
        _maxMovement = transform.position.z + 15;

        scores.Add(0); 
        scores.Add(0);

        _moveTo = transform.position.z;
    }

    void Update()
    {
        if (!_gameManager.gameStarted) { return; }

        handleWheelRotation();
        
        if (_movingTimer < _moveTime && !_gameManager.gameOver)
        {
            _movingTimer += Time.deltaTime;
            handleCarMovement();
        }
        else if (!_gameManager.gameOver)
        {
            _movingTimer = 0;
            GetMovementValues();
        }
        
        if (_gameManager.gameOver) { handleGameOver(); }
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

    void GetMovementValues()
    {
        _moveTime = UnityEngine.Random.Range(3, 6);

        List<int> prevScores = new List<int>(scores);

        scores[0] = _gameManager.gameState.players[_gameManager.playerId].score;
        scores[1] = _gameManager.gameState.players[_gameManager.adversaryPlayerId].score;

        if (gameObject.CompareTag("PlayerCar"))
        {
            if (prevScores[0] == scores[0] && transform.position.z <= _moveTo) 
            {
                _moveTo = Math.Max(_minMovement, _moveTo - 1);
            } else {
                _moveTo = (scores[0] * (_maxMovement - _minMovement) / 100) + _minMovement;
            }
        } 
        else 
        {
            if (prevScores[1] == scores[1] && transform.position.z <= _moveTo) 
            {
                _moveTo = Math.Max(_minMovement, _moveTo - 1);
            } else {
                _moveTo = (scores[1] * (_maxMovement - _minMovement) / 100) + _minMovement;
            }
        }

        Debug.Log($"[GetMovementValues] [{gameObject.tag}] {_moveTime} / {_minMovement - _moveTo}");
    }

    void handleCarMovement()
    {
        transform.position = Vector3.SmoothDamp(
            transform.position,                                                // Current position
            new Vector3(transform.position.x, transform.position.y, _moveTo),  // Target position
            ref _velocity,                                                     // Reference to velocity
            _moveTime                                                          // Smooth time (time to reach target)
        );
    }

    void handleGameOver()
    {
        if (_handledGameOver) { return; }

        //Debug.Log($"Current car: {gameObject.tag}, winner: {_gameManager.winner}");

        if (gameObject.CompareTag("PlayerCar")) // Finish logic for player car
        {
            if (_gameManager.winner == 0) // If win
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
            if (_gameManager.winner == 1) // If win
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
