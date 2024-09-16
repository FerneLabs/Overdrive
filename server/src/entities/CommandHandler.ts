import { EventStore } from './EventStore';
import { GameEvent, AdvanceEvent, AttackEvent, DefendEvent, EnergizeEvent, JoinEvent, SpinEvent, SpinItem, GameOverEvent } from './GameEvent';

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

  public playerAdvance(matchId: string, playerId: string, increment: number, cost: number) {
    const event: AdvanceEvent = {
      type: 'Advance',
      timestamp: Date.now(),
      matchId,
      playerId,
      increment,
      cost
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public playerAttack(matchId: string, playerId: string, targetId: string, damage: number, cost: number) {
    const event: AttackEvent = {
      type: 'Attack',
      timestamp: Date.now(),
      matchId,
      playerId,
      targetId,
      damage,
      cost
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public playerDefend(matchId: string, playerId: string, increment: number, cost: number) {
    const event: DefendEvent = {
      type: 'Defend',
      timestamp: Date.now(),
      matchId,
      playerId,
      increment,
      cost
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }

  public playerEnergize(matchId: string, playerId: string, increment: number) {
    const event: EnergizeEvent = {
      type: 'Energize',
      timestamp: Date.now(),
      matchId,
      playerId,
      increment
    };
    this.eventStore.saveEvent(event);
    this.logEvent(event);
  }
}
