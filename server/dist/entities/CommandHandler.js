"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.GameCommandHandler = void 0;
class GameCommandHandler {
    constructor(eventStore) {
        this.eventStore = eventStore;
    }
    logEvent(event) {
        switch (event.type) {
            case 'GameOver':
                console.log(`[LOG] [EVENT] [${event.matchId}] :: ${new Date(event.timestamp).toISOString()} = ${event.type} | Interrumpted = ${event.matchInterrumpted} | Winner [${event.winnerPlayer.playerId}] | Final score [${event.winnerPlayer.score} - ${event.loserPlayer.score}]`);
                break;
            default:
                console.log(`[LOG] [EVENT] [${event.matchId}] [${event.playerId}] :: ${new Date(event.timestamp).toISOString()} = ${event.type}`);
                break;
        }
    }
    playerJoin(matchId, playerId) {
        const event = {
            type: 'Join',
            timestamp: Date.now(),
            matchId,
            playerId
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    gameOver(matchId, matchInterrumpted, winnerPlayer, loserPlayer) {
        const event = {
            type: 'GameOver',
            timestamp: Date.now(),
            matchId,
            matchInterrumpted,
            winnerPlayer,
            loserPlayer
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    updateDeck(matchId, playerId, deck) {
        const event = {
            type: 'UpdateDeck',
            timestamp: Date.now(),
            matchId,
            playerId,
            deck
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    playerSpin(matchId, playerId, slots) {
        const event = {
            type: 'Spin',
            timestamp: Date.now(),
            matchId,
            playerId,
            slots
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    playerAdvance(matchId, playerId, increment, cost, isCombo) {
        const event = {
            type: 'Advance',
            timestamp: Date.now(),
            matchId,
            playerId,
            increment,
            cost,
            isCombo
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    playerAttack(matchId, playerId, targetId, damage, cost, isCombo) {
        const event = {
            type: 'Attack',
            timestamp: Date.now(),
            matchId,
            playerId,
            targetId,
            damage,
            cost,
            isCombo
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    playerDefend(matchId, playerId, increment, cost, isCombo) {
        const event = {
            type: 'Defend',
            timestamp: Date.now(),
            matchId,
            playerId,
            increment,
            cost,
            isCombo
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
    playerEnergize(matchId, playerId, increment, isCombo) {
        const event = {
            type: 'Energize',
            timestamp: Date.now(),
            matchId,
            playerId,
            increment,
            isCombo
        };
        this.eventStore.saveEvent(event);
        this.logEvent(event);
    }
}
exports.GameCommandHandler = GameCommandHandler;
