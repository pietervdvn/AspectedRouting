name = "bicycle.fastest"
generationDate = "2021-01-27T15:51:22"
description = "The fastest route to your destination (Profile for a normal bicycle)"

--[[
Calculate the actual factor.forward and factor.backward for a segment with the given properties
]]
function factor(tags, result)
    
    -- initialize the result table on the default values
    result.forward_speed = 0
    result.backward_speed = 0
    result.forward = 0
    result.backward = 0
    result.canstop = true
    result.attributes_to_keep = {} -- not actually used anymore, but the code generation still uses this


    local parameters = default_parameters()
    parameters.timeNeeded = 1


    local oneway = bicycle_oneway(parameters, tags, result)
    tags.oneway = oneway
    -- An aspect describing oneway should give either 'both', 'against' or 'width'


    -- forward calculation. We set the meta tag '_direction' to 'width' to indicate that we are going forward. The other functions will pick this up
    tags["_direction"] = "with"
    local access_forward = bicycle_legal_access(parameters, tags, result)
    if(oneway == "against") then
        -- no 'oneway=both' or 'oneway=with', so we can only go back over this segment
        -- we overwrite the 'access_forward'-value with no; whatever it was...
        access_forward = "no"
    end
    if(access_forward ~= nil and access_forward ~= "no") then
        tags.access = access_forward -- might be relevant, e.g. for 'access=dismount' for bicycles
        result.forward_speed = 
            min({
             legal_maxspeed_be(parameters, tags, result),
             parameters["defaultSpeed"]
            })
        tags.speed = result.forward_speed
        local priority = calculate_priority(parameters, tags, result, access_forward, oneway, result.forward_speed)
        if (priority <= 0) then
            result.forward_speed = 0
        else
            result.forward = 1 / priority
         end
    end

    -- backward calculation
    tags["_direction"] = "against" -- indicate the backward direction to priority calculation
    local access_backward = bicycle_legal_access(parameters, tags, result)
    if(oneway == "with") then
        -- no 'oneway=both' or 'oneway=against', so we can only go forward over this segment
        -- we overwrite the 'access_forward'-value with no; whatever it was...
        access_backward = "no"
    end
    if(access_backward ~= nil and access_backward ~= "no") then
        tags.access = access_backward
        result.backward_speed = 
            min({
             legal_maxspeed_be(parameters, tags, result),
             parameters["defaultSpeed"]
            })
        tags.speed = result.backward_speed
        local priority = calculate_priority(parameters, tags, result, access_backward, oneway, result.backward_speed)
        if (priority <= 0) then
            result.backward_speed = 0
        else
            result.backward = 1 / priority
         end
    end
end

--[[
Generates the factor according to the priorities and the parameters for this behaviour
Note: 'result' is not actually used
]]
function calculate_priority(parameters, tags, result, access, oneway, speed)
    local distance = 1
    local priority = 
        1 * speed
    return priority
end

function default_parameters()
    local parameters = {}
    parameters.defaultSpeed = 15
    parameters.timeNeeded = 0
    parameters.distance = 0
    parameters.comfort = 0

    return parameters
end


--[[
Gives, for each type of highway, whether or not a normal bicycle can enter legally.
Note that legal access is a bit 'grey' in the case of roads marked private and permissive, in which case these values are returned 

Unit: 'designated': Access is allowed and even specifically for bicycles
'yes': bicycles are allowed here
'permissive': bicycles are allowed here, but this might be a private road or service where usage is allowed, but could be retracted one day by the owner
'dismount': cycling here is not allowed, but walking with the bicycle is
'destination': cycling is allowed here, but only if truly necessary to reach the destination
'private': this is a private road, only go here if the destination is here
'no': do not cycle here
Created by 
Originally defined in bicycle.legal_access.json
Uses tags: access, highway, service, bicycle, anyways:bicycle, anyways:access, anyways:construction
Used parameters: 
Number of combintations: 54
Returns values: 
]]
function bicycle_legal_access(parameters, tags, result)
    return default("no", first_match_of(tags, result, 
        {"anyways:bicycle", "anyways:access", "anyways:construction", "bicycle", "access", "service", "highway"},
        {
            access = {
                no = "no",
                customers = "private",
                private = "private",
                permissive = "permissive",
                destination = "destination",
                delivery = "destination",
                service = "destination",
                permit = "destination"
            },
            highway = {
                cycleway = "designated",
                residential = "yes",
                living_street = "yes",
                service = "yes",
                services = "yes",
                track = "yes",
                crossing = "dismount",
                footway = "dismount",
                pedestrian = "dismount",
                corridor = "dismount",
                construction = "dismount",
                steps = "dismount",
                path = "yes",
                primary = "yes",
                primary_link = "yes",
                secondary = "yes",
                secondary_link = "yes",
                tertiary = "yes",
                tertiary_link = "yes",
                unclassified = "yes",
                road = "yes"
            },
            service = {
                parking_aisle = "permissive",
                driveway = "private",
                alley = "yes",
                bus = "no"
            },
            bicycle = {
                yes = "yes",
                no = "no",
                use_sidepath = "no",
                designated = "designated",
                permissive = "permissive",
                private = "private",
                official = "designated",
                dismount = "dismount",
                permit = "destination"
            },
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
Number of combintations: 32
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
                none = "with",
                yes = "both",
                lane = "both",
                track = "both",
                shared_lane = "both",
                share_busway = "both",
                opposite_lane = "both",
                opposite_track = "both",
                opposite = "both"
            }
        }))
end

--[[
Gives, for each type of highway, which the default legal maxspeed is in Belgium. This file is intended to be reused for in all vehicles, from pedestrian to car. In some cases, a legal maxspeed is not really defined (e.g. on footways). In that case, a socially acceptable speed should be taken (e.g.: a bicycle on a pedestrian path will go say around 12km/h)

Unit: km/h
Created by 
Originally defined in legal_maxspeed_be.json
Uses tags: maxspeed, highway, designation
Used parameters: 
Number of combintations: 26
Returns values: 
]]
function legal_maxspeed_be(parameters, tags, result)
    return default(30, first_match_of(tags, result, 
        {"maxspeed", "designation", "highway"},
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
            }
        }))
end

failed_profile_tests = false
--[[
Unit test of a behaviour function for an itinero 2.0 profile
]]

function unit_test_profile(profile_function, profile_name, index, expected, tags)
    -- Note: we don't actually use 'profile_function'

    local result = {}
    local profile_failed = false
    factor(tags, result)
    
 
    local forward_access = result.forward_speed > 0 and result.forward > 0;
    local backward_access = result.backward_speed > 0 and result.backward > 0;

    if (not forward_access and not backward_access) then

        if (expected.access == "no" or expected.speed <= 0 or expected.priority <= 0) then
            -- All is fine, we can't access this thing anyway
            return
        end

        profile_failed = true
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".access: expected " .. expected.access .. " but forward and backward are 0 (for either speed or factor)")
    end

    if (expected.oneway == "with") then
        if (backward_access) then
            -- we can go against the direction, not good
            print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but going against the direction is possible")
            profile_failed = true;
        end
        if (not forward_access) then
            print("Test " .. tostring(index) .. " warning for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but going with the direction is not possible")
        end
    end

    if (expected.oneway == "against") then
        if (forward_access) then
            -- we can go against the direction, not good
            print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but going with the direction is possible")
        end
        if (not backward_access) then
            print("Test " .. tostring(index) .. " warning for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but going against the direction is not possible")
        end
    end

    if (result.forward_speed ~= expected.speed and result.backward_speed ~= expected.speed) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".speed: expected " .. expected.speed .. " but got " .. result.forward_speed .. " forward and " .. result.backward_speed .. " backward")
        profile_failed = true;
    end


    if (result.forward ~= expected.priority and result.backward ~= expected.priority) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".priority: expected " .. expected.priority .. " but got " .. result.forward .. " forward and " .. result.backward .. " backward")
        profile_failed = true;
    end

    if(profile_failed) then
        failed_profile_tests = true;
        debug_table(tags, "tags: ")
        debug_table(expected, "expected: ")
        debug_table(result, "result: ")
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

function debug_table_str(table, prefix)
    if (prefix == nil) then
        prefix = ""
    end
    local str = "";
    for k, v in pairs(table) do

        if (type(v) == "table") then
            str = str .. "," .. debug_table_str(v, "   ")
        else
            str = str .. "," .. (prefix .. tostring(k) .. " = " .. tostring(v))
        end
    end
    return str
end

function default(defaultValue, realValue)
    if(realValue ~= nil) then
        return realValue
    end
    return defaultValue
end

function first_match_of(tags, result, order_of_keys, table)
    for _, key in pairs(order_of_keys) do
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

    if(string:match("%d+:%d+")) then
        -- duration in minute
        local duration = 0
        for part in string:gmatch "%d+" do
            duration = duration * 60 + tonumber(part)
        end
        return duration
    end


    return tonumber(string)
end

function eq(a, b)
    if (a == b) then
        return "yes"
    else
        return "no"
    end
end


function min(list)
    local min
    for _, value in pairs(list) do
        if(value ~= nil) then
            if (min == nil) then
                min = value
            elseif (value < min) then
                min = value
            end
        end
    end

    return min;
end

function test_all()


    


  -- Behaviour tests --


    unit_test_profile(behaviour_bicycle_fastest, "fastest", 0, {access = "no", speed = 0, oneway = "both", priority = inv(0) }, {})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 1, {access = "designated", speed = 15, oneway = "both", priority = inv(15) }, {highway = "cycleway"})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 2, {access = "yes", speed = 15, oneway = "both", priority = inv(15) }, {highway = "residential"})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 3, {access = "yes", speed = 15, oneway = "both", priority = inv(15) }, {highway = "pedestrian", bicycle = "yes"})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 4, {access = "yes", speed = 15, oneway = "both", priority = inv(15) }, {highway = "unclassified", ["cycleway:left"] = "track", oneway = "yes", ["oneway:bicycle"] = "no"})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 5, {access = "yes", speed = 15, oneway = "both", priority = inv(15) }, {highway = "service"})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 6, {access = "yes", speed = 15, oneway = "both", priority = inv(15) }, {highway = "tertiary", access = "yes", maxspeed = "50"})
    unit_test_profile(behaviour_bicycle_fastest, "fastest", 7, {access = "yes", speed = 15, oneway = "with", priority = inv(15) }, {highway = "residential", junction = "roundabout"})


end

if (itinero == nil) then
    itinero = {}
    itinero.log = print

    -- Itinero is not defined -> we are running from a lua interpreter -> the tests are intended
    runTests = true


else
    print = itinero.log
end

test_all()
if (not failed_tests and not failed_profile_tests and print ~= nil) then
    print("Tests OK")
end