# The Brainstorm

The following document is a copy of the [issue](https://github.com/anyways-open/routing-profiles/issues/25) in the routing-profiles. The issue started out as a brainstorming, but laid the fundaments and the vision for this repository.

Not everything here got implemented or got implemented exactly as mentioned here. As a result, this is a **historic document and _not_ documentation!**


# Issue #25

In order to make the next step in route planning, the next step in structuring programs should be made.

This issue keeps track of some ideas on how to do 

# One vehicle per routerdb

To make things simple and scalable, we would have one vehicle per routerdb/network (even though more are possible).

A vehicle is a mode of transportation (e.g. a car, bicycle, pedestrian, ...). Note that, depending on who is driving the vehicle, behavior can be different. Some like to have the fastest route, others the shortest, safest, most comfortable, follow a routing network, ... Such differing behavior is called a *profile*.

# Aspects

A profile  can be seen as multiple aspects working together. Some aspects are the same for all profiles (e.g.: can this vehicle access this road), others are highly subjective (actual speed on a segment)  or not relevant for every profile (e.g. how comfortable is this road). Such an aspect is equivalent to *resistance* in electronics, as it imposes a cost/distance (e.g. time/meter = speed; fuel/meter = distance; risk/meter = safety ...)

Another important thing to keep in mind is **directionality** of the edge. In some cases, the weight of an aspect is different regarding the order. For example, in the case of cyclists, having a shared lane going one way (cycleway:right=shared_lane) vs a track on the other side (cycleway:left=track) is a huge difference in safety and comfort.


Possible aspects are:

- Estimated time needed/meter aka speed
- Legal minimal time needed/meter aka maximal legal speed
- Accessibility (can access/cannot access due to legal restrictions) but also 'softer' accessibility such as service ways, destination, permissive, ... In combination with the 'willingness to trespass', one could offer a profile taking shortcuts via parking lots vs not taking them
- Direction of traffic (oneways, roundabouts, ...)
- Maximum width of the way
- Maximum height of the way
- Dropping of: can we stop at the side of the road to drop packages? can we start? Can we start/stop at the right/left side of the road? Can we stop/start while driving in a direction?
- Elevation: average elevation difference (joules/meter)
- Elevation: max elevation percentage in the segment (peak watt) - (in order to prevent to steep pieces)
- Comfort: how comfortable is a road to drive on?
- Safety: how safe is a road to drive on?
- Is the way lit?
- Part of a route network relation (part of any network, a specific category of networks or even a very specific network
- Mode of the vehicle: e.g. cycling vs walking with the bicycle next to the cyclist

And eventually even 'realtime' data:

- Realtime speed of vehicles
- Realtime business of roads
- Realtime pollution data (e.g. fine dust)

At last, aspects could also be added for technical reasons (but it is not certain this will be the best technical approach):

- Is there a turn restriction on this edge (e.g. is this edge part of a 'forbidden sequence')?
- Is there a penalty along the way (e.g. a bollard causes an additional 5 seconds of delay due to the cyclist slowing down), ...

One aspect that will _not_ be handled by aspects are turning weights, these will be implemented by keeping them on the node.

# Profiles

A profile is then a combination of aspects: it uses a _non-turing-complete_ formula to calculate if it can go over an edge, and if so, in what directions and what the 'cost' is in each direction (implying that the cost can be different for some cases, e.g. a cyclist having only a 'shared line' going with the flow of cars but having a segregated track going against the flow)

# Data structure

The routerdb has the edge information as `edgeID, nodeID_From, nodeId_to, distance` and maybe some other information. One could append the value for each aspect to this data structure.

Alternatively, to save space and to speed up things, the profiles can be precached in a lookup table. Often, the same 'aspect values' will be encountered, eg: `[aspect1=XX, aspect2=YY]`. This can be put into a lookup table:

```
1 --> aspect1 = XX; aspect2 = YY
2 --> aspect1 = ZZ; aspect2 = WW
```

If this approach is used, then only the lookup table index has to be saved next to the edge.

Based on this lookup table, the actual cost for each profile can be precalculated. As the lookup table contains all values for all aspects, the lookup table can be used to calculate the cost/meter for every profile: `1 --> 123cost/meter; 2 --> 456cost/meter`. Getting the cost/meter can thus be calculated (at runtime) with one array lookup + multiplication with the length of the edge. This can be especially useful to provide more dynamic profiles, e.g. for having support for multiple 'widths' and 'heights' of cars; or having multiple default speeds. This might even be feasible to do on the fly (without contraction) 


# The actual profile format

For the profile format, I would propose to make one lua-file for each aspect.

For the actual profiles, a simpler format could suffice (json, yaml, ...) where one describes the formula used to calculate cost, oneway and access. As this formula is simple, one can easily determine which aspects are used and only calculate those.

Calculating an aspect boils down to having a `static double CalculateCostFor(List<Tag> tagsOfWay, List<Relations> memberOfTheseRelations)` (or something comparable in lua)

# Turning weights/obstacles along the way

Turning weights can be modelled using a matrix, attached on the node. The matrix would indicate the cost of going from one edge onto another edge.

To compress all these matrices, they can be put into a lookup table too, as one can expect 99% of these matrices to be the same.

Calculating the turning weights needs a function `static Matrix TurningWeightsFor([Tag] tagsOfNode, ([Tag], Angle) TagsOfWays)`. (Note that this method does _not_ worry about accessibility of ways neither with turn restrictions)

Note that one matrix again describes but the cost of a certain aspect. Turning weights could describe the cost of turning in seconds, but also the safety feeling: as a cyclist, crossing a road might not only have a time penalty, but also a safety or comfort penalty.

At last, this aspected-turn restrictions can also be used to describe obstacles along the way. A prime example is a bollard on a cycle path: it has a small (sometimes negligable) time penalty, but it has a certain safety cost (especially older ppl don't like them), might impose a max_width along a part of way (slowing cargo bikes that barely fit and forbidding to fat cargobikes all togehter).

A few things I think of straight away

- Bollards slow down cyclists, are unsafe and might slow down/block cargo bikes
- Traffic lights slow down traffic and are unsafe for cyclists, but a shoulder (fietsvoorsorteerstrook) and 'turn right through red' improve things
- crossings can be marked, unmarked, raised; cyclists might have priority or might not have priority on some crossings (also: how to tag this)

Note that a highway in itself can cause a fixed 'obstacle weight' as well, think a ferry where getting on takes a quarter on average, a corridor where one has to dismount taking a few seconds, .... It should however be noted that these are penalties on state changes.


# Turn restrictions

Turn restrictions are "another pair of sleeves" ;)

Turn restrictions are relations depicting a sequence of forbidden edges: coming from edge A, it is forbidden to turn left onto edge B. Or more complicated, Coming from edge A, then B, one cannot turn onto edge C. (But D -> B -> C is fine though)

For this, one has to view the entire turn restriction relation at once and then determine one or more 'forbidden sequences' out of them, thus giving `static [ForbiddenSequence] CalculateTurnRestrictions(CompleteRelation r)` 

Forbidden sequences are relatively rare and normally the same within the vehicle type. As they contain edge-ids, they will not be compressable into a lookup table (or rather: it'll be a normal array without reuse).
 To encode them, one could place the 'forbidden-sequence-id' either directly onto the edgeid (giving one extra number per vehicle the network supports) or one could create a special 'aspect' that indicates what forbidden sequence is there (copying entire entries of the lookup table).

An open question: normally a 'forbidden sequence' is only 'not accessible'. One can however wonder if there are cases where having a 'this specific sequence has this extra cost on that aspect-metric' is needed, but I cannot contrive such an example right away.

# Metadata

Apart from the calculations, every aspect and profile-file should also contain a name, a description and optionally a version number.

# Advanced preprocessing

At last, one should think about more advanced cases, such as

- Low Emission Zone Polygons
- Advanced crossroad topologies

These steps will probably need an advanced preprocessing, applying the LEZ-tags onto the underlying ways and similar tag rewriting and addition. This is for v3.0


# Navigation and turn-by-turn

Generating turn-by-turn instructions is out of scope for this issue, and should be totally decoupled from the routing profiles.

Also keep in mind that some ppl like a warning for e.g. bollards

Note that advanced preprocessing could hugely improve turn-by-turn with stuff as 'go beneath the railway bridge, then turn right', 'go over 't Albertkanaal, then turn left', turn right behind shop ABC. 

That would be tremendously cool, but is sadly tremendously out of scope

# State changes

Sometimes, changing states has a certain (fixed) penalty. Think about a cyclist having to dismount for a small patch, where dismounting and mounting takes a few seconds - even if the path where to dismount is only a few meters long.

OsmAnd has a similar system, where cars get a 'slowing down' penalty for going from a high-speed to a lower-speed segment of highway (because one has to slow down in order to be able to enter that highway).
 
