import "dotenv/config";
import { Judge } from "../judge.js";
import * as Errors from "../errors.js";

function makeJudgeWithStubbedResponse(stubResponseFactory) {
  const judge = new Judge();

  judge.client = {
    responses: {
      parse: async (request) => stubResponseFactory(request),
    },
  };

  return judge;
}

describe("Judge (stubbed OpenAI)", () => {
  test("setupContext: sends json_schema format + returns output_parsed", async () => {
    const judge = makeJudgeWithStubbedResponse((request) => {
      expect(request).toHaveProperty("text.format.type", "json_schema");
      expect(request).toHaveProperty("text.format.strict", true);
      expect(request).toHaveProperty("text.format.schema");
      expect(request).toHaveProperty("text.format.name"); // 중요: 400 방지

      return {
        id: "resp_test_1",
        output_parsed: {
          phase: "context_setup",
          context: {
            summary:
              "This is a sufficiently long summary to satisfy minLength constraints.",
          },
          factions: [
            {
              id: "fac_01",
              name: "Alpha",
              summary: "Alpha is a faction description long enough.",
              resources: [
                { name: "ResA", description: "Desc A long enough." },
                { name: "ResB", description: "Desc B long enough." },
                { name: "ResC", description: "Desc C long enough." },
              ],
            },
            {
              id: "fac_02",
              name: "Beta",
              summary: "Beta is a faction description long enough.",
              resources: [
                { name: "ResD", description: "Desc D long enough." },
                { name: "ResE", description: "Desc E long enough." },
                { name: "ResF", description: "Desc F long enough." },
              ],
            },
          ],
        },
      };
    });

    const result = await judge.setupContext(
      { rawContextDescription: "raw context..." },
      [
        { id: "fac_01", name: "Alpha", rawConcept: "c1", rawFlaw: "f1" },
        { id: "fac_02", name: "Beta", rawConcept: "c2", rawFlaw: "f2" },
      ]
    );

    expect(result.phase).toBe("context_setup");
    expect(result.context.summary.length).toBeGreaterThanOrEqual(40);
    expect(result.factions).toHaveLength(2);
    expect(result.factions[0].id).toBe("fac_01");
  });

  test("analyzeCompetition: returns output_parsed analysis_results per match", async () => {
    const judge = makeJudgeWithStubbedResponse((request) => {
      expect(request).toHaveProperty("text.format.type", "json_schema");
      expect(request).toHaveProperty("text.format.strict", true);
      expect(request).toHaveProperty("text.format.schema");
      expect(request).toHaveProperty("text.format.name");

      return {
        id: "resp_test_2",
        output_parsed: {
          phase: "competition_analyze",
          analysis_results: [
            {
              match_id: "m_01",
              attacker: { tags: ["coherence"] },
              defender: { tags: ["pressure"] },
              targeted_resources: ["ResD"],
              protected_resources: ["ResD"],
            },
            {
              match_id: "m_02",
              attacker: { tags: ["creativity"] },
              defender: { tags: [] },
              targeted_resources: [],
              protected_resources: [],
            },
          ],
        },
      };
    });

    const result = await judge.analyzeCompetition(
      { description: "ctx", eventLog: "" },
      [
        { id: "fac_01", name: "Alpha", description: "A", resources: [{ name: "ResA" }] },
        { id: "fac_02", name: "Beta", description: "B", resources: [{ name: "ResD" }] },
      ],
      [
        {
          id: "m_01",
          attackerId: "fac_01",
          defenderId: "fac_02",
          attackDescription: "atk1",
          defenseDescription: "def1",
        },
        {
          id: "m_02",
          attackerId: "fac_02",
          defenderId: "fac_01",
          attackDescription: "atk2",
          defenseDescription: "def2",
        },
      ]
    );

    expect(result.phase).toBe("competition_analyze");
    expect(result.analysis_results).toHaveLength(2);
    expect(result.analysis_results[0].match_id).toBe("m_01");
  });

  test("narrateCompetition: returns output_parsed results + context_log", async () => {
    const judge = makeJudgeWithStubbedResponse((request) => {
      expect(request).toHaveProperty("text.format.type", "json_schema");
      expect(request).toHaveProperty("text.format.strict", true);
      expect(request).toHaveProperty("text.format.schema");
      expect(request).toHaveProperty("text.format.name");

      return {
        id: "resp_test_3",
        output_parsed: {
          phase: "competition_narrate",
          results: [
            {
              match_id: "m_01",
              display_narrative:
                "A narrative that is comfortably longer than 120 characters. "
                  .repeat(4)
                  .trim(),
            },
          ],
          context_log: "Context log long enough to satisfy constraints.",
        },
      };
    });

    const result = await judge.narrateCompetition(
      { description: "ctx", eventLog: "" },
      [
        { id: "fac_01", name: "Alpha", description: "A", resources: [{ name: "ResA" }] },
        { id: "fac_02", name: "Beta", description: "B", resources: [{ name: "ResD" }] },
      ],
      [
        {
          id: "m_01",
          attackerId: "fac_01",
          defenderId: "fac_02",
          attackDescription: "atk",
          defenseDescription: "def",
          winnerId: "fac_01",
          lostResource: "ResD",
        },
      ]
    );

    expect(result.phase).toBe("competition_narrate");
    expect(result.results).toHaveLength(1);
    expect(typeof result.context_log).toBe("string");
  });

  test("throws InvalidJudgeResponseError when output_parsed is missing", async () => {
    const judge = makeJudgeWithStubbedResponse(() => ({
      id: "resp_no_parsed",
      output_parsed: null,
      output_text: "Some text that did not get parsed.",
    }));

    await expect(
      judge.setupContext(
        { rawContextDescription: "raw..." },
        [
          { id: "fac_01", name: "A", rawConcept: "c1", rawFlaw: "f1" },
          { id: "fac_02", name: "B", rawConcept: "c2", rawFlaw: "f2" },
        ]
      )
    ).rejects.toBeInstanceOf(Errors.InvalidJudgeResponseError);
  });

  test("wraps OpenAI errors into JudgeCallError", async () => {
    const judge = makeJudgeWithStubbedResponse(() => {
      throw new Error("boom");
    });

    await expect(
      judge.analyzeCompetition(
        { description: "ctx", eventLog: "" },
        [
          { id: "fac_01", name: "Alpha", description: "A", resources: [{ name: "ResA" }] },
          { id: "fac_02", name: "Beta", description: "B", resources: [{ name: "ResD" }] },
        ],
        [
          {
            id: "m_01",
            attackerId: "fac_01",
            defenderId: "fac_02",
            attackDescription: "atk1",
            defenseDescription: "def1",
          },
        ]
      )
    ).rejects.toBeInstanceOf(Errors.JudgeCallError);
  });

  test("previous_response_id is chained after first call, and resetSession clears it", async () => {
    const requests = [];

    const judge = makeJudgeWithStubbedResponse((request) => {
      requests.push(request);

      if (requests.length === 1) {
        expect(request).not.toHaveProperty("previous_response_id");
        return {
          id: "resp_chain_1",
          output_parsed: {
            phase: "context_setup",
            context: {
              summary:
                "This is a sufficiently long summary to satisfy minLength constraints.",
            },
            factions: [
              {
                id: "fac_01",
                name: "Alpha",
                summary: "Alpha summary long enough.",
                resources: [
                  { name: "ResA", description: "Desc A long enough." },
                  { name: "ResB", description: "Desc B long enough." },
                  { name: "ResC", description: "Desc C long enough." },
                ],
              },
              {
                id: "fac_02",
                name: "Beta",
                summary: "Beta summary long enough.",
                resources: [
                  { name: "ResD", description: "Desc D long enough." },
                  { name: "ResE", description: "Desc E long enough." },
                  { name: "ResF", description: "Desc F long enough." },
                ],
              },
            ],
          },
        };
      }

      if (requests.length === 2) {
        expect(request).toHaveProperty("previous_response_id", "resp_chain_1");
        return {
          id: "resp_chain_2",
          output_parsed: {
            phase: "competition_analyze",
            analysis_results: [],
          },
        };
      }

      if (requests.length === 3) {
        expect(request).not.toHaveProperty("previous_response_id");
        return {
          id: "resp_chain_3",
          output_parsed: {
            phase: "competition_analyze",
            analysis_results: [],
          },
        };
      }

      throw new Error("Unexpected extra call");
    });

    await judge.setupContext(
      { rawContextDescription: "raw context..." },
      [
        { id: "fac_01", name: "Alpha", rawConcept: "c1", rawFlaw: "f1" },
        { id: "fac_02", name: "Beta", rawConcept: "c2", rawFlaw: "f2" },
      ]
    );

    await judge.analyzeCompetition(
      { description: "ctx", eventLog: "" },
      [
        { id: "fac_01", name: "Alpha", description: "A", resources: [{ name: "ResA" }] },
        { id: "fac_02", name: "Beta", description: "B", resources: [{ name: "ResD" }] },
      ],
      [
        {
          id: "m_01",
          attackerId: "fac_01",
          defenderId: "fac_02",
          attackDescription: "atk1",
          defenseDescription: "def1",
        },
      ]
    );

    judge.resetSession();

    await judge.analyzeCompetition(
      { description: "ctx", eventLog: "" },
      [
        { id: "fac_01", name: "Alpha", description: "A", resources: [{ name: "ResA" }] },
        { id: "fac_02", name: "Beta", description: "B", resources: [{ name: "ResD" }] },
      ],
      [
        {
          id: "m_02",
          attackerId: "fac_02",
          defenderId: "fac_01",
          attackDescription: "atk2",
          defenseDescription: "def2",
        },
      ]
    );

    expect(requests).toHaveLength(3);
  });
});
