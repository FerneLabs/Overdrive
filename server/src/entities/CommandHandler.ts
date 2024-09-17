import { EventStore } from './EventStore';
import { GameEvent, AdvanceEvent, AttackEvent, DefendEvent, EnergizeEvent, JoinEvent, SpinEvent, SpinItem, GameOverEvent, UpdateDeckEvent } from './GameEvent';

export class GameCommandHandler {
  private eventStore: EventStore;

  constructor(eventStore: EventStore) {
    this.eventStore = eventStore;
  }

  private logEvent(event: GameEvent) {
    switch(event.type) {
      case 'GameOver':
        console.log(`[LOG] [EVENT] [${event.matchId}] :: ${new Date(event.timestamp).toISOString()} = ${event.type} | Interrumpted = ${event.matchInterrumpted} | Winner [${event.winnerPlayer.playerId}] | Final score [${event.winnerPlayer.score} - ${event.loserPlayer.score}]`);
        break;
      default:
        console.log(`[LOG] [EVENT] [${event.matchId}] [${event.playerId}] :: ${new Date(event.timestamp).toISOString()} = ${event.type}`);
        break;
    }
  }

  public playerJoin(matchId: string, playerId: string) {
    const event: JoinEvent = {
      type: 'Join',
      timestamp: Date.now(),
      matchId,
      playerId
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public gameOver(matchId: string, matchInterrumpted: boolean,  winnerPlayer: {playerId: string, score: number}, loserPlayer: {playerId: string, score: number}) {
    const event: GameOverEvent = {
      type: 'GameOver',
      timestamp: Date.now(),
      matchId,
      matchInterrumpted,
      winnerPlayer,
      loserPlayer
    }
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public updateDeck(matchId: string, playerId: string, deck: SpinItem[]) {
    const event: UpdateDeckEvent = {
      type: 'UpdateDeck',
      timestamp: Date.now(),
      matchId,
      playerId,
      deck
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public playerSpin(matchId: string, playerId: string, slots: SpinItem[]) {
    const event: SpinEvent = {
      type: 'Spin',
      timestamp: Date.now(),
      matchId,
      playerId,
      slots
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public playerAdvance(matchId: string, playerId: string, increment: number, cost: number, isCombo: boolean) {
    const event: AdvanceEvent = {
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

  public playerAttack(matchId: string, playerId: string, targetId: string, damage: number, cost: number, isCombo: boolean) {
    const event: AttackEvent = {
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

  public playerDefend(matchId: string, playerId: string, increment: number, cost: number, isCombo: boolean) {
    const event: DefendEvent = {
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

  public playerEnergize(matchId: string, playerId: string, increment: number, isCombo: boolean) {
    const event: EnergizeEvent = {
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
