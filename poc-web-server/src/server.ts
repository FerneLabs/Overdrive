import { WebSocket } from "ws";
import { v4 as uuidv4 } from "uuid";
import { GameService } from "./services/GameService";
import { EventStore } from "./entities/EventStore";
import { GameCommandHandler } from "./entities/CommandHandler";
import { REGEN_TIMER, SPIN_COST } from "./entities/Globals";

interface WaitListPlayer {
    playerId: string;
    ws: WebSocket;
    matchId: string
}

interface MatchRooms {
    [key: string]: Set<WebSocket>
}

interface ClientMessage {
    type: "searchGame" | "requestSpin" | "sendAction";
    matchId: string | null;
    playerId: string;
    actions: SpinItem[];
    deck: SpinItem[];
}

interface SpinItem {
    type: string,
    value: number,
    valid?: boolean
}

const spinChance = [
    { type: 'Advance',  chance: [0, 4]   },
    { type: 'Attack',   chance: [4, 6]   },
    { type: 'Defend',   chance: [6, 7.5] },
    { type: 'Energize', chance: [7.5, 9] }
]

const eventStore: EventStore = new EventStore();
const commandHandler = new GameCommandHandler(eventStore);
const gameService = new GameService(eventStore);

const lookingForGame: WaitListPlayer[] = [];
const matchRooms: MatchRooms = {};

// Initialize WebSocket server
const wss = new WebSocket.Server({ port: 8080 });
console.log('running on port 8080');

// WebSocket event handling
wss.on("connection", (ws) => {
    let currentMatchId: string | null = null;
    let currentPlayerId: string = '';
    let currentRegenInterval: NodeJS.Timeout;

    // Event listener for incoming messages
    ws.on("message", (message) => {
        const data: ClientMessage = JSON.parse(message.toString());
        currentPlayerId = data.playerId;

        if (currentMatchId) { // Don't handle any more actions if game is over.
            const gameState = gameService.getMatchState(currentMatchId);
            const gameEvents = eventStore.getEvents(currentMatchId);
            const gameOverHandled = gameEvents.find(event => event.type === 'GameOver');
            if (gameState.isGameOver && !gameOverHandled) { // Avoid logging multiple GameOver events.
                handleGameOver(currentMatchId);
                return;
            }
        }

        switch (data.type) {
            case "searchGame":
                // Check if players in matchmaking
                if (lookingForGame.length > 0) {
                    const adversaryPlayer = lookingForGame.shift()!;
                    currentMatchId = adversaryPlayer.matchId;

                    const players: WaitListPlayer[] = [
                        { playerId: data.playerId, ws: ws, matchId: currentMatchId },
                        adversaryPlayer
                    ];
                    console.log(`[LOG] [SERVER] [LookingForMatch}] [${players[1].playerId}] :: ${new Date().toISOString()} = Removed player from wait list`);

                    if (players[0].playerId === players[1].playerId) return; // Avoid starting a game with one player

					// Send Join action to initialize state of both users
					commandHandler.playerJoin(currentMatchId, players[0].playerId);
					commandHandler.playerJoin(currentMatchId, players[1].playerId);

					// Create room and send data to client
                    handleJoinGame(currentMatchId, players[0].ws);
					handleJoinGame(currentMatchId, players[1].ws);

                    // Create interval for energy regen
                    currentRegenInterval = initEnergyRegen(currentMatchId, [players[0].playerId, players[1].playerId]);
                } else {
                    // If no player waiting, add current player to waitlist
                    const matchId = uuidv4();
					currentMatchId = matchId;
                    lookingForGame.push({ playerId: data.playerId, ws: ws, matchId: currentMatchId});
					ws.send(JSON.stringify({ type: "waitingForPlayers" }));

                    console.log(`[LOG] [SERVER] [LookingForMatch}] [${data.playerId}] :: ${new Date().toISOString()} = Added player to wait list`);
                }
                break;

            case "requestSpin":
                if (!data.matchId) {
                    ws.send(JSON.stringify({ error: "matchId missing in message!" }));
                    return;
                }
                handleRequestSpin(ws, data.matchId, data.playerId);
                
                break;

            case "sendAction":
                if (!data.matchId || !data.actions) {
                    ws.send(JSON.stringify({ error: "matchID or action missing in message!" }));
                    return;
                }
                handleSendAction(data.matchId, data.playerId, data.actions, data.deck);
                break;

            default:
                ws.send(JSON.stringify({ error: "Unknown message type" }));
        }
    });

	// Remove ws from match room / waiting list if the client disconnects
    ws.on('close', () => {
        console.log(`[LOG] [SERVER] [ON_CONN_CLOSE}] [${currentPlayerId}] :: ${new Date().toISOString()}`);
		if (currentMatchId && matchRooms[currentMatchId]) {
            let gameState = gameService.getMatchState(currentMatchId);

            if (gameState.isGameOver) {
                handleGameOver(currentMatchId);
            } else { // Disconnection during game
                broadcastMessage(currentMatchId, {
                    type: 'playerDisconnection',
                    matchId: currentMatchId,
                    playerId: currentPlayerId
                });
                
                handleGameOver(currentMatchId, currentPlayerId);
                console.log(`[LOG] [SERVER] [MatchRoom}] [${currentMatchId}] :: ${new Date().toISOString()} = ${currentPlayerId} disconnected. Removed player from match room`);
            }
		}

        // Handle connection closed while waiting for match
        if (currentPlayerId) {
            const playerIndex = lookingForGame.findIndex(player => player.playerId === currentPlayerId);
            if (playerIndex) {
                lookingForGame.splice(playerIndex);
                console.log(`[LOG] [SERVER] [LookingForMatch}] [${currentPlayerId}] :: ${new Date().toISOString()} = Removed player from wait list`);
            }
        }

        if (currentRegenInterval) {
            clearInterval(currentRegenInterval);
        }
	});
});

function initEnergyRegen(matchId: string, playersId: string[]) {
    const regenInterval = setInterval(() => {
        let regenTriggered = false;
        let gameState = gameService.getMatchState(matchId);

        playersId.forEach(playerId => {
            if (gameState.players[playerId].energy < 10) { // Only trigger regen if energy is below 10
                commandHandler.playerEnergize(matchId, playerId, 1, false);
                regenTriggered = true;
            }
        });

        if (regenTriggered) {
            gameState = gameService.getMatchState(matchId); // Update state before sending
            broadcastMessage(matchId, {
                type: 'updateGameState',
                matchId: matchId,
                state: gameState
            });
        }
    }, REGEN_TIMER);
    return regenInterval;
}

function broadcastMessage(matchId: string, message: {}) {
    const matchRoom = matchRooms[matchId];
    for (const clientWs of matchRoom) {
        if (clientWs.readyState === WebSocket.OPEN) {
            clientWs.send(
                JSON.stringify(message)
            );
        }
    }
}

// Handle joining a game
function handleJoinGame(matchId: string, ws: WebSocket) {
    const gameState = gameService.getMatchState(matchId);
    
    if (!matchRooms[matchId]) { // Create room if it doesn't exist
        matchRooms[matchId] = new Set();
    }

    matchRooms[matchId].add(ws);

    ws.send(
        JSON.stringify({
            type: "initialState",
			matchId: matchId,
            state: gameState
        })
    );
}

function handleGameOver(matchId: string, disconnected?: string) {
    let gameState = gameService.getMatchState(matchId);
    // Avoid logging multiple game over events
    const gameEvents = eventStore.getEvents(matchId);
    const gameOverHandled = gameEvents.find(event => event.type === 'GameOver');
    if (gameOverHandled) return

    let winnerPlayer = { playerId: '', score: 0 };
    let loserPlayer = { playerId: '', score: 0 };

    const playerIds = Object.keys(gameState.players);

    if (disconnected) { // If a player disconnected, the win goes to the other player no matter the score.
        loserPlayer = { playerId: disconnected, score: gameState.players[disconnected].score };
        
        const winnerId = playerIds.find(id => id != disconnected);
        if (winnerId) {
            winnerPlayer = { playerId: winnerId, score: gameState.players[winnerId].score };
        }

        commandHandler.gameOver(matchId, true, winnerPlayer, loserPlayer);
    }

    // There should never be a draw, as the first to reach 100 should be marked as gameOver, and the server would not accept any further messages.
    if (gameState.players[playerIds[0]].score > gameState.players[playerIds[1]].score && !disconnected) {
        winnerPlayer.playerId = playerIds[0];
        winnerPlayer.score = gameState.players[playerIds[0]].score;
        loserPlayer.playerId = playerIds[1];
        loserPlayer.score = gameState.players[playerIds[1]].score;

        commandHandler.gameOver(matchId, false, winnerPlayer, loserPlayer);
    } else if (!disconnected){
        winnerPlayer.playerId = playerIds[1];
        winnerPlayer.score = gameState.players[playerIds[1]].score;
        loserPlayer.playerId = playerIds[0];
        loserPlayer.score = gameState.players[playerIds[0]].score;

        commandHandler.gameOver(matchId, false, winnerPlayer, loserPlayer);
    }

    gameState = gameService.getMatchState(matchId); // Update state to include Game Over
    broadcastMessage(matchId, {
        type: 'gameOver',
        state: gameState
    });

    // Trigger disconnection of clients to handle all disconnection logic.
    // If client disconnected during match, this is already handled and the room
    matchRooms[matchId].forEach(client => { 
        client.close();
    });
    
    setTimeout(() => {
        delete matchRooms[matchId];
    }, 5000); // Wait for broadcast to be done before deleting the room.
}

function handleRequestSpin(ws: WebSocket, matchId: string, playerId: string) {
    let gameState = gameService.getMatchState(matchId);
    const playerStatus = gameState.players[playerId];
    
    if (!playerStatus) {
        ws.send(JSON.stringify({ error: `Server error. Player status does not exist. Player ID: ${playerId}` }));
        console.log(`[ERROR] [SERVER] [handleRequestSpin}] [${playerId}] :: ${new Date().toISOString()} = Player status does not exist. Handling gracefully.`);
        return;
    }

    if (playerStatus.energy < SPIN_COST) {
        ws.send(JSON.stringify({ error: "Not enough energy for spin" }));
        return;
    }

    const spinResult: SpinItem[] = [];

    for (let i = 0; i < 3; i++) {
        const randType = Math.floor(Math.random() * 9); // Generate 0 to 9
        const randValue = Math.floor(Math.random() * 4) + 1; // Generate 1 to 5
        const randValueAdvance = Math.floor( Math. random() * (10 - 5 + 1) + 5 ); // Generate 5 to 10
        const result = spinChance.find(item => item.chance[0] <= randType && randType < item.chance[1]); // Find the element that surrounds the randType number 
        if (result) {
            let value = 0; 
            if (result.type === 'Advance') {
                value = randValueAdvance;
            } else {
                value = randValue;
            }
            spinResult.push({ type: result.type, value: value })
        } else {
            console.error('Spin result returned undefined');
        }
    }

    if (spinResult.length === 3) {
        commandHandler.playerSpin(matchId, playerId, spinResult); // Send spin Event to update state
        gameState = gameService.getMatchState(matchId); // Retrieve latest update to send to client

        ws.send(
            JSON.stringify({
                type: "spinResult",
                matchId: matchId,
                state: gameState,
                spinResult: spinResult
            })
        );
    }
}

function handleSendAction(matchId: string, playerId: string, actions: SpinItem[], deck: SpinItem[]) {
    let gameState = gameService.getMatchState(matchId);
    const adversaryPlayerId = Object.keys(gameState.players).find(player => player != playerId);
    let isCombo = false;
    // Check for max combo
    if (actions.length === 3 
        && actions[0].type === actions[1].type 
        && actions[0].type === actions[2].type) {
        isCombo = true;
    }

    // Check if at least there are two equal types
    if (actions[0] && actions[1] 
        && actions[0].type === actions[1].type) {
        actions[0].valid = true;
        actions[1].valid = true;
    }
    if (actions[0] && actions[2] 
        && actions[0].type === actions[2].type) {
        actions[0].valid = true;
        actions[2].valid = true;
    }
    if (actions[1] && actions[2] 
        && actions[1].type === actions[2].type) {
        actions[1].valid = true;
        actions[2].valid = true;
    }

    actions.forEach(action => {
        if (action && action.valid) {
            let actionValue = action.value;
            if (isCombo) actionValue *= 2 // Multiply by 2 if is combo

            switch(action.type.toLowerCase()) {
                case 'advance':
                    commandHandler.playerAdvance(matchId, playerId, actionValue, 0, isCombo);
                    break;
                case 'attack':
                    if (adversaryPlayerId)
                        commandHandler.playerAttack(matchId, playerId, adversaryPlayerId, actionValue, 0, isCombo);
                    break;
                case 'defend':
                    commandHandler.playerDefend(matchId, playerId, actionValue, 0, isCombo);
                    break;
                case 'energize':
                    commandHandler.playerEnergize(matchId, playerId, actionValue, isCombo);
                    break;
                default:
                    console.log(`[LOG] [SERVER] [HandleAction}] [${playerId}] :: ${new Date().toISOString()} = Action type not recognized: ${action.type} | ${action.type.toLowerCase()}`);
                    break;
            }
        }
    });

    commandHandler.updateDeck(matchId, playerId, deck);
    
    gameState = gameService.getMatchState(matchId);

    // Check if this action caused the game to finish
    if (gameState.isGameOver) {
        handleGameOver(matchId);
        gameState = gameService.getMatchState(matchId); // Update state to include Game Over to client message
    }

    broadcastMessage(matchId, {
        type: 'updateGameState',
        matchId: matchId,
        state: gameState
    });
}

