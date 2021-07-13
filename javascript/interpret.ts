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