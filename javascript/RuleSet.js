/**
 * RuleSet Class
 * Constructor
 * @param name {string} Name of RuleSet
 * @param defaultValue {number} Default score value
 * @param values {object} Main data object
 */
class RuleSet {
    constructor(config) {
        delete config["name"]
        delete config["unit"]
        delete config["description"]
        this.program = config
    }


    /**
     * getScore calculates a score for the RuleSet
     * @param tags {object} Active tags to compare against
     */
    runProgram(tags, program = this.program) {
        if (typeof program !== "object") {
            return program;
        }

        let functionName /*: string*/ = undefined;
        let functionArguments /*: any */ = undefined
        let otherValues = {}
        Object.entries(program).forEach(
            entry => {
                const [key, value] = entry
                if (key.startsWith("$")) {
                    functionName = key
                    functionArguments = value
                } else {
                    otherValues[key] = value
                }
            }
        )

        if (functionName === undefined) {
            return this.interpretAsDictionary(program, tags)
        }

        if (functionName === '$multiply') {
            return this.multiplyScore(tags, functionArguments);
        } else if (functionName === '$firstMatchOf') {
            this.order = keys;
            return this.getFirstMatchScore(tags);
        } else if (functionName === '$min') {
           return this.getMinValue(tags, functionArguments);
        } else if (functionName === '$max') {
            return this.getMaxValue(tags, functionArguments);
        } else if (functionName === '$default') {
            return this.defaultV(functionArguments, otherValues, tags)
        } else {
            console.error(`Error: Program ${functionName} is not implemented yet. ${JSON.stringify(program)}`);
        }
    }

    /**
     * Given a 'program' without function invocation, interprets it as a dictionary
     *
     * E.g., given the program
     *
     * {
     *     highway: {
     *         residential: 30,
     *         living_street: 20
     *     },
     *     surface: {
     *         sett : 0.9
     *     }
     *     
     * }
     *
     * in combination with the tags {highway: residential},
     *
     * the result should be [30, undefined];
     *
     * For the tags {highway: residential, surface: sett} we should get [30, 0.9]
     *
     *
     * @param program
     * @param tags
     * @return {(undefined|*)[]}
     */
    interpretAsDictionary(program, tags) {
        return Object.entries(tags).map(tag => {
            const [key, value] = tag;
            const propertyValue = program[key]
            if (propertyValue === undefined) {
                return undefined
            }
            if (typeof propertyValue !== "object") {
                return propertyValue
            }
            return propertyValue[value]
        });
    }

    defaultV(subProgram, otherArgs, tags) {
        const normalProgram = Object.entries(otherArgs)[0][1]
        const value = this.runProgram(tags, normalProgram)
        if (value !== undefined) {
            return value;
        }
        return this.runProgram(tags, subProgram)
    }

    /**
     * Multiplies the default score with the proper values
     * @param tags {object} the active tags to check against
     * @param subprogram which should generate a list of values
     * @returns score after multiplication
     */
    multiplyScore(tags, subprogram) {
        let number = 1
        this.runProgram(tags, subprogram).filter(r => r !== undefined).forEach(r => number *= parseFloat(r))
        return number.toFixed(2);
    }

    getFirstMatchScore(tags) {
        let matchFound = false;
        let match = "";
        let i = 0;

        for (let key of this.order) {
            i++;
            for (let entry of Object.entries(JSON.parse(tags))) {
                const [tagKey, tagValue] = entry;

                if (key === tagKey) {
                    const valueReply = this.checkValues(entry);

                    if (!!valueReply) {
                        match = valueReply;
                        matchFound = true;
                        return match;
                    }
                }
            }
        }

        if (!matchFound) {
            match = this.defaultValue;
            return match;
        }
    }

    checkValues(tag) {
        const [tagKey, tagValue] = tag;
        const options = Object.entries(this.scoreValues[1])

        for (let option of options) {
            const [optKey, optValues] = option;

            if (optKey === tagKey) {
                return optValues[`${tagValue}`];
            }
        }
        return null;
    }

    getMinValue(tags, subprogram) {
        console.log("Running min with", tags, subprogram)
        const minArr = subprogram.map(part => {
            if (typeof (part) === 'object') {
                const calculatedValue = this.runProgram(tags, part)
                return parseFloat(calculatedValue)
            } else {
                return parseFloat(part);
            }
        }).filter(v => !isNaN(v));
        return Math.min(...minArr);
    }

    getMaxValue(tags, subprogram) {
        const maxArr = subprogram.map(part => {
            if (typeof (part) === 'object') {
                return parseFloat(this.runProgram(tags, part))
            } else {
                return parseFloat(part);
            }
        }).filter(v => !isNaN(v));
        return Math.max(...maxArr);
    }
}


export default RuleSet;