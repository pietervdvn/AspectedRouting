/**
 * Import packages
 */
import fs from 'fs';
import RuleSet from './RuleSet.js';


/**
 * Example tags
 */
const tags1 = {
    "highway": "residential", // 1 // Expect "yes"
    "surface": "paved", // 0.99
}

const tags2 = {
    "bicycle": "yes", // Expect "yes"
    "cycleway": "lane",
    "highway": "secondary",
    "maxspeed": "50",
}

const tags3 = {
    "cyclestreet": "yes",
    "highway": "residential", // Expect "yes"
    "maxspeed": "30",
    "surface": "asphalt"
}

const tags4 = {
    "highway": "track", // Expect "yes"
    "surface": "asphalt",
    "incline": "10%"
}

const tags5 = {
    "access": "no", // Expect "no"
    "bicycle": "official",
    "area": "yes"
}

const tags6 = {
    "surface":"dirt",
    "highway":"track",
    /*
    "surface":"paved",
    "highway":"path"
    */
}

/**
 * Interpret JsonFile w/ Tags
 * @param definitionFile {any} JSON input defining the score distribution
 * @param tags {any} OSM tags as key/value pairs
 */
const interpret = (definitionFile, tags) => {
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
}; 
/* 
console.info('Comfort')
interpret("./.routeExamples/bicycle.comfort.json", tags1);
interpret("./.routeExamples/bicycle.comfort.json", tags2);
interpret("./.routeExamples/bicycle.comfort.json", tags3);
interpret("./.routeExamples/bicycle.comfort.json", tags4);
console.log('*******************')

console.info('Safety')
interpret("./.routeExamples/bicycle.safety.json", tags1);
interpret("./.routeExamples/bicycle.safety.json", tags2);
interpret("./.routeExamples/bicycle.safety.json", tags3);
interpret("./.routeExamples/bicycle.safety.json", tags4);
console.log('*******************') */

/* console.info('Legal Access')
interpret("./.routeExamples/bicycle.legal_access.json", tags1);
interpret("./.routeExamples/bicycle.legal_access.json", tags2);
interpret("./.routeExamples/bicycle.legal_access.json", tags3);
interpret("./.routeExamples/bicycle.legal_access.json", tags4);
interpret("./.routeExamples/bicycle.legal_access.json", tags5); */
// console.log('*******************')

/* console.info('Speed Factor')
interpret("./.routeExamples/bicycle.speed_factor.json", tags1);
interpret("./.routeExamples/bicycle.speed_factor.json", tags2);
interpret("./.routeExamples/bicycle.speed_factor.json", tags3);
interpret("./.routeExamples/bicycle.speed_factor.json", tags4);
console.log('*******************') */

console.info('Test min')
/* interpret("./.routeExamples/bicycle.min_test.json", tags1);
interpret("./.routeExamples/bicycle.min_test.json", tags2);
interpret("./.routeExamples/bicycle.min_test.json", tags3);
interpret("./.routeExamples/bicycle.min_test.json", tags4); */
// interpret("./.routeExamples/bicycle.min_test.json", tags6);
const rawData = fs.readFileSync("./.routeExamples/bicycle.min_test.json");
const ruleSet = JSON.parse(rawData);
interpret(ruleSet, tags6);

console.log('*******************')

export default interpret;
