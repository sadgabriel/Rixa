import OpenAI from "openai";
import "dotenv/config";

export class LlmJudge {
  constructor() {
    // 1️⃣ LLM 클라이언트 내부 생성
    // API 키는 환경변수에서 가져오는 게 Node.js 관례입니다.
    this.client = new OpenAI({
      apiKey: process.env.OPENAI_API_KEY,
    });

    // 2️⃣ 사용할 모델 (지금은 하드코딩)
    this.model = "gpt-5-mini";

    // 3️⃣ 연속 응답을 위한 최소 상태
    this.previousResponseId = null;
  }

  /**
   * 세션 초기화
   * - 이전 응답과의 연결을 끊고 새 판정을 시작
   */
  resetSession() {
    this.previousResponseId = null;
  }

  /**
   * 세계관 / 진영 확정 + 진영별 자원 3종 생성
   * @param {object} input
   * @returns {Promise<object>}
   */
  async finalizeSetup(input) {
    const system = `
You are an impartial game judge.
Return ONLY valid JSON. No explanations.
`.trim();

    const user = JSON.stringify({
      task: "finalize_setup",
      input,
    });

    const response = await this._call(system, user);
    return this._parseJson(response);
  }

  /**
   * 전투 판정
   * @param {object} input
   * @returns {Promise<object>}
   */
  async judgeBattle(input) {
    const system = `
You are an impartial game judge.
Use the provided setting and logs as canon.
Return ONLY valid JSON.
`.trim();

    const user = JSON.stringify({
      task: "judge_battle",
      input,
    });

    const response = await this._call(system, user);
    return this._parseJson(response);
  }

  // =========================
  // 내부 전용 함수들
  // =========================

  async _call(systemText, userText) {
    const request = {
      model: this.model,
      store: true,
      input: [
        { role: "system", content: systemText },
        { role: "user", content: userText },
      ],
    };

    // 이전 응답이 있으면 연결
    if (this.previousResponseId !== null) {
      request.previous_response_id = this.previousResponseId;
    }

    const response = await this.client.responses.create(request);

    // 다음 호출을 위해 ID만 저장
    this.previousResponseId = response.id ?? null;

    return response.output_text ?? "";
  }

  _parseJson(text) {
    // 지금 단계에서는 실패하면 바로 터지는 게 낫습니다
    return JSON.parse(text);
  }
}
