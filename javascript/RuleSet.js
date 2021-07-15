/**
 * RuleSet Class
 * Constructor
 * @param name {string} Name of RuleSet
 * @param defaultValue {number} Default score value
 * @param values {object} Main data object
 */
class RuleSet {
    constructor(name, defaultValue = 1, values) {
        this.name = name;
        this.defaultValue = defaultValue;
        this.values = values;
        this.score = this.defaultValue;
        this.scoreValues = null;
        this.order = null;
    }
    /**
     * toString
     * Returns constructor values in string for display in the console
     */
    toString () {
        return `${this.name} | ${this.defaultValue} | ${this.values}`;
    }

    /**
     * getScore calculates a score for the RuleSet
     * @param tags {object} Active tags to compare against
     */
    runProgram (tags, initValues = this.values) {
        const [
            [program, keys], values
        ] = Object.entries(initValues);

        if (program === '$multiply') {
            this.scoreValues = keys;
            this.score = this.multiplyScore(tags);
            return `"${this.name.slice(8)}":"${this.score}"`;

        } else if (program === '$firstMatchOf') {
            this.scoreValues = values;
            this.order = keys;
            const match = this.getFirstMatchScore(tags);
            return `"${this.name.slice(8)}":"${match}"`;
            
        } else {
            console.error(`Error: Program ${program} is not implemented yet. ${JSON.stringify(keys)}`);
        }
    }
    /**
     * Multiplies the default score with the proper values
     * @param tags {object} the active tags to check against
     * @returns score after multiplication
     */
    multiplyScore (tags) {
        let number = this.defaultValue;

        Object.entries(tags).forEach(tag => {
            const [key, value] = tag;
        
            Object.entries(this.scoreValues).forEach(property => {
                const [propKey, propValues] = property;
        
                if (key === propKey) {
                    for (let propEntry of Object.entries(propValues)) {
                        const [propValueKey, propValue] = propEntry;

                        if (value === propValueKey) number *= propValue;
                    }
                }
            })
        });
        return number.toFixed(2);
    }
    getFirstMatchScore (tags) {
        let matchFound = false;
        let match = "";

        for (let key of this.order) {
            for (let entry of Object.entries(tags)) {
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

    checkValues (tag) {
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


}


export default RuleSet;