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
}
