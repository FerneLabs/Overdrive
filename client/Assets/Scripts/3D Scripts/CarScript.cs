using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Rendering.Universal.Internal;

public class CarScript : MonoBehaviour
{
    [SerializeField] private int gearCount = 5;
    [SerializeField] private float WheelRoationSpeed = 180;
    [SerializeField] private GameObject[] carWheels;
    [SerializeField] private AudioSource engineAudioSource;
    [SerializeField] private AudioSource engineSFXAudioSource;
    [SerializeField] private AudioClip idleClip;
    [SerializeField] private AudioClip lowClip;
    [SerializeField] private AudioClip midClip;
    [SerializeField] private AudioClip highClip;
    [SerializeField] private AudioClip maxClip;
    [SerializeField] private AudioClip nitroClip;
    [SerializeField] private AudioClip downshiftClip;
    private GameManager _gameManager;
    private bool _handledGameOver = false;
    private bool _playNitro = false;
    private bool _playDownshift = false;
    private bool _isDownshifting = false;
    private float _moveTo = 0;
    private int _moveTime = 0;
    private float _movingTimer = 0;
    private float _minMovement = 0;
    private float _maxMovement = 0;
    private int _currentGear = 1;
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
        transform.position = new Vector3(transform.position.x, transform.position.y, _moveTo); // Make sure car position is the same as _moveTo at the start

        // Initialize the engine sound
        engineAudioSource.loop = true;
        engineAudioSource.volume = 0;
    }

    void Update()
    {
        if (!_gameManager.gameStarted) { return; }
        if (!_gameManager.gameOver && !engineAudioSource.isPlaying)
        {
            engineAudioSource.clip = midClip;
            engineAudioSource.Play();
            StartCoroutine(SoundManager.instance.FadeIn(engineAudioSource, 1));
        }

        if (!_gameManager.gameOver)
        {
            handleWheelRotation();
            handleEngineSound();

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

            if (gameObject.CompareTag("PlayerCar")) 
            {
                Debug.Log($"{_currentGear} | {RoadSpawnerScript.instance.speed} | {engineAudioSource.pitch} | {_isDownshifting}");
            }
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

    void handleEngineSound()
    {
        if (_isDownshifting) return;

        float speed = RoadSpawnerScript.instance.speed;  // Get current road speed
        int maxGearRPM = (int)RoadSpawnerScript.instance.maxSpeed / gearCount;

        // Calculate the target pitch based on the current speed relative to max RPM for the current gear
        float targetPitch = Mathf.Lerp(0.8f, 2f, Mathf.Clamp01(speed / (maxGearRPM * _currentGear)));
        float shiftBuffer = maxGearRPM * _currentGear * 1.05f;

        if (speed >= shiftBuffer) // Shift up
        {
            if (_currentGear < gearCount)
            {
                _currentGear++;
                engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 1f, 0.1f); // Decrease pitch for gear shift
            }
        }
        else // Increase pitch 
        {
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, Time.deltaTime * 2f);
        }
    }

    void GetMovementValues()
    {
        _moveTime = UnityEngine.Random.Range(3, 6);

        List<int> prevScores = new List<int>(scores);

        scores[0] = _gameManager.gameState.players[_gameManager.playerId].score;
        scores[1] = _gameManager.gameState.players[_gameManager.adversaryPlayerId].score;

        if (gameObject.CompareTag("PlayerCar"))
        {
            _playNitro = scores[0] > prevScores[0];
            _playDownshift = scores[0] < prevScores[0];
            
            if (prevScores[0] == scores[0] && transform.position.z <= _moveTo)
            {
                _moveTo = Math.Max(_minMovement, _moveTo - 1);
            }
            else
            {
                _moveTo = (scores[0] * (_maxMovement - _minMovement) / 100) + _minMovement;
            }
        }
        else
        {
            if (prevScores[1] == scores[1] && transform.position.z <= _moveTo)
            {
                _moveTo = Math.Max(_minMovement, _moveTo - 1);
            }
            else
            {
                _moveTo = (scores[1] * (_maxMovement - _minMovement) / 100) + _minMovement;
            }
        }
        // Debug.Log($"[GetMovementValues] [{gameObject.tag}] {_moveTime} / {_minMovement - _moveTo}");
    }

    void handleCarMovement()
    {
        if (_playNitro)
        {
            Debug.Log("Playing Nitro");
            engineSFXAudioSource.clip = nitroClip;
            engineSFXAudioSource.volume = 1;
            engineSFXAudioSource.Play();

            _playNitro = false;
        }

        if (_playDownshift)
        {
            Debug.Log("Playing downshift");
            engineSFXAudioSource.clip = downshiftClip;
            engineSFXAudioSource.volume = 1;
            engineSFXAudioSource.Play();

            _playDownshift = false;
            StartCoroutine(Downshift(1));
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,                                                // Current position
            new Vector3(transform.position.x, transform.position.y, _moveTo),  // Target position
            ref _velocity,                                                     // Reference to velocity
            _moveTime                                                          // Smooth time (time to reach target)
        );
    }

    IEnumerator Downshift(float targetPitch)
    {
        float tolerance = 0.01f; // Set a small tolerance for floating-point comparison

        RoadSpawnerScript.instance.ReduceSpeed(RoadSpawnerScript.instance.maxSpeed / gearCount);
        _isDownshifting = true;
        _currentGear--;

        while (Mathf.Abs(engineAudioSource.pitch - targetPitch) > tolerance)
        {
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, targetPitch, 0.1f);
            yield return null; // Wait for next frame
        }

        engineAudioSource.pitch = targetPitch;  // Manually set pitch due to Lerp possible inaccuracy
        _isDownshifting = false;
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

        engineAudioSource.volume = Mathf.Lerp(engineAudioSource.volume, 0, 3 * Time.deltaTime);
        engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 2.5f, 3 * Time.deltaTime);

        if (gameObject.transform.position.z > 40)
        {
            // StartCoroutine(SoundManager.instance.FadeOut(engineAudioSource, 3));
            engineAudioSource.Stop();
            _handledGameOver = true;
        } // Mark as handled when the car is out of screen
    }

}
