import { jest } from "@jest/globals";
import { Game, GamePhase } from "../game.js";
import * as Errors from "../errors.js";

function makeJudgeStub() {
  return {
    setupContext: jest.fn(async (context, factions) => ({
      phase: "context_setup",
      context: {
        summary: "World summary",
      },
      factions: factions.map(f => ({
        id: f.id,
        summary: `Summary for ${f.id}`,
        resources: [
          { name: "manpower", description: "", count: 0 },
          { name: "morale", description: "", count: 0 },
          { name: "intel", description: "", count: 0 },
        ],
      })),
    })),

    analyzeCompetition: jest.fn(async (context, factions, matches) => ({
      phase: "competition_analyze",
      analysis_results: matches.map(m => ({
        match_id: m.id,
        attacker: { tags: ["coherence"] },
        defender: { tags: [] },
        targeted_resources: ["manpower"],
        protected_resources: [],
      })),
    })),

    narrateCompetition: jest.fn(async (context, factions, matches) => ({
      phase: "competition_narrate",
      context_log: { type: "round_end" },
      results: matches.map(m => ({
        match_id: m.id,
        display_narrative: `Narrative for ${m.id}`,
        context_logs: [],
      })),
    })),
  };
}

beforeEach(() => {
  jest.restoreAllMocks();
});

describe("Game (unit test, real deps except judge)", () => {
  test("initial state", () => {
    const game = new Game(makeJudgeStub());

    expect(game.phase).toBe(GamePhase.LOBBY);
    expect(game.players).toHaveLength(0);
    expect(game.factions).toHaveLength(0);
    expect(game.matches).toHaveLength(0);
    expect(game.round).toBe(0);
  });

  test("addPlayer / removePlayer only allowed in LOBBY", () => {
    const game = new Game(makeJudgeStub());

    const p1 = game.addPlayer("Alice");
    const p2 = game.addPlayer("Bob");

    expect(game.players.map(p => p.name)).toEqual(["Alice", "Bob"]);

    game.removePlayer(p1);
    expect(game.players).toHaveLength(1);

    game.phase = GamePhase.CONTEXT_INPUT;
    expect(() => game.addPlayer("Charlie"))
      .toThrow(Errors.InvalidGamePhaseError);
    expect(() => game.removePlayer(p2))
      .toThrow(Errors.InvalidGamePhaseError);
  });

  test("gameStart creates factions and moves to CONTEXT_INPUT", () => {
    const game = new Game(makeJudgeStub());

    expect(() => game.gameStart())
      .toThrow(Errors.NotEnoughPlayersError);

    game.addPlayer("Alice");
    game.addPlayer("Bob");

    game.gameStart();

    expect(game.phase).toBe(GamePhase.CONTEXT_INPUT);
    expect(game.factions).toHaveLength(2);

    for (const player of game.players) {
      expect(player.factionId).toBeTruthy();
    }
  });

  test("context & faction input phase flow", () => {
    const game = new Game(makeJudgeStub());
    game.addPlayer("A");
    game.addPlayer("B");
    game.gameStart();

    game.setRawContext("raw world");
    expect(game.phase).toBe(GamePhase.FACTION_CONCEPT_INPUT);

    for (const faction of game.factions) {
      game.setRawFactionConceptAndName(
        faction.id,
        "concept",
        `name-${faction.id}`
      );
    }
    expect(game.phase).toBe(GamePhase.FACTION_FLAW_INPUT);

    for (const faction of game.factions) {
      game.setRawFactionFlaw(faction.id, "flaw");
    }
    expect(game.phase).toBe(GamePhase.CONTEXT_SETUP);
  });

  test("setupContext initializes round, resources, matches", async () => {
    const judge = makeJudgeStub();
    const game = new Game(judge);

    jest.spyOn(Math, "random").mockReturnValue(0);

    game.addPlayer("A");
    game.addPlayer("B");
    game.gameStart();
    game.setRawContext("raw");
    for (const f of game.factions) {
      game.setRawFactionConceptAndName(f.id, "c", "n");
    }
    for (const f of game.factions) {
      game.setRawFactionFlaw(f.id, "f");
    }

    await game.setupContext();

    expect(judge.setupContext).toHaveBeenCalled();
    expect(game.phase).toBe(GamePhase.ATTACK);
    expect(game.round).toBe(1);
    expect(game.matches).toHaveLength(2);

    for (const faction of game.factions) {
      expect(faction.resources).toHaveLength(3);
      for (const r of faction.resources) {
        expect(r.count).toBe(3);
      }
    }
  });

  test("attack → defense → analyze flow", async () => {
    const judge = makeJudgeStub();
    const game = new Game(judge);
    jest.spyOn(Math, "random").mockReturnValue(0);

    game.addPlayer("A");
    game.addPlayer("B");
    game.gameStart();
    game.setRawContext("raw");
    for (const f of game.factions) {
      game.setRawFactionConceptAndName(f.id, "c", "n");
    }
    for (const f of game.factions) {
      game.setRawFactionFlaw(f.id, "f");
    }
    await game.setupContext();

    for (const m of game.matches) {
      game.submitAttack(m.attackerId, "attack");
    }
    expect(game.phase).toBe(GamePhase.DEFENSE);

    for (const m of game.matches) {
      game.submitDefense(m.defenderId, "defense");
    }
    expect(game.phase).toBe(GamePhase.COMPETITION_ANALYZE);

    await game.analyzeCompetition();
    expect(judge.analyzeCompetition).toHaveBeenCalled();
    expect(game.phase).toBe(GamePhase.COMPETITION_NARRATE);

    for (const m of game.matches) {
      expect(m.winnerId).toBeTruthy();
      expect(m.lostResource).toBeTruthy();
    }
  });

  test("narrateCompetition advances round or ends game", async () => {
    const judge = makeJudgeStub();
    const game = new Game(judge);
    jest.spyOn(Math, "random").mockReturnValue(0);

    game.addPlayer("A");
    game.addPlayer("B");
    game.gameStart();
    game.setRawContext("raw");
    for (const f of game.factions) {
      game.setRawFactionConceptAndName(f.id, "c", "n");
    }
    for (const f of game.factions) {
      game.setRawFactionFlaw(f.id, "f");
    }
    await game.setupContext();

    for (const m of game.matches) game.submitAttack(m.attackerId, "a");
    for (const m of game.matches) game.submitDefense(m.defenderId, "d");
    await game.analyzeCompetition();

    const prevRound = game.round;
    await game.narrateCompetition();

    expect(judge.narrateCompetition).toHaveBeenCalled();
    expect(game.context.eventLog).toHaveLength(1);
    expect(game.round).toBe(prevRound + 1);
    expect(game.phase).toBe(GamePhase.ATTACK);

    game.phase = GamePhase.COMPETITION_NARRATE;
    game.round = 6;
    await game.narrateCompetition();
    expect(game.phase).toBe(GamePhase.END);
  });
});
