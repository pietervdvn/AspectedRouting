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
--------------| - | - | - 
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

Argument name |  |  |  
--------------| - | - | - 
**a** | $a	 | $a	 | 
**b** | $a	 | $a	 | 
_return type_ | bool	 | string	 | 


Returns 'yes' if the two passed in values are _not_ the same



Lua implementation:

````lua
function notEq(a, b)
    if (a ~= b) then
        return "yes"
    else
        return "no"
    end
end
````


#### not

Argument name |  |  |  
--------------| - | - | - 
**a** | $a	 | $a	 | 
**b** | $a	 | $a	 | 
_return type_ | bool	 | string	 | 


Returns 'yes' if the two passed in values are _not_ the same



Lua implementation:

````lua
function notEq(a, b)
    if (a ~= b) then
        return "yes"
    else
        return "no"
    end
end
````


#### inv

Argument name |  |  |  
--------------| - | - | - 
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
--------------| - | - 
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

Argument name |  |  
--------------| - | - 
**s** | string	 | 
_return type_ | double	 | 


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
--------------| - | - 
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
--------------| - | - 
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


#### min

Argument name |  |  |  |  |  |  
--------------| - | - | - | - | - | - 
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
--------------| - | - | - | - | - | - 
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
--------------| - | - | - | - | - | - 
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
--------------| - | - | - | - | - | - 
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
--------------| - | - 
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
--------------| - | - 
**neededKeys (filled in by parser)** | list (string)	 | 
**f** | tags -> list (bool)	 | 
_return type_ | tags -> bool	 | 


Every key that is used in the subfunction must be present.
If, on top, a value is present with a mapping, every key/value will be executed and must return a value that is not 'no' or 'false'
Note that this is a privileged builtin function, as the parser will automatically inject the keys used in the called function.



Lua implementation:

````lua
function must_match(tags, result, needed_keys, table)
    local result_list = {}
    for _, key in ipairs(needed_keys) do
        local v = tags[key]
        if (v == nil) then
            return false
        end

        local mapping = table[key]
        if (type(mapping) == "table") then
            local resultValue = mapping[v]
            if (v == nil or v == false) then
                return false
            end
            if (v == "no" or v == "false") then
                return false
            end

            result.attributes_to_keep[key] = v
        else
            error("The mapping is not a table. This is not supported")
        end
    end
    return true;
end
````


#### memberOf

- (tags -> $a) -> tags -> list ($a)

This function uses memberships of relations to calculate values.

Consider all the relations the scrutinized way is part of.The enclosed function is executed for every single relation which is part of it, generating a list of results.This list of results is in turn returned by 'memberOf'
In itinero 1/lua, this is implemented by converting the matching relations and by adding the tags of the relations to the dictionary (or table) with the highway tags.The prefix is '_relation:n:key=value', where 'n' is a value between 0 and the number of matching relations (implying that all of these numbers are scanned).The matching relations can be extracted by the compiler for the preprocessing.

For testing, the relation can be emulated by using e.g. '_relation:0:key=value'



Lua implementation:

````lua
function member_of()
    ???
end
````


#### if_then_else

Argument name |  |  |  
--------------| - | - | - 
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
--------------| - | - | - 
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
--------------| - | - 
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
--------------| - | - 
**a** | $a	 | 
**b** | $b	 | 
_return type_ | $a	 | 


Small utility function, which takes two arguments `a` and `b` and returns `a`. Used extensively to insert freedom



Lua implementation:

````lua

````


#### constRight

Argument name |  |  
--------------| - | - 
**a** | $a	 | 
**b** | $b	 | 
_return type_ | $b	 | 


Small utility function, which takes two arguments `a` and `b` and returns `b`. Used extensively to insert freedom



Lua implementation:

````lua

````


#### dot

Argument name |  |  
--------------| - | - 
**f** | $gType -> $arg	 | 
**g** | $fType -> $gType	 | 
**a** | $fType	 | 
_return type_ | $arg	 | 


Higher order function: converts `f (g a)` into `(dot f g) a`. In other words, this fuses `f` and `g` in a new function, which allows the argument to be lifted out of the expression 



Lua implementation:

````lua

````


#### listDot

Argument name |  |  
--------------| - | - 
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

- ($a -> $b) -> ($c -> $d) -> $a -> $b
- ($a -> $b) -> ($c -> $d) -> $c -> $d





Lua implementation:

````lua

````


#### stringToTags

- (string -> string -> $a) -> tags -> list ($a)





Lua implementation:

````lua
print("ERROR: stringToTag is needed. This should not happen")
````


