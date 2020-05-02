-- Itinero 1.0-profile, generated on 2020-05-03T00:30:52




----------------------------- UTILS ---------------------------


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


function default(defaultValue, realValue)
    if(realValue ~= nil) then
        return realValue
    end
    return defaultValue
end


function multiply(list)
    local factor = 1
    for _, value in ipairs(list) do
        factor = factor * value
    end
    return factor;
end


function table_to_list(tags, result, factor_table)
    local list = {}
    for key, mapping in pairs(factor_table) do
        local v = tags[key]
        if (v ~= nil) then
            if (type(mapping) == "table") then
                local f = mapping[v]
                if (f ~= nil) then
                    table.insert(list, f);
                    result.attributes_to_keep[key] = v
                end
            else
                table.insert(list, mapping);
                result.attributes_to_keep[key] = v
            end
        end
    end

    return list;
end


failed_tests = false
function unit_test(f, fname, index, expected, parameters, tags)
    local result = {attributes_to_keep = {}}
    local actual = f(parameters, tags, result)
    if (tostring(actual) ~= expected) then
        print("[" .. fname .. "] " .. index .. " failed: expected " .. expected .. " but got " .. tostring(actual))
        failed_tests = true
    end
end


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


function eq(a, b)
    if (a == b) then
        return "yes"
    else
        return "no"
    end
end



function containedIn(list, a)
    for _, value in ipairs(list) do
        if (value == a) then
            return true
        end
    end

    return false;
end


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


function string.start(strt, s)
    return string.sub(s, 1, string.len(strt)) == strt
end


-- every key starting with "_relation:<name>:XXX" is rewritten to "_relation:XXX"
function remove_relation_prefix(tags, name)

    local new_tags = {}
    for k, v in pairs(tags) do
        local prefix = "_relation:" .. name;
        if (string.start(prefix, k)) then
            local new_key = "_relation:" .. string.sub(k, string.len(prefix))
            new_tags[new_key] = v
        else
            new_tags[k] = v
        end
    end
    return new_tags
end


function debug_table(table, prefix)
    if (prefix == nil) then
        prefix = ""
    end
    for k, v in pairs(table) do

        if (type(v) == "table") then
            debug_table(v, "   ")
        else
            print(prefix .. tostring(k) .. " = " .. tostring(v))
        end
    end
    print("")
end


failed_profile_tests = false
--[[
expected should be a table containing 'access', 'speed' and 'weight'
]]
function unit_test_profile(profile_function, profile_name, index, expected, tags)
    local result = { attributes_to_keep = {} }
    local profile_failed = false
    profile_function(tags, result)

    local accessCorrect = (result.access == 0 and expected.access == "no") or result.access == 1
    if (not accessCorrect) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".access: expected " .. expected.access .. " but got " .. result.access)
        profile_failed = true
        failed_profile_tests = true
    end

    if (expected.access == "no") then
        -- we cannot access this road, the other results are irrelevant
        if (profile_failed) then
            print("The used tags for test " .. tostring(index) .. " are:")
            debug_table(tags)
        end
        return
    end

    if (not double_compare(result.speed, expected.speed)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".speed: expected " .. expected.speed .. " but got " .. result.speed)
        failed_profile_tests = true
        profile_failed = true
    end


    local actualOneway = result.oneway;
    if (result.oneway == 0) then
        actualOneway = "both"
    elseif (result.oneway == 1) then
        actualOneway = "with"
    elseif (result.oneway == 2) then
        actualOneway = "against"
    end

    if (expected.oneway ~= actualOneway) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but got " .. actualOneway)
        failed_profile_tests = true
        profile_failed = true
    end


    if (not double_compare(result.factor, expected.weight)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".factor: expected " .. expected.weight .. " but got " .. result.factor)
        failed_profile_tests = true
        profile_failed = true
    end

    if (profile_failed == true) then
        print("The used tags for test " .. tostring(index) .. " are:")
        debug_table(tags)
    end
end


function inv(n)
    return 1/n
end


function double_compare(a, b)
    if (b == nil) then
        return false
    end
    
    if (type(a) ~= "number") then
        a = parse(a)
    end

    if(type(b) ~= "number") then
        b = parse(b)
    end
    if (a == b) then
        return true
    end

    return math.abs(a - b) < 0.0001
end




----------------------------- PROFILE ---------------------------




profile_whitelist = {"type", "route", "state", "operator", "access", "motor", "foot", "bicycle", "cyclestreet", "towpath", "designation", "highway", "cycleway", "cycleway:left", "cycleway:right", "railway", "surface", "oneway", "oneway:bicycle", "junction", "anyways:bicycle", "anyways:access", "anyways:construction", "tracktype", "incline", "maxspeed", "ferry", "_relation:bicycle_network_by_operator"}


--[[
The 'bicycle.network_score' returns true if the way is part of a cycling network of a certain (group of) operators

Unit: 
Created by 
Originally defined in bicycle.network_by_operator.json
Uses tags: type, route, state, operator
Used parameters: #networkOperator
Number of combintations: 5
Returns values: 
]]
function bicycle_network_by_operator(parameters, tags, result)
    local funcName = "bicycle_network_by_operator"
    return member_of(funcName, parameters, tags, result)
end


--[[
The 'bicycle.network_score' returns true if the way is part of a cycling network

Unit: 
Created by 
Originally defined in bicycle.network_score.json
Uses tags: type, route, state
Used parameters: 
Number of combintations: 4
Returns values: 
]]
function bicycle_network_score(parameters, tags, result)
    local funcName = "bicycle_network_score"
    return member_of(funcName, parameters, tags, result)
end


--[[
Determines how safe a cyclist feels on a certain road, mostly based on car pressure. This is a relatively 

Unit: safety
Created by 
Originally defined in bicycle.safety.json
Uses tags: access, motor, foot, bicycle, cyclestreet, towpath, designation, highway, cycleway, cycleway:left, cycleway:right
Used parameters: 
Number of combintations: 50
Returns values: 
]]
function bicycle_safety(parameters, tags, result)
    return default(1, multiply(table_to_list(tags, result, 
        {
            access = {
                no = 1.1
            },
            motor = {
                no = 1.5
            },
            foot = {
                designated = 0.95
            },
            bicycle = {
                designated = 1.5
            },
            cyclestreet = {
                yes = 1.5
            },
            towpath = {
                yes = 1.1
            },
            designation = {
                towpath = 1.5
            },
            highway = {
                cycleway = 1,
                primary = 0.3,
                secondary = 0.4,
                tertiary = 0.5,
                unclassified = 0.8,
                track = 0.95,
                residential = 0.9,
                living_street = 1,
                footway = 1,
                path = 1
            },
            cycleway = {
                yes = 0.95,
                no = 0.5,
                lane = 1,
                shared = 0.8,
                shared_lane = 0.8,
                share_busway = 0.9,
                track = 1.5
            },
            ["cycleway:left"] = {
                yes = 0.95,
                no = 0.5,
                lane = 1,
                shared = 0.8,
                shared_lane = 0.8,
                share_busway = 0.9,
                track = 1.5
            },
            ["cycleway:right"] = {
                yes = 0.95,
                no = 0.5,
                lane = 1,
                shared = 0.8,
                shared_lane = 0.8,
                share_busway = 0.9,
                track = 1.5
            }
        })))
end


--[[
Gives a comfort factor for a road, purely based on physical aspects of the road, which is a bit subjective; this takes a bit of scnery into account with a preference for `railway=abandoned` and `towpath=yes`

Unit: [0, 2]
Created by 
Originally defined in bicycle.comfort.json
Uses tags: highway, railway, towpath, cycleway, cyclestreet, access, surface
Used parameters: 
Number of combintations: 44
Returns values: 
]]
function bicycle_comfort(parameters, tags, result)
    return default(1, multiply(table_to_list(tags, result, 
        {
            highway = {
                cycleway = 1.2,
                primary = 0.3,
                secondary = 0.4,
                tertiary = 0.5,
                unclassified = 0.8,
                track = 0.95,
                residential = 1,
                living_street = 1.1,
                footway = 0.95,
                path = 0.5
            },
            railway = {
                abandoned = 2
            },
            towpath = {
                yes = 2
            },
            cycleway = {
                track = 1.2
            },
            cyclestreet = {
                yes = 1.1
            },
            access = {
                designated = 1.2,
                dismount = 0.5
            },
            surface = {
                paved = 0.99,
                ["concrete:lanes"] = 0.8,
                ["concrete:plates"] = 1,
                sett = 0.9,
                unhewn_cobblestone = 0.75,
                cobblestone = 0.8,
                unpaved = 0.75,
                compacted = 1.1,
                fine_gravel = 0.99,
                gravel = 0.9,
                dirt = 0.6,
                earth = 0.6,
                grass = 0.6,
                grass_paver = 0.9,
                ground = 0.7,
                sand = 0.5,
                woodchips = 0.5,
                snow = 0.5,
                pebblestone = 0.5,
                mud = 0.4
            }
        })))
end


--[[
Determines wether or not a bicycle can go in both ways in this street, and if it is oneway, in what direction

Unit: both: direction is allowed in both direction
with: this is a oneway street with direction allowed with the grain of the way
against: oneway street with direction against the way
Created by 
Originally defined in bicycle.oneway.json
Uses tags: oneway, oneway:bicycle, junction, cycleway, cycleway:left
Used parameters: 
Number of combintations: 28
Returns values: 
]]
function bicycle_oneway(parameters, tags, result)
    return default("both", first_match_of(tags, result, 
        {"oneway:bicycle", "junction", "cycleway", "cycleway:left", "oneway"},
        {
            oneway = {
                yes = "with",
                no = "both",
                ["1"] = "with",
                ["-1"] = "against"
            },
            ["oneway:bicycle"] = {
                yes = "with",
                no = "both",
                ["1"] = "with",
                ["-1"] = "against"
            },
            junction = {
                roundabout = "with"
            },
            cycleway = {
                right = "against",
                opposite_lane = "both",
                track = "both",
                lane = "both",
                opposite = "both",
                opposite_share_busway = "both",
                opposite_track = "both"
            },
            ["cycleway:left"] = {
                no = "with",
                yes = "both",
                lane = "both",
                track = "both",
                shared_lane = "both",
                share_busway = "both"
            }
        }))
end


--[[
Gives, for each type of highway, whether or not a normal bicycle can enter legally.
Note that legal access is a bit 'grey' in the case of roads marked private and permissive, in which case these values are returned 

Unit: 'designated': Access is allowed and even specifically for bicycles
'yes': bicycles are allowed here
'permissive': bicycles are allowed here, but this might be a private road or service where usage is allowed, but uncommon
'dismount': cycling here is not allowed, but walking with the bicycle is
'destination': cycling is allowed here, but only if truly necessary to reach the destination
'private': this is a private road, only go here if the destination is here
'no': do not cycle here
Created by 
Originally defined in bicycle.legal_access.json
Uses tags: access, highway, bicycle, cycleway:right, cycleway:left, cycleway, anyways:bicycle, anyways:access, anyways:construction
Used parameters: 
Number of combintations: 48
Returns values: 
]]
function bicycle_legal_access(parameters, tags, result)
    return default("no", first_match_of(tags, result, 
        {"anyways:bicycle", "anyways:access", "anyways:construction", "bicycle", "access", "cycleway:right", "cycleway:left", "cycleway", "highway"},
        {
            access = {
                no = "no",
                customers = "no",
                private = "no",
                permissive = "permissive",
                destination = "destination",
                delivery = "destination",
                service = "destination"
            },
            highway = {
                cycleway = "designated",
                residential = "yes",
                living_street = "yes",
                service = "permissive",
                services = "permissive",
                track = "yes",
                crossing = "dismount",
                footway = "dismount",
                pedestrian = "dismount",
                corridor = "dismount",
                path = "permissive",
                primary = "no",
                primary_link = "no",
                secondary = "yes",
                secondary_link = "yes",
                tertiary = "yes",
                tertiary_link = "yes",
                unclassified = "yes",
                road = "yes"
            },
            bicycle = {
                yes = "yes",
                no = "no",
                use_sidepath = "no",
                designated = "designated",
                permissive = "permissive",
                private = "private",
                official = "designated",
                dismount = "dismount"
            },
            ["cycleway:right"] = notEq("no", tags["cycleway:right"]),
            ["cycleway:left"] = notEq("no", tags["cycleway:left"]),
            cycleway = notEq("no", tags["cycleway"]),
            ["anyways:bicycle"] = tags["anyways:bicycle"],
            ["anyways:access"] = {
                no = "no",
                destination = "destination",
                yes = "yes"
            },
            ["anyways:construction"] = {
                yes = "no"
            }
        }))
end


--[[
Calculates a speed factor for bicycles based on physical features, e.g. a sand surface will slow a cyclist down; going over pedestrian areas even more, ...

Unit: 
Created by 
Originally defined in bicycle.speed_factor.json
Uses tags: access, highway, surface, tracktype, incline
Used parameters: 
Number of combintations: 49
Returns values: 
]]
function bicycle_speed_factor(parameters, tags, result)
    return multiply(table_to_list(tags, result, 
        {
            access = {
                dismount = 0.15
            },
            highway = {
                path = 0.5,
                track = 0.7
            },
            surface = {
                paved = 0.99,
                asphalt = 1,
                concrete = 1,
                metal = 1,
                wood = 1,
                ["concrete:lanes"] = 0.95,
                ["concrete:plates"] = 1,
                paving_stones = 1,
                sett = 0.9,
                unhewn_cobblestone = 0.75,
                cobblestone = 0.8,
                unpaved = 0.75,
                compacted = 0.99,
                fine_gravel = 0.99,
                gravel = 0.9,
                dirt = 0.6,
                earth = 0.6,
                grass = 0.6,
                grass_paver = 0.9,
                ground = 0.7,
                sand = 0.5,
                woodchips = 0.5,
                snow = 0.5,
                pebblestone = 0.5,
                mud = 0.4
            },
            tracktype = {
                ["grade1"] = 0.99,
                ["grade2"] = 0.8,
                ["grade3"] = 0.6,
                ["grade4"] = 0.3,
                ["grade5"] = 0.1
            },
            incline = {
                up = 0.75,
                down = 1.25,
                ["0"] = 1,
                ["0%"] = 1,
                ["10%"] = 0.9,
                ["-10%"] = 1.1,
                ["20%"] = 0.8,
                ["-20%"] = 1.2,
                ["30%"] = 0.7,
                ["-30%"] = 1.3
            }
        }))
end


--[[
Gives, for each type of highway, which the default legal maxspeed is in Belgium. This file is intended to be reused for in all vehicles, from pedestrian to car. In some cases, a legal maxspeed is not really defined (e.g. on footways). In that case, a socially acceptable speed should be taken (e.g.: a bicycle on a pedestrian path will go say around 12km/h)

Unit: km/h
Created by 
Originally defined in legal_maxspeed_be.json
Uses tags: maxspeed, highway, designation, ferry
Used parameters: 
Number of combintations: 27
Returns values: 
]]
function legal_maxspeed_be(parameters, tags, result)
    return default(30, first_match_of(tags, result, 
        {"maxspeed", "designation", "highway", "ferry"},
        {
            maxspeed = parse(tags["maxspeed"]),
            highway = {
                cycleway = 30,
                footway = 20,
                crossing = 20,
                pedestrian = 15,
                path = 15,
                corridor = 5,
                residential = 30,
                living_street = 20,
                service = 30,
                services = 30,
                track = 50,
                unclassified = 50,
                road = 50,
                motorway = 120,
                motorway_link = 120,
                primary = 90,
                primary_link = 90,
                secondary = 50,
                secondary_link = 50,
                tertiary = 50,
                tertiary_link = 50
            },
            designation = {
                towpath = 30
            },
            ferry = 5
        }))
end


--[[
Function preprocessing needed for aspect bicycle.network_by_operator, called by the relation preprocessor

Unit: 
Created by Generator
Originally defined in NA
Uses tags: type, route, state, operator
Used parameters: #networkOperator
Number of combintations: 5
Returns values: 
]]
function relation_preprocessing_for_bicycle_network_by_operator(parameters, tags, result)
    return must_match(tags, result, 
        {"type", "route", "state", "operator"},
        {
            type = eq("route", tags["type"]),
            route = eq("bicycle", tags["route"]),
            state = notEq("proposed", tags["state"]),
            operator = containedIn(parameters["networkOperator"], tags["operator"])
        })
end




-- Processes the relation. All tags which are added to result.attributes_to_keep will be copied to 'attributes' of each individual way
function relation_tag_processor(relation_tags, result)
    local parameters = {}
    local subresult = {}
    local matched = false
    result.attributes_to_keep = {}


    subresult.attributes_to_keep = {}
    parameters = default_parameters()
    parameters.timeNeeded = 1

    matched = relation_preprocessing_for_bicycle_network_by_operator(parameters, relation_tags, subresult)
    if (matched) then
        result.attributes_to_keep["_relation:fastest:bicycle_network_by_operator"] = "yes"
    end


    subresult.attributes_to_keep = {}
    parameters = default_parameters()
    parameters.distance = 1

    matched = relation_preprocessing_for_bicycle_network_by_operator(parameters, relation_tags, subresult)
    if (matched) then
        result.attributes_to_keep["_relation:shortest:bicycle_network_by_operator"] = "yes"
    end


    subresult.attributes_to_keep = {}
    parameters = default_parameters()
    parameters.safety = 1

    matched = relation_preprocessing_for_bicycle_network_by_operator(parameters, relation_tags, subresult)
    if (matched) then
        result.attributes_to_keep["_relation:safety:bicycle_network_by_operator"] = "yes"
    end


    subresult.attributes_to_keep = {}
    parameters = default_parameters()
    parameters.comfort = 1

    matched = relation_preprocessing_for_bicycle_network_by_operator(parameters, relation_tags, subresult)
    if (matched) then
        result.attributes_to_keep["_relation:comfort:bicycle_network_by_operator"] = "yes"
    end


    subresult.attributes_to_keep = {}
    parameters = default_parameters()
    parameters.comfort = 1
    parameters.safety = 1

    matched = relation_preprocessing_for_bicycle_network_by_operator(parameters, relation_tags, subresult)
    if (matched) then
        result.attributes_to_keep["_relation:comfort_safety:bicycle_network_by_operator"] = "yes"
    end


    subresult.attributes_to_keep = {}
    parameters = default_parameters()
    parameters.network = 20
    parameters.networkOperator = {"Brussels Mobility"}
    parameters.comfort = 1
    parameters.safety = 1

    matched = relation_preprocessing_for_bicycle_network_by_operator(parameters, relation_tags, subresult)
    if (matched) then
        result.attributes_to_keep["_relation:brussels:bicycle_network_by_operator"] = "yes"
    end
end




name = "bicycle"
normalize = false
vehicle_type = {"vehicle", "bicycle"}
meta_whitelist = {"name", "bridge", "tunnel", "colour", "cycle_network_colour", "ref", "status", "network"}



--[[
bicycle
This is the main function called to calculate the access, oneway and speed.
Comfort is calculated as well, based on the parameters which are padded in

Created by 
Originally defined in /home/pietervdvn/werk/AspectedRouting/AspectedRouting/Profiles/bicycle
Used parameters: 
    #maxspeed: pdouble
        Used in bicycle.speed
    #defaultSpeed: pdouble
        Used in bicycle.speed
    comfort: $parameter
        Used in bicycle.priority.lefthand
    safety: $parameter
        Used in bicycle.priority.lefthand
    network: $parameter
        Used in bicycle.priority.lefthand
    timeNeeded: $parameter
        Used in bicycle.priority.lefthand
    distance: $parameter
        Used in bicycle.priority.lefthand
    #networkOperator: list (string)
        Used in bicycle.network_by_operator
    
]]
function bicycle(parameters, tags, result)

    -- initialize the result table on the default values
    result.access = 0
    result.speed = 0
    result.factor = 1
    result.direction = 0
    result.canstop = true
    result.attributes_to_keep = {}

    local access = bicycle_legal_access(parameters, tags, result)
    if (access == nil or access == "no") then
         return
    end
    tags.access = access
    local oneway = bicycle_oneway(parameters, tags, result)
    tags.oneway = oneway
    local speed = 
        min({
         legal_maxspeed_be(parameters, tags, result),
         parameters["maxspeed"],
         
        multiply({
         parameters["defaultSpeed"],
         bicycle_speed_factor(parameters, tags, result)
        })
        })
    tags.speed = speed
    local distance = 1 -- the weight per meter for distance travelled is, well, 1m/m

    local priority = 
        parameters["comfort"] * bicycle_comfort(parameters, tags, result) + 
        parameters["safety"] * bicycle_safety(parameters, tags, result) + 
        parameters["network"] * parse(bicycle_network_by_operator(parameters, tags, result)) + 
        parameters["timeNeeded"] * speed + 
        parameters["distance"] * distance


    -- put all the values into the result-table, as needed for itinero
    result.access = 1
    result.speed = speed
    result.factor = priority

    if (oneway == "both") then
        result.oneway = 0
    elseif (oneway == "with") then
        result.oneway = 1
    else
         result.oneway = 2
    end

end


function default_parameters()
    local parameters = {}
    parameters.defaultSpeed = 15
    parameters.maxspeed = 30
    parameters.timeNeeded = 0
    parameters.distance = 0
    parameters.comfort = 0
    parameters.safety = 0
    parameters.network = 0
    parameters.networkOperator = {}

    return parameters
end


--[[
"The fastest route to your destination"
]]
function profile_bicycle_fastest(tags, result)
    tags = remove_relation_prefix(tags, "fastest")
    local parameters = default_parameters()
    parameters.timeNeeded = 1
    bicycle(parameters, tags, result)
end

--[[
"The shortest route, independent of of speed"
]]
function profile_bicycle_shortest(tags, result)
    tags = remove_relation_prefix(tags, "shortest")
    local parameters = default_parameters()
    parameters.distance = 1
    bicycle(parameters, tags, result)
end

--[[
"A defensive route shying away from big roads with lots of cars"
]]
function profile_bicycle_safety(tags, result)
    tags = remove_relation_prefix(tags, "safety")
    local parameters = default_parameters()
    parameters.safety = 1
    bicycle(parameters, tags, result)
end

--[[
"A comfortable route preferring well-paved roads, smaller roads and a bit of scenery at the cost of speed"
]]
function profile_bicycle_comfort(tags, result)
    tags = remove_relation_prefix(tags, "comfort")
    local parameters = default_parameters()
    parameters.comfort = 1
    bicycle(parameters, tags, result)
end

--[[
"A route which aims to be both safe and comfortable at the cost of speed"
]]
function profile_bicycle_comfort_safety(tags, result)
    tags = remove_relation_prefix(tags, "comfort_safety")
    local parameters = default_parameters()
    parameters.comfort = 1
    parameters.safety = 1
    bicycle(parameters, tags, result)
end

--[[
"A route preferring the cycling network by operator 'Brussels Mobility'"
]]
function profile_bicycle_brussels(tags, result)
    tags = remove_relation_prefix(tags, "brussels")
    local parameters = default_parameters()
    parameters.network = 20
    parameters.networkOperator = {"Brussels Mobility"}
    parameters.comfort = 1
    parameters.safety = 1
    bicycle(parameters, tags, result)
end



profiles = {
    {
    name = "fastest",
        function_name = "profile_bicycle_fastest",
        metric = "custom"
    },
    {
        name = "shortest",
        function_name = "profile_bicycle_shortest",
        metric = "custom"
    },
    {
        name = "safety",
        function_name = "profile_bicycle_safety",
        metric = "custom"
    },
    {
        name = "comfort",
        function_name = "profile_bicycle_comfort",
        metric = "custom"
    },
    {
        name = "comfort_safety",
        function_name = "profile_bicycle_comfort_safety",
        metric = "custom"
    },
    {
        name = "brussels",
        function_name = "profile_bicycle_brussels",
        metric = "custom"
    }
}




 ------------------------------- TESTS -------------------------


unit_test(bicycle_safety, "bicycle.safety", 0, "0.15", {}, {highway = "primary", cycleway = "no"})
unit_test(bicycle_safety, "bicycle.safety", 1, "0.285", {}, {highway = "primary", cycleway = "yes"})
unit_test(bicycle_safety, "bicycle.safety", 2, "0.45", {}, {highway = "primary", cycleway = "track"})
unit_test(bicycle_safety, "bicycle.safety", 3, "0.4", {}, {highway = "secondary", cycleway = "lane"})
unit_test(bicycle_safety, "bicycle.safety", 4, "0.9", {}, {highway = "residential"})
unit_test(bicycle_safety, "bicycle.safety", 5, "1", {}, {highway = "cycleway"})
unit_test(bicycle_safety, "bicycle.safety", 6, "1.35", {}, {highway = "residential", cyclestreet = "yes"})
unit_test(bicycle_safety, "bicycle.safety", 7, "0.95", {}, {highway = "cycleway", foot = "designated"})
unit_test(bicycle_safety, "bicycle.safety", 8, "0.95", {}, {highway = "footway", foot = "designated"})
unit_test(bicycle_safety, "bicycle.safety", 9, "1.425", {}, {highway = "path", foot = "designated", bicycle = "designated"})
unit_test(bicycle_safety, "bicycle.safety", 10, "1.5", {}, {highway = "path", bicycle = "designated"})
unit_test(bicycle_safety, "bicycle.safety", 11, "0.4", {}, {highway = "secondary", ["cycleway:right"] = "lane"})


unit_test(bicycle_comfort, "bicycle.comfort", 0, "1", {}, {})
unit_test(bicycle_comfort, "bicycle.comfort", 1, "1", {}, {highway = "residential"})
unit_test(bicycle_comfort, "bicycle.comfort", 2, "1.1", {}, {highway = "residential", cyclestreet = "yes"})
unit_test(bicycle_comfort, "bicycle.comfort", 3, "1.2", {}, {highway = "cycleway"})
unit_test(bicycle_comfort, "bicycle.comfort", 4, "1.2", {}, {highway = "cycleway", foot = "designated"})
unit_test(bicycle_comfort, "bicycle.comfort", 5, "0.5", {}, {highway = "path", foot = "designated", bicycle = "designated"})
unit_test(bicycle_comfort, "bicycle.comfort", 6, "0.5", {}, {highway = "path", bicycle = "designated"})
unit_test(bicycle_comfort, "bicycle.comfort", 7, "0.95", {}, {highway = "footway", foot = "designated"})
unit_test(bicycle_comfort, "bicycle.comfort", 8, "0.3", {}, {highway = "primary", cycleway = "no"})
unit_test(bicycle_comfort, "bicycle.comfort", 9, "0.3", {}, {highway = "primary", cycleway = "yes"})
unit_test(bicycle_comfort, "bicycle.comfort", 10, "0.36", {}, {highway = "primary", cycleway = "track"})
unit_test(bicycle_comfort, "bicycle.comfort", 11, "0.4", {}, {highway = "secondary", cycleway = "lane"})
unit_test(bicycle_comfort, "bicycle.comfort", 12, "0.4", {}, {highway = "secondary", ["cycleway:right"] = "lane", surface = "asphalt"})
unit_test(bicycle_comfort, "bicycle.comfort", 13, "1.1", {}, {highway = "residential", cyclestreet = "yes", surface = "asphalt"})
unit_test(bicycle_comfort, "bicycle.comfort", 14, "2", {}, {railway = "abandoned"})
unit_test(bicycle_comfort, "bicycle.comfort", 15, "2", {}, {towpath = "yes"})
unit_test(bicycle_comfort, "bicycle.comfort", 16, "4", {}, {railway = "abandoned", towpath = "yes"})


unit_test(bicycle_oneway, "bicycle.oneway", 0, "both", {}, {})
unit_test(bicycle_oneway, "bicycle.oneway", 1, "both", {}, {oneway = "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 2, "with", {}, {oneway = "no", ["oneway:bicycle"] = "yes"})
unit_test(bicycle_oneway, "bicycle.oneway", 3, "with", {}, {junction = "roundabout"})
unit_test(bicycle_oneway, "bicycle.oneway", 4, "both", {}, {oneway = "yes", cycleway = "opposite"})
unit_test(bicycle_oneway, "bicycle.oneway", 5, "against", {}, {oneway = "yes", ["oneway:bicycle"] = "-1"})
unit_test(bicycle_oneway, "bicycle.oneway", 6, "with", {}, {highway = "residential", oneway = "yes"})
unit_test(bicycle_oneway, "bicycle.oneway", 7, "both", {}, {highway = "residential", oneway = "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 8, "both", {}, {highway = "residential", oneway = "yes", ["oneway:bicycle"] = "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 9, "with", {}, {highway = "residential", junction = "roundabout"})
unit_test(bicycle_oneway, "bicycle.oneway", 10, "both", {}, {highway = "residential", ["oneway:bicycle"] = "no", junction = "roundabout"})
unit_test(bicycle_oneway, "bicycle.oneway", 11, "against", {}, {highway = "residential", ["oneway:bicycle"] = "-1"})
unit_test(bicycle_oneway, "bicycle.oneway", 12, "both", {}, {highway = "residential", oneway = "invalidKey", ["oneway:bicycle"] = "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 13, "with", {}, {highway = "secondary", oneway = "yes", ["cycleway:right"] = "track"})
unit_test(bicycle_oneway, "bicycle.oneway", 14, "both", {}, {highway = "secondary", oneway = "yes", ["cycleway:left"] = "track"})
unit_test(bicycle_oneway, "bicycle.oneway", 15, "both", {}, {highway = "secondary", oneway = "yes", cycleway = "track"})
unit_test(bicycle_oneway, "bicycle.oneway", 16, "with", {}, {oneway = "yes", ["cycleway:left"] = "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 17, "both", {}, {highway = "residential", oneway = "yes", ["cycleway:left"] = "lane"})


unit_test(bicycle_legal_access, "bicycle.legal_access", 0, "no", {}, {})
unit_test(bicycle_legal_access, "bicycle.legal_access", 1, "no", {}, {access = "no"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 2, "yes", {}, {bicycle = "yes", access = "no"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 3, "permissive", {}, {highway = "path"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 4, "yes", {}, {highway = "pedestrian", bicycle = "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 5, "dismount", {}, {highway = "pedestrian"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 6, "designated", {}, {highway = "cycleway"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 7, "destination", {}, {highway = "residential", access = "destination"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 8, "no", {}, {highway = "residential", access = "private"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 9, "designated", {}, {highway = "residential", bicycle = "designated"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 10, "designated", {}, {highway = "motorway", bicycle = "designated"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 11, "no", {}, {highway = "residential", bicycle = "use_sidepath"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 12, "yes", {}, {highway = "residential", access = "no", ["anyways:access"] = "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 13, "no", {}, {highway = "primary"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 14, "yes", {}, {highway = "primary", ["cycleway:right"] = "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 15, "yes", {}, {highway = "primary", ["cycleway:right"] = "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 16, "yes", {}, {highway = "secondary", ["cycleway:right"] = "track"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 17, "destination", {}, {highway = "service", access = "destination"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 18, "no", {}, {highway = "residential", bicycle = "use_sidepath"})


unit_test(bicycle_speed_factor, "bicycle.speed_factor", 0, "1", {}, {})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 1, "1", {}, {highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 2, "0.75", {}, {incline = "up", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 3, "1.25", {}, {incline = "down", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 4, "0.3", {}, {incline = "up", surface = "mud", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 5, "1.125", {}, {incline = "down", surface = "sett", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 6, "0.675", {}, {incline = "up", surface = "sett", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 7, "0.9", {}, {surface = "sett", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 8, "1", {}, {surface = "asphalt", highway = "residential"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 9, "0.15", {}, {highway = "residential", access = "dismount"})
unit_test(bicycle_speed_factor, "bicycle.speed_factor", 10, "0.0315", {}, {incline = "up", surface = "mud", highway = "track", access = "dismount"})


unit_test_profile(profile_bicycle_fastest, "fastest", 0, {access = "no", speed = 0, oneway = "both", weight = 0 }, {})
unit_test_profile(profile_bicycle_fastest, "fastest", 1, {access = "designated", speed = 15, oneway = "both", weight = 15 }, {highway = "cycleway"})
unit_test_profile(profile_bicycle_fastest, "fastest", 2, {access = "no", speed = 0, oneway = "both", weight = 0 }, {highway = "primary"})
unit_test_profile(profile_bicycle_fastest, "fastest", 3, {access = "dismount", speed = 2.25, oneway = "both", weight = 2.25 }, {highway = "pedestrian"})
unit_test_profile(profile_bicycle_fastest, "fastest", 4, {access = "yes", speed = 15, oneway = "both", weight = 15 }, {highway = "pedestrian", bicycle = "yes"})
unit_test_profile(profile_bicycle_fastest, "fastest", 5, {access = "yes", speed = 15, oneway = "both", weight = 15 }, {highway = "residential"})


unit_test_profile(profile_bicycle_brussels, "brussels", 0, {access = "no", speed = 0, oneway = "both", weight = 0 }, {})
unit_test_profile(profile_bicycle_brussels, "brussels", 1, {access = "yes", speed = 15, oneway = "both", weight = 1.9 }, {highway = "residential"})
unit_test_profile(profile_bicycle_brussels, "brussels", 2, {access = "yes", speed = 15, oneway = "both", weight = 21.9 }, {highway = "residential", ["_relation:bicycle_network_by_operator"] = "yes"})



if (itinero == nil) then
    itinero = {}
    itinero.log = print

    -- Itinero is not defined -> we are running from a lua interpreter -> the tests are intended
    runTests = true


else
    print = itinero.log
end

if (not failed_tests and not failed_profile_tests) then
    print("Tests OK")
end