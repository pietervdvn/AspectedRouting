name = "bicycle.shortest"
generationDate = "2021-01-27T15:51:22"
description = "The shortest route, independent of of speed (Profile for a normal bicycle)"

--[[
Calculate the actual factor.forward and factor.backward for a segment with the given properties
]]
function factor(tags, result)
    
    -- Cleanup the relation tags to make them usable with this profile
    tags = remove_relation_prefix(tags, "shortest")
    
    -- initialize the result table on the default values
    result.forward_speed = 0
    result.backward_speed = 0
    result.forward = 0
    result.backward = 0
    result.canstop = true
    result.attributes_to_keep = {} -- not actually used anymore, but the code generation still uses this


    local parameters = default_parameters()
    parameters.distance = 1


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
        1 * distance
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
    local result
    if (tags["highway"] ~= nil) then
        local v
        v = tags["highway"]
        if (v == "cycleway") then
            result = "designated"
        elseif (v == "residential") then
            result = "yes"
        elseif (v == "living_street") then
            result = "yes"
        elseif (v == "service") then
            result = "yes"
        elseif (v == "services") then
            result = "yes"
        elseif (v == "track") then
            result = "yes"
        elseif (v == "crossing") then
            result = "dismount"
        elseif (v == "footway") then
            result = "dismount"
        elseif (v == "pedestrian") then
            result = "dismount"
        elseif (v == "corridor") then
            result = "dismount"
        elseif (v == "construction") then
            result = "dismount"
        elseif (v == "steps") then
            result = "dismount"
        elseif (v == "path") then
            result = "yes"
        elseif (v == "primary") then
            result = "yes"
        elseif (v == "primary_link") then
            result = "yes"
        elseif (v == "secondary") then
            result = "yes"
        elseif (v == "secondary_link") then
            result = "yes"
        elseif (v == "tertiary") then
            result = "yes"
        elseif (v == "tertiary_link") then
            result = "yes"
        elseif (v == "unclassified") then
            result = "yes"
        elseif (v == "road") then
            result = "yes"
        end
    end
    if (tags["service"] ~= nil) then
        local v0
        v0 = tags["service"]
        if (v0 == "parking_aisle") then
            result = "permissive"
        elseif (v0 == "driveway") then
            result = "private"
        elseif (v0 == "alley") then
            result = "yes"
        elseif (v0 == "bus") then
            result = "no"
        end
    end
    if (tags["access"] ~= nil) then
        local v1
        v1 = tags["access"]
        if (v1 == "no") then
            result = "no"
        elseif (v1 == "customers") then
            result = "private"
        elseif (v1 == "private") then
            result = "private"
        elseif (v1 == "permissive") then
            result = "permissive"
        elseif (v1 == "destination") then
            result = "destination"
        elseif (v1 == "delivery") then
            result = "destination"
        elseif (v1 == "service") then
            result = "destination"
        elseif (v1 == "permit") then
            result = "destination"
        end
    end
    if (tags["bicycle"] ~= nil) then
        local v2
        v2 = tags["bicycle"]
        if (v2 == "yes") then
            result = "yes"
        elseif (v2 == "no") then
            result = "no"
        elseif (v2 == "use_sidepath") then
            result = "no"
        elseif (v2 == "designated") then
            result = "designated"
        elseif (v2 == "permissive") then
            result = "permissive"
        elseif (v2 == "private") then
            result = "private"
        elseif (v2 == "official") then
            result = "designated"
        elseif (v2 == "dismount") then
            result = "dismount"
        elseif (v2 == "permit") then
            result = "destination"
        end
    end
    if (tags["anyways:construction"] ~= nil) then
        local v3
        v3 = tags["anyways:construction"]
        if (v3 == "yes") then
            result = "no"
        end
    end
    if (tags["anyways:access"] ~= nil) then
        local v4
        v4 = tags["anyways:access"]
        if (v4 == "no") then
            result = "no"
        elseif (v4 == "destination") then
            result = "destination"
        elseif (v4 == "yes") then
            result = "yes"
        end
    end
    if (tags["anyways:bicycle"] ~= nil) then
        result = tags["anyways:bicycle"]
    end
    
    if (result == nil) then
        result = "no"
    end
    return result
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
    local result
    if (tags["oneway"] ~= nil) then
        local v5
        v5 = tags["oneway"]
        if (v5 == "yes") then
            result = "with"
        elseif (v5 == "no") then
            result = "both"
        elseif (v5 == "1") then
            result = "with"
        elseif (v5 == "-1") then
            result = "against"
        end
    end
    if (tags["cycleway:left"] ~= nil) then
        local v6
        v6 = tags["cycleway:left"]
        if (v6 == "no") then
            result = "with"
        elseif (v6 == "none") then
            result = "with"
        elseif (v6 == "yes") then
            result = "both"
        elseif (v6 == "lane") then
            result = "both"
        elseif (v6 == "track") then
            result = "both"
        elseif (v6 == "shared_lane") then
            result = "both"
        elseif (v6 == "share_busway") then
            result = "both"
        elseif (v6 == "opposite_lane") then
            result = "both"
        elseif (v6 == "opposite_track") then
            result = "both"
        elseif (v6 == "opposite") then
            result = "both"
        end
    end
    if (tags["cycleway"] ~= nil) then
        local v7
        v7 = tags["cycleway"]
        if (v7 == "right") then
            result = "against"
        elseif (v7 == "opposite_lane") then
            result = "both"
        elseif (v7 == "track") then
            result = "both"
        elseif (v7 == "lane") then
            result = "both"
        elseif (v7 == "opposite") then
            result = "both"
        elseif (v7 == "opposite_share_busway") then
            result = "both"
        elseif (v7 == "opposite_track") then
            result = "both"
        end
    end
    if (tags["junction"] ~= nil) then
        local v8
        v8 = tags["junction"]
        if (v8 == "roundabout") then
            result = "with"
        end
    end
    if (tags["oneway:bicycle"] ~= nil) then
        local v9
        v9 = tags["oneway:bicycle"]
        if (v9 == "yes") then
            result = "with"
        elseif (v9 == "no") then
            result = "both"
        elseif (v9 == "1") then
            result = "with"
        elseif (v9 == "-1") then
            result = "against"
        end
    end
    
    if (result == nil) then
        result = "both"
    end
    return result
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
    local result
    if (tags["highway"] ~= nil) then
        local v10
        v10 = tags["highway"]
        if (v10 == "cycleway") then
            result = 30
        elseif (v10 == "footway") then
            result = 20
        elseif (v10 == "crossing") then
            result = 20
        elseif (v10 == "pedestrian") then
            result = 15
        elseif (v10 == "path") then
            result = 15
        elseif (v10 == "corridor") then
            result = 5
        elseif (v10 == "residential") then
            result = 30
        elseif (v10 == "living_street") then
            result = 20
        elseif (v10 == "service") then
            result = 30
        elseif (v10 == "services") then
            result = 30
        elseif (v10 == "track") then
            result = 50
        elseif (v10 == "unclassified") then
            result = 50
        elseif (v10 == "road") then
            result = 50
        elseif (v10 == "motorway") then
            result = 120
        elseif (v10 == "motorway_link") then
            result = 120
        elseif (v10 == "primary") then
            result = 90
        elseif (v10 == "primary_link") then
            result = 90
        elseif (v10 == "secondary") then
            result = 50
        elseif (v10 == "secondary_link") then
            result = 50
        elseif (v10 == "tertiary") then
            result = 50
        elseif (v10 == "tertiary_link") then
            result = 50
        end
    end
    if (tags["designation"] ~= nil) then
        local v11
        v11 = tags["designation"]
        if (v11 == "towpath") then
            result = 30
        end
    end
    if (tags["maxspeed"] ~= nil) then
        result = parse(tags["maxspeed"])
    end
    
    if (result == nil) then
        result = 30
    end
    return result
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

    if (math.abs(result.forward_speed - expected.speed) >= 0.001 and math.abs(result.backward_speed - expected.speed) >= 0.001) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".speed: expected " .. expected.speed .. " but got " .. result.forward_speed .. " forward and " .. result.backward_speed .. " backward")
        profile_failed = true;
    end


    if (math.abs(result.forward - expected.priority) >= 0.001 and math.abs(result.backward - expected.priority) >= 0.001) then
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


function string_start(strt, s)
    return string.sub(s, 1, string.len(strt)) == strt
end


-- every key starting with "_relation:<name>:XXX" is rewritten to "_relation:XXX"
function remove_relation_prefix(tags, name)

    local new_tags = {}
    for k, v in pairs(tags) do
        local prefix = "_relation:" .. name .. ":";
        if (string_start(prefix, k)) then
            local new_key = "_relation:" .. string.sub(k, string.len(prefix) + 1) -- plus 1: sub uses one-based indexing to select the start
            new_tags[new_key] = v
        else
            new_tags[k] = v
        end
    end
    return new_tags
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


    unit_test_profile(behaviour_bicycle_shortest, "shortest", 0, {access = "no", speed = 0, oneway = "both", priority = inv(0) }, {})
    unit_test_profile(behaviour_bicycle_shortest, "shortest", 1, {access = "designated", speed = 15, oneway = "both", priority = inv(1) }, {highway = "cycleway"})
    unit_test_profile(behaviour_bicycle_shortest, "shortest", 2, {access = "yes", speed = 15, oneway = "both", priority = inv(1) }, {highway = "path", surface = "ground"})


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