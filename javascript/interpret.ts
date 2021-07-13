const interpret = (definitionFile, tags) => {


}


const example = interpret({
        "highway": {
            "residential": 1.2
        }
    },
    {
        "highway": "residential"

    })

console.log("Expect", 1.2, "got", example)

const example1 = interpret({
        "$multiply": {
            "highway": {
                "residential": 1.2
            },
            "surface": {"asphalt": 1.1}
        }
    },
    {
        "highway": "residential",
        "surface": "asphalt"

    })