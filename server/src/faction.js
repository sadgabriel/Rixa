export function createFaction(id) {
    return {
        id: id,
        name: null,
        rawConcept: null,
        rawFlaw: null,
        summary: null,
        log: null,
        resources: [],
        score: 0
    };
}