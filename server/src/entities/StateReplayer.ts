import { emit } from "process";
import {
  GameEvent,
  AdvanceEvent,
  AttackEvent,
  DefendEvent,
  EnergizeEvent,
  JoinEvent,
  SpinEvent,
} from "./GameEvent";

interface PlayerState {
  score: number;
  shield: number;
  energy: number;
}

interface GameState {
  players: {
    [playerId: string]: PlayerState;
  };
  isGameOver: boolean;
}

function initialState(): PlayerState {
  return {
    score: 0,
    shield: 0,
    energy: 6,
  };
}

export function buildState(events: GameEvent[]): GameState {
  const state: GameState = { players: {}, isGameOver: false };

  for (const event of events) {
    switch (event.type) {
      case "Join": {
        const joinEvent = event as JoinEvent;
        state.players[joinEvent.playerId] = initialState();
        break;
      }

      case "Spin": {
        const spinEvent = event as SpinEvent;
        let emitter = state.players[spinEvent.playerId];

        emitter.energy -= 2;
        
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
