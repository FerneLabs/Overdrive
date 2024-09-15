import { WebSocket } from "ws";
import { v4 as uuidv4 } from "uuid";
import { GameService } from "./services/GameService";
import { EventStore } from "./entities/EventStore";
import { GameCommandHandler } from "./entities/CommandHandler";

interface WaitListPlayer {
    playerId: string;
    ws: WebSocket;
}

interface MatchRooms {
    [key: string]: Set<WebSocket>
}

interface ClientMessage {
    type: "searchGame" | "requestSpin" | "sendAction";
    matchId: string | null;
    playerId: string;
    action: "Advance" | "Attack" | "Defend" | "Energize" | null;
}

interface SpinItem {
    type: string,
    value: number
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
    let currentPlayerId: string;

    // Event listener for incoming messages
    ws.on("message", (message) => {
        const data: ClientMessage = JSON.parse(message.toString());
        currentPlayerId = data.playerId;

        switch (data.type) {
            case "searchGame":
                // Check if players in matchmaking
                if (lookingForGame.length > 0) {
                    const matchId = uuidv4();
					currentMatchId = matchId;

                    const players: WaitListPlayer[] = [
                        { playerId: data.playerId, ws: ws },
                        lookingForGame.shift()!,
                    ];
                    console.log(`[LOG] [SERVER] [LookingForMatch}] [${players[1].playerId}] :: ${new Date().toISOString()} = Removed player from wait list`);

                    if (players[0].playerId === players[1].playerId) return; // Avoid starting a game with one player

					// Send Join action to initialize state of both users
					commandHandler.playerJoin(matchId, players[0].playerId);
					commandHandler.playerJoin(matchId, players[1].playerId);

					// Create room and send data to client
                    handleJoinGame(matchId, players[0].ws);
					handleJoinGame(matchId, players[1].ws);
                } else {
                    // If no player waiting, add current player to waitlist
                    lookingForGame.push({ playerId: data.playerId, ws: ws });
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

            // case "sendAction":
            //     handleSendAction(ws, data.matchId, data.playerId, data.action);
            //     break;

            default:
                ws.send(JSON.stringify({ error: "Unknown message type" }));
        }
    });

	// Remove ws from match room if the client disconnects
    ws.on('close', () => {
		if (currentMatchId && matchRooms[currentMatchId]) {
		  matchRooms[currentMatchId].delete(ws);
          console.log(`[LOG] [SERVER] [MatchRoom}] [${currentPlayerId}] :: ${new Date().toISOString()} = Removed player match room`);
		}

        // Handle connection closed while waiting for match
        if (currentPlayerId) {
            const playerIndex = lookingForGame.findIndex(player => player.playerId === currentPlayerId);
            if (playerIndex) {
                lookingForGame.splice(playerIndex);
                console.log(`[LOG] [SERVER] [LookingForMatch}] [${currentPlayerId}] :: ${new Date().toISOString()} = Removed player from wait list`);
            }
        }
	});
});

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

function handleRequestSpin(ws: WebSocket, matchId: string, playerId: string) {
    let gameState = gameService.getMatchState(matchId);
    const playerStatus = gameState.players[playerId];
    
    if (playerStatus.energy < 2) {
        ws.send(JSON.stringify({ error: "Not enough energy for spin" }));
        return;
    }

    const spinResult: SpinItem[] = [];

    for (let i = 0; i < 3; i++) {
        const randType = Math.floor(Math.random() * 9); // Generate 0 to 9
        const randValue = Math.floor(Math.random() * 4) + 1; // Generate 1 to 5
        const result = spinChance.find(item => item.chance[0] <= randType && randType < item.chance[1]); // Find the element that surrounds the randType number 
        if (result) {
            spinResult.push({ type: result.type, value: randValue })
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

function handleSendAction(ws: WebSocket, matchId: string, playerId: string) {}
