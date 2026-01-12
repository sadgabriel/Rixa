import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";
import OpenAI from "openai";
import "dotenv/config";
import * as Errors from "./errors.js";

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
            raw_flaw: rawFlaw,
        }));

        const input = {
            phase: "context_setup",
            context: { raw_context_description: context.rawContextDescription },
            factions: factions,
        };

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", this.FORMAT_CONTEXT);

        const output = await this._callWithFormat(
            developerText,
            userText,
            JSON.parse(this.FORMAT_CONTEXT)
        );

        return output;
    }

    async analyzeCompetition(context, factions, matches) {
        const developerText = this.DEVELOPER_MESSAGE_TEMPLATE
            .replace("{{POLICY}}", this.POLICY)
            .replace("{{RULE}}", this.RULE)
            .replace("{{ROLE}}", this.ROLE_COMPETITION_ANALYZE);

        context = {
            description: context.description,
            event_log: context.eventLog,
        };

        factions = factions.map(({ id, name, description, resources }) => ({
            faction_id: id,
            name,
            description,
            resources,
        }));

        matches = matches.map(
            ({ id, attackerId, defenderId, attackDescription, defenseDescription }) => ({
                match_id: id,
                attacker_id: attackerId,
                defender_id: defenderId,
                attack_description: attackDescription,
                defense_description: defenseDescription,
            })
        );

        const input = {
            phase: "competition_analyze",
            context,
            factions,
            matches,
        };

        const schema = JSON.parse(this.FORMAT_COMPETITION_ANALYZE);
        schema.properties.analysis_results.minItems = matches.length;
        schema.properties.analysis_results.maxItems = matches.length;

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", JSON.stringify(schema));

        const output = await this._callWithFormat(
            developerText,
            userText,
            schema
        );

        return output;
    }

    async narrateCompetition(context, factions, matches) {
        const developerText = this.DEVELOPER_MESSAGE_TEMPLATE
            .replace("{{POLICY}}", this.POLICY)
            .replace("{{RULE}}", this.RULE)
            .replace("{{ROLE}}", this.ROLE_COMPETITION_NARRATE);

        context = {
            description: context.description,
            event_log: context.eventLog,
        };

        factions = factions.map(({ id, name, description, resources }) => ({
            faction_id: id,
            name,
            description,
            resources,
        }));

        matches = matches.map(
            ({
                id,
                attackerId,
                defenderId,
                attackDescription,
                defenseDescription,
                winnerId,
                lostResource,
            }) => ({
                match_id: id,
                attacker_id: attackerId,
                defender_id: defenderId,
                attack_description: attackDescription,
                defense_description: defenseDescription,
                winner_id: winnerId,
                lost_resource: lostResource,
            })
        );

        const input = {
            phase: "competition_narrate",
            context: context,
            factions: factions,
            matches: matches,
        };

        const schema = JSON.parse(this.FORMAT_COMPETITION_NARRATE);
        schema.properties.results.minItems = matches.length;
        schema.properties.results.maxItems = matches.length;

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", JSON.stringify(schema));

        const output = await this._callWithFormat(
            developerText,
            userText,
            schema
        );

        return output;
    }

    async _callWithFormat(developerText, userText, outputFormat) {
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

            request.text = {
                format: {
                    type: "json_schema",
                    name: outputFormat.title ?? "structured_output",
                    strict: true,
                    schema: outputFormat,
                },
            };

            const response = await this.client.responses.parse(request);

            this.previousResponseId = response.id ?? null;

            if (response.output_parsed != null) {
                return response.output_parsed;
            }

            const snippet = (response.output_text ?? "").slice(0, 300);
            throw new Errors.InvalidJudgeResponseError(
                `No output_parsed (parse). output_text snippet: ${JSON.stringify(snippet)}`
            );
        } catch (error) {
            if (error instanceof Errors.InvalidJudgeResponseError) {
                throw error;
            }
            throw new Errors.JudgeCallError(error.message);
        }
    }
}
