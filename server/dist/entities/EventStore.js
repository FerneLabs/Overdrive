"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.EventStore = void 0;
class EventStore {
    constructor() {
        this.events = [];
    }
    saveEvent(event) {
        this.events.push(event);
    }
    getEvents(matchId) {
        return this.events.filter(event => event.matchId === matchId);
    }
}
exports.EventStore = EventStore;
