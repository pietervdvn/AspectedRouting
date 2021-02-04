# AspectedRouting

Building routing profiles easily

## What is aspected routing

Aspected routing is a system where routeplanning factors are constructed in multiple steps.

First, there are "aspects", which convert a set of attributes into some value (a number, a boolean, a string) - a function of `Tags -> a` basically.
These are grouped together in a `profile`, which uses these functions to state the behaviour - e.g. wether the vehicle can enter a way, can go both ways or just a single way, how fast it would realistically go, and finally how 'optimal' the road is.

## Using AspectedRouting

At the moment, this project takes in a collection of `.json`-files and converts them into a `.lua`-script which is compatible with [Itinero 1.0](https://github.com/itinero/routing) and  [Itinero 2.0](https://github.com/itinero/routing2)

PR's to support other systems (e.g. OsmAnd) are welcome. However, keep in mind that we will not be able to _maintain_ them for you.
