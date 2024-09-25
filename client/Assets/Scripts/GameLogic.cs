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
    [SerializeField] private float gameDuration = 15;

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
}
