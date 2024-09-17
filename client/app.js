const serverAddress = document.querySelector("#server-address");
const idInput = document.querySelector("#player-id");
const joinBtn = document.querySelector('#join-btn');

let stateElement = document.querySelector("#state");
let enemyStateElement = document.querySelector("#state-enemy");

let spinBtn = document.querySelector('#spin-btn');
let confirmBtn = document.querySelector('#confirm-btn');
let slotMachineContainer = document.querySelector("#slot-machine");

let deckContainer = document.querySelector('#deck');

let messagesElement = document.querySelector("#messages");

let matchId = "";
let spinResult = [];
let deck = [];

function joinMatch() {
    const playerId = idInput.value;
    const message = {
        type: "searchGame",
        matchId: null,
        playerId: playerId,
        action: [],
        deck: deck
    };
    socket.send(JSON.stringify(message));
    joinBtn.textContent = "Looking for match..."
    joinBtn.disabled = true;
    idInput.disabled = true;
    serverAddress.disabled = true;
}

function requestSpin() {
    const playerId = idInput.value;

    if (!matchId) {
        console.error("No match ID present");
        return;
    }

    const message = {
        type: "requestSpin",
        matchId: matchId,
        playerId: playerId,
        actions: [],
        deck: deck
    };
    socket.send(JSON.stringify(message));
    spinBtn.disabled = true;
}

function sendSpin() {
    const playerId = idInput.value;

    if (!matchId) {
        console.error("No match ID present");
        return;
    }

    const message = {
        type: "sendAction",
        matchId: matchId,
        playerId: playerId,
        actions: spinResult,
        deck: deck
    };
    socket.send(JSON.stringify(message));

    spinResult = [];
    slotMachineContainer.innerHTML = "";
    spinBtn.disabled = false;
}

function saveToDeck(slotIndex) {
    console.log('saving to deck slot: ', spinResult[slotIndex], 'current deck: ', deck);
    deck.push(spinResult[slotIndex]);
    spinResult.splice(slotIndex, 1);
    console.log('new deck: ', deck);
    triggerRender();
}

function sendToSlot(deckIndex) {
    console.log('adding to slot machine: ', deckIndex);
    spinResult.push(deck[deckIndex]);
    deck.splice(deckIndex, 1);
    triggerRender();
}

function insertIntoSlot(slot, deckIndex) {
    console.log(`overwritting slot ${slot} with deck index ${deckIndex}`);
    spinResult[slot] = deck[deckIndex];
    deck.splice(deckIndex, 1);
    triggerRender();
}

function triggerRender() {

    if (spinResult) {
        slotMachineContainer.innerHTML = "";
        spinResult.forEach((slot, index) => {
            slotMachineContainer.innerHTML += `
                <div id="deck-item-${index}">
                    <h4>Slot #${index + 1}</h4>
                    <p>Type: ${slot.type}</p>
                    <p>Value: ${slot.value}</p>
                    <button onclick="saveToDeck(${index})" ${deck.length > 4 ? 'disabled' : ''}>Save to deck</button>
                </div>
            `;
        });
    }

    if (deck) {
        deckContainer.innerHTML = "";
        console.log('spin length', spinResult.length);
        deck.forEach((item, index) => {
            deckContainer.innerHTML += `
                <div id="deck-item-${index}">
                    <p>Type: ${item.type}</p>
                    <p>Value: ${item.value}</p>
                    <button onclick="sendToSlot(${index})" ${spinResult.length > 2 ? 'disabled' : ''}>Send to slot machine</button>
                    <p>Or send to slot number:</p>
                    <button onclick="insertIntoSlot(0, ${index})">#1</button>
                    <button onclick="insertIntoSlot(1, ${index})">#2</button>
                    <button onclick="insertIntoSlot(2, ${index})">#3</button>
                </div>
            `;
        });
    }
}

const socket = new WebSocket(serverAddress.value);

// Event listener for WebSocket connection open
socket.addEventListener("open", () => {
    console.log("Connected to WebSocket server.");
});

socket.addEventListener("close", () => {
    console.log("Disconnected from WebSocket server.");
});

// Event listener for incoming messages
socket.addEventListener("message", (event) => {
    let data = JSON.parse(event.data.toString());
    const playerId = idInput.value;

    if (data.type === 'initialState') {
        matchId = data.matchId;
        joinBtn.textContent = 'Match started!';
    }

    // Log messages
    // Overwrite with new message on top and the rest behind
    messagesElement.innerHTML = `<li>${JSON.stringify(
        data
    )}</li><li> -------- </li>${messagesElement.innerHTML}`;

    // Update state
    if (data.state) {
        let playerKeys = Object.keys(data.state.players);
        playerKeys.forEach((playerIdKey) => {
            if (playerIdKey === playerId) {
                stateElement.innerHTML = `
                    <li>Current score: ${data.state.players[playerIdKey].score}</li>
                    <li>Active shield: ${data.state.players[playerIdKey].shield}</li>
                    <li>Available energy: ${data.state.players[playerIdKey].energy}</li>
                `;
            } else {
                enemyStateElement.innerHTML = `
                    <li>Current score: ${data.state.players[playerIdKey].score}</li>
                `;
            }
        });
    }

    // Update spin data
    if (data.spinResult) {
        spinResult = data.spinResult;
        triggerRender();
    }

    // Populate deck
    if (data.state && data.type === 'spinResult') { // Overwrite deck with server values only when requesting a spin to allow for modification on client before sending actions
        let playerKeys = Object.keys(data.state.players);
        playerKeys.forEach((playerIdKey) => {
            if (playerIdKey === playerId) {
                deck = data.state.players[playerId].deck;
                console.log('Setting deck with server values', deck);
            }
        });
    }

    // Handle game over
    if (data.state?.isGameOver && data.state.winner) {
        // Let some time to update UI to latest state
        setTimeout(() => {
            window.alert(
                data.state.winner === playerId ? `You win!!` : `You lost :( XD)`
            );
            window.location.reload();
        }, 1000);
    }

    console.log(data);
});