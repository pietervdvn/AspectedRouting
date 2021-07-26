/**
 * Import packages
 */
import fs from 'fs';
import {argv} from 'process';
import RuleSet from './RuleSet.js';

const app = {
    init() {
        if (!!argv && argv.length < 4) {
            console.info(`Invalid command. In order to run the JavaScript interpreter please use the following format:
            > node index.js [ruleset JSON file] [tags]`)
        } else if (!!argv && argv.length === 4) {
            const definitionFile = argv[2];
            const tags = argv[3];
            const result = this.interpret(definitionFile, tags);
            console.log(result)
        }
    },
    /**
     * Interpret JsonFile and apply it as a tag. To use with CLI only
     * @param definitionFile {any} JSON input defining the score distribution
     * @param tags {any} OSM tags as key/value pairs
     */
    interpret(definitionFile, tags) {
        if (typeof tags === "string") {
            tags = JSON.parse(tags)
        }
        const rawData = fs.readFileSync(definitionFile);
        const program = JSON.parse(rawData);
        return new RuleSet(program).runProgram(tags)
    }
};

app.init();

export default app;
