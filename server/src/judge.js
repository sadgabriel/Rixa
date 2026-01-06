import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";
import OpenAI from "openai";
import "dotenv/config";

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
        this.ROLE_CONTEXT = loadPrompt("role_context.txt");
        this.ROLE_COMPETITION_ANALYZE = loadPrompt("role_competition_analyze.txt");
        this.ROLE_COMPETITION_NARRATE = loadPrompt("role_competition_narrate.txt");
        this.USER_MESSAGE_TEMPLATE = loadPrompt("user_message_template.txt");
        this.DEVELOPER_MESSAGE_TEMPLATE = loadPrompt("developer_message_template.txt");
        this.FORMAT_CONTEXT = loadPrompt("format_context.json");
        this.FORMAT_COMPETITION_ANALYZE = loadPrompt("format_competition_analyze.json");
        this.FORMAT_COMPETITION_NARRATE = loadPrompt("format_competition_narrate.json");
    }

    resetSession() {
        this.previousResponseId = null;
    }

    async evaluateSetting(rawContextDescription, factions) {
        const developerText = this.DEVELOPER_MESSAGE_TEMPLATE
            .replace("{{POLICY}}", this.POLICY)
            .replace("{{RULE}}", this.RULE)
            .replace("{{ROLE}}", this.ROLE_CONTEXT);

        factions = factions.map(faction => {
            return {
                id: faction.id,
                name: faction.name,
                concept: faction.rawConcept,
                flaw: faction.rawFlaw
            }
        });

        const input = {
            phase: "context_setup",
            context_draft: rawContextDescription,
            factions: factions
        };

        const userText = this.USER_MESSAGE_TEMPLATE
            .replace("{{INPUT}}", JSON.stringify(input))
            .replace("{{FORMAT}}", this.FORMAT_CONTEXT);

        const responseText = await this._call(developerText, userText, JSON.parse(this.FORMAT_CONTEXT));
        return this._parseJson(responseText);
    }

    async _call(developerText, userText, outputFormat = null) {
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

        if (outputFormat !== null) {
            request.output_format = {
                type: "json_schema",
                schema: outputFormat,
                strict: true
            }
        }

        const response = await this.client.responses.create(request);

        this.previousResponseId = response.id ?? null;
        return response.output_text ?? "";
    }

    _extractFirstJsonObject(text) {
        const start = text.indexOf("{");
        const end = text.lastIndexOf("}");
        if (start === -1 || end === -1 || end <= start) {
            throw new Error("No JSON object found in model output.");
        }
        return text.slice(start, end + 1);
        }

    _parseJson(text) {
        const jsonText = this._extractFirstJsonObject(text);
        return JSON.parse(jsonText);
    }
}