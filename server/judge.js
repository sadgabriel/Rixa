import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";
import OpenAI from "openai";
import "dotenv/config";
import * as Errors from './errors.js';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

function loadPrompt(filename) {
    const promptPath = path.join(__dirname, "prompts", filename);
    return fs.readFileSync(promptPath, "utf-8");
}

export class Judge {
    constructor() {
        this.client = new OpenAI({
            apiKey: process.env.OPENAI_API_KEY,
        });

        this.model = "gpt-5-mini";
        this.previousResponseId = null;

        this.POLICY = loadPrompt("policy.txt");
        this.RULE = loadPrompt("rule.txt");
        this.ROLE_CONTEXT = loadPrompt("role-context.txt");
        this.ROLE_COMPETITION_ANALYZE = loadPrompt("role-competition-analyze.txt");
        this.ROLE_COMPETITION_NARRATE = loadPrompt("role-competition-narrate.txt");
        this.USER_MESSAGE_TEMPLATE = loadPrompt("user-message-template.txt");
        this.DEVELOPER_MESSAGE_TEMPLATE = loadPrompt("developer-message-template.txt");
        this.FORMAT_CONTEXT = loadPrompt("format-context.json");
        this.FORMAT_COMPETITION_ANALYZE = loadPrompt("format-competition-analyze.json");
        this.FORMAT_COMPETITION_NARRATE = loadPrompt("format-competition-narrate.json");
    }

    resetSession() {
        this.previousResponseId = null;
    }

    async setupContext(context, factions) {
        this.resetSession();

        const developerText = this.DEVELOPER_MESSAGE_TEMPLATE
            .replace("{{POLICY}}", this.POLICY)
            .replace("{{RULE}}", this.RULE)
            .replace("{{ROLE}}", this.ROLE_CONTEXT);

        factions = factions.map(({ id, name, rawConcept, rawFlaw }) => ({
            faction_id: id,
            name,
            raw_concept: rawConcept,
            raw_flaw: rawFlaw
        }));

        const input = {
            phase: "context_setup",
            context: { raw_context_description: context.rawContextDescription },
            factions: factions
        };

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", this.FORMAT_CONTEXT);

        const responseText = await this._call(developerText, userText, JSON.parse(this.FORMAT_CONTEXT));
        return this._parseJson(responseText);
    }

    async analyzeCompetition(context, factions, matches) {
        const developerText = this.DEVELOPER_MESSAGE_TEMPLATE
            .replace("{{POLICY}}", this.POLICY)
            .replace("{{RULE}}", this.RULE)
            .replace("{{ROLE}}", this.ROLE_COMPETITION_ANALYZE);

        
        context = {
            description: context.description,
            event_log: context.eventLog
        }

        factions = factions.map(({ id, name, description, resources }) => ({
            faction_id: id,
            name,
            description,
            resources
        }));

        matches = matches.map(({id, attackerId, defenderId, attackDescription, defenseDescription}) => ({
            match_id: id,
            attacker_id: attackerId,
            defender_id: defenderId,
            attack_description: attackDescription,
            defense_description: defenseDescription
        }));

        const input = {
            phase: "competition_analyze",
            context,
            factions,
            matches
        };

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", this.FORMAT_COMPETITION_ANALYZE);

        const responseText = await this._call(developerText, userText, JSON.parse(this.FORMAT_COMPETITION_ANALYZE));
        return this._parseJson(responseText);
    }

    async narrateCompetition(context, factions, matches) {
        const developerText = this.DEVELOPER_MESSAGE_TEMPLATE
            .replace("{{POLICY}}", this.POLICY)
            .replace("{{RULE}}", this.RULE)
            .replace("{{ROLE}}", this.ROLE_COMPETITION_NARRATE);

        context = {
            description: context.description,
            event_log: context.eventLog
        }

        factions = factions.map(({ id, name, description, resources }) => ({
            faction_id: id,
            name,
            description,
            resources
        }));

        matches = matches.map(({id, attackerId, defenderId, attackDescription, defenseDescription, winnerId, lostResource}) => ({
            match_id: id,
            attacker_id: attackerId,
            defender_id: defenderId,
            attack_description: attackDescription,
            defense_description: defenseDescription,
            winner_id: winnerId,
            lost_resource: lostResource
        }));

        const input = {
            phase: "competition_narrate",
            context: context,
            factions: factions,
            matches: matches
        };

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", this.FORMAT_COMPETITION_NARRATE);

        const responseText = await this._call(developerText, userText, JSON.parse(this.FORMAT_COMPETITION_NARRATE));
        return this._parseJson(responseText);
    }

    async _call(developerText, userText, outputFormat = null) {
        try {
            const request = {
                model: this.model,
                reasoning: { effort: "low" },
                store: true,
                input: [
                    { role: "developer", content: developerText },
                    { role: "user", content: userText },
                ],
            };

            if (this.previousResponseId !== null) {
                request.previous_response_id = this.previousResponseId;
            }

            if (outputFormat) {
                request.text = {
                    format: {
                    type: "json_schema",
                    strict: true,
                    schema: outputFormat,
                    },
                };
            }

            const response = await this.client.responses.create(request);

            this.previousResponseId = response.id ?? null;
            return response.output_text ?? "";
        } catch (error) {
            throw new Errors.JudgeCallError(error.message);
        }
    }

    _extractFirstJsonObject(text) {
        const start = text.indexOf("{");
        const end = text.lastIndexOf("}");
        if (start === -1 || end === -1 || end <= start) {
            throw new Errors.InvalidJudgeResponseError("No valid JSON object found in the response.");
        }
        return text.slice(start, end + 1);
    }

    _parseJson(text) {
        try {
            const jsonText = this._extractFirstJsonObject(text);
            return JSON.parse(jsonText);
        } catch (error) {
            if (error instanceof Errors.InvalidJudgeResponseError) {
                throw error;
            } else {
                throw new Errors.InvalidJudgeResponseError(error.message);
            }
        }
    }
}