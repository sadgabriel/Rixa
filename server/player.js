import { v4 as uuidv4 } from 'uuid';

export function createPlayer(name) {
    return {
        id: uuidv4(),
        name: name,
        factionId: null,
        ready: false,
        contextSetupFinishReady: false,
        competitionFinishReady: false,
    };
}