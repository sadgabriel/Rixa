import WebSocket from 'ws';

const PORT = 7363;
const wss = new WebSocket.Server({ port: PORT });

const idToClientMap = new Map();
const socketToClientMap = new Map();

wss.on('connection', (ws) => {
  const client = new Client(ws);
  idToClientMap.set(client.id, client);
  socketToClientMap.set(ws, client);

  ws.on('message', (message) => {

  })

  ws.on('close', () => {
    if (client.game) {
      // Handle client disconnection from game
    }

    idToClientMap.delete(client.id);
    socketToClientMap.delete(ws);
  })
})

function broadcast(type, data) {
  for (const client of idToClientMap.values()) {
    if (client.isConnected()) {
      client.sendMessage(type, data);
    }
  }
}

function broadcastToGame(gameId, type, data) {
  // TBD
}
