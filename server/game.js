import { v4 as uuidv4 } from 'uuid';
import { createPlayer } from './player.js';
import { createFaction } from './faction.js';
import { createMatch } from './match.js';
import { createContext } from './context.js';
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
    constructor(judge, name = "new game") {
        this.id = uuidv4();
        this.judge = judge;
        this.name = name;
        this.phase = GamePhase.LOBBY;
        this.idToPlayerMap = new Map();
        this.idToFactionMap = new Map();
        this.idToMatchMap = new Map();
        this.round = 0;
        this.context = createContext();
        this.roundOffsets = [];
        this.playerCycle = [];
        this.setupOffset = 0;
    }

    _checkPhase(required) {
        if (this.phase !== required) {
            throw new Errors.InvalidGamePhaseError(this.phase);
        }
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

    getPlayer(playerId) {
        const player = this.idToPlayerMap.get(playerId);
        if (!player) {
            throw new Errors.PlayerNotFoundError(playerId);
        }
        return player;
    }

    getFaction(factionId) {
        const faction = this.idToFactionMap.get(factionId);
        if (!faction) {
            throw new Errors.FactionNotFoundError(factionId);
        }
        return faction;
    }

    getMatch(matchId) {
        const match = this.idToMatchMap.get(matchId);
        if (!match) {
            throw new Errors.MatchNotFoundError(matchId);
        }
        return match;
    }

    getFactionByPlayerId(playerId) {
        const player = this.getPlayer(playerId);
        return this.getFaction(player.factionId);
    }

    getMatchByAttacker(attackerId) {
        const match = this.matches.find(match => match.attackerId === attackerId);
        if (!match) {
            throw new Errors.MatchNotFoundError(attackerId);
        }
        return match;
    }

    getMatchByDefender(defenderId) {
        const match = this.matches.find(match => match.defenderId === defenderId);
        if (!match) {
            throw new Errors.MatchNotFoundError(defenderId);
        }
        return match;
    }

    addPlayer(name) {
        this._checkPhase(GamePhase.LOBBY);

        const player = createPlayer(name);
        this.idToPlayerMap.set(player.id, player);
        return player.id;
    }

    removePlayer(playerId) {
        this._checkPhase(GamePhase.LOBBY);

        this.idToPlayerMap.delete(playerId);
    }

    setReady(playerId, ready = true) {
        this._checkPhase(GamePhase.LOBBY);

        const player = this.getPlayer(playerId);
        player.ready = ready;

        if (this.players.every(p => p.ready) && this.players.length > 1) {
            this.gameStart();
        }
    }

    gameStart() {
        this._checkPhase(GamePhase.LOBBY);

        if (!this.players.every(p => p.ready)){
            throw new Errors.PlayersNotReadyError();
        }

        this.round = 1;
        this._createFactions();
        this.playerCycle = shuffle(this.players);
        this._generateRoundOffsets();
        this.setupOffset = Math.floor(Math.random() * (this.players.length - 1)) + 1;
        this._createMatches();

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

    submitContext(rawContextDescription) {
        this._checkPhase(GamePhase.CONTEXT_INPUT);

        if (this.context.rawContextDescription !== null){
            throw new Errors.AlreadySubmittedError();
        }

        this.context.rawContextDescription = rawContextDescription;
        this.phase = GamePhase.FACTION_CONCEPT_INPUT;
    }

    submitFactionConceptAndName(factionId, rawConceptDescription, name) {
        this._checkPhase(GamePhase.FACTION_CONCEPT_INPUT);

        const faction = this.getFaction(factionId);

        if (faction.rawConcept !== null){
            throw new Errors.AlreadySubmittedError();
        }

        faction.rawConcept = rawConceptDescription;
        faction.name = name;

        if (this.factions.every(f => f.rawConcept !== null)) {
            this.phase = GamePhase.FACTION_FLAW_INPUT;
        }
    }

    submitFactionFlaw(factionId, rawFlawDescription) {
        this._checkPhase(GamePhase.FACTION_FLAW_INPUT);

        const faction = this.getFaction(factionId);

        if (faction.rawFlaw !== null){
            throw new Errors.AlreadySubmittedError();
        }

        faction.rawFlaw = rawFlawDescription;

        if (this.factions.every(f => f.rawFlaw !== null)) {
            this.phase = GamePhase.CONTEXT_SETUP;
        }
    }

    async setupContext() {
        this._checkPhase(GamePhase.CONTEXT_SETUP);
        console.log("Setting up context with judge...");

        const output = await this.judge.setupContext(this.context, this.factions);

        this.context.description = output.context.summary;
        for (const factionResult of output.factions) {
            const faction = this.getFaction(factionResult.id);
            faction.description = factionResult.summary;
            faction.resources = factionResult.resources;
            for (const resource of faction.resources) {
                resource.count = MAX_RESOURCE;
            }
        }
        console.log("Context setup complete.");

        this.phase = GamePhase.ATTACK;
    }

    _generateRoundOffsets() {
        const n = this.players.length;
        const offsets = [];
        
        for (let i = 1; i < n; i++) {
            offsets.push(i);
        }
        
        this.roundOffsets = [];
        for (let round = 0; round < TOTAL_ROUNDS; round++) {
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
            const matchId = `m_${String(index++).padStart(2, '0')}`;
            const match = createMatch(matchId);

            match.attackerId = this.getFactionByPlayerId(playerIds[i]).id;
            match.defenderId = this.getFactionByPlayerId(playerIds[(i + offset) % n]).id;
            
            this.idToMatchMap.set(match.id, match);
        }
    }

    submitAttack(attackerId, attackDescription) {
        this._checkPhase(GamePhase.ATTACK);

        const match = this.getMatchByAttacker(attackerId);

        if (match.attackDescription !== null){
            throw new Errors.AlreadySubmittedError();
        }

        match.attackDescription = attackDescription;

        if (this.matches.every(m => m.attackDescription !== null)) {
            this.phase = GamePhase.DEFENSE;
        }
    }

    submitDefense(defenderId, defenseDescription) {
        this._checkPhase(GamePhase.DEFENSE);

        const match = this.getMatchByDefender(defenderId);

        if (match.defenseDescription !== null){
            throw new Errors.AlreadySubmittedError();
        }

        match.defenseDescription = defenseDescription;

        if (this.matches.every(m => m.defenseDescription !== null)) {
            this.phase = GamePhase.COMPETITION_ANALYZE;
        }
    }

    async analyzeCompetition() {
        this._checkPhase(GamePhase.COMPETITION_ANALYZE);

        const output = await this.judge.analyzeCompetition(this.context, this.factions, this.matches);

        for (const matchAnalysis of output.analysis_results) {
            const match = this.getMatch(matchAnalysis.match_id);
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

            const defenderFaction = this.getFaction(match.defenderId);

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
        this._checkPhase(GamePhase.COMPETITION_NARRATE);

        const output = await this.judge.narrateCompetition(this.context, this.factions, this.matches);

        this.context.eventLog.push(output.context_log);

        for (const matchNarration of output.results) {
            const match = this.getMatch(matchNarration.match_id);
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

    restart() {
        // TBD
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
