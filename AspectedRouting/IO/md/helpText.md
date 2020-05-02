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
- max
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

Argument name |  |  |  
-------------- | - | - | - 
**a** | $a	 | $a	 | 
**b** | $a	 | $a	 | 
_return type_ | bool	 | string	 | 


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

Argument name |  |  |  |  
-------------- | - | - | - | - 
**a** | $a	 | $a	 | bool	 | 
**b** | $a	 | $a	 | _none_	 | 
_return type_ | bool	 | string	 | bool	 | 


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

Argument name |  |  |  |  
-------------- | - | - | - | - 
**a** | $a	 | $a	 | bool	 | 
**b** | $a	 | $a	 | _none_	 | 
_return type_ | bool	 | string	 | bool	 | 


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

Argument name |  |  |  
-------------- | - | - | - 
**d** | pdouble	 | double	 | 
_return type_ | pdouble	 | double	 | 


Calculates `1/d`



Lua implementation:

````lua
function inv(n)
    return 1/n
end
````


#### default

Argument name |  |  
-------------- | - | - 
**defaultValue** | $a	 | 
**f** | $b -> $a	 | 
_return type_ | $b -> $a	 | 


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

Argument name |  |  |  
-------------- | - | - | - 
**s** | string	 | string	 | 
_return type_ | double	 | pdouble	 | 


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

Argument name |  |  
-------------- | - | - 
**obj** | $a	 | 
_return type_ | string	 | 


Converts a value into a human readable string



Lua implementation:

````lua
function to_string(o)
    return o;
end
````


#### concat

Argument name |  |  
-------------- | - | - 
**a** | string	 | 
**b** | string	 | 
_return type_ | string	 | 


Concatenates two strings



Lua implementation:

````lua
function concat(a, b)
    return a .. b
end
````


#### containedIn

Argument name |  |  
-------------- | - | - 
**list** | list ($a)	 | 
**a** | $a	 | 
_return type_ | bool	 | 


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

Argument name |  |  |  |  |  |  
-------------- | - | - | - | - | - | - 
**list** | list (nat)	 | list (int)	 | list (pdouble)	 | list (double)	 | list (bool)	 | 
_return type_ | nat	 | int	 | pdouble	 | double	 | bool	 | 


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

Argument name |  |  |  |  |  |  
-------------- | - | - | - | - | - | - 
**list** | list (nat)	 | list (int)	 | list (pdouble)	 | list (double)	 | list (bool)	 | 
_return type_ | nat	 | int	 | pdouble	 | double	 | bool	 | 


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

Argument name |  |  |  |  |  |  
-------------- | - | - | - | - | - | - 
**list** | list (nat)	 | list (int)	 | list (pdouble)	 | list (double)	 | list (bool)	 | 
_return type_ | nat	 | int	 | pdouble	 | double	 | int	 | 


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

Argument name |  |  |  |  |  |  
-------------- | - | - | - | - | - | - 
**list** | list (nat)	 | list (int)	 | list (pdouble)	 | list (double)	 | list (bool)	 | 
_return type_ | nat	 | int	 | pdouble	 | double	 | bool	 | 


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

Argument name |  |  
-------------- | - | - 
**s** | list (string)	 | 
_return type_ | (tags -> list ($a)) -> tags -> $a	 | 


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

Argument name |  |  
-------------- | - | - 
**neededKeys (filled in by parser)** | list (string)	 | 
**f** | tags -> list (bool)	 | 
_return type_ | tags -> bool	 | 


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

Argument name |  |  
-------------- | - | - 
**f** | tags -> bool	 | 
**tags** | tags	 | 
_return type_ | bool	 | 


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

Argument name |  |  |  
-------------- | - | - | - 
**condition** | bool	 | bool	 | 
**then** | $a	 | $a	 | 
**else** | $a	 | _none_	 | 
_return type_ | $a	 | $a	 | 


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

Argument name |  |  |  
-------------- | - | - | - 
**condition** | bool	 | bool	 | 
**then** | $a	 | $a	 | 
**else** | $a	 | _none_	 | 
_return type_ | $a	 | $a	 | 


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

Argument name |  |  
-------------- | - | - 
**a** | $a	 | 
_return type_ | $a	 | 


Returns the argument unchanged - the identity function. Seems useless at first sight, but useful in parsing



Lua implementation:

````lua
function id(v)
    return v
end
````


#### const

Argument name |  |  
-------------- | - | - 
**a** | $a	 | 
**b** | $b	 | 
_return type_ | $a	 | 


Small utility function, which takes two arguments `a` and `b` and returns `a`. Used extensively to insert freedom



Lua implementation:

````lua
function const(a, b)
    return a
end
````


#### constRight

Argument name |  |  
-------------- | - | - 
**a** | $a	 | 
**b** | $b	 | 
_return type_ | $b	 | 


Small utility function, which takes two arguments `a` and `b` and returns `b`. Used extensively to insert freedom



Lua implementation:

````lua

````


#### dot

Argument name |  |  
-------------- | - | - 
**f** | $b -> $c	 | 
**g** | $a -> $b	 | 
**a** | $a	 | 
_return type_ | $c	 | 


Higher order function: converts `f (g a)` into `(dot f g) a`. In other words, this fuses `f` and `g` in a new function, which allows the argument to be lifted out of the expression 



Lua implementation:

````lua

````


#### listDot

Argument name |  |  
-------------- | - | - 
**list** | list ($a -> $b)	 | 
**a** | $a	 | 
_return type_ | list ($b)	 | 


Listdot takes a list of functions `[f, g, h]` and and an argument `a`. It applies the argument on every single function.It conveniently lifts the argument out of the list.



Lua implementation:

````lua
-- TODO 
-- listDot
````


#### eitherFunc

Argument name |  |  |  
-------------- | - | - | - 
**f** | $a -> $b	 | $a -> $b	 | 
**g** | $c -> $d	 | $c -> $d	 | 
**a** | $a	 | $c	 | 
_return type_ | $b	 | $d	 | 


EitherFunc is a small utility function, mostly used in the parser. It allows the compiler to choose a function, based on the types.

Consider the mapping `{'someKey':'someValue'}`. Under normal circumstances, this acts as a pointwise-function, converting the string `someKey` into `someValue`, just like an ordinary dictionary would do. However, in the context of `mustMatch`, we would prefer this to act as a _check_, that the highway _has_ a key `someKey` which is `someValue`, thus acting as `{'someKey': {'$eq':'someValue'}}. Both behaviours are automatically supported in parsing, by parsing the string as `(eitherFunc id eq) 'someValue'`. The type system is then able to figure out which implementation is needed.

Disclaimer: _you should never ever need this in your profiles_



Lua implementation:

````lua

````


#### stringToTags

Argument name |  |  
-------------- | - | - 
**f** | string -> string -> $a	 | 
**tags** | tags	 | 
_return type_ | list ($a)	 | 


stringToTags converts a function `string -> string -> a` into a function `tags -> [a]`



Lua implementation:

````lua
print("ERROR: stringToTag is needed. This should not happen")
````


