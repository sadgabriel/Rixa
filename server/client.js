import { v4 as uuidv4 } from 'uuid';
import WebSocket from 'ws';
import * as Errors from './errors.js';

export class Client {
    constructor(socket) {
        this.socket = socket;
        this.id = uuidv4();
        this.gameId = null;
        this.playerId = null;
    }

    send(message) {
        if (this.socket.readyState !== WebSocket.OPEN) {
            throw new Errors.SocketNotOpenError(this.id);
        }

        this.socket.send(message, (error) => {
            if (error) {
                console.error(`Failed to send message to client ${this.id}:`, error);
            }
        });
    }

    sendMessage(type, data) {
        console.log(`Sending message to client ${this.id}:`, type);
        const message = JSON.stringify({ type, data });
        this.send(message);
    }

    isConnected() {
        return this.socket.readyState === WebSocket.OPEN;
    }

    disconnect(reason = 'Normal Closure') {
        if (this.socket.readyState <= WebSocket.OPEN) {
            this.socket.close(1000, reason);
        }
    }
}