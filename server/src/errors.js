export class PlayerExistsError extends Error {
    constructor(playerId) {
        super(`Player with ID ${playerId} already exists.`);
        this.name = 'PlayerExistsError';
        this.playerId = playerId;
    }
}

export class PlayerNameTakenError extends Error {
    constructor(name) {
        super(`Player name "${name}" is already taken.`);
        this.name = 'PlayerNameTakenError';
        this.playerName = name;
    }
}

export class InvalidGamePhaseError extends Error {
    constructor(phase) {
        super(`Invalid game phase: ${phase}.`);
        this.name = 'InvalidGamePhaseError';
        this.phase = phase;
    }
}

export class NotEnoughPlayersError extends Error {
    constructor(count) {
        super(`Not enough players to start the game. Current: ${count}, Required: 2.`);
        this.name = 'NotEnoughPlayersError';
        this.count = count;
    }
}

export class PlayerNotFoundError extends Error {
    constructor(playerId) {
        super(`Player with ID ${playerId} not found.`);
        this.name = 'PlayerNotFoundError';
        this.playerId = playerId;
    }
}

export class FactionNotFoundError extends Error {
    constructor(factionId) {
        super(`Faction with ID ${factionId} not found.`);
        this.name = 'FactionNotFoundError';
        this.factionId = factionId;
    }
}

export class MatchNotFoundError extends Error {
    constructor(playerId) {
        super(`Match for player ID ${playerId} not found.`);
        this.name = 'MatchNotFoundError';
        this.playerId = playerId;
    }
}

export class FactionAlreadySetError extends Error {
    constructor(playerId) {
        super(`Player with ID ${playerId} has already set their faction.`);
        this.name = 'FactionAlreadySetError';
        this.playerId = playerId;
    }
}

export class TooManyAttacksError extends Error {
    constructor(playerId) {
        super(`Player with ID ${playerId} has too many attacks.`);
        this.name = 'TooManyAttacksError';
        this.playerId = playerId;
    }
}

export class TooManyDefensesError extends Error {
    constructor(playerId) {
        super(`Player with ID ${playerId} has too many defenses.`);
        this.name = 'TooManyDefensesError';
        this.playerId = playerId;
    }
}

export class SocketNotOpenError extends Error {
    constructor(clientId) {
        super(`Socket for client ID ${clientId} is not open.`);
        this.name = 'SocketNotOpenError';
        this.clientId = clientId;
    }
}

export class JudgeCallError extends Error {
    constructor(message) {
        super(`Judge call failed: ${message}`);
        this.name = 'JudgeCallError';
    }
}

export class InvalidJudgeResponseError extends Error {
    constructor(message) {
        super(`Invalid judge response: ${message}`);
        this.name = 'InvalidJudgeResponseError';
    }
}

