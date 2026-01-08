export function createMatch(id) {
    return {
        id: id,
        attackerId: null,
        defenderId: null,
        attackDescription: null,
        defenseDescription: null,
        attackerTags: [],
        defenderTags: [],
        targetedResources: [],
        protectedResources: [],
        winnerId: null,
        lostResource: null,
        displayNarrative: null
    };
}