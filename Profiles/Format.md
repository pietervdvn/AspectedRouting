
# Functions

- `$funcName` indicates a builtin function
- `#parameter` indicates a configurable parameter

A profile is a function which maps a set of tags onto a value. It is basically Haskell, except that functions can have _multiple_ types. During typechecking, some types will turn out not to be possible and they will dissappear, potentially fixing the implementation of the function itself.


# Profile

- Metdata: these tags will be copied to the routerdb, but can not be used for routeplanning. A prime example is `name` (the streetname), as it is useful for routeplanning but very useful for navigation afterwards
- vehciletypes: used for turn restrictions, legacy for use with itinero 1.0
- defaults: a dictionary of `{"#paramName": "value"}`, used in determining the weight of an edge. note: the `#` has to be included
- `access`, `oneway`, `speed`: three expressions indicating respectively if access is allowed (if not equals to no), in what direction one can drive (one of `with`, `against` or `both`) and how fast one will go there. (Hint: this should be capped on legal_max_speed)
- `weights`: a table of `{'#paramName', expression}` determining the weight (aka COST) of a way, per meter. The formula used is `paramName * expression + paramName0 * expression0 + ...` (`$speed`, `$access` and `$oneway` can be used here to indicate the earlier defined respective aspects). Use a weight == 1 to get the shortest route or `$inv: $speed` to get the fastest route


# Pitfalls

"$all" should not be used together with a mapping: it checks if all _present_ keys return true or yes (or some other value); it does _not_ check that all the specified keys in the mapping are present.

For this, an additional 'mustHaveKeys' should be added added 