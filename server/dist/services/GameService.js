"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.GameService = void 0;
const StateReplayer_1 = require("../entities/StateReplayer");
class GameService {
    constructor(eventStore) {
        this.eventStore = eventStore;
    }
    getMatchState(matchId) {
        const events = this.eventStore.getEvents(matchId);
        return (0, StateReplayer_1.buildState)(events);
    }
}
exports.GameService = GameService;
