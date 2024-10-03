using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using NativeWebSocket;
using Newtonsoft.Json;
using UnityEngine.Rendering.UI;
using UnityEngine.Playables;

using System.Threading.Tasks;
using UnityEngine.Windows;
using UnityEngine.UI;
using Unity.Burst.Intrinsics;
using UnityEngine.SceneManagement;
using System.Linq;

public struct SpinItem
{
    public SpinItem(string type, int value)
    {
        this.type = type;
        this.value = value;
    }

    public string type;
    public int value;

    public override string ToString() => $"type: {type}; value: {value};";
}

public struct ClientMessage 
{
    public ClientMessage(string type, string matchId, string playerId, List<SpinItem> actions, List<SpinItem> deck)
    {
        this.type = type;
        this.matchId = matchId;
        this.playerId = playerId;
        this.actions = actions;
        this.deck = deck;
    }

    public string type;
    public string matchId;
    public string playerId;
    public List<SpinItem> actions;
    public List<SpinItem> deck;
}

public struct PlayerState
{
    public PlayerState(int score, int shield, int energy, List<SpinItem> deck)
    {
        this.score = score;
        this.shield = shield;
        this.energy = energy;
        this.deck = deck;
    }

    public int score;
    public int shield;
    public int energy;
    public List<SpinItem> deck;

    public override string ToString() => $"score: {score}; shield: {shield}; energy: {energy}; deck: {deck}";
}

public struct GameState
{
    #nullable enable
    public Dictionary<string, PlayerState> players;
    public bool isGameOver;
    public string? winner;
    
    public GameState(Dictionary<string, PlayerState> players, bool isGameOver, string? winner)
    {
        this.players = players;
        this.isGameOver = isGameOver;
        this.winner = winner;
    }
    #nullable disable

    public override string ToString() => $"players: {players}; isGameOver: {isGameOver}; winner: {winner}";
}

public struct ServerMessage
{
    public ServerMessage(string type, string matchId, GameState state, List<SpinItem> spinResult, string error)
    {
        this.type = type;
        this.matchId = matchId;
        this.state = state;
        this.spinResult = spinResult;
        this.error = error;
    }

    public string type;
    public string matchId;
    public GameState state;
    public List<SpinItem> spinResult;
    public string error;
}

public class GameLogic : MonoBehaviour
{
    public bool gameStarted = false;
    public bool gameOver = false;
    public int winner;
    public string playerId;
    public string adversaryPlayerId;
    public string matchId;
    public GameState gameState;

    [SerializeField] private int fetchCost = 4;
    [SerializeField] private GameObject connectionScreen;
    [SerializeField] private GameObject gameplayOverlay;
    [SerializeField] private GameObject gameoverScreen;
    [SerializeField] private TMP_Text[] gameStateCounters;
    [SerializeField] private Button fetchCiphersButton;
    [SerializeField] private Button runModuleButton;
    [SerializeField] private GameObject fetchContainer;
    [SerializeField] private GameObject moduleContainer;
    [SerializeField] private TMP_Text moduleStatText;
    [SerializeField] private RawImage moduleStatIcon;
    [SerializeField] private TMP_Text moduleStatComboIndicator;
    [SerializeField] private Texture2D[] typeIcons;
    [SerializeField] private GameObject deckContainer;
    [SerializeField] private GameObject cipherPrefab;

    [SerializeField] private TMP_Text overlayCurrentPlayerText;
    [SerializeField] private TMP_Text overlayAdversaryPlayerText;

    [SerializeField] private TMP_Text raceResultText;
    [SerializeField] private TMP_Text currentPlayerText;
    [SerializeField] private TMP_Text adversaryPlayerText;
    [SerializeField] private TMP_Text currentPlayerScore;
    [SerializeField] private TMP_Text adversaryPlayerScore;

    private WebSocket _websocket;
    private List<string> _serverAddresses = new List<string>();
    private List<SpinItem> _currentSpinResult = new List<SpinItem>();
    private List<SpinItem> _currentModule = new List<SpinItem>();
    private List<SpinItem> _currentDeck = new List<SpinItem>();

    private GameObject _playerIdInput;
    private GameObject _serverChoiceDropdown;
    private TMP_Text _statusText;

    private void Start()
    {
        _serverAddresses.Add("wss://overdrive-api.joeper.myds.me");
        _serverAddresses.Add("ws://localhost:8080");

        _playerIdInput = GameObject.FindGameObjectWithTag("InputID");
        _serverChoiceDropdown = GameObject.FindGameObjectWithTag("ServerChoice");
        _statusText = GameObject.FindGameObjectWithTag("ConnectionStatus").GetComponent<TMP_Text>();
    }

    private void Update()
    {
        // Manually process server messages in the editor.
        // In WebGL build this is done automatically.
        if (_websocket == null) { return; }
        #if !UNITY_WEBGL || UNITY_EDITOR
                _websocket.DispatchMessageQueue();
        #endif

        if (gameStarted)
        {
            UpdateCounters(gameState);

            fetchCiphersButton.enabled = gameState.players[playerId].energy < fetchCost ? false : true;
            runModuleButton.enabled = moduleContainer.transform.childCount > 0;
        }

    }

    public void StartGame()
    {   
        playerId = _playerIdInput.GetComponent<TMP_InputField>().text;
        int serverAddressChoice = _serverChoiceDropdown.GetComponent<TMP_Dropdown>().value;

        playerId += $"-{UnityEngine.Random.Range(10000000, 100000000)}"; // Append random 8 digits int to username
        ConnectToServer(_serverAddresses[serverAddressChoice]);
    }

    private async void ConnectToServer(string serverAddress)
    {
        _websocket = new WebSocket(serverAddress);
        _websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            JoinMatch();
        };

        _websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
            _statusText.text = e;
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

            HandleMessage(message);
        };

       // waiting for messages
       await _websocket.Connect();
    }

    private async void OnApplicationQuit()
    {
        await _websocket.Close();
    }

    private void HandleMessage(string message)
    {
        ServerMessage serverMessage = JsonConvert.DeserializeObject<ServerMessage>(message);

        switch (serverMessage.type)
        {
            case "waitingForPlayers":
                _statusText.text = "Looking for match...";
                break;

            case "initialState":
                matchId = serverMessage.matchId;
                gameState = serverMessage.state;

                StartCoroutine(LoadGame());
                break;

            case "spinResult":
                _currentSpinResult = serverMessage.spinResult;
                gameState = serverMessage.state;
                RenderSpin();
                break;

            case "updateGameState":
                gameState = serverMessage.state;
                break;

            case "gameOver":
                gameState = serverMessage.state;
                gameOver = true;
                StartCoroutine(HandleGameOver());
                break;

            default:
                Debug.LogError(serverMessage.error);
                break;
        }
    }

    private async void JoinMatch()
    {
        ClientMessage message = new ClientMessage(
            "searchGame", 
            "", 
            playerId, 
            new List<SpinItem>(), 
            new List<SpinItem>()
        );

        string messageJSON = JsonConvert.SerializeObject(message);
        await _websocket.SendText(messageJSON);
        Debug.Log($"Message sent: {messageJSON}");
    }

    private IEnumerator LoadGame()
    {
        Debug.Log("Loading Game!");
        _statusText.text = "Game found!";

        foreach (var player in gameState.players)
        {
            if (player.Key != playerId) { adversaryPlayerId = player.Key; }
        }

        overlayCurrentPlayerText.text = playerId;
        overlayAdversaryPlayerText.text = adversaryPlayerId;

        yield return new WaitForSeconds(1);

        gameStarted = true;

        connectionScreen.SetActive(false);
        gameplayOverlay.SetActive(true);
    }

    public async void FetchCiphers()
    {
        ClientMessage message = new ClientMessage(
            "requestSpin",
            matchId,
            playerId,
            new List<SpinItem>(),
            new List<SpinItem>()
        );

        string messageJSON = JsonConvert.SerializeObject(message);
        await _websocket.SendText(messageJSON);
        Debug.Log($"Message sent: {messageJSON}");
    }

    public async void RunModule()
    {   
        ClientMessage message = new ClientMessage(
            "sendAction",
            matchId,
            playerId,
            _currentModule,
            _currentDeck
        );

        string messageJSON = JsonConvert.SerializeObject(message);
        await _websocket.SendText(messageJSON);
        Debug.Log($"Message sent: {messageJSON}");

        // fetchCiphersButton.gameObject.SetActive(true);
        // runModuleButton.gameObject.SetActive(false);

        foreach (Transform childTransform in moduleContainer.transform) // Clear the current ciphers in module and module stats
        {
            Destroy(childTransform.gameObject);
        }
        moduleStatText.text = "0";
        moduleStatIcon.gameObject.SetActive(false);
        moduleStatComboIndicator.gameObject.SetActive(false);
    }

    public void SyncContainers() // This will be called in every OnEndDrag event to keep the private fields updated with the UI content
    {
        Debug.Log("[SyncContainers] Running sync");
        Dictionary<string, List<int>> cardTypesCount = new()
        {
            ["advance"] = new List<int>(),
            ["attack"] = new List<int>(),
            ["defend"] = new List<int>(),
            ["energize"] = new List<int>()
        };

        _currentSpinResult.Clear();
        foreach (Transform cipher in fetchContainer.transform)
        {
            Card cardScript = cipher.GetComponent<Card>();
            SpinItem item = new SpinItem(cardScript.cardType, cardScript.cardValue);
            _currentSpinResult.Add(item);
        }

        _currentModule.Clear();
        foreach (Transform cipher in moduleContainer.transform)
        {
            Card cardScript = cipher.GetComponent<Card>();
            SpinItem item = new SpinItem(cardScript.cardType, cardScript.cardValue);
            _currentModule.Add(item);

            cardTypesCount[cardScript.cardType].Add(cardScript.cardValue);
            Debug.Log($"[SyncContainers] Found type {cardScript.cardType}");
        }

        SetModuleStats(cardTypesCount);

        _currentDeck.Clear();
        foreach (Transform cipher in deckContainer.transform)
        {
            Card cardScript = cipher.GetComponent<Card>();
            SpinItem item = new SpinItem(cardScript.cardType, cardScript.cardValue);
            _currentDeck.Add(item);
        }
    }

    private void SetModuleStats(Dictionary<string, List<int>> cardTypeGroups) 
    {
        Debug.Log("[SetModuleStats] Running set");

        (string, int) typeValueTuple = ("", 0);

        foreach (var type in cardTypeGroups)
        {
            if (type.Value.Count >= 2)
            {
                // Sum the card values for the group
                int groupSum = type.Value.Sum(value => value);
                
                typeValueTuple.Item1 = type.Key;
                typeValueTuple.Item2 += groupSum;
            }
        }

        if (typeValueTuple.Item1 == "") 
        {
            moduleStatIcon.gameObject.SetActive(false);
        }
        else {
            // Debug.Log($"[SetModuleStats] Tuple: {typeValueTuple.Item1} ({GetTypeIndex(typeValueTuple.Item1)}) | {typeValueTuple.Item2}");
            moduleStatIcon.texture = typeIcons[GetTypeIndex(typeValueTuple.Item1)];
            moduleStatIcon.gameObject.SetActive(true);

            if (cardTypeGroups[typeValueTuple.Item1].Count == 3) 
            {
                moduleStatComboIndicator.gameObject.SetActive(true);
            } 
            else 
            {
                moduleStatComboIndicator.gameObject.SetActive(false);
            }
        }

        moduleStatText.text = $"{typeValueTuple.Item2}";
    }

    public int GetTypeIndex(string cardType) 
    {
        int typeIndex;
        switch (cardType)
        {
            case "advance":
                typeIndex = 0;
                break;
            case "attack":
                typeIndex = 1;
                break;
            case "defend":
                typeIndex = 2;
                break;
            case "energize":
                typeIndex = 3;
                break;
            default:
                Debug.LogError($"Unknown card type received! == {cardType}");
                typeIndex = -1;
                break;
        }
        return typeIndex;
    }

    private void UpdateCounters(GameState gameState)
    {
        // Check if players is null or if the player doesn't exist
        if (gameState.players == null || !gameState.players.ContainsKey(playerId))
        {
            Debug.LogError($"Player with ID {playerId} not found or 'players' dictionary is null");
            return;
        }

        PlayerState? playerState = gameState.players[playerId];

        // Make sure playerState is not null
        if (playerState == null)
        {
            Debug.LogError($"PlayerState for playerId {playerId} is null");
            return;
        }

        gameStateCounters[0].text = $"{gameState.players[playerId].score}"; // Control panel
        gameStateCounters[1].text = $"{gameState.players[playerId].score}"; // Car overlay
        gameStateCounters[2].text = $"{gameState.players[playerId].shield}"; // Control panel
        gameStateCounters[3].text = $"{gameState.players[playerId].energy}"; // Control panel
        gameStateCounters[4].text = $"{gameState.players[adversaryPlayerId].score}"; // Car overlay
    }

    private void RenderSpin()
    {
        if (_currentSpinResult.Count > 0)
        {
            foreach (Transform childTransform in fetchContainer.transform) // Clear the current ciphers in fetch
            {
                Destroy(childTransform.gameObject);
            }

            _currentSpinResult.ForEach(item =>
            {
                Debug.Log(item.type.ToLower());
                GameObject instance = Instantiate(cipherPrefab, fetchContainer.transform);
                Card instanceScript = instance.GetComponent<Card>();
                instanceScript.cardType = item.type.ToLower();
                instanceScript.cardValue = item.value;
            });
            // fetchCiphersButton.gameObject.SetActive(false);
            // runModuleButton.gameObject.SetActive(true);
        }
    }

    private IEnumerator HandleGameOver()
    {
        bool currentPlayerWon = gameState.winner == playerId;
        winner = currentPlayerWon ? 0 : 1;
        Debug.Log($"Winner is: {gameState.winner}");

        yield return new WaitForSeconds(4);

        raceResultText.text = currentPlayerWon ? "You are showing no mercy!" : "Someone was faster than you...";
        currentPlayerText.text = playerId;
        adversaryPlayerText.text = adversaryPlayerId;
        currentPlayerScore.text = $"{gameState.players[playerId].score}";
        adversaryPlayerScore.text = $"{gameState.players[adversaryPlayerId].score}";
        gameplayOverlay.SetActive(false);
        gameoverScreen.SetActive(true);
    }

    public void PlayAgain()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentSceneName);
    }
}
