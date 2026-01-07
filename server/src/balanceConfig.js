export const BALANCE = Object.freeze({
  baseWinChance: 0.50,

  tagWeights: {
    // positive tags
    coherence: 0.05,
    evidence_and_reasoning: 0.05,
    situational_leverage: 0.05,
    anticipation_and_counterplay: 0.05,
    creativity: 0.05,

    deception: 0.05,
    pressure: 0.05,
    escalation: 0.05,
    high_variance: 0.05,

    // negative tags
    nonresponsive: -0.05,
    unsupported_or_logical_gap: -0.05,
    self_contradiction_or_overreach: -0.05,
  },

  minWinChance: 0.20,
  maxWinChance: 0.80,

  resourceLoss: {
    baseWeight: 1,
    targetedBonusWeight: 1,
    protectedPenaltyWeight: 1,
    minWeight: 0.25,
    scorePenaltyIfResourceEmpty: 1,
  },
});
