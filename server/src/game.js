import { v4 as uuidv4 } from 'uuid';
import { createPlayer } from './player.js';
import { createFaction } from './faction.js';
import * as Errors from './errors.js';

export const GamePhase = Object.freeze({
    LOBBY: 'lobby',
    CONTEXT_SETTING: 'context_setting',
    FACTION_CONCEPT_SETTING: 'faction_concept_setting',
    FACTION_FLAW_SETTING: 'faction_flaw_setting',
    SETTING_EVALUATION: 'setting_evaluation',
    ATTACK: 'attack',
    DEFENSE: 'defense',
    COMPETITION_EVALUATION: 'competition_evaluation',
    END: 'end',
});

const TOTAL_ROUNDS = 6;

export class Game {
    #id;
    #judge;
    constructor(judge) {
        this.#id = uuidv4();
        this.#judge = judge;
        this.phase = GamePhase.LOBBY;
        this.idToPlayerMap = new Map();
        this.idToFactionMap = new Map();
        this.round = 0;
        this.rawContextDescription = null;
        this.roundOffsets = [];
        this.playerCycle = [];
        this.currentMatches = [];
    }

    get id() {
        return this.#id;
    }

    get players() {
        return Array.from(this.idToPlayerMap.values());
    }

    get factions() {
        return Array.from(this.idToFactionMap.values());
    }

    addPlayer(name) {
        if (this.phase !== GamePhase.LOBBY) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const player = createPlayer(name);
        this.idToPlayerMap.set(player.id, player);
        return player.id;
    }

    removePlayer(playerId) {
        if (this.phase !== GamePhase.LOBBY) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        this.idToPlayerMap.delete(playerId);
    }

    gameStart() {
        if (this.phase !== GamePhase.LOBBY) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }
        if (this.players.length < 2) {
            throw new Errors.NotEnoughPlayersError(this.players.length);
        }

        this._createFactions();

        this.phase = GamePhase.CONTEXT_SETTING;
    }

    _createFactions() {
    let index = 1;
    for (const player of this.players) {
        const factionId = `fac_${String(index++).padStart(2, '0')}`;
        const faction = createFaction(factionId);
        this.idToFactionMap.set(faction.id, faction);
        player.factionId = faction.id;
    }
}

    setRawContext(rawContextDescription) {
        if (this.phase !== GamePhase.CONTEXT_SETTING) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        this.rawContextDescription = rawContextDescription;
        this.phase = GamePhase.FACTION_CONCEPT_SETTING;
    }

    setRawFactionConcept(factionId, rawConceptDescription) {
        if (this.phase !== GamePhase.FACTION_CONCEPT_SETTING) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const faction = this.idToFactionMap.get(factionId);
        if (!faction) {
            throw new Errors.FactionNotFoundError(factionId);
        }

        faction.rawConcept = rawConceptDescription;

        if (this.factions.every(f => f.rawConcept !== null)) {
            this.phase = GamePhase.FACTION_FLAW_SETTING;
        }
    }

    setRawFactionFlaw(factionId, rawFlawDescription) {
        if (this.phase !== GamePhase.FACTION_FLAW_SETTING) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const faction = this.idToFactionMap.get(factionId);
        if (!faction) {
            throw new Errors.FactionNotFoundError(factionId);
        }

        faction.rawFlaw = rawFlawDescription;

        if (this.factions.every(f => f.rawFlaw !== null)) {
            this.phase = GamePhase.SETTING_EVALUATION;
            // TBD: Trigger evaluation
        }
    }

    async _evaluateSetting() {
        if (this.phase !== GamePhase.SETTING_EVALUATION) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        // TBD: Use judge to evaluate context and faction 

        this.phase = GamePhase.ATTACK;
        this.round = 1;

        this.playerCycle = shuffle(this.players);
        this._generateRoundOffsets();
        this._createMatches();
        
    }

    _generateRoundOffsets() {
        const n = this.players.length;
        const offsets = [];
        
        for (let i = 1; i < n; i++) {
            offsets.push(i);
        }
        
        this.roundOffsets = [];
        for (let round = 0; round < 6; round++) {
            this.roundOffsets.push(offsets[round % offsets.length]);
        }
    }

    _createMatches() {
        const playerIds = Array.from(this.playerCycle.map(p => p.id));
        const n = playerIds.length;
        const offset = this.roundOffsets[this.round - 1];
        
        this.currentMatches = [];
        for (let i = 0; i < n; i++) {
            this.currentMatches.push({
                attackerId: playerIds[i],
                defenderId: playerIds[(i + offset) % n],
                attackDescription: null,
                defenseDescription: null,
            });
        }
    }

    findMatchByAttacker(attackerId) {
        return this.currentMatches.find(match => match.attackerId === attackerId);
    }

    findMatchByDefender(defenderId) {
        return this.currentMatches.find(match => match.defenderId === defenderId);
    }

    submitAttack(attackerId, attackDescription) {
        const match = this.findMatchByAttacker(attackerId);
        if (!match) {
            throw new Errors.MatchNotFoundError(attackerId);
        }
        match.attackDescription = attackDescription;

        if (this.currentMatches.every(m => m.attackDescription !== null)) {
            this.phase = GamePhase.DEFENSE;
        }
    }

    submitDefense(defenderId, defenseDescription) {
        const match = this.findMatchByDefender(defenderId);
        if (!match) {
            throw new Errors.MatchNotFoundError(defenderId);
        }
        match.defenseDescription = defenseDescription;

        if (this.currentMatches.every(m => m.defenseDescription !== null)) {
            this.phase = GamePhase.COMPETITION_EVALUATION;

            // TBD: Trigger battle evaluation
        }
    }

    async _evaluateBattles() {

    }

    
}

function shuffle(array) {
    const result = [...array];
    
    for (let i = result.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [result[i], result[j]] = [result[j], result[i]];
    }
    
    return result;
}
