import {
  GameEvent,
  AdvanceEvent,
  AttackEvent,
  DefendEvent,
  EnergizeEvent,
  JoinEvent,
  SpinEvent,
  GameOverEvent,
  UpdateDeckEvent,
} from "./GameEvent";
import { SPIN_COST } from "./Globals";

import { SpinItem } from "./GameEvent";

interface PlayerState {
  score: number;
  shield: number;
  energy: number;
  deck: SpinItem[];
}

export interface GameState {
  players: {
    [playerId: string]: PlayerState;
  };
  isGameOver: boolean;
  winner: string | null;
}

function initialState(): PlayerState {
  return {
    score: 0,
    shield: 0,
    energy: 6,
    deck: [],
  };
}

export function buildState(events: GameEvent[]): GameState {
  const state: GameState = { players: {}, isGameOver: false, winner: null };

  for (const event of events) {
    switch (event.type) {
      case "Join": {
        const joinEvent = event as JoinEvent;
        state.players[joinEvent.playerId] = initialState();
        break;
      }

      case "GameOver": {
        const gameOverEvent = event as GameOverEvent;
        state.isGameOver = true;
        state.winner = gameOverEvent.winnerPlayer.playerId;
        break;
      }

      case "UpdateDeck": {
        const updateDeckEvent = event as UpdateDeckEvent;
        let emitter = state.players[updateDeckEvent.playerId];

        emitter.deck = updateDeckEvent.deck;

        break;
      }

      case "Spin": {
        const spinEvent = event as SpinEvent;
        let emitter = state.players[spinEvent.playerId];

        emitter.energy -= SPIN_COST;
        
        break;
      }

      case "Advance": {
        const advanceEvent = event as AdvanceEvent;
        let emitter = state.players[advanceEvent.playerId];

        emitter.score += advanceEvent.increment;
        emitter.energy -= advanceEvent.cost;

        if (emitter.score >= 100) {
          state.isGameOver = true;
        }
        break;
      }

      case "Attack": {
        const attackEvent = event as AttackEvent;
        let emitter = state.players[attackEvent.playerId];
        const target = state.players[attackEvent.targetId];

        if (target) {
          // Apply damage to shield first, and then the rest if any to the score
          if (target.shield >= attackEvent.damage) {
            target.shield -= attackEvent.damage;
          } else {
            target.score -= attackEvent.damage - target.shield;
            target.score = target.score < 0 ? 0 : target.score;
            target.shield = 0;
          } 

          emitter.energy -= attackEvent.cost;
        }

        break;
      }

      case "Defend": {
        const defendEvent = event as DefendEvent;
        let emitter = state.players[defendEvent.playerId];

        emitter.shield += defendEvent.increment;
        emitter.energy -= defendEvent.cost;

        break;
      }

      case "Energize": {
        const energizeEvent = event as EnergizeEvent;
        let emitter = state.players[energizeEvent.playerId];

        emitter.energy += energizeEvent.increment;
        
        break;
      }
    }
  }

  return state;
}
