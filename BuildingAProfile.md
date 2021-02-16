# Building a routeplanner

This document was originally written as blog post. It gives a practical, example first example to build a custom route planner.

In order to deploy:

- Build your profile by creating the relevant `.json`-files in a directory; take a peek at `Examples`
- Run the project: `cd AspectedRouting && dotnet run <inputdir> <outputdir>` (make sure the outputDirectory is _not_ a subdirectory of the input directory)
- In outputDir, you will find a bunch of lua-scripts which can be used with itinero

## A step by step example for an aspect

Let us recreate (a small part) of the legal access aspect for cyclists. The file will answer the question: __can a bicycle enter this road?__

First, we start with some metadata:

```
"name": "bicycle.legal_access",
  "description": "Can a bicycle enter this road segment?",
  "unit": "Yes, No",
```
  
The `name` field is an important one, as the aspect can be called with it in other files. The `description` and `unit`-fields however are purely as documentation - but are nonetheless important. Writing down exactly what an aspect means helps to clarify what is calculated before coding it and makes life easier down the road.

### Building the access-function

To call a function in an aspect, one creates a hash in the JSON where exactly one key starts with a `$`. The rest of the key determines which function is called, the value of the key is its first argument whereas the other keys in the hash function as other parameters. One could for example check that two values are the same with:

```
{
  "$eq": "someValue",
  "b": "otherValue"  	
}
```

Interpreting the above expression will aways yield `no` when evaluating, as the parameters have different values. The type of the above expression is thus `Bool`.

If no key has a function invocation (thus no key starts with `$`), the hash is interpreted as a mapping:

```
{
  "yes": "yes"
  "no": "no"
  "customers": "no"
}

```

The above expression is a function of type `string -> double`. If invoked, it will convert `yes` into the value `yes` and `customers` into the value `no`. Any string not in the mapping will result in `null`.


Every expression in AspectedRouting is implicitly yet strongly typed at compile time. Having types around is cool and good for correctness, but can be constraining and the cause of boilerplate. Therefore, expressions are allowed to have _multiple_ types. Due to the context of how it is called and what the parameters of functions are, the compiler can determine exaclty which type is meant.

For example, a mapping like above can also be used to match OSM-keys:

```
{
  "access": {
    "yes": "no",
    "no": "no",
    "customers":"no"
  },
  "bicycle": "$id",
  "construction": "no"
}
```

There is a lot to unpack here. A mapping as above is either a function taking a `string` and returning a value, or it is a function taking a `Tags`-collection and returning a collection of calculated values.

For example, passing in the collection `access=customers` in the above function will result into the value `["no"]`. Passing `access=dismount;bicycle=yes` will result in `[null, "yes"]` - the value corresponding with `access` is passed into the mapping `{"yes":"yes", "customers":"no", ...}` where no match is found resulting in `null`. The value for `bicycle` is passed into the `$id` function which simply passes back its argument.

At last, there is the cryptical `"construction":"no"`. This expression indicates that if a construction-tag is present, the resulting value should always be `no`. But how does it work exactly? When writing a constant (such as `"no"`) in an Aspected-Routing file, it is interpreted as either being the literal constant _or_ as being a function which ignores the parameter! `"no"` has thus the types `string` and `a -> string`. When used in a single mapping with type `string -> string` it is clear the first one is meant, when used in a tagsmapping with type `Tags -> string` (e.g. `{"key": "f"}`, the type of the function `f` should be `string -> b`, clearly indicating that `"no"` should be interpreted as the function which ignores the parameter. If this sounds like magic to you - don't worry about it too much. In practice, you just type what feels logical and it'll work out.

#### Combining multiple tags

The above aspect is already pretty close to a working access-calculation for cyclists - but we still have a collection of values, not a single one. We have a clear order in which we want to evaluate the tags. This too can be done with a builtin function, namely `$firstMatchOf` with the type `[string] -> (Tags -> [a]) -> (Tags -> a)`. For those not familiar with this notation for the types, this reads as: given a list of `string` and a function (which converts tags into a list of `a`), I'll give back a function that converts `Tags` into some `a`  

It is used in the following way:

```
{
  "$firstMatchOf":["bicycle", "construction", "access"],
  "f": { ... above code ... }
}
```

At last, what if _none_ of the tags match? What do we do then? For that, there is `$default: a -> (x -> a) -> (x -> a)`. More comprehensively, this function needs a (default) value `a`, and a function calculating some `a` based on `x` and it'll give back a function that calculates an `a` based on an `x`.

Here too is an example clearer then trying to explain it:

```
{
  "$default": "no",
  "f": { ... above code ... }
}

```

#### Combining everything

Everything together, this gives a very basic implementation of where a cyclists can cycle! If we throw it all together, we get the following JSON file:


```
{

 "name": "bicycle.legal_access",
  "description": "Gives, for each type of highway, whether or not a normal bicycle can enter legally.\nNote that legal access is a bit 'grey' in the case of roads marked private and permissive, in which case these values are returned ",
  "unit": "yes, no",


  "$default": "no",
  "f": {
    "$firstMatchOf": ["bicycle", "construction", "access"],
    "f": {
      "access": {
        "yes": "no",
        "no": "no",
        "customers":"no"
      },
      "bicycle": "$id",
      "construction": "no"
    }
  }
}

```

It should be noted that the _actual_ implementation is more complicated then that. There are more tags to keep track of, but the above explanation should be enough to get a grasp of [legal-access-aspect for bicycles](https://github.com/pietervdvn/AspectedRouting/blob/master/Examples/bicycle/aspects/bicycle.legal_access.json). An overview of all the functions and available types, have a look [here](https://github.com/pietervdvn/AspectedRouting/blob/master/Examples/TypesAndFunctions.md)

### Building a profile

Having accessibility alone isn't enough to create a route planner for cyclists. In a similar way, one can create an aspect that defines [if the street is a oneway](https://github.com/pietervdvn/AspectedRouting/blob/master/Examples/bicycle/aspects/bicycle.oneway.json) or how [comfortable a street is](https://github.com/pietervdvn/AspectedRouting/blob/master/Examples/bicycle/aspects/bicycle.comfort.json). (Please note that the linked examples are stripped down examples. Our actual routeplanner has a few more aspect files and more tags).

At last, we have to combine those aspects into something that actually creates the profile. This is done by another JSON-file, such as [this one](https://github.com/pietervdvn/AspectedRouting/blob/master/Examples/bicycle/bicycle.json). Lets break it down:

```
{
  "name": "bicycle",
  "description": "Profile for a normal bicycle",
```

This is some metadata, mostly meant for humans.

```
  
  "defaults": {
    "#maxspeed": 20,
    "#timeNeeded": 0,
    "#comfort": 0,
    "#distance": 0,
  },
```

This declares some variables, which can only be used in the scope of the profile. Variables always start with `#` and are either a `number`, a `boolean` or a `string`. They are used to below the actual aspects of the profile:

```
  "access": "$bicycle.legal_access",
```
This states when a segment is accessible. It expects a function `Tags -> string` and a segment is considered not accessible if this value is `"no"`; it is accessible otherwise.
  
```
  "oneway": "$bicycle.oneway",
```
This indicates if the street is a oneway, it expects a function `Tags -> string` where the resulting value is one of `both`,`with` or `against`


```
  "speed": {
    "$min": [
      "#defaultSpeed",
      "$legal_maxspeed_be"
    ]
  },
```

This states how fast a bicycle would be going on the segment; it expects a function `Tags -> number`. It is the first interesting case: both the variable `#maxspeed` (defined in `defaults`) is used, together with a function calculating the _legal_ max speed for a road segment. The lowest of the two is taken, by the function `$min`

```  
  "behaviours": {
    "fastest": {
      "description": "The fastest route to your destination",
      "#timeNeeded": 1,
    },
    "shortest": {
      "description": "The shortest route, independent of of speed",
      "#distance": 1,
    },
    "comfort": {
      "description": "A comfortable route preferring well-paved roads, smaller roads and a bit of scenery at the cost of speed",
      "#comfort": 1
    },
    "electric":{
      "description": "An electrical bicycle",
      "#maxspeed": 25,
      "#comfort":1,
      "#timeNeeded": 5
    },
    "electric_fastest":{
      "description": "An electrical bicycle, focussed on speed",
      "#maxspeed": 25,
    }
  },
```

The above code defines _behaviours_ of the cyclist. It allows to overwrite a variable which influences the routeplanning. For example, the behavour `electrical` above will overwrite the maxspeed, changing the `speed`-aspect at the top of the file. However, these variables are most important in the priority below:

```  
  "priority": {
    "#comfort": "$bicycle.comfort",
    "#timeNeeded": "$speed",
    "#distance": "$distance",
  }
}

```

The priority is the core of the customizibility and calculates the priority of the segment. First, the function on the right is calculated with the tags of the segment - e.g. for a segment with tags `highway=residential;surface=sett;` this will yield `{"#comfort": 0.9, "#timeNeeded": 25, "#distance": 1}`.

These values are multiplied with the variables and summed, giving the _priority_ of the segment - where the variable are set by the requested profile; e.g. for `electrical` this will yield `(#comfort = 1) * 0.9 + (#timeNeeded = 5) * 25 + (#distance = 0) * 1`, giving the priority of `125.9`. The _cost_ per meter is then the inverted value, thus `1 / 125.9` or approximately `0.008/m`. This cost seems relatively low - but that doesn't matter as all costs are in the same range.
