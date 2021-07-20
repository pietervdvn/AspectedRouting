# StressMap interpreter
CLI program that generates a stress score based on a RuleSet JSON file and a tag object.

## Installation
`npm i mapcomplete-stressmap`

## How to use
This program can be used from the command line interface:
`node mapcomplete-stressmap [ruleset.json] [tags object]`

Example tags: 
```json
{
    "cyclestreet": "yes",
    "highway": "residential", // Expect "yes"
    "maxspeed": "30",
    "surface": "asphalt"
}
```

Guidelines on JSON Ruleset (from [AspectedRouting](https://www.github.com/pietervdvn/AspectedRouting.git))

[Building a Profile](./BuildingAProfile.md)
