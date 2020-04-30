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


function default(defaultValue, realValue)
    if(realValue ~= nil) then
        return realValue
    end
    return defaultValue
end





function double_compare(a, b)
    if (type(a) ~= "number") then
        a = parse(a)
    end

    if(type(b) ~= "number") then
        b = parse(b)
    end
    return math.abs(a - b) > 0.001
end


function eq(a, b)
    if (a == b) then
        return "yes"
    else
        return "no"
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


function inv(n)
    return 1/n
end


function inv(n)
    return 1/n
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


function multiply(list)
    local factor = 1
    for _, value in ipairs(list) do
        factor = factor * value
    end
    return factor;
end


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


function notEq(a, b)
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


failed_profile_tests = false
--[[
expected should be a table containing 'access', 'speed' and 'weight'
]]
function unit_test_profile(profile_function, profile_name, index, expected, tags)
    result = {attributes_to_keep = {}}
    profile_function(tags, result)

    if (result.access ~= expected.access) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".access: expected " .. expected.access .. " but got " .. result.access)
        failed_profile_tests = true
    end

    if (result.access == 0) then
        -- we cannot access this road, the other results are irrelevant
        return
    end

    if (double_compare(result.speed, expected.speed)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".speed: expected " .. expected.speed .. " but got " .. result.speed)
        failed_profile_tests = true
    end

    if (double_compare(result.oneway, expected.oneway)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but got " .. result.oneway)
        failed_profile_tests = true
    end

    if (double_compare(result.oneway, expected.oneway)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but got " .. result.oneway)
        failed_profile_tests = true
    end

    if (double_compare(inv(result.factor), 0.033333)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".factor: expected " .. expected.weight .. " but got " .. inv(result.factor))
        failed_profile_tests = true
    end
end




profile_whitelist = {"access", "highway", "bicycle", "cycleway:right", "cycleway:left", "cycleway", "anyways:bicycle", "anyways:access", "anyways:construction", "oneway", "oneway:bicycle", "junction", "maxspeed", "designation", "ferry", "cyclestreet", "motor", "foot", "towpath", "type", "route"}


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
Gives a comfort factor, purely based on physical aspects of the road

Unit: [0, 2]
Created by 
Originally defined in bicycle.comfort.json
Uses tags: cyclestreet
Used parameters: 
Number of combintations: 3
Returns values: 
]]
function bicycle_comfort(parameters, tags, result)
    return default(1, multiply(table_to_list(tags, result, 
        {
            cyclestreet = {
                yes = 1.5
            }
        })))
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
The 'bicycle.network_score' is a bit of a catch-all for bicycle networks and indicates wether or not the road is part of a matching cycling network.

Unit: 
Created by 
Originally defined in bicycle.network_score.json
Uses tags: type, route
Used parameters: 
Number of combintations: 3
Returns values: 
]]
function bicycle_network_score(parameters, tags, result)
    return must_match(tags, result, 
        {"type", "route"},
        {
            type = eq("route", tags["type"]),
            route = eq("bicycle", tags["route"])
        })
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
    #defaultSpeed: double
        Used in bicycle.speed
    
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
    local oneway = bicycle_oneway(parameters, tags, result)
    local speed = 
        min({
         legal_maxspeed_be(parameters, tags, result),
         parameters["#defaultSpeed"]
        })
    local distance = 1 -- the weight per meter for distance travelled is, well, 1m/m

    local weight = 
        parameters["comfort"] * dot(bicycle_comfort(parameters, tags, result), bicycle_comfort(parameters, tags, result)) + 
        parameters["safety"] * dot(bicycle_safety(parameters, tags, result), bicycle_safety(parameters, tags, result)) + 
        parameters["network"] * dot(bicycle_network_score(parameters, tags, result), bicycle_network_score(parameters, tags, result)) + 
        parameters["time_needed"] * inv(speed) + 
        parameters["distance"] * distance


    -- put all the values into the result-table, as needed for itinero
    result.access = 1
    result.speed = speed
    result.factor = 1/weight

    if (oneway == "both") then
        result.oneway = 0
    elseif (oneway == "with") then
        result.oneway = 1
    else
         result.oneway = 2
    end

end


function default_parameters()
    return {max_speed =  30, defaultSpeed =  15, speed =  0, distance =  0, comfort =  0, safety =  0, withDirection =  "yes", againstDirection =  "yes", network =  0}
end


--[[
The fastest route to your destination
]]
function profile_bicycle_fastest(tags, result)
    local parameters = default_parameters()
    parameters.description = "The fastest route to your destination"
    parameters.time_needed = 1
    bicycle(parameters, tags, result)
end

--[[
The shortest route, independent of of speed
]]
function profile_bicycle_shortest(tags, result)
    local parameters = default_parameters()
    parameters.description = "The shortest route, independent of of speed"
    parameters.distance = 1
    bicycle(parameters, tags, result)
end

--[[
A defensive route shying away from big roads with lots of cars
]]
function profile_bicycle_safety(tags, result)
    local parameters = default_parameters()
    parameters.description = "A defensive route shying away from big roads with lots of cars"
    parameters.safety = 1
    bicycle(parameters, tags, result)
end

--[[
A comfortable route preferring well-paved roads, smaller roads and a bit of scenery at the cost of speed
]]
function profile_bicycle_comfort(tags, result)
    local parameters = default_parameters()
    parameters.description = "A comfortable route preferring well-paved roads, smaller roads and a bit of scenery at the cost of speed"
    parameters.comfort = 1
    bicycle(parameters, tags, result)
end

--[[
A route which aims to be both safe and comfortable at the cost of speed
]]
function profile_bicycle_comfort_safety(tags, result)
    local parameters = default_parameters()
    parameters.description = "A route which aims to be both safe and comfortable at the cost of speed"
    parameters.comfort = 1
    parameters.safety = 1
    bicycle(parameters, tags, result)
end

--[[
A route which prefers cycling over cycling node networks. Non-network parts prefer some comfort and safety. The on-network-part is considered flat (meaning that only distance on the node network counts)
]]
function profile_bicycle_node_networks(tags, result)
    local parameters = default_parameters()
    parameters.description = "A route which prefers cycling over cycling node networks. Non-network parts prefer some comfort and safety. The on-network-part is considered flat (meaning that only distance on the node network counts)"
    parameters.network = 20
    parameters.network_node_network_only = "yes"
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
        name = "node_networks",
        function_name = "profile_bicycle_node_networks",
        metric = "custom"
    }
}







--------------------------- Test code -------------------------



unit_test(bicycle_safety, "bicycle.safety", 0, "0.15", {}, {highway =  "primary", cycleway =  "no"})
unit_test(bicycle_safety, "bicycle.safety", 1, "0.285", {}, {highway =  "primary", cycleway =  "yes"})
unit_test(bicycle_safety, "bicycle.safety", 2, "0.45", {}, {highway =  "primary", cycleway =  "track"})
unit_test(bicycle_safety, "bicycle.safety", 3, "0.4", {}, {highway =  "secondary", cycleway =  "lane"})
unit_test(bicycle_safety, "bicycle.safety", 4, "0.9", {}, {highway =  "residential"})
unit_test(bicycle_safety, "bicycle.safety", 5, "1", {}, {highway =  "cycleway"})
unit_test(bicycle_safety, "bicycle.safety", 6, "1.35", {}, {highway =  "residential", cyclestreet =  "yes"})
unit_test(bicycle_safety, "bicycle.safety", 7, "0.95", {}, {highway =  "cycleway", foot =  "designated"})
unit_test(bicycle_safety, "bicycle.safety", 8, "0.95", {}, {highway =  "footway", foot =  "designated"})
unit_test(bicycle_safety, "bicycle.safety", 9, "1.425", {}, {highway =  "path", foot =  "designated", bicycle =  "designated"})
unit_test(bicycle_safety, "bicycle.safety", 10, "1.5", {}, {highway =  "path", bicycle =  "designated"})
unit_test(bicycle_safety, "bicycle.safety", 11, "0.4", {}, {highway =  "secondary", ["cycleway:right"] =  "lane"})


unit_test(bicycle_oneway, "bicycle.oneway", 0, "both", {}, {})
unit_test(bicycle_oneway, "bicycle.oneway", 1, "both", {}, {oneway =  "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 2, "with", {}, {oneway =  "no", ["oneway:bicycle"] =  "yes"})
unit_test(bicycle_oneway, "bicycle.oneway", 3, "with", {}, {junction =  "roundabout"})
unit_test(bicycle_oneway, "bicycle.oneway", 4, "both", {}, {oneway =  "yes", cycleway =  "opposite"})
unit_test(bicycle_oneway, "bicycle.oneway", 5, "against", {}, {oneway =  "yes", ["oneway:bicycle"] =  "-1"})
unit_test(bicycle_oneway, "bicycle.oneway", 6, "with", {}, {highway =  "residential", oneway =  "yes"})
unit_test(bicycle_oneway, "bicycle.oneway", 7, "both", {}, {highway =  "residential", oneway =  "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 8, "both", {}, {highway =  "residential", oneway =  "yes", ["oneway:bicycle"] =  "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 9, "with", {}, {highway =  "residential", junction =  "roundabout"})
unit_test(bicycle_oneway, "bicycle.oneway", 10, "both", {}, {highway =  "residential", ["oneway:bicycle"] =  "no", junction =  "roundabout"})
unit_test(bicycle_oneway, "bicycle.oneway", 11, "against", {}, {highway =  "residential", ["oneway:bicycle"] =  "-1"})
unit_test(bicycle_oneway, "bicycle.oneway", 12, "both", {}, {highway =  "residential", oneway =  "invalidKey", ["oneway:bicycle"] =  "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 13, "with", {}, {highway =  "secondary", oneway =  "yes", ["cycleway:right"] =  "track"})
unit_test(bicycle_oneway, "bicycle.oneway", 14, "both", {}, {highway =  "secondary", oneway =  "yes", ["cycleway:left"] =  "track"})
unit_test(bicycle_oneway, "bicycle.oneway", 15, "both", {}, {highway =  "secondary", oneway =  "yes", cycleway =  "track"})
unit_test(bicycle_oneway, "bicycle.oneway", 16, "with", {}, {oneway =  "yes", ["cycleway:left"] =  "no"})
unit_test(bicycle_oneway, "bicycle.oneway", 17, "both", {}, {highway =  "residential", oneway =  "yes", ["cycleway:left"] =  "lane"})


unit_test(bicycle_legal_access, "bicycle.legal_access", 0, "no", {}, {})
unit_test(bicycle_legal_access, "bicycle.legal_access", 1, "no", {}, {access =  "no"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 2, "yes", {}, {bicycle =  "yes", access =  "no"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 3, "permissive", {}, {highway =  "path"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 4, "yes", {}, {highway =  "pedestrian", bicycle =  "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 5, "dismount", {}, {highway =  "pedestrian"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 6, "designated", {}, {highway =  "cycleway"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 7, "destination", {}, {highway =  "residential", access =  "destination"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 8, "no", {}, {highway =  "residential", access =  "private"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 9, "designated", {}, {highway =  "residential", bicycle =  "designated"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 10, "designated", {}, {highway =  "motorway", bicycle =  "designated"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 11, "no", {}, {highway =  "residential", bicycle =  "use_sidepath"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 12, "yes", {}, {highway =  "residential", access =  "no", ["anyways:access"] =  "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 13, "no", {}, {highway =  "primary"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 14, "yes", {}, {highway =  "primary", ["cycleway:right"] =  "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 15, "yes", {}, {highway =  "primary", ["cycleway:right"] =  "yes"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 16, "yes", {}, {highway =  "secondary", ["cycleway:right"] =  "track"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 17, "destination", {}, {highway =  "service", access =  "destination"})
unit_test(bicycle_legal_access, "bicycle.legal_access", 18, "no", {}, {highway =  "residential", bicycle =  "use_sidepath"})

unit_test_profile(profile_bicycle_fastest, "fastest", 0, {access = 0, speed = 0, oneway = 0, weight = 0 }, {})
unit_test_profile(profile_bicycle_fastest, "fastest", 1, {access = 1, speed = 30, oneway = 0, weight = 0.03333 }, {highway =  "cycleway"})
unit_test_profile(profile_bicycle_fastest, "fastest", 2, {access = 0, speed = 0, oneway = 0, weight = 0 }, {highway =  "primary"})
unit_test_profile(profile_bicycle_fastest, "fastest", 3, {access = 1, speed = 15, oneway = 0, weight = 0.06666 }, {highway =  "pedestrian"})
unit_test_profile(profile_bicycle_fastest, "fastest", 4, {access = 1, speed = 15, oneway = 0, weight = 0.0666 }, {highway =  "pedestrian", bicycle =  "yes"})
unit_test_profile(profile_bicycle_fastest, "fastest", 5, {access = 1, speed = 30, oneway = 0, weight = 0.033333 }, {highway =  "residential"})


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