name = "pedestrian.shortest"
generationDate = "2021-01-27T15:51:22"
description = "The shortest route, independent of of speed (Profile for someone who is walking)"

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
    parameters.leastSafetyPenalty = 2


    local oneway = firstArg("both")
    tags.oneway = oneway
    -- An aspect describing oneway should give either 'both', 'against' or 'width'


    -- forward calculation. We set the meta tag '_direction' to 'width' to indicate that we are going forward. The other functions will pick this up
    tags["_direction"] = "with"
    local access_forward = pedestrian_legal_access(parameters, tags, result)
    if(oneway == "against") then
        -- no 'oneway=both' or 'oneway=with', so we can only go back over this segment
        -- we overwrite the 'access_forward'-value with no; whatever it was...
        access_forward = "no"
    end
    if(access_forward ~= nil and access_forward ~= "no") then
        tags.access = access_forward -- might be relevant, e.g. for 'access=dismount' for bicycles
        result.forward_speed = firstArg(parameters["defaultSpeed"])
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
    local access_backward = pedestrian_legal_access(parameters, tags, result)
    if(oneway == "with") then
        -- no 'oneway=both' or 'oneway=against', so we can only go forward over this segment
        -- we overwrite the 'access_forward'-value with no; whatever it was...
        access_backward = "no"
    end
    if(access_backward ~= nil and access_backward ~= "no") then
        tags.access = access_backward
        result.backward_speed = firstArg(parameters["defaultSpeed"])
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
        1 * distance +
       1 * clean_permission_score(parameters, tags, result)
    return priority
end

function default_parameters()
    local parameters = {}
    parameters.defaultSpeed = 4
    parameters.maxspeed = 6
    parameters.timeNeeded = 0
    parameters.distance = 0
    parameters.slow_road_preference = 0
    parameters.trespassingPenalty = 1

    return parameters
end


--[[
Gives, for each type of highway, whether or not someone can enter legally.
Note that legal access is a bit 'grey' in the case of roads marked private and permissive, in which case these values are returned 

Unit: 'designated': Access is allowed and even specifically for pedestrian
'yes': pedestrians are allowed here
'permissive': pedestrians are allowed here, but this might be a private road or service where usage is allowed, but could be retracted one day by the owner
'destination': walking is allowed here, but only if truly necessary to reach the destination (e.g. a service road)
'private': this is a private road, only go here if the destination is here
'no': do not walk here
Created by 
Originally defined in pedestrian.legal_access.json
Uses tags: access, highway, service, foot, anyways:foot, anyways:access, anyways:construction
Used parameters: 
Number of combintations: 53
Returns values: 
]]
function pedestrian_legal_access(parameters, tags, result)
    local result
    if (tags["highway"] ~= nil) then
        local v
        v = tags["highway"]
        if (v == "pedestrian") then
            result = "designated"
        elseif (v == "footway") then
            result = "designated"
        elseif (v == "living_street") then
            result = "designated"
        elseif (v == "steps") then
            result = "yes"
        elseif (v == "corridor") then
            result = "designated"
        elseif (v == "residential") then
            result = "yes"
        elseif (v == "service") then
            result = "yes"
        elseif (v == "services") then
            result = "yes"
        elseif (v == "track") then
            result = "yes"
        elseif (v == "crossing") then
            result = "yes"
        elseif (v == "construction") then
            result = "permissive"
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
    if (tags["foot"] ~= nil) then
        local v2
        v2 = tags["foot"]
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
    if (tags["anyways:foot"] ~= nil) then
        result = tags["anyways:foot"]
    end
    
    if (result == nil) then
        result = "no"
    end
    return result
end

--[[
Gives 0 on private roads, 0.1 on destination-only roads, and 0.9 on permissive roads; gives 1 by default. This helps to select roads with no access retrictions on them

Unit: 
Created by 
Originally defined in clean_permission_score.json
Uses tags: access
Used parameters: 
Number of combintations: 5
Returns values: 
]]
function clean_permission_score(parameters, tags, result)
    local result
    result = head(stringToTags({
            access = {
                private = -50,
                destination = -3,
                permissive = -1
            }
        }, tags))
    if (result == nil) then
        result = 0
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

function firstArg(a, b)
    -- it turns out that 'const' is a reserved token in some lua implementations
    return a
end

function head(ls)
   if(ls == nil) then
       return nil
   end
   for _, v in pairs(ls) do
       if(v ~= nil) then
           return v
       end
   end
   return nil
end

print("ERROR: stringToTag is needed. This should not happen")



function test_all()


    


  -- Behaviour tests --


    


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