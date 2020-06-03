
# Functions

- `$funcName` indicates a builtin function
- `#parameter` indicates a configurable parameter

A profile is a function which maps a set of tags onto a value. It is basically Haskell, except that functions can have _multiple_ types. During typechecking, some types will turn out not to be possible and they will dissappear, potentially fixing the implementation of the function itself.


# Vehicle.json

- Metdata: these tags will be copied to the routerdb, but can not be used for routeplanning. A prime example is `name` (the streetname), as it is useful for routeplanning but very useful for navigation afterwards
- vehciletypes: used for turn restrictions, legacy for use with itinero 1.0
- defaults: a dictionary of `{"#paramName": "value"}`, used in determining the weight of an edge. note: the `#` has to be included
- `access` is a field in the vehicle file. It should be an expression returning a string. If (and only if) this string is `no`, the way will be marked as not accessible and no more values will be calculated. All other values are regarded as being accessible. When calculated, the tag `access` with the calculated value is written into the tag table or the other aspects to use.
 - `oneway` is a field in the vehicle file. It should be an expression returning `both`, `with` or `against`. 
 When calculated, the tag `oneway` is added to the tags for the other aspects to be calculated.
 - `speed`: an expression indicating how fast the vehicle can go there. It should take into account legal, practical and social aspects. An example expression could be `{"$min", ["$legal_maxspeed", "#defaultspeed"]}`
 
- `priorities`: a table of `{'#paramName', expression}` determining the priority (1/cost) of a way, per meter. The formula used is `paramName * expression + paramName0 * expression0 + ...` (`speed`, `access` and `oneway` can be used here as tags indicate the earlier defined respective aspects). Use a weight == 1 to get the shortest route or `$speed` to get the fastest route


# Pitfalls

"$all" should not be used together with a mapping: it checks if all _present_ keys return true or yes (or some other value); it does _not_ check that all the specified keys in the mapping are present.

For this, an additional 'mustHaveKeys' should be added added 