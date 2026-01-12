import "dotenv/config";
import { Judge } from "../judge.js";
import { jest } from "@jest/globals";

const shouldRun =
  process.env.RUN_INTEGRATION === "1" ||
  process.env.RUN_INTEGRATION === "true" ||
  process.env.RUN_INTEGRATION === "yes";

const hasKey =
  typeof process.env.OPENAI_API_KEY === "string" &&
  process.env.OPENAI_API_KEY.length > 0;

const describeIntegration = shouldRun && hasKey ? describe : describe.skip;

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
    eventLog:
      "사건 경과: 1) 소송 제기 2) 증거 제출 3) 변론 종결 4) 판결 선고를 앞둔 상태.",
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

  // 1) 길이 동일
  expect(actualIds.length).toBe(expectedIds.length);

  // 2) 순서 동일
  for (let i = 0; i < expectedIds.length; i++) {
    expect(actualIds[i]).toBe(expectedIds[i]);
  }

  // 3) 중복 없음(방어적)
  expect(new Set(actualIds).size).toBe(actualIds.length);
}

describeIntegration("Judge (integration: real OpenAI call)", () => {
  jest.setTimeout(90_000);

  test("E2E: setupContext -> analyzeCompetition -> narrateCompetition (single session)", async () => {
    const judge = new Judge();
    judge.resetSession();

    // 1) setupContext
    const setup = await judge.setupContext(
      {
        rawContextDescription:
          "우주 법원에서 '민트초코 마라탕'의 레시피 저작권을 두고 초대형 분쟁이 터졌다. 해당 재판에서 패소한 쪽은 막대한 손해배상금을 지불해야 한다.",
      },
      [
        {
          id: "fac_01",
          name: "민초사랑단",
          rawConcept:
            "코에서 민트초코가 나오는 코끼리형 종족. 전 우주 민초 생산량의 99%를 책임지고 있는 종족이며 그 민초는 매우 맛있다고 한다. 특히 그들 행성에서 자생하는 마라탕후루 거북으로부터 얻은 마라탕과 합쳐 만든 민트초코 마라탕은 모두가 좋아하는 전통 음식이라 할 수 있다.",
          rawFlaw:
            "안 씻고 민초를 만드는 비위생적인 코끼리들 문제가 전 우주에서 사랑받은 고발 방송 '그것이 알고싶다'에서 다뤄졌고 대중들로부터 인식이 크게 나빠졌다.",
        },
        {
          id: "fac_02",
          name: "더본스페이스",
          rawConcept:
            "우주 요식업계의 대부 빽종원이 대표로 있는 대기업. 수많은 프랜차이즈들을 산하에 두고 있으며 최근에 민트초코 마라탕을 주력으로 하는 새로운 프랜차이즈 '원조 할매 마라탕집'을 만들어 큰 성공을 했다.",
          rawFlaw:
            "대표 빽종원이 뭐만 하면 남이 만든 걸 보고 '아 사실 그것도 제가 만든 거거든유~'하며 저작권을 뺏으려 한다는 인식이 사람들 사이에 존재한다.",
        },
      ]
    );

    dumpParsed("context_setup", setup);

    expect(setup).toBeDefined();
    expect(setup.phase).toBe("context_setup");
    expect(typeof setup.context?.summary).toBe("string");
    expect(setup.context.summary.length).toBeGreaterThanOrEqual(40);
    expect(hasKorean(setup.context.summary)).toBe(true);

    expect(Array.isArray(setup.factions)).toBe(true);
    expect(setup.factions.length).toBe(2);

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
    expect(factions.length).toBe(2);

    const fac1 = factions[0];
    const fac2 = factions[1];

    const fac1Res0 = fac1.resources?.[0]?.name;
    const fac2Res0 = fac2.resources?.[0]?.name;
    expect(typeof fac1Res0).toBe("string");
    expect(typeof fac2Res0).toBe("string");

    const fac1ResNames = fac1.resources.map((r) => r.name);
    const fac2ResNames = fac2.resources.map((r) => r.name);

    // 2) analyzeCompetition (2 matches)
    const matchesForAnalyze = [
      {
        id: "m_01",
        attackerId: fac1.id,
        defenderId: fac2.id,
        attackDescription: `상대의 핵심 자원(${fac2Res0})을 저작권 침해 증거로 정조준하며, 배심원 설득 자료를 대량 제출한다.`,
        defenseDescription: `원조성 주장과 상표권/관행을 근거로 반박하고, 자원(${fac2Res0}) 보호를 위해 증인 진술을 강화한다.`,
      },
      {
        id: "m_02",
        attackerId: fac2.id,
        defenderId: fac1.id,
        attackDescription: `민초사랑단의 위생 논란을 부각해 신뢰를 무너뜨리고, 자원(${fac1Res0})을 흔든다.`,
        defenseDescription: `위생 개선 기록과 제3자 인증을 제시하며 반박하고, 자원(${fac1Res0}) 방어에 집중한다.`,
      },
    ];

    const analyzed = await judge.analyzeCompetition(context, factions, matchesForAnalyze);
    dumpParsed("competition_analyze", analyzed);

    expect(analyzed).toBeDefined();
    expect(analyzed.phase).toBe("competition_analyze");
    expect(Array.isArray(analyzed.analysis_results)).toBe(true);

    // 입력과 동일한 개수/순서/중복 방지
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

      // 태그는 enum 중 하나여야 함(스키마가 보장하지만, 방어적으로 체크)
      for (const t of r.attacker.tags) expect(typeof t).toBe("string");
      for (const t of r.defender.tags) expect(typeof t).toBe("string");

      expect(Array.isArray(r.targeted_resources)).toBe(true);
      expect(Array.isArray(r.protected_resources)).toBe(true);
      expect(r.targeted_resources.length).toBeLessThanOrEqual(3);
      expect(r.protected_resources.length).toBeLessThanOrEqual(3);
    }

    // 3) narrateCompetition (2 matches: 누락 방지 검증)
    const matchesForNarrate = [
      {
        id: "m_01",
        attackerId: fac1.id,
        defenderId: fac2.id,
        attackDescription: matchesForAnalyze[0].attackDescription,
        defenseDescription: matchesForAnalyze[0].defenseDescription,
        winnerId: fac1.id,
        // 방어자(fac2)의 자원 중 하나만 허용
        lostResource: fac2Res0,
      },
      {
        id: "m_02",
        attackerId: fac2.id,
        defenderId: fac1.id,
        attackDescription: matchesForAnalyze[1].attackDescription,
        defenseDescription: matchesForAnalyze[1].defenseDescription,
        winnerId: fac2.id,
        // 방어자(fac1)의 자원 중 하나만 허용
        lostResource: fac1Res0,
      },
    ];

    // 입력이 말이 되는지 사전 검증(테스트 실수 방지)
    expect(fac2ResNames.includes(matchesForNarrate[0].lostResource)).toBe(true);
    expect(fac1ResNames.includes(matchesForNarrate[1].lostResource)).toBe(true);

    const narrated = await judge.narrateCompetition(context, factions, matchesForNarrate);
    dumpParsed("competition_narrate", narrated);

    expect(narrated).toBeDefined();
    expect(narrated.phase).toBe("competition_narrate");

    expect(typeof narrated.context_log).toBe("string");
    expect(narrated.context_log.length).toBeGreaterThanOrEqual(40);
    expect(hasKorean(narrated.context_log)).toBe(true);

    // context_log에 match_id 같은 내부 식별자 금지 (예: m_01)
    expect(narrated.context_log).not.toMatch(/\bm_\d{2}\b/);

    expect(Array.isArray(narrated.results)).toBe(true);

    // 입력과 동일한 개수/순서/중복 방지
    assertSameIdsInOrder({
      expectedIds: matchesForNarrate.map((m) => m.id),
      actualIds: narrated.results.map((r) => r.match_id),
    });

    for (const item of narrated.results) {
      expect(typeof item.display_narrative).toBe("string");
      expect(item.display_narrative.length).toBeGreaterThanOrEqual(120);
      expect(hasKorean(item.display_narrative)).toBe(true);
      expect(item.display_narrative).not.toMatch(/\bm_\d{2}\b/);
    }
  });
});
