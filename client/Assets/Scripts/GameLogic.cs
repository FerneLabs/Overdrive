using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NativeWebSocket;

public class GameLogic : MonoBehaviour
{
    public bool gameStarted = false;
    public bool gameOver = false;
    public int winner;
    [SerializeField] private string serverAddress = "ws://localhost:8080";
    [SerializeField] private float gameDuration = 15;

    private WebSocket _websocket;

    public void startGame(int designatedWinner)
    {
        gameStarted = true;
        winner = designatedWinner;
        Invoke("handleGameOver", gameDuration);
    }

    void handleGameOver()
    {
        gameOver = true;
    }

    private async void Start()
    {
        _websocket = new WebSocket(serverAddress);
        _websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
        };

        _websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        _websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        _websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");
            string message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log(message);
        };

        // waiting for messages
        await _websocket.Connect();
    }

}
