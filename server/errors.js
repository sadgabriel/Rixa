class AppError extends Error {
  constructor(code, message) {
    super(message);
    this.name = this.constructor.name;
    this.code = code;

    if (Error.captureStackTrace) {
      Error.captureStackTrace(this, this.constructor);
    }
  }
}

export class BadMessageError extends AppError {
  constructor(message = "Invalid message format.") {
    super("BAD_MESSAGE", message);
  }
}

export class UnknownTypeError extends AppError {
  constructor(type) {
    super("UNKNOWN_TYPE", `Unknown message type: ${type}`);
    this.type = type;
  }
}

export class GameNotFoundError extends AppError {
  constructor(gameId) {
    super("GAME_NOT_FOUND", `Game not found: ${gameId}`);
    this.gameId = gameId;
  }
}

export class NotInGameError extends AppError {
  constructor(clientId) {
    super("NOT_IN_GAME", `Client is not in a game: ${clientId}`);
    this.clientId = clientId;
  }
}

export class AlreadyInGameError extends AppError {
  constructor(gameId) {
    super("ALREADY_IN_GAME", `Client is already in a game: ${gameId}`);
    this.gameId = gameId;
  }
}

export class PlayerExistsError extends AppError {
  constructor(playerId) {
    super("PLAYER_EXISTS", `Player with ID ${playerId} already exists.`);
    this.playerId = playerId;
  }
}

export class PlayerNotFoundError extends AppError {
  constructor(playerId) {
    super("PLAYER_NOT_FOUND", `Player with ID ${playerId} not found.`);
    this.playerId = playerId;
  }
}

export class PlayerNameTakenError extends AppError {
  constructor(playerName) {
    super("PLAYER_NAME_TAKEN", `Player name "${playerName}" is already taken.`);
    this.playerName = playerName;
  }
}

export class InvalidGamePhaseError extends AppError {
  constructor(phase) {
    super("INVALID_GAME_PHASE", `Invalid game phase: ${phase}.`);
    this.phase = phase;
  }
}

export class NotEnoughPlayersError extends AppError {
  constructor(count, required = 2) {
    super("NOT_ENOUGH_PLAYERS", `Not enough players to start the game. Current: ${count}, Required: ${required}.`);
    this.count = count;
    this.required = required;
  }
}

export class FactionNotFoundError extends AppError {
  constructor(factionId) {
    super("FACTION_NOT_FOUND", `Faction with ID ${factionId} not found.`);
    this.factionId = factionId;
  }
}

export class MatchNotFoundError extends AppError {
  constructor(playerId) {
    super("MATCH_NOT_FOUND", `Match for player ID ${playerId} not found.`);
    this.playerId = playerId;
  }
}

export class FactionAlreadySetError extends AppError {
  constructor(playerId) {
    super("FACTION_ALREADY_SET", `Player with ID ${playerId} has already set their faction.`);
    this.playerId = playerId;
  }
}

export class TooManyAttacksError extends AppError {
  constructor(playerId) {
    super("TOO_MANY_ATTACKS", `Player with ID ${playerId} has too many attacks.`);
    this.playerId = playerId;
  }
}

export class TooManyDefensesError extends AppError {
  constructor(playerId) {
    super("TOO_MANY_DEFENSES", `Player with ID ${playerId} has too many defenses.`);
    this.playerId = playerId;
  }
}

export class SocketNotOpenError extends AppError {
  constructor(clientId) {
    super("SOCKET_NOT_OPEN", `Socket for client ID ${clientId} is not open.`);
    this.clientId = clientId;
  }
}

export class JudgeCallError extends AppError {
  constructor(message = "Judge call failed.") {
    super("JUDGE_CALL_FAILED", message);
  }
}

export class InvalidJudgeResponseError extends AppError {
  constructor(message = "Invalid judge response.") {
    super("INVALID_JUDGE_RESPONSE", message);
  }
}
