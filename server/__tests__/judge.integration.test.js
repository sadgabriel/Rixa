import "dotenv/config";
import { Judge } from "../judge.js";
import { jest } from "@jest/globals";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function hasKorean(text) {
  if (typeof text !== "string") return false;
  return /[가-힣]/.test(text);
}

function dumpParsed(title, obj) {
  console.log(`\n===== output_parsed (${title}) =====\n`);
  console.log(JSON.stringify(obj, null, 2));
  console.log("\n===================================\n");
}

function toContextAndFactionsFromSetup(setup) {
  const context = {
    description: setup.context?.summary ?? "",
    eventLog: [],
  };

  const factions = (setup.factions ?? []).map((f) => ({
    id: f.id,
    name: f.name,
    description: f.summary,
    resources: f.resources,
  }));

  return { context, factions };
}

function assertSameIdsInOrder({ expectedIds, actualIds }) {
  expect(Array.isArray(expectedIds)).toBe(true);
  expect(Array.isArray(actualIds)).toBe(true);
  expect(actualIds.length).toBe(expectedIds.length);
  
  for (let i = 0; i < expectedIds.length; i++) {
    expect(actualIds[i]).toBe(expectedIds[i]);
  }
  
  expect(new Set(actualIds).size).toBe(actualIds.length);
}

function loadTestScenario(filename) {
  const scenarioPath = path.join(__dirname, filename);
  const data = fs.readFileSync(scenarioPath, "utf-8");
  return JSON.parse(data);
}

describe("Judge (integration: real OpenAI call)", () => {
  jest.setTimeout(90_000);

  test("E2E: setupContext -> analyzeCompetition -> narrateCompetition (single session)", async () => {
    const scenario = loadTestScenario("test_scenario_1.json");
    
    const judge = new Judge({ useState: false, store: false });
    judge.resetSession();

    const inputContext = {
      rawContextDescription: scenario.factions[0].context,
    };

    const inputFactions = scenario.factions.map((f, index) => ({
      id: `fac_${String(index + 1).padStart(2, "0")}`,
      name: f.name,
      rawConcept: f.concept,
      rawFlaw: f.flaw,
    }));

    const setup = await judge.setupContext(inputContext, inputFactions);

    dumpParsed("context_setup", setup);

    expect(setup).toBeDefined();
    expect(setup.phase).toBe("context_setup");
    expect(typeof setup.context?.summary).toBe("string");
    expect(hasKorean(setup.context.summary)).toBe(true);

    expect(Array.isArray(setup.factions)).toBe(true);
    expect(setup.factions.length).toBe(scenario.factions.length);

    assertSameIdsInOrder({
      expectedIds: inputFactions.map((f) => f.id),
      actualIds: setup.factions.map((f) => f.id),
    });

    for (const fac of setup.factions) {
      expect(typeof fac.id).toBe("string");
      expect(typeof fac.name).toBe("string");
      expect(typeof fac.summary).toBe("string");
      expect(hasKorean(fac.summary)).toBe(true);

      expect(Array.isArray(fac.resources)).toBe(true);
      expect(fac.resources.length).toBe(3);

      for (const r of fac.resources) {
        expect(typeof r.name).toBe("string");
        expect(typeof r.description).toBe("string");
        expect(hasKorean(r.description)).toBe(true);
      }
    }

    const { context, factions } = toContextAndFactionsFromSetup(setup);
    expect(factions.length).toBe(scenario.factions.length);

    const fac1 = factions[0];
    const fac2 = factions[1];

    const fac1Res0 = fac1.resources?.[0]?.name;
    const fac2Res0 = fac2.resources?.[0]?.name;
    expect(typeof fac1Res0).toBe("string");
    expect(typeof fac2Res0).toBe("string");

    const fac1ResNames = fac1.resources.map((r) => r.name);
    const fac2ResNames = fac2.resources.map((r) => r.name);

    const matchesForAnalyze = [
      {
        id: "m_01",
        attackerId: fac1.id,
        defenderId: fac2.id,
        attackDescription: scenario.factions[0].attack,
        defenseDescription: scenario.factions[1].defense,
      },
      {
        id: "m_02",
        attackerId: fac2.id,
        defenderId: fac1.id,
        attackDescription: scenario.factions[1].attack,
        defenseDescription: scenario.factions[0].defense,
      },
    ];

    const analyzed = await judge.analyzeCompetition(context, factions, matchesForAnalyze);
    dumpParsed("competition_analyze", analyzed);

    expect(analyzed).toBeDefined();
    expect(analyzed.phase).toBe("competition_analyze");
    expect(Array.isArray(analyzed.analysis_results)).toBe(true);

    assertSameIdsInOrder({
      expectedIds: matchesForAnalyze.map((m) => m.id),
      actualIds: analyzed.analysis_results.map((r) => r.match_id),
    });

    for (let i = 0; i < matchesForAnalyze.length; i++) {
      const r = analyzed.analysis_results[i];

      expect(r).toHaveProperty("attacker.tags");
      expect(r).toHaveProperty("defender.tags");
      expect(Array.isArray(r.attacker.tags)).toBe(true);
      expect(Array.isArray(r.defender.tags)).toBe(true);

      for (const t of r.attacker.tags) expect(typeof t).toBe("string");
      for (const t of r.defender.tags) expect(typeof t).toBe("string");

      expect(Array.isArray(r.targeted_resources)).toBe(true);
      expect(Array.isArray(r.protected_resources)).toBe(true);
      expect(r.targeted_resources.length).toBeLessThanOrEqual(3);
      expect(r.protected_resources.length).toBeLessThanOrEqual(3);
    }

    const matchesForNarrate = [
      {
        id: "m_01",
        attackerId: fac1.id,
        defenderId: fac2.id,
        attackDescription: matchesForAnalyze[0].attackDescription,
        defenseDescription: matchesForAnalyze[0].defenseDescription,
        winnerId: fac1.id,
        lostResource: fac2Res0,
      },
      {
        id: "m_02",
        attackerId: fac2.id,
        defenderId: fac1.id,
        attackDescription: matchesForAnalyze[1].attackDescription,
        defenseDescription: matchesForAnalyze[1].defenseDescription,
        winnerId: fac2.id,
        lostResource: fac1Res0,
      },
    ];

    expect(fac2ResNames.includes(matchesForNarrate[0].lostResource)).toBe(true);
    expect(fac1ResNames.includes(matchesForNarrate[1].lostResource)).toBe(true);

    const narrated = await judge.narrateCompetition(context, factions, matchesForNarrate);
    dumpParsed("competition_narrate", narrated);

    expect(narrated).toBeDefined();
    expect(narrated.phase).toBe("competition_narrate");

    expect(typeof narrated.context_log).toBe("string");
    expect(hasKorean(narrated.context_log)).toBe(true);

    expect(Array.isArray(narrated.results)).toBe(true);

    assertSameIdsInOrder({
      expectedIds: matchesForNarrate.map((m) => m.id),
      actualIds: narrated.results.map((r) => r.match_id),
    });

    for (const item of narrated.results) {
      expect(typeof item.display_narrative).toBe("string");
      expect(hasKorean(item.display_narrative)).toBe(true);
      expect(item.display_narrative).not.toMatch(/\bm_\d{2}\b/);
    }
  });
});