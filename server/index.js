import WebSocket from 'ws';
import { Client } from './client.js'
import { Game, GamePhase } from './game.js'
import { Judge } from './judge.js'
import * as Errors from './errors.js';

const PORT = 7363;
const wss = new WebSocket.Server({ port: PORT });

const idToClientMap = new Map();
const socketToClientMap = new Map();
const idToGameMap = new Map();

wss.on('connection', (ws) => {
    const client = new Client(ws);
    idToClientMap.set(client.id, client);
    socketToClientMap.set(ws, client);

    ws.on('message', (message) => {
        const client = socketToClientMap.get(ws);

        try {
            const [type, data] = parseJson(message);

            switch (type) {
                case "lobby.create_game":
                    handleLobbyCreateGame(client, data);
                    break;
                case "lobby.join_game":
                    handleLobbyJoinGame(client, data);
                    break;
                case "lobby.state":
                    handleLobbyState(client, data);
                    break;
                case "game.leave":
                    handleGameLeave(client, data);
                    break;
                case "game.state":
                    handleGameState(client, data);
                    break;
                case "game.submit":
                    handleGameSubmit(client, data);
                    break;
            }
        } catch (error) {
            client.sendMessage("error", {
                code: error?.code ?? "INTERNAL",
                message: error?.message ?? "Internal error."
            })
        }
    })

    ws.on('close', () => {
        if (client.gameId !== null) {
            // Handle client disconnection from game
        }

        idToClientMap.delete(client.id);
        socketToClientMap.delete(ws);
    })
})

function findClientByPlayerId(playerId){
    for (const client of idToClientMap.values()){
        if (client.playerId === playerId){
            return client;
        }
    }
    throw new Errors.PlayerNotFoundError(playerId);
}

function parseJson(message) {
    let obj;
    try {
        const text = typeof message === "string" ? message : message.toString("utf-8");
        obj = JSON.parse(text);
    } catch {
        throw new Errors.BadMessageError();
    }
    return [obj.type, obj.data];
}

function handleLobbyCreateGame(client, data) {
    const game = createGame(data.game_name);
    joinGame(client, game, data.player_name);
    broadcastLobbyState();
}

function handleLobbyJoinGame(client, data) {
    const game = idToGameMap.get(data.game_id);
    if (game != null) {
        joinGame(client, game, data.player_name);
        broadcastLobbyState();
    } else {
        throw new Errors.GameNotFoundError(data.game_id);
    }
}

function handleLobbyState(client, data) {
    client.sendMessage("lobby.state", formLobbyState());
}

function handleGameLeave(client, data) {
    if (client.gameId !== null) {
        const game = idToGameMap.get(client.gameId);
        if (game == null) throw new Errors.GameNotFoundError(client.gameId);

        game.removePlayer(client.playerId);
        client.gameId = null;
        client.playerId = null;
    } else {
        throw new Errors.NotInGameError(client.id);
    }
}

function handleGameState(client, data) {
    if (client.gameId !== null) {
        const game = idToGameMap.get(client.gameId);
        client.sendMessage("game.state", formGameState(game));
    } else {
        throw new Errors.NotInGameError(client.id);
    }
}

function handleGameSubmit(client, data) {
    const kind = data.kind;
    const payload = data.payload;
    
    if (client.gameId == null) {
        throw new Errors.NotInGameError(client.id);
    }

    const game = idToGameMap.get(client.gameId);

    if (game == null) {
        throw new Errors.GameNotFoundError(client.gameId);
    }

    switch (kind) {
        case "ready":
            break;
        case "unready":
            break;
    }
}

function createGame(gameName){
    const game = new Game(new Judge(), gameName);
    idToGameMap.set(game.id, game);
    return game;
}

function joinGame(client, game, playerName){
    if (client.gameId !== null){
        throw new Errors.AlreadyInGameError(client.gameId);
    }

    client.playerId = game.addPlayer(playerName);
    client.gameId = game.id;
}

function leaveGame(client, game) {
    if (client.gameId === null || client.gameId !== game.id) {
        throw new Errors.NotInGameError(client.id);
    }

    game.removePlayer(client.playerId);
    client.gameId = null;
    client.playerId = null;
}

function formLobbyState() {
    const games = [];
    for (const game of idToGameMap.values()){
        games.push({
            id: game.id,
            name: game.name,
            state: game.phase === GamePhase.LOBBY ? "waiting": "playing",
            player_count: game.players.length
        })
    }
    return {
        games
    }
}

function formGameState(game) {
    return {
        game_id: game.id,
        phase: game.phase,
        round: game.round,
        context: game.context,
        players: game.players,
        factions: game.factions,
        matches: game.matches
    }
}

function broadcast(type, data) {
    for (const client of idToClientMap.values()) {
        if (client.isConnected()) {
            client.sendMessage(type, data);
        }
    }
}

function broadcastToGame(game, type, data) {
    for (const playerId of game.players.map(player => player.id)){
        const client = findClientByPlayerId(playerId);
        client.sendMessage(type, data);
    }
}

function broadcastLobbyState() {
    const data = formLobbyState();
    broadcast("lobby.state", data);
}
