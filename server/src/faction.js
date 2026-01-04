export function createFaction(id) {
    return {
        id: id,
        name: null,
        rawConcept: null,
        rawFlaw: null,
        resources: [],
        score: 0
    };
}