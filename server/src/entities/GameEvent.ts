// Define common interfaces for events
export interface BaseEvent {
    type: string;
    matchId: string;
    timestamp: number;
}

export interface JoinEvent extends BaseEvent {
    type: 'Join';
    playerId: string;
}

export interface SpinEvent extends BaseEvent {
    type: 'Spin';
    playerId: string;
    slots: SpinItem[]
}
  
export interface AdvanceEvent extends BaseEvent {
    type: 'Advance';
    cost: number;
    playerId: string;
    increment: number;
}
  
export interface AttackEvent extends BaseEvent {
    type: 'Attack';
    cost: number;
    playerId: string;
    targetId: string;
    damage: number;
}

export interface DefendEvent extends BaseEvent {
    type: 'Defend';
    cost: number;
    playerId: string;
    increment: number;
}

export interface EnergizeEvent extends BaseEvent {
    type: 'Energize';
    playerId: string;
    increment: number;
}

// Union type for all possible game events
export type GameEvent = JoinEvent | SpinEvent | AdvanceEvent | AttackEvent | DefendEvent | EnergizeEvent;

export interface SpinItem {
    type: string,
    value: number
}
  