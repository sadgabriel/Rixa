export function createMatch(id) {
    return {
        id: id,
        attackerId: null,
        defenderId: null,
        attackDescription: null,
        defenseDescription: null,
        attackerTags: [],
        defenderTags: [],
        targeted_resources: [],
        protected_resources: [],
        winner: null,
        lost_resource: null,
        resourceLossApplied: null
    };
}