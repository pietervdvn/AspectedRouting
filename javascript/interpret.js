/**
 * Import packages
 */
import fs from 'fs';
import RuleSet from './RuleSet.js';


/**
 * Example tags
 */
const tags1 = {
    "highway": "residential", // 1
    "surface": "paved", // 0.99
}

const tags2 = {
    "bicycle": "yes",
    "cycleway": "lane",
    "highway": "secondary",
    "maxspeed": "50",
}

const tags3 = {
    "cyclestreet": "yes",
    "highway": "residential",
    "maxspeed": "30",
    "surface": "asphalt"
}

const tags4 = {
    "highway": "track",
    "surface": "asphalt",
    "incline": "10%"
}

const tags5 = {
    "access": "no",
    "bicycle": "official",
    "area": "yes"
}

/**
 * Interpret JsonFile w/ Tags
 * @param definitionFile {any} JSON input defining the score distribution
 * @param tags {any} OSM tags as key/value pairs
 */
const interpret = (definitionFile, tags) => {
    const rawData = fs.readFileSync(definitionFile);
    const ruleSet = JSON.parse(rawData);

    const { name, $default, $multiply, value} = ruleSet;

    if (!!value) {
        const currentSet = new RuleSet(name, $default, value);
        console.log(currentSet.runProgram(tags));
    } else {
        const currentSet = new RuleSet(name, $default, {$multiply});
        // currentSet.toString();
        console.log(currentSet.runProgram(tags));
    }
}; 
/* 
console.info('Comfort')
interpret("../Examples/bicycle/aspects/bicycle.comfort.json", tags1);
interpret("../Examples/bicycle/aspects/bicycle.comfort.json", tags2);
interpret("../Examples/bicycle/aspects/bicycle.comfort.json", tags3);
interpret("../Examples/bicycle/aspects/bicycle.comfort.json", tags4);
console.log('*******************')

console.info('Safety')
interpret("../Examples/bicycle/aspects/bicycle.safety.json", tags1);
interpret("../Examples/bicycle/aspects/bicycle.safety.json", tags2);
interpret("../Examples/bicycle/aspects/bicycle.safety.json", tags3);
interpret("../Examples/bicycle/aspects/bicycle.safety.json", tags4);
console.log('*******************') */

console.info('Legal Access')
interpret("../Examples/bicycle/aspects/bicycle.legal_access.json", tags1);
interpret("../Examples/bicycle/aspects/bicycle.legal_access.json", tags2);
interpret("../Examples/bicycle/aspects/bicycle.legal_access.json", tags3);
interpret("../Examples/bicycle/aspects/bicycle.legal_access.json", tags4);
interpret("../Examples/bicycle/aspects/bicycle.legal_access.json", tags5);

// !TODO: Add default value = "no" as a fallback. Fix logic

console.log('*******************')

/* console.info('Speed Factor')
interpret("./.routeExamples/bicycle.speed_factor.json", tags1);
interpret("./.routeExamples/bicycle.speed_factor.json", tags2);
interpret("./.routeExamples/bicycle.speed_factor.json", tags3);
interpret("./.routeExamples/bicycle.speed_factor.json", tags4);
console.log('*******************') */

export default interpret;
