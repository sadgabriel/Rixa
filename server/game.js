import { v4 as uuidv4 } from 'uuid';
import { createPlayer } from './player.js';
import { createFaction } from './faction.js';
import { createMatch } from './match.js';
import { BALANCE } from './balanceConfig.js';
import * as Errors from './errors.js';

export const GamePhase = Object.freeze({
    LOBBY: 'lobby',
    CONTEXT_INPUT: 'context_input',
    FACTION_CONCEPT_INPUT: 'faction_concept_input',
    FACTION_FLAW_INPUT: 'faction_flaw_input',
    CONTEXT_SETUP: 'context_setup',
    ATTACK: 'attack',
    DEFENSE: 'defense',
    COMPETITION_ANALYZE: 'competition_analyze',
    COMPETITION_NARRATE: 'competition_narrate',
    END: 'end',
});

const TOTAL_ROUNDS = 6;
const MAX_RESOURCE = 3;

export class Game {
    #id;
    #judge;
    constructor(judge) {
        this.#id = uuidv4();
        this.#judge = judge;
        this.phase = GamePhase.LOBBY;
        this.idToPlayerMap = new Map();
        this.idToFactionMap = new Map();
        this.idToMatchMap = new Map();
        this.round = 0;
        this.context = { rawContextDescription: "", description: "", eventLog: []};
        this.roundOffsets = [];
        this.playerCycle = [];
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

    get matches() {
        return Array.from(this.idToMatchMap.values());
    }

    findFactionByPlayerId(playerId) {
        const player = this.idToPlayerMap.get(playerId);
        const faction = this.idToFactionMap.get(player.factionId);
        return faction;
    }

    findMatchByAttacker(attackerId) {
        return this.matches.find(match => match.attackerId === attackerId);
    }

    findMatchByDefender(defenderId) {
        return this.matches.find(match => match.defenderId === defenderId);
    }

    findPlayerByFactionId(factionId) {
        return this.players.find(player => player.factionId === factionId);
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

        this.phase = GamePhase.CONTEXT_INPUT;
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
        if (this.phase !== GamePhase.CONTEXT_INPUT) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        this.context.rawContextDescription = rawContextDescription;
        this.phase = GamePhase.FACTION_CONCEPT_INPUT;
    }

    setRawFactionConceptAndName(factionId, rawConceptDescription, name) {
        if (this.phase !== GamePhase.FACTION_CONCEPT_INPUT) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const faction = this.idToFactionMap.get(factionId);
        if (!faction) {
            throw new Errors.FactionNotFoundError(factionId);
        }

        faction.rawConcept = rawConceptDescription;
        faction.name = name;

        if (this.factions.every(f => f.rawConcept !== null)) {
            this.phase = GamePhase.FACTION_FLAW_INPUT;
        }
    }

    setRawFactionFlaw(factionId, rawFlawDescription) {
        if (this.phase !== GamePhase.FACTION_FLAW_INPUT) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const faction = this.idToFactionMap.get(factionId);
        if (!faction) {
            throw new Errors.FactionNotFoundError(factionId);
        }

        faction.rawFlaw = rawFlawDescription;

        if (this.factions.every(f => f.rawFlaw !== null)) {
            this.phase = GamePhase.CONTEXT_SETUP;
        }
    }

    async setupContext() {
        if (this.phase !== GamePhase.CONTEXT_SETUP) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const output = await this.#judge.setupContext(this.context, this.factions);

        this.context.description = output.context.summary;
        for (const factionResult of output.factions) {
            const faction = this.idToFactionMap.get(factionResult.id);
            if (faction) {
                faction.description = factionResult.summary;
                faction.resources = factionResult.resources;
                for (const resource of faction.resources) {
                    resource.count = MAX_RESOURCE;
                }
            }
        }

        this.round = 1;

        this.playerCycle = shuffle(this.players);
        this._generateRoundOffsets();
        this._createMatches();
        this.phase = GamePhase.ATTACK;
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
        
        this.idToMatchMap = new Map();
        let index = 1;
        for (let i = 0; i < n; i++) {
            const matchId = `m_${String(index++).padStart(2, '0')}`
            const match = createMatch(matchId);

            match.attackerId = this.findFactionByPlayerId(playerIds[i]).id;
            match.defenderId = this.findFactionByPlayerId(playerIds[(i + offset) % n]).id;
            
            this.idToMatchMap.set(match.id, match);
        }
    }

    submitAttack(attackerId, attackDescription) {
        if (this.phase !== GamePhase.ATTACK) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const match = this.findMatchByAttacker(attackerId);
        if (!match) {
            throw new Errors.MatchNotFoundError(attackerId);
        }
        match.attackDescription = attackDescription;

        if (this.matches.every(m => m.attackDescription !== null)) {
            this.phase = GamePhase.DEFENSE;
        }
    }

    submitDefense(defenderId, defenseDescription) {
        if (this.phase !== GamePhase.DEFENSE) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const match = this.findMatchByDefender(defenderId);
        if (!match) {
            throw new Errors.MatchNotFoundError(defenderId);
        }
        match.defenseDescription = defenseDescription;

        if (this.matches.every(m => m.defenseDescription !== null)) {
            this.phase = GamePhase.COMPETITION_ANALYZE;
        }
    }

    async analyzeCompetition() {
        if (this.phase !== GamePhase.COMPETITION_ANALYZE) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const output = await this.#judge.analyzeCompetition(this.context, this.factions, this.matches);

        for (const matchAnalysis of output.analysis_results) {
            const match = this.idToMatchMap.get(matchAnalysis.match_id);
            match.attackerTags = matchAnalysis.attacker.tags;
            match.defenderTags = matchAnalysis.defender.tags;
            match.targetedResources = matchAnalysis.targeted_resources;
            match.protectedResources = matchAnalysis.protected_resources;
        }

        this._calculateCompetition();

        this.phase = GamePhase.COMPETITION_NARRATE;
    }

    _calculateCompetition() {
        for (const match of this.matches) {
            let winChance = BALANCE.baseWinChance;
            let attackerAdvantage = 0;
            let defenderAdvantage = 0;

            for (const tag of match.attackerTags) {
                attackerAdvantage += BALANCE.tagWeights[tag] ?? 0;
            }

            for (const tag of match.defenderTags) {
                defenderAdvantage += BALANCE.tagWeights[tag] ?? 0;
            }

            const advantage = attackerAdvantage - defenderAdvantage;
            winChance += advantage;

            winChance = Math.min(
                BALANCE.maxWinChance,
                Math.max(BALANCE.minWinChance, winChance)
            );

            const attackerWins = Math.random() < winChance;
            match.winnerId = attackerWins ? match.attackerId : match.defenderId;

            if (!attackerWins) continue;

            const defenderFaction = this.idToFactionMap.get(match.defenderId);

            const resourceNames = defenderFaction.resources.map(r => r.name);

            const weights = buildResourceWeights(
                resourceNames,
                match.targetedResources,
                match.protectedResources
            );

            const chosenName = pickWeightedOne(resourceNames, weights);
            match.lostResource = chosenName;

            const resObj = defenderFaction.resources.find(r => r.name === chosenName);

            if (resObj.count > 0) {
                resObj.count -= 1;
            } else {
                const scorePenalty = BALANCE.resourceLoss.scorePenaltyIfResourceEmpty;
                defenderFaction.score -= scorePenalty;
            }
        }
    }

    async narrateCompetition() {
        if (this.phase !== GamePhase.COMPETITION_NARRATE) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }

        const output = await this.#judge.narrateCompetition(this.context, this.factions, this.matches);

        this.context.eventLog.push(output.context_log);

        for (const matchNarration of output.results) {
            const match = this.idToMatchMap.get(matchNarration.match_id);
            match.displayNarrative = matchNarration.display_narrative;
        }

        if (this.round >= TOTAL_ROUNDS) {
            this.phase = GamePhase.END;
        } else {
            this.round += 1;
            this._createMatches();
            this.phase = GamePhase.ATTACK;
        }
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

function pickWeightedOne(keys, weightsByKey) {
    let total = 0;
    for (const k of keys) total += (weightsByKey[k] ?? 0);

    if (total <= 0) {
        return keys[Math.floor(Math.random() * keys.length)];
    }

    let r = Math.random() * total;
    for (const k of keys) {
        r -= (weightsByKey[k] ?? 0);
        if (r <= 0) return k;
    }
    return keys[keys.length - 1];
}

function buildResourceWeights(resourceNames, targetedResources, protectedResources) {
    const cfg = BALANCE.resourceLoss;
    const weights = {};

    for (const name of resourceNames) {
        weights[name] = cfg.baseWeight;
    }

    for (const name of (targetedResources ?? [])) {
        if (weights[name] !== undefined) weights[name] += cfg.targetedBonusWeight;
    }

    for (const name of (protectedResources ?? [])) {
        if (weights[name] !== undefined) weights[name] -= cfg.protectedPenaltyWeight;
    }

    for (const name of resourceNames) {
        weights[name] = Math.max(cfg.minWeight, weights[name]);
    }

    return weights;
}
