# Aspected Routing

## Introduction

Generating a good route for travellers is hard; especially for cyclists. They can be very picky and the driving style
and purposes are diverse. Think about:

- A lightweight food delivery driver, who wants to be at their destination as soon as possible
- A cargo bike, possibly with a cart or electrically supported, doing heavy delivery
- Someone commuting from an to their work on a high-speed electrical bicycle
- Grandma cycling along a canal on sunday afternoon
- Someone bringing their kids to school

It is clear that these persona's on top have very different wishes for their route. A road with a high car pressure
won't pose a problem for the food delivery, whereas grandma wouldn't even think about going there. And this is without
mentioning the speed these cyclists drive, where they are allowed to drive, ...

Generating a cycle route for these persons is thus clearly far from simply picking the shortest possible path. On top of
that, a consumer expects the route calculations to be both customizable and to be blazingly fast.

In order to simplify the generation of these routing profiles, this repository introduces _aspected routing_.

In _aspected routing_, one does not try to tackle the routing problem all at once, but one tries to dissassemble the
preferences of the travellers into multiple, orthogonal aspects. These aspects can then be combined in a linear way,
giving a fast and flexible system.

Some aspects can be:

- Can we enter this road _legally_ with this vehicle?
- Can we enter this road _physically_ with this vehicle?
- What is the legal maximum speed here?
- What is the physical maximum speed here?
- Is this road oneway?
- How comfortable is this road?
- How safe is this road?
- Is this road lit?
- ...

One can come up with a zillion aspects, each relevant to a small subset of people.

# The data model

Even though this repository is heavily inspired on OpenStreetMap, it can be generalized to other road networks.

## Road network assumptions

The only assumptions made are that roads have a **length** and a collection of **tags**, this is a dictionary mapping
strings onto strings. These tags encode the properties of the road (e.g. road classification, name, surface, ...)

OpenStreetMap also has a concept of **relations**. A special function is available for that. However, in a preprocessing
step, the relations that a road is a member of, are converted into tags on every way with a `_network:i:key=value`
format, where `i` is the number of the relation, and `key`=`value` is a tag present on the relation.

## Describing an aspect

