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
        message = JSON.parse(message);
        const type = message.type;
        const data = message.data;
        const client = socketToClientMap.get(ws);

        switch (type) {
            case "lobby.create_game":
                const game = createGame(data.game_name);
                joinGame(client, game, data.player_name);
                broadcastLobbyState();
                break;
            case "lobby.join_game":
                game = idToGameMap.get(data.game_id);
                if (game !== null) {                             
                    joinGame(client, game, data.player_name)
                    broadcastLobbyState();
                } else {
                    // Error
                }
                break;
            case "lobby.state":
                client.sendMessage("lobby.state", formLobbyState())
                break;
            case "game.leave":
                break;
            case "game.state":
                
                break;
            case "game.submit":
                break;
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
