import { EventStore } from '../entities/EventStore';
import { buildState } from '../entities/StateReplayer';

export class GameService {
  private eventStore: EventStore;

  constructor(eventStore: EventStore) {
    this.eventStore = eventStore;
  }

  public getMatchState(matchId: string) {
    const events = this.eventStore.getEvents(matchId);
    return buildState(events);
  }
}
