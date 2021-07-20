/**
 * Import packages
 */
import fs from 'fs';
import { argv } from 'process';
import RuleSet from './RuleSet.js';

const app = {
    init () {
        if (!!argv && argv.length < 4) {
            console.info(`Invalid command. In order to run the JavaScript interpreter please use the following format:
            > node index.js [ruleset JSON file] [tags]`)
        } else if (!!argv && argv.length === 4) {
            const definitionFile = argv[2];
            const tags = argv[3];
            this.interpret(definitionFile, tags);
        }
    },
    /**
     * Interpret JsonFile w/ Tags
     * @param definitionFile {any} JSON input defining the score distribution
     * @param tags {any} OSM tags as key/value pairs
     */
    interpret (definitionFile, tags) {
        const rawData = fs.readFileSync(definitionFile);
        const ruleSet = JSON.parse(rawData);
        const { name, $default, $multiply, $min, value} = ruleSet;

        if (!!value) {
            const currentSet = new RuleSet(name, $default, value);
            console.log(currentSet.runProgram(tags));
        } else if (!!$multiply) {
            const currentSet = new RuleSet(name, $default, {$multiply});
            console.log(currentSet.runProgram(tags));
        } else if (!!$min) {
            const currentSet = new RuleSet(name, $default, {$min});
            console.log(currentSet.runProgram(tags));
        }
    } 
};

app.init();

export default app;
