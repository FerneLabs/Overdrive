import { GameEvent } from './GameEvent';

export class EventStore {
  private events: GameEvent[] = [];

  public saveEvent(event: GameEvent) {
    this.events.push(event);
  }

  public getEvents(matchId: string): GameEvent[] {
    return this.events.filter(event => event.matchId === matchId);
  }
}