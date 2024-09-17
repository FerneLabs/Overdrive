const idInput = document.getElementById("player-id");

let stateElement = document.querySelector("#state");
let enemyStateElement = document.querySelector("#state-enemy");

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
}

function saveToDeck(slotIndex) {
    console.log(slotIndex, spinResult[slotIndex], deck);
    deck.push(spinResult[slotIndex]);
    spinResult.splice(slotIndex, 1);
    triggerRender();
}

function insertIntoSlot(slot, deckIndex) {
    console.log(slot, deckIndex);
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
                    <h4>Slot ${index + 1}</h4>
                    <p>Type: ${slot.type}</p>
                    <p>Value: ${slot.value}</p>
                    <button onclick="saveToDeck(${index})">Save to deck</button>
                </div>
            `;
        });
    }

    if (deck) {
        deckContainer.innerHTML = "";
        deck.forEach((item, index) => {
            deckContainer.innerHTML += `
                <div id="deck-item-${index}">
                    <p>Type: ${item.type}</p>
                    <p>Value: ${item.value}</p>
                    <p>Insert into:</p>
                    <button onclick="insertIntoSlot(0, ${index})">Slot 1</button>
                    <button onclick="insertIntoSlot(1, ${index})">Slot 2</button>
                    <button onclick="insertIntoSlot(2, ${index})">Slot 3</button>
                </div>
            `;
        });
    }
}

const socket = new WebSocket("ws://localhost:8080");

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
    if (data.matchId) {
        matchId = data.matchId;
    }
    const playerId = idInput.value;

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
                                <li>${JSON.stringify(
                                    data.state?.players[playerIdKey]
                                )}</li> 
                            `;
            } else {
                enemyStateElement.innerHTML = `
                                <li>${JSON.stringify(
                                    data.state?.players[playerIdKey]
                                )}</li> 
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
    if (data.state.isGameOver && data.state.winner) {
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