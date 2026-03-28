import { WebSocketServer } from 'ws';
import { Client } from './client.js'
import { Game, GamePhase } from './game.js'
import { Judge } from './judge.js'
import * as Errors from './errors.js';

const PORT = 7363;
const wss = new WebSocketServer({ port: PORT });

console.log(`WebSocket server is running on ws://localhost:${PORT}`);

const idToClientMap = new Map();
const socketToClientMap = new Map();
const idToGameMap = new Map();

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

wss.on('connection', (ws) => {
    const client = new Client(ws);
    idToClientMap.set(client.id, client);
    socketToClientMap.set(ws, client);

    console.log(`Client connected: ${client.id}`);

    client.sendMessage("welcome", {
        clientState: formClientState(client),
        lobbyState: formLobbyState()
    })

    ws.on('message', async (message) => {
        const client = socketToClientMap.get(ws);

        try {
            const [type, data] = parseJson(message);
            console.log(`Received message from client ${client.id}:`, type);

            switch (type) {
                case "client.state":
                    handleClientState(client, data);
                    break;
                case "lobby.createGame":
                    handleLobbyCreateGame(client, data);
                    break;
                case "lobby.joinGame":
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
                    await handleGameSubmit(client, data);
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
            const game = idToGameMap.get(client.gameId);
            if (game != null) {
                if (game.phase === GamePhase.LOBBY) {
                    game.removePlayer(client.playerId);

                    if (game.players.length === 0){
                        idToGameMap.delete(game.id);
                    }

                    broadcastLobbyState();
                    broadcastGameState(game);
                } else {
                    for (const player of game.players) {
                        try {
                            const playerClient = findClientByPlayerId(player.id);
                            playerClient.gameId = null;
                            playerClient.playerId = null;
                            playerClient.sendMessage("client.state", formClientState(playerClient));
                        } catch (error) {
                            if (!(error instanceof Errors.PlayerNotFoundError)) {
                                console.error(`Error handling disconnect for player ${player.id}:`, error);
                            }
                        }
                    }

                    idToGameMap.delete(game.id);
                    broadcastLobbyState();
                }
            }
        }

        idToClientMap.delete(client.id);
        socketToClientMap.delete(ws);
        console.log(`Client disconnected: ${client.id}`);
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

function handleClientState(client, data){
    client.sendMessage("client.state", formClientState(client));
}

function handleLobbyCreateGame(client, data) {
    const game = createGame(data.gameName);
    joinGame(client, game, data.playerName);
    client.sendMessage("client.state", formClientState(client));
    broadcastLobbyState();
    broadcastGameState(game);
}

function handleLobbyJoinGame(client, data) {
    const game = idToGameMap.get(data.gameId);
    if (game != null) {
        joinGame(client, game, data.playerName);
        client.sendMessage("client.state", formClientState(client));
        broadcastLobbyState();
        broadcastGameState(game);
    } else {
        throw new Errors.GameNotFoundError(data.gameId);
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

        if (game.players.length === 0){
            idToGameMap.delete(game.id);
        }
    } else {
        throw new Errors.NotInGameError(client.id);
    }

    client.sendMessage("client.state", formClientState(client));
    broadcastLobbyState();
}

function handleGameState(client, data) {
    if (client.gameId !== null) {
        const game = idToGameMap.get(client.gameId);
        if (game == null) {
            throw new Errors.GameNotFoundError(client.gameId);
        }

        client.sendMessage("game.state", formGameState(game));
    } else {
        throw new Errors.NotInGameError(client.id);
    }
}

async function handleGameSubmit(client, data) {
    const kind = data.kind;
    const payload = data.payload;
    
    if (client.gameId == null) {
        throw new Errors.NotInGameError(client.id);
    }

    const game = idToGameMap.get(client.gameId);

    if (game == null) {
        throw new Errors.GameNotFoundError(client.gameId);
    }

    let factionId;

    switch (kind) {
        case "ready":
            game.setReady(client.playerId, true);
            break;
        case "unready":
            game.setReady(client.playerId, false);
            break;
        case "context":
            game.submitContext(payload.contextDescription);
            break;
        case "factionConcept":
            factionId = game.getFactionByPlayerId(client.playerId).id;
            game.submitFactionConceptAndName(factionId, payload.factionConceptDescription, payload.factionName);
            break;
        case "factionFlaw":
            factionId = payload.factionId;
            game.submitFactionFlaw(factionId, payload.factionFlawDescription);
            
            if (game.phase === GamePhase.CONTEXT_SETUP){
                broadcastGameState(game);
                await game.setupContext();
                broadcastGameState(game);
                await sleep(10000);
                    game.endContextSetupFinish();
            }
            break;
        case "attack":
            factionId = game.getFactionByPlayerId(client.playerId).id;
            game.submitAttack(factionId, payload.attackDescription);
            break;
        case "defense":
            factionId = game.getFactionByPlayerId(client.playerId).id;
            game.submitDefense(factionId, payload.defenseDescription);

            if (game.phase === GamePhase.COMPETITION_ANALYZE) {
                broadcastGameState(game);
                await game.analyzeCompetition();

                broadcastGameState(game);
                await game.narrateCompetition();

                broadcastGameState(game);
                await sleep(10000);
                game.endCompetitionFinish();
                if (game.phase === GamePhase.END) {
                    await sleep(10000);
                    game.reset();
                }
            }
            break;
    }

    broadcastGameState(game);
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

function formLobbyState() {
    const games = [];
    for (const game of idToGameMap.values()){
        games.push({
            id: game.id,
            name: game.name,
            state: game.phase === GamePhase.LOBBY ? "waiting": "playing",
            playerCount: game.players.length
        })
    }
    return {
        games
    }
}

function formGameState(game) {
    return {
        id: game.id,
        phase: game.phase,
        round: game.round,
        context: game.context,
        players: game.players,
        leadPlayerId: game.playerCycle.length > 0 ? game.playerCycle[0].id : null,
        factions: game.factions,
        matches: game.matches,
        setupOffset: game.setupOffset,
    }
}

function formClientState(client) {
    return {
        id: client.id,
        gameId: client.gameId,
        playerId: client.playerId
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
    broadcast("lobby.state", formLobbyState());
}

function broadcastGameState(game) {
    broadcastToGame(game, "game.state", formGameState(game));
}

