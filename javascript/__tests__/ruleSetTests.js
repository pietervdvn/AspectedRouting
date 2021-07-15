import { expect, jest } from '@jest/globals';
import RuleSet from '../RuleSet.js';

describe('RuleSet', () => {
    const exampleJSON = {
        "name": "real.name",
        "$default": "1",
        "value": {
            "$multiply": {
                "example": {
                    "score": "1.2"
                },
                "other": {
                    "thing": "1",
                    "something": "1.2",
                    "other_thing": "1.4"
                }
            },
            "$firstMatchOf": [
                "area",               
                "empty",
                "things"
            ],
            "value": {
                "area": {
                    "yes": "no"
                },
                "access:": {
                    "no": "no",
                    "customers": "private"
                },
                "$multiply": {
                    "access": {
                        "dismount": 0.15
                    },
                    "highway": {
                        "path": 0.5
                    }
                }
            
            },

        }
    };
    const tags = {
        "example": "score",
        "other": "something",
        "highway": "track",
        "surface": "sett",
        "cycleway": "lane"
    }
    test('it should resolve', () => {
        const ruleSet = exampleJSON;
        const { name, $default, $multiply, value} = ruleSet;

        if (!!value) {
            const currentSet = new RuleSet(name, $default, value);
            currentSet.runProgram(tags);
            expect(currentSet.runProgram).resolves;
            expect(currentSet.toString).resolves;
        } else {
            const currentSet = new RuleSet(name, $default, {$multiply});
            currentSet.toString();
            currentSet.runProgram(tags);
        }
        
    })
})
