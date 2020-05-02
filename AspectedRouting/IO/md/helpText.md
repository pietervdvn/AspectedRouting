## Types

- double
- pdouble
- nat
- int
- string
- tags
- bool
## Builtin functions

- eq
- notEq
- not
- inv
- default
- parse
- to_string
- concat
- containedIn
- min
- and
- max
- or
- sum
- multiply
- firstMatchOf
- mustMatch
- memberOf
- if_then_else
- if
- id
- const
- constRight
- dot
- listDot
- eitherFunc
- stringToTags


### Function overview

#### eq

a | b |
--- | --- |
$a | $a | bool |
$a | $a | string |

Returns 'yes' if both values _are_ the same



Lua implementation:

````lua
function eq(a, b)
    if (a == b) then
        return "yes"
    else
        return "no"
    end
end

````


#### notEq

a | b |
--- | --- |
$a | $a | bool |
$a | $a | string |
bool | bool |

OVerloaded function, either boolean not or returns 'yes' if the two passed in values are _not_ the same;



Lua implementation:

````lua
function notEq(a, b)
    if (b == nil) then
        b = "yes"
    end
    
    if (a ~= b) then
        return "yes"
    else
        return "no"
    end
end
````


#### not

a | b |
--- | --- |
$a | $a | bool |
$a | $a | string |
bool | bool |

OVerloaded function, either boolean not or returns 'yes' if the two passed in values are _not_ the same;



Lua implementation:

````lua
function notEq(a, b)
    if (b == nil) then
        b = "yes"
    end
    
    if (a ~= b) then
        return "yes"
    else
        return "no"
    end
end
````


#### inv

d |
--- |
pdouble | pdouble |
double | double |

Calculates `1/d`



Lua implementation:

````lua
function inv(n)
    return 1/n
end
````


#### default

defaultValue | f |
--- | --- |
$a | $b -> $a | $b | $a |

Calculates function `f` for the given argument. If the result is `null`, the default value is returned instead



Lua implementation:

````lua
function default(defaultValue, realValue)
    if(realValue ~= nil) then
        return realValue
    end
    return defaultValue
end
````


#### parse

s |
--- |
string | double |
string | pdouble |

Parses a string into a numerical value



Lua implementation:

````lua
function parse(string)
    if (string == nil) then
        return 0
    end
    if (type(string) == "number") then
        return string
    end

    if (string == "yes" or string == "true") then
        return 1
    end

    if (string == "no" or string == "false") then
        return 0
    end

    if (type(string) == "boolean") then
        if (string) then
            return 1
        else
            return 0
        end
    end


    return tonumber(string)
end
````


#### to_string

obj |
--- |
$a | string |

Converts a value into a human readable string



Lua implementation:

````lua
function to_string(o)
    return o;
end
````


#### concat

a | b |
--- | --- |
string | string | string |

Concatenates two strings



Lua implementation:

````lua
function concat(a, b)
    return a .. b
end
````


#### containedIn

list | a |
--- | --- |
list ($a) | $a | bool |

Given a list of values, checks if the argument is contained in the list.



Lua implementation:

````lua
function containedIn(list, a)
    for _, value in ipairs(list) do
        if (value == a) then
            return true
        end
    end

    return false;
end
````


#### min

list |
--- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Out of a list of values, gets the smallest value. IN case of a list of bools, this acts as `and`



Lua implementation:

````lua
function min(list)
    local min
    for _, value in ipairs(list) do
        if (min == nil) then
            min = value
        elseif (min > value) then
            min = value
        end
    end

    return min;
end
````


#### and

list |
--- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Out of a list of values, gets the smallest value. IN case of a list of bools, this acts as `and`



Lua implementation:

````lua
function min(list)
    local min
    for _, value in ipairs(list) do
        if (min == nil) then
            min = value
        elseif (min > value) then
            min = value
        end
    end

    return min;
end
````


#### max

list |
--- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Returns the biggest value in the list. For a list of booleans, this acts as 'or'



Lua implementation:

````lua
function max(list)
    local max
    for _, value in ipairs(list) do
        if (max == nil) then
            max = value
        elseif (max < value) then
            max = value
        end
    end

    return max;
end
````


#### or

list |
--- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Returns the biggest value in the list. For a list of booleans, this acts as 'or'



Lua implementation:

````lua
function max(list)
    local max
    for _, value in ipairs(list) do
        if (max == nil) then
            max = value
        elseif (max < value) then
            max = value
        end
    end

    return max;
end
````


#### sum

list |
--- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | int |

Sums all the numbers in the given list. If the list contains bool, `yes` or `true` will be considered to equal `1`



Lua implementation:

````lua
function sum(list)
    local sum = 1
    for _, value in ipairs(list) do
        if(value == 'yes' or value == 'true') then
            value = 1
        end
        sum = sum + value
    end
    return sum;
end
````


#### multiply

list |
--- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Multiplies all the values in a given list. On a list of booleans, this acts as 'and' or 'all'



Lua implementation:

````lua
function multiply(list)
    local factor = 1
    for _, value in ipairs(list) do
        factor = factor * value
    end
    return factor;
end
````


#### firstMatchOf

s |
--- |
list (string) | tags -> list ($a) | tags | $a |

Parses a string into a numerical value



Lua implementation:

````lua
function first_match_of(tags, result, order_of_keys, table)
    for _, key in ipairs(order_of_keys) do
        local v = tags[key]
        if (v ~= nil) then

            local mapping = table[key]
            if (type(mapping) == "table") then
                local resultValue = mapping[v]
                if (v ~= nil) then
                    result.attributes_to_keep[key] = v
                    return resultValue
                end
            else
                result.attributes_to_keep[key] = v
                return mapping
            end
        end
    end
    return nil;
end
````


#### mustMatch

neededKeys (filled in by parser) | f |
--- | --- |
list (string) | tags -> list (bool) | tags | bool |

Every key that is used in the subfunction must be present.
If, on top, a value is present with a mapping, every key/value will be executed and must return a value that is not 'no' or 'false'
Note that this is a privileged builtin function, as the parser will automatically inject the keys used in the called function.



Lua implementation:

````lua
function must_match(tags, result, needed_keys, table)
    for _, key in ipairs(needed_keys) do
        local v = tags[key]
        if (v == nil) then
            return false
        end

        local mapping = table[key]
        if (type(mapping) == "table") then
            local resultValue = mapping[v]
            if (resultValue == nil or
                    resultValue == false or
                    resultValue == "no" or
                    resultValue == "false") then
                return false
            end
        elseif (type(mapping) == "string") then
            local bool = mapping
            if (bool == "yes" or bool == "1") then
                return true
            elseif (bool == "no" or bool == "0") then
                return false
            end
            error("MustMatch got a string value it can't handle: " .. bool)
        else
            error("The mapping is not a table. This is not supported. We got " .. mapping)
        end
    end

        -- Now that we know for sure that every key matches, we add them all
        for _, key in ipairs(needed_keys) do
            local v = tags[key]
            result.attributes_to_keep[key] = v
        end

    return true;
end
````


#### memberOf

f | tags |
--- | --- |
tags -> bool | tags | bool |

This function returns true, if the way is member of a relation matching the specified function.

In order to use this for itinero 1.0, the membership _must_ be the top level expression.

Conceptually, when the aspect is executed for a way, every relation will be used as argument in the subfunction `f`
If this subfunction returns 'true', the entire aspect will return true.

In the lua implementation for itinero 1.0, this is implemented slightly different: a flag `_relation:<aspect_name>="yes"` will be set if the aspect matches on every way for where this aspect matches.
However, this plays poorly with parameters (e.g.: what if we want to cycle over a highway which is part of a certain cycling network with a certain `#network_name`?) Luckily, parameters can only be simple values. To work around this problem, an extra tag is introduced for _every single profile_:`_relation:<profile_name>:<aspect_name>=yes'. The subfunction is thus executed `countOr(relations) * countOf(profiles)` time, yielding `countOf(profiles)` tags. The profile function then picks the tags for himself and strips the `<profile_name>:` away from the key.



In the test.csv, one can simply use `_relation:<aspect_name>=yes` to mimic relations in your tests



Lua implementation:

````lua
function member_of(calledIn, parameters, tags, result)
    local k = "_relation:" .. calledIn
    -- This tag is conventiently setup by all the preprocessors, which take the parameters into account
    local doesMatch = tags[k]
    if (doesMatch == "yes") then
        result.attributes_to_keep[k] = "yes"
        return true
    end
    return false
end
````


#### if_then_else

condition | then | else |
--- | --- | --- |
bool | $a | $a | $a |
bool | $a | $a |

Selects either one of the branches, depending on the condition.If the `else` branch is not set, `null` is returned in the condition is false.



Lua implementation:

````lua
function if_then_else(condition, thn, els)
    if (condition) then
        return thn
    else
        return els -- if no third parameter is given, 'els' will be nil
    end
end
````


#### if

condition | then | else |
--- | --- | --- |
bool | $a | $a | $a |
bool | $a | $a |

Selects either one of the branches, depending on the condition.If the `else` branch is not set, `null` is returned in the condition is false.



Lua implementation:

````lua
function if_then_else(condition, thn, els)
    if (condition) then
        return thn
    else
        return els -- if no third parameter is given, 'els' will be nil
    end
end
````


#### id

a |
--- |
$a | $a |

Returns the argument unchanged - the identity function. Seems useless at first sight, but useful in parsing



Lua implementation:

````lua
function id(v)
    return v
end
````


#### const

a | b |
--- | --- |
$a | $b | $a |

Small utility function, which takes two arguments `a` and `b` and returns `a`. Used extensively to insert freedom



Lua implementation:

````lua
function const(a, b)
    return a
end
````


#### constRight

a | b |
--- | --- |
$a | $b | $b |

Small utility function, which takes two arguments `a` and `b` and returns `b`. Used extensively to insert freedom



Lua implementation:

````lua

````


#### dot

f | g | a |
--- | --- | --- |
$b -> $c | $a -> $b | $a | $c |

Higher order function: converts `f (g a)` into `(dot f g) a`. In other words, this fuses `f` and `g` in a new function, which allows the argument to be lifted out of the expression 



Lua implementation:

````lua

````


#### listDot

list | a |
--- | --- |
list ($a -> $b) | $a | list ($b) |

Listdot takes a list of functions `[f, g, h]` and and an argument `a`. It applies the argument on every single function.It conveniently lifts the argument out of the list.



Lua implementation:

````lua
-- TODO 
-- listDot
````


#### eitherFunc

f | g | a |
--- | --- | --- |
$a -> $b | $c -> $d | $a | $b |
$a -> $b | $c -> $d | $c | $d |

EitherFunc is a small utility function, mostly used in the parser. It allows the compiler to choose a function, based on the types.

Consider the mapping `{'someKey':'someValue'}`. Under normal circumstances, this acts as a pointwise-function, converting the string `someKey` into `someValue`, just like an ordinary dictionary would do. However, in the context of `mustMatch`, we would prefer this to act as a _check_, that the highway _has_ a key `someKey` which is `someValue`, thus acting as `{'someKey': {'$eq':'someValue'}}. Both behaviours are automatically supported in parsing, by parsing the string as `(eitherFunc id eq) 'someValue'`. The type system is then able to figure out which implementation is needed.

Disclaimer: _you should never ever need this in your profiles_



Lua implementation:

````lua

````


#### stringToTags

f | tags |
--- | --- |
string -> string -> $a | tags | list ($a) |

stringToTags converts a function `string -> string -> a` into a function `tags -> [a]`



Lua implementation:

````lua
print("ERROR: stringToTag is needed. This should not happen")
````


