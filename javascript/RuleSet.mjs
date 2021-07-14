/**
 * RuleSet Class
 * Constructor
 * @param name {string} Name of RuleSet
 * @param defaultValue {number} Default score value
 * @param unit {string} Meta field with some info // !TODO: Decide if I'm going to keep this or remove it from the class
 * @param values {object} Main data object
 */
class RuleSet {
    constructor(name, defaultValue = 1, unit, values) {
        this.name = name;
        this.defaultValue = defaultValue;
        this.values = values;
        this.unit = unit;
        this.score = this.defaultValue;
        this.scoreValues = null;
        this.order = null;
    }
    /**
     * toString
     * Returns constructor values in string for display in the console
     */
    toString() {
        return `${this.name} |  ${this.unit} | ${this.defaultValue} | ${this.values}`;
    }

    /**
     * getScore calculates a score for the RuleSet
     * @param tags {object} Active tags to compare against
     */
    getScore(tags) {
        const [[program,keys], values] = Object.entries(this.values);
        
        if (program === '$multiply') {
            this.scoreValues = keys;
            this.score *= this.multiplyScore(tags);
            console.log(`${this.name}: ${this.score}`)
        } else if (program === '$firstMatchOf') {
            this.scoreValues = values;
            this.order = keys;
            this.getFirstMatchScore(tags);
        } else {
            console.error(`Error: Program ${program} is not implemented yet. ${JSON.stringify(keys)}`);
        }
    }
    /**
     * Multiplies the default score with the proper values
     * @param tags {object} the active tags to check against
     * @returns score after multiplication
     */
    multiplyScore(tags) {
        let number = this.defaultValue;
        Object.entries(tags).forEach(tag => {
            const [key, value] = tag;
            Object.entries(this.scoreValues).forEach(property => {
                const [propKey, propValues] = property;
                // console.log(propKey, key)
                if (key === propKey) {
                    for (let propEntry of Object.entries(propValues)) {
                        const [propValueKey, propValue] = propEntry;
                        if (value === propValueKey) number = propValue;
                    }
                }
            })
        });
        return number;
    }
    getFirstMatchScore(tags) {
        const [[tagKey, tagValue]] = Object.entries(tags);
        for (let item of this.order) {
            if (item === tagKey) {
                const options = Object.entries(this.scoreValues[1]);
                // console.log(tagKey)
                for (let optGroup of options) {
                    if (tagKey === optGroup[0]) {
                        for (let prop of Object.entries(optGroup[1])) {
                            const [propKey, propValue] = prop;
                            if (tagValue === propKey) {
                                console.log(`${this.name}: ${propValue}`);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}

export default RuleSet;