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
- plus
- add
- multiply
- atleast
- firstMatchOf
- mustMatch
- memberOf
- if_then_else
- if
- if_then_else_dotted
- ifdotted
- ifDotted
- id
- const
- constRight
- dot
- listDot
- eitherFunc
- stringToTags
- head


### Function overview

#### eq

a | b | returns |
--- | --- | --- |
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

a | b | returns |
--- | --- | --- |
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

a | b | returns |
--- | --- | --- |
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

d | returns |
--- | --- |
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

defaultValue | f | returns |
--- | --- | --- |
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

s | returns |
--- | --- |
string | double |
string | pdouble |

Parses a string into a numerical value. Returns 'null' if parsing fails or no input is given



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

obj | returns |
--- | --- |
$a | string |

Converts a value into a human readable string



Lua implementation:

````lua
function to_string(o)
    return o;
end
````


#### concat

a | b | returns |
--- | --- | --- |
string | string | string |

Concatenates two strings



Lua implementation:

````lua
function concat(a, b)
    return a .. b
end
````


#### containedIn

list | a | returns |
--- | --- | --- |
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

list | returns |
--- | --- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Out of a list of values, gets the smallest value. In case of a list of bools, this acts as `and`. Note that 'null'-values are ignored.



Lua implementation:

````lua
function min(list)
    local min
    for _, value in ipairs(list) do
        if (min == nil) then
            min = value
        elseif (value < min) then
            min = value
        end
    end

    return min;
end
````


#### and

list | returns |
--- | --- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Out of a list of values, gets the smallest value. In case of a list of bools, this acts as `and`. Note that 'null'-values are ignored.



Lua implementation:

````lua
function min(list)
    local min
    for _, value in ipairs(list) do
        if (min == nil) then
            min = value
        elseif (value < min) then
            min = value
        end
    end

    return min;
end
````


#### max

list | returns |
--- | --- |
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

list | returns |
--- | --- |
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

list | returns |
--- | --- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | int |

Sums all the numbers in the given list. If the list is a list of booleans, `yes` or `true` will be considered to equal `1`. Null values are ignored (and thus handled as being `0`)



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


#### plus

list | returns |
--- | --- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | int |

Sums all the numbers in the given list. If the list is a list of booleans, `yes` or `true` will be considered to equal `1`. Null values are ignored (and thus handled as being `0`)



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


#### add

list | returns |
--- | --- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | int |

Sums all the numbers in the given list. If the list is a list of booleans, `yes` or `true` will be considered to equal `1`. Null values are ignored (and thus handled as being `0`)



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

list | returns |
--- | --- |
list (nat) | nat |
list (int) | int |
list (pdouble) | pdouble |
list (double) | double |
list (bool) | bool |

Multiplies all the values in a given list. On a list of booleans, this acts as 'and' or 'all', as `false` and `no` are interpreted as zero. Null values are ignored and thus considered to be `one`



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


#### atleast

minimum | f | then | else | returns |
--- | --- | --- | --- | --- |
double | $b -> double | $a | $a | $b | $a |

Returns 'yes' if the second argument is bigger then the first argument. (Works great in combination with $dot)



Lua implementation:

````lua
function atleast(minimumExpected, actual, thn, els)
    if (minimumExpected <= actual) then
        return thn;
    end
    return els
end
````


#### firstMatchOf

s | returns |
--- | --- |
list (string) | tags -> list ($a) | tags | $a |

This higherorder function takes a list of keys, a mapping (function over tags) and a collection of tags. It will try the function for the first key (and it's respective value). If the function fails (it gives null), it'll try the next key.

E.g. `$firstMatchOf ['maxspeed','highway'] {'maxspeed' --> $parse, 'highway' --> {residential --> 30, tertiary --> 50}}` applied on `{maxspeed=70, highway=tertiary}` will yield `70` as that is the first key in the list; `{highway=residential}` will yield `30`.



Lua implementation:

````lua
function first_match_of(tags, result, order_of_keys, table)
    for _, key in ipairs(order_of_keys) do
        local v = tags[key]
        if (v ~= nil) then

            local mapping = table[key]
            if (type(mapping) == "table") then
                local resultValue = mapping[v]
                if (resultValue ~= nil) then
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

neededKeys (filled in by parser) | f | returns |
--- | --- | --- |
list (string) | tags -> list (string) | tags | bool |

Checks that every specified key is present and gives a non-false value
.
If, on top, a value is present with a mapping, every key/value will be executed and must return a value that is not 'no' or 'false'
Note that this is a privileged builtin function, as the parser will automatically inject the keys used in the called function.



Lua implementation:

````lua
--[[
must_match checks that a collection of tags matches a specification.

The function is not trivial and contains a few subtilities.

Consider the following source:

{"$mustMatch":{ "a":"X", "b":{"not":"Y"}}}

This is desugared into

{"$mustMatch":{ "a":{"$eq":"X"}, "b":{"not":"Y"}}}

When applied on the tags {"a" : "X"}, this yields the table {"a":"yes", "b":"yes} (as `notEq` "Y" "nil") yields "yes"..
MustMatch checks that every key in this last table yields yes - even if it is not in the original tags!


]]
function must_match(tags, result, needed_keys, table)
    for _, key in ipairs(needed_keys) do
        local v = table[key] -- use the table here, as a tag that must _not_ match might be 'nil' in the tags
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
            if (bool == "no" or bool == "0") then
                return false
            end

            if (bool ~= "yes" and bool ~= "1") then
                error("MustMatch got a string value it can't handle: " .. bool)
            end
        elseif (type(mapping) == "boolean") then
            if(not mapping) then
                return false
            end
        else
            error("The mapping is not a table. This is not supported. We got " .. tostring(mapping) .. " (" .. type(mapping)..")")
        end
    end

    -- Now that we know for sure that every key matches, we add them all
    for _, key in ipairs(needed_keys) do
        local v = tags[key] -- this is the only place where we use the original tags
        if (v ~= nil) then
            result.attributes_to_keep[key] = v
        end
    end

    return true
end
````


#### memberOf

f | tags | returns |
--- | --- | --- |
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

condition | then | else | returns |
--- | --- | --- | --- |
bool | $a | $a | $a |
bool | $a | $a |
string | $a | $a | $a |
string | $a | $a |

Selects either one of the branches, depending on the condition. The 'then' branch is returned if the condition returns the string `yes` or `true`. Otherwise, the `else` branch is taken (including if the condition returns `null`)If the `else` branch is not set, `null` is returned in the condition is false.



Lua implementation:

````lua
function if_then_else(condition, thn, els)
    if (condition ~= nil and (condition == "yes" or condition == true or condition == "true") then
        return thn
    else
        return els -- if no third parameter is given, 'els' will be nil
    end
end
````


#### if

condition | then | else | returns |
--- | --- | --- | --- |
bool | $a | $a | $a |
bool | $a | $a |
string | $a | $a | $a |
string | $a | $a |

Selects either one of the branches, depending on the condition. The 'then' branch is returned if the condition returns the string `yes` or `true`. Otherwise, the `else` branch is taken (including if the condition returns `null`)If the `else` branch is not set, `null` is returned in the condition is false.



Lua implementation:

````lua
function if_then_else(condition, thn, els)
    if (condition ~= nil and (condition == "yes" or condition == true or condition == "true") then
        return thn
    else
        return els -- if no third parameter is given, 'els' will be nil
    end
end
````


#### if_then_else_dotted

condition | then | else | returns |
--- | --- | --- | --- |
$b -> bool | $b -> $a | $b | $a |
$b -> string | $b -> $a | $b | $a |
$b -> bool | $b -> $a | $b -> $a | $b | $a |
$b -> string | $b -> $a | $b -> $a | $b | $a |

An if_then_else, but one which takes an extra argument and applies it on the condition, then and else.
Consider `fc`, `fthen` and `felse` are all functions taking an `a`, then:
`(ifDotted fc fthen felse) a` === `(if (fc a) (fthen a) (felse a)Selects either one of the branches, depending on the condition. The 'then' branch is returned if the condition returns the string `yes` or `true` or the boolean `true`If the `else` branch is not set, `null` is returned in the condition is false.In case the condition returns 'null', then the 'else'-branch is taken.



Lua implementation:

````lua
function applyIfNeeded(f, arg)
    if(f == nil) then
        return nil
    end
    if(type(f) == "function") then
        return f(arg)
     else
        return f
    end
end

function if_then_else_dotted(conditionf, thnf, elsef, arg)
    local condition = applyIfNeeded(conditionf, arg); 
    if (condition) then
        return applyIfNeeded(thnf, arg)
    else
        if(elsef == nil) then
            return nil
         end
        return applyIfNeeded(elsef, arg) -- if no third parameter is given, 'els' will be nil
    end
end
````


#### ifdotted

condition | then | else | returns |
--- | --- | --- | --- |
$b -> bool | $b -> $a | $b | $a |
$b -> string | $b -> $a | $b | $a |
$b -> bool | $b -> $a | $b -> $a | $b | $a |
$b -> string | $b -> $a | $b -> $a | $b | $a |

An if_then_else, but one which takes an extra argument and applies it on the condition, then and else.
Consider `fc`, `fthen` and `felse` are all functions taking an `a`, then:
`(ifDotted fc fthen felse) a` === `(if (fc a) (fthen a) (felse a)Selects either one of the branches, depending on the condition. The 'then' branch is returned if the condition returns the string `yes` or `true` or the boolean `true`If the `else` branch is not set, `null` is returned in the condition is false.In case the condition returns 'null', then the 'else'-branch is taken.



Lua implementation:

````lua
function applyIfNeeded(f, arg)
    if(f == nil) then
        return nil
    end
    if(type(f) == "function") then
        return f(arg)
     else
        return f
    end
end

function if_then_else_dotted(conditionf, thnf, elsef, arg)
    local condition = applyIfNeeded(conditionf, arg); 
    if (condition) then
        return applyIfNeeded(thnf, arg)
    else
        if(elsef == nil) then
            return nil
         end
        return applyIfNeeded(elsef, arg) -- if no third parameter is given, 'els' will be nil
    end
end
````


#### ifDotted

condition | then | else | returns |
--- | --- | --- | --- |
$b -> bool | $b -> $a | $b | $a |
$b -> string | $b -> $a | $b | $a |
$b -> bool | $b -> $a | $b -> $a | $b | $a |
$b -> string | $b -> $a | $b -> $a | $b | $a |

An if_then_else, but one which takes an extra argument and applies it on the condition, then and else.
Consider `fc`, `fthen` and `felse` are all functions taking an `a`, then:
`(ifDotted fc fthen felse) a` === `(if (fc a) (fthen a) (felse a)Selects either one of the branches, depending on the condition. The 'then' branch is returned if the condition returns the string `yes` or `true` or the boolean `true`If the `else` branch is not set, `null` is returned in the condition is false.In case the condition returns 'null', then the 'else'-branch is taken.



Lua implementation:

````lua
function applyIfNeeded(f, arg)
    if(f == nil) then
        return nil
    end
    if(type(f) == "function") then
        return f(arg)
     else
        return f
    end
end

function if_then_else_dotted(conditionf, thnf, elsef, arg)
    local condition = applyIfNeeded(conditionf, arg); 
    if (condition) then
        return applyIfNeeded(thnf, arg)
    else
        if(elsef == nil) then
            return nil
         end
        return applyIfNeeded(elsef, arg) -- if no third parameter is given, 'els' will be nil
    end
end
````


#### id

a | returns |
--- | --- |
$a | $a |

Returns the argument unchanged - the identity function. Seems useless at first sight, but useful in parsing



Lua implementation:

````lua
function id(v)
    return v
end
````


#### const

a | b | returns |
--- | --- | --- |
$a | $b | $a |

Small utility function, which takes two arguments `a` and `b` and returns `a`. Used extensively to insert freedom



Lua implementation:

````lua
function const(a, b)
    return a
end
````


#### constRight

a | b | returns |
--- | --- | --- |
$a | $b | $b |

Small utility function, which takes two arguments `a` and `b` and returns `b`. Used extensively to insert freedom



Lua implementation:

````lua

````


#### dot

f | g | a | returns |
--- | --- | --- | --- |
$b -> $c | $a -> $b | $a | $c |

Higher order function: converts `f (g a)` into `(dot f g) a`. In other words, this fuses `f` and `g` in a new function, which allows the argument to be lifted out of the expression 



Lua implementation:

````lua

````


#### listDot

list | a | returns |
--- | --- | --- |
list ($a -> $b) | $a | list ($b) |

Listdot takes a list of functions `[f, g, h]` and and an argument `a`. It applies the argument on every single function.It conveniently lifts the argument out of the list.



Lua implementation:

````lua
-- TODO 
-- listDot
````


#### eitherFunc

f | g | a | returns |
--- | --- | --- | --- |
$a -> $b | $c -> $d | $a | $b |
$a -> $b | $c -> $d | $c | $d |

EitherFunc is a small utility function, mostly used in the parser. It allows the compiler to choose a function, based on the types.

Consider the mapping `{'someKey':'someValue'}`. Under normal circumstances, this acts as a pointwise-function, converting the string `someKey` into `someValue`, just like an ordinary dictionary would do. However, in the context of `mustMatch`, we would prefer this to act as a _check_, that the highway _has_ a key `someKey` which is `someValue`, thus acting as `{'someKey': {'$eq':'someValue'}}. Both behaviours are automatically supported in parsing, by parsing the string as `(eitherFunc id eq) 'someValue'`. The type system is then able to figure out which implementation is needed.

Disclaimer: _you should never ever need this in your profiles_



Lua implementation:

````lua

````


#### stringToTags

f | tags | returns |
--- | --- | --- |
string -> string -> $a | tags | list ($a) |

stringToTags converts a function `string -> string -> a` into a function `tags -> [a]`



Lua implementation:

````lua
print("ERROR: stringToTag is needed. This should not happen")
````


#### head

ls | returns |
--- | --- |
list ($a) | $a |

Select the first non-null value of a list; returns 'null' on empty list or on null



Lua implementation:

````lua
function head(ls)
   if(ls == nil) then
       return nil
   end
   for _, v in ipairs(ls) do
       if(v ~= nil) then
           return v
       end
   end
   return nil
end
````


