-- The different profiles
profiles = {
    


    {
        name = "b2w",
        description = "[Custom] Route for bike2work. Same as 'commute' ATM. Migth diverge in the future. In use.",
        function_name = "determine_weights_commute",
        metric = "custom"
    },
    {
        name = "networks",
        description = "A recreative route following existing cycling networks. Might make longer detours",
        function_name = "determine_weights_networks",
        metric = "custom"
    },
    {
        name = "node_network",
        description = "A recreative route following existing cycle node networks. Might make longer detours",
        function_name = "determine_weights_networks_node_network",
        metric = "custom"
    },
    {
        name = "genk",
        description = "[Custom] A route following the Genk cycle network",
        function_name = "determine_weights_genk",
        metric = "custom"
    },
    {
        name = "brussels",
        description = "[Custom] A route following the Brussels cycle network",
        function_name = "determine_weights_brussels",
        metric = "custom"
    },
    {
        name = "cycle_highway",
        description = "A functional route, preferring cycle_highways or the Brussels Mobility network. If none are availale, will favour speed",
        function_name = "determine_weights_cycle_highways",
        metric = "custom"
    },
    {
        name = "commute",
        description = "A functional route which is aimed to commuters. It is a mix of safety, comfort, a pinch of speed and cycle_highway",
        function_name = "determine_weights_commute",
        metric = "custom"
    },
    {
        name = "anyways.network", -- prefers the anyways network, thus where `operator=Anyways` - 
        description = "A route following the cycle network with the operator 'Anyways'. This is for use in ShortCut/Impact as there are no such networks existing in OSM.",
        function_name = "determine_weights_anyways_network",
        metric = "custom"
    },
    
    {
        name = "commute.race",
        description = "[Deprecated] Same as commute, please use that one instead. Might be unused",
        function_name = "determine_weights_cycle_highways", -- TODO tweak this profile
        metric = "custom"
    },
    {
        name = "opa",
        description = "[Deprecated][Custom] Same as fastest, please use that one instead. Might be unused. Note: all profiles take anyways:* tags into account",
        function_name = "determine_weights", 
        metric = "custom"
    },
}






-- Returns 1 if no access restrictions, 0 if it is permissive
function determine_permissive_score(attributes, result)
    if (attributes.access == "permissive") then
        return 0
    end
    return 1
end


--[[ Gives 1 if this is on a cycling network.
 If attributes.settings.cycle_network_operator is defined, then only follow these networks
 If attributes.settings.cycle_network_highway is defined, then only follow the 'Fietssnelwegen'
 
 The colour _will only be copied_ if the score is one
 ]]
cycle_network_attributes_to_match = { "cycle_network_highway", "cycle_network_node_network" }
function determine_network_score(attributes, result)

    if (attributes.cycle_network == nil) then
        return 0
    end

    for i = 1, #cycle_network_attributes_to_match do

        local key = cycle_network_attributes_to_match[i]
        local expected = attributes.settings[key]
    

        if (expected ~= nil) then
            -- we have to check that this tag matches
            local value = attributes[key]
            if (value == nil) then
                -- the waysegment doesn't have this attribute - abort
                return 0
            end
            
            if (value ~= expected) then
                -- the way segment doesn't have the expected value - abort
                return 0
            end

            -- hooray, we have a match!
            result.attributes_to_keep[key] = value
        end
    end


    if (attributes.settings.cycle_network_operator ~= nil) then
        local expected = attributes.settings.cycle_network_operator;
        expected = expected:gsub(" ", "");
        if (attributes[expected] ~= "yes") then
            return 0
        end
    end
   
    result.attributes_to_keep.cycle_network = "yes";

    return 1
end

function determine_weights_comfort_safety(attributes, result)

    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 1
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 1
        attributes.settings.network_weight = 0
        attributes.settings.clear_restrictions_preference = 1;
    end

    determine_weights(attributes, result)
end


function determine_weights_commute(attributes, result)
    -- lots of safety and comfort, but also slightly prefers 'fietssnelwegen' and 'Brussels Mobility' 'route cyclable', as they are functional

    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.clear_restrictions_preference = 1
        attributes.settings.safety_weight = 3
        attributes.settings.time_weight = 1
        attributes.settings.comfort_weight = 2

        attributes.settings.network_weight = 3



    end

    determine_weights(attributes, result)

    -- commute is a big exception to the other profiles, as in that we overwrite the result.factor here
    -- this is in order to support _two_ cycling networks


    local safety = determine_safety(attributes, result);
    local comfort = determine_comfort(attributes, result);

    attributes.cycle_network = "yes"
    attributes.settings.cycle_network_highway = "yes"
    local cycle_highway_score = determine_network_score(attributes, result);
    attributes.settings.cycle_network_highway = nil


    attributes.cycle_network = "yes"
    attributes.settings.cycle_network_operator = "Brussels Mobility"
    local brussels_mobility_score = determine_network_score(attributes, result);
    attributes.settings.cycle_network_operator = nil

    local network = math.min(1, cycle_highway_score + brussels_mobility_score);


    local clear_restrictions = determine_permissive_score(attributes, result);


    result.factor = 1 /
            (safety * attributes.settings.safety_weight +
                    result.speed * attributes.settings.time_weight +
                    comfort * attributes.settings.comfort_weight +
                    network * attributes.settings.network_weight +
                    clear_restrictions * attributes.settings.clear_restrictions_preference);

end

function determine_weights_cycle_highways(attributes, result)
    -- heavily prefers 'fietssnelwegen' and 'Brussels Mobility' 'route cyclable', as they are functional

    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.clear_restrictions_preference = 1
        attributes.settings.safety_weight = 1
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 0
    
        attributes.settings.network_weight = 20
        
        

    end

    determine_weights(attributes, result)
    
    -- commute is a big exception to the other profiles, as in that we overwrite the result.factor here
    -- this is in order to support _two_ cycling networks


    local safety = determine_safety(attributes, result);
    local comfort = determine_comfort(attributes, result);

    attributes.cycle_network = "yes"
    attributes.settings.cycle_network_highway = "yes"
    local cycle_highway_score = determine_network_score(attributes, result);
    attributes.settings.cycle_network_highway = nil


    attributes.cycle_network = "yes"
    attributes.settings.cycle_network_operator = "Brussels Mobility"
    local brussels_mobility_score = determine_network_score(attributes, result);
    attributes.settings.cycle_network_operator = nil

    local network = math.min(1, cycle_highway_score + brussels_mobility_score);


    local clear_restrictions = determine_permissive_score(attributes, result);


    result.factor = 1 /
            (safety * attributes.settings.safety_weight +
                    result.speed * attributes.settings.time_weight +
                    comfort * attributes.settings.comfort_weight +
                    network * attributes.settings.network_weight +
                    clear_restrictions * attributes.settings.clear_restrictions_preference);

end


function determine_weights_networks(attributes, result)

    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 1
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 0
        attributes.settings.network_weight = 3
        attributes.settings.clear_restrictions_preference = 1;
    end

    determine_weights(attributes, result)
end

function determine_weights_networks_node_network(attributes, result)

    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 1
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 0
        attributes.settings.network_weight = 10
        attributes.settings.clear_restrictions_preference = 1

        attributes.cycle_network = "yes"
        attributes.settings.cycle_network_node_network = "yes"
    end

    determine_weights(attributes, result)
end

function determine_weights_genk(attributes, result)

    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 1
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 0
        attributes.settings.network_weight = 3
        attributes.settings.clear_restrictions_preference = 1
    
        attributes.cycle_network = "yes"
        attributes.settings.cycle_network_operator = "Stad Genk"
    end

    determine_weights(attributes, result)
end

function determine_weights_brussels(attributes, result)


    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 1
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 1
        attributes.settings.network_weight = 5
        attributes.settings.clear_restrictions_preference = 1;

        attributes.settings.cycle_network_operator = "Brussels Mobility"
    end

    determine_weights(attributes, result)
end


function determine_weights_anyways_network(attributes, result)


    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 0
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 0
        attributes.settings.network_weight = 10
        attributes.settings.clear_restrictions_preference = 1;

        attributes.settings.cycle_network_operator = "Anyways"
    end
    

    attributes.access = nil -- little hack: remove ALL access restrictions in order to go onto private parts

    determine_weights(attributes, result)
end


function determine_weights(attributes, result)

    -- we add a 'settings' element to attributes, they can be used by other profiles
    if (attributes.settings == nil) then
        attributes.settings = {}
        attributes.settings.default_speed = 15
        attributes.settings.min_speed = 3
        attributes.settings.max_speed = 30

        attributes.settings.safety_weight = 0
        attributes.settings.time_weight = 0
        attributes.settings.comfort_weight = 0
        attributes.settings.network_weight = 0
        attributes.settings.clear_restrictions_preference = 1; -- if 1: discourage 'permissive' access tags

        attributes.settings.cycle_network_operator = nil -- e.g. "Stad Genk" of "Brussels Mobility"
        attributes.settings.cycle_network_highway = nil -- if "yes", will only follow the 'fietsostrades'
        attributes.settings.cycle_network_node_network = nil -- if "yes", will only follow the 'node_networks'
    end


    -- Init default values
    result.access = 0
    result.speed = 0
    result.factor = 1
    result.direction = 0
    result.canstop = true
    result.attributes_to_keep = {}

    -- Do misc preprocessing, such as handling the ferry case
    preprocess(attributes, result);


    -- 1) Can we enter this segment legally?
    if (not can_access_legally(attributes)) then
        return
    end
    result.access = 1;
    result.attributes_to_keep.highway = attributes.highway
    result.attributes_to_keep.access = attributes.access


    -- 2) Is this a oneway?
    determine_oneway(attributes, result)

    -- 3) How fast would one drive on average on this segment?
    determine_speed(attributes, result);
    -- Cap using settings and legal max speed
    result.speed = math.max(attributes.settings.min_speed, result.speed)
    result.speed = math.min(attributes.settings.max_speed, result.speed)
    local legal_max_speed = highway_types[attributes.highway].speed
    result.speed = math.min(legal_max_speed, result.speed)


    -- 4) What is the factor of this segment? 
    --[[
     This is determined by multiple factors and the weight that is given to them by the settings
     Factors are:
     - safety
     - comfort
     - ...
     ]]

    local safety = determine_safety(attributes, result);
    local comfort = determine_comfort(attributes, result);
    local network = determine_network_score(attributes, result);
    local clear_restrictions = determine_permissive_score(attributes, result);

    result.factor = 1 /
            (safety * attributes.settings.safety_weight +
                    result.speed * attributes.settings.time_weight +
                    comfort * attributes.settings.comfort_weight +
                    network * attributes.settings.network_weight +
                    clear_restrictions * attributes.settings.clear_restrictions_preference);
end










-- Unit test: are all tags in profile_whitelist
cycling_network_operators_to_tag = { "Stad Genk", "Anyways", "Brussels Mobility" }
cycling_network_tags = { "cycle_network", "cycle_network_highway", "cycle_network_operator", "cycle_network_colour", "cycle_network_node_network", "operator", "StadGenk", "Anyways", "BrusselsMobility" }


--[[ Copies all relevant relation tags onto the way 
All these tags start with 'cycle_network', e.g. cycle_network_colour

Some of them are exclusively for the metadata (cyclecolour as prime example)

Note that all the tags used are listed in 'cycling_network_tags' as well, for unit testing purposes

]]
function cycling_network_tag_processor(attributes, result)
    result.attributes_to_keep.cycle_network = "yes"

    if (attributes.cycle_network == "cycle_highway"
            and attributes.state ~= "proposed") then
        result.attributes_to_keep.cycle_network_highway = "yes"
    end

    if (attributes.cycle_network == "node_network") then
        result.cycle_network_node_network = "yes"
    end


    if (attributes.operator ~= nil) then
        for k, v in pairs(cycling_network_operators_to_tag) do
            v = v:gsub(" ", "") -- remove spaces from the operator as lua can't handle them
            result.attributes_to_keep[v] = "yes"
        end
        
    end

    if (attributes.colour ~= nil) then
        result.attributes_to_keep.cycle_network_colour = attributes.colour
    end

    if (attributes.color ~= nil) then
        -- for the americans!
        result.attributes_to_keep.cycle_network_colour = attributes.color
    end
end

-- Processes the relation. All tags which are added to result.attributes_to_keep will be copied to 'attributes' of each individual way
function relation_tag_processor(attributes, result)
    result.attributes_to_keep = {}


    if (attributes.route == "bicycle") then
        -- This is a cycling network!
        cycling_network_tag_processor(attributes, result)
    end
end










-----------------------------------------------------------------------------------------------------------------------

function unit_tests()

 

    unit_test_relation_tag_processor({ route = "bicycle", operator = "Stad Genk", color = "red", type = "route" },
        { StadGenk = "yes", cycle_network_colour = "red", cycle_network = "yes" });
    unit_test_relation_tag_processor({ route = "bicycle", cycle_network = "cycle_highway", color = "red", type = "route" },
        { cycle_network_colour = "red", cycle_network = "yes", cycle_network_highway = "yes" });


    unit_test_cycle_networks({ highway = "residential", settings = {} }, 0)
    unit_test_cycle_networks({ highway = "residential", cycle_network = "yes", settings = {} }, 1)
    unit_test_cycle_networks({
        highway = "residential",
        cycle_network = "yes",
        settings = { cycle_network_operator = "Stad Genk" }
    }, 0 --[[Not the right network, not Genk]])

    unit_test_weights(determine_weights_speed_first, { highway = "residential" }, 0.0625);
    unit_test_weights(determine_weights_speed_first, { highway = "cycleway" }, 0.0625);    -- unit_test_weights({ highway = "residential", access = "destination", surface = "sett" });
    unit_test_weights(determine_weights_speed_first, { highway = "primary", bicycle="yes", surface = "sett" }, 0.068965517241379);
   
    unit_test_weights(determine_weights_speed_first, { highway = "primary" , bicycle="yes"}, 0.0625);
    unit_test_weights(determine_weights_safety_first, { highway = "primary" , bicycle="yes"}, 0.76923076923077);
    
   	-- regression test
    unit_test_weights(determine_weights_safety_first, { ["cycleway:left"] = "track", ["cycleway:left:oneway"] = "no", highway="unclassified", oneway="yes", ["oneway:bicycle"]=no}, 0.45454545454545);
    unit_test_weights(determine_weights_speed_first, { ["cycleway:left"] = "track", ["cycleway:left:oneway"] = "no", highway="unclassified", oneway="yes", ["oneway:bicycle"]=no}, 0.0625);
 
    
    unit_test_weights(determine_weights_genk, { highway = "residential" }, 0.52631578947368);
    unit_test_weights(determine_weights_genk,
        { highway = "residential", StadGenk = "yes", cycle_network = "yes" },
        0.20408163265306); -- factor = 1 / preference, should be lower
    unit_test_weights(determine_weights_genk,
        { highway = "residential", cycle_network_operator = nil, cycle_network = "yes" },
        0.52631578947368);
    unit_test_weights(determine_weights_genk, {
        highway = "residential",
        cycle_network = "yes",
        cycle_network_operator = "Niet Stad Genk"
    }, 0.52631578947368);

    unit_test_weights(determine_weights_networks_node_network, {
        highway = "residential",
        cycle_network = "yes",
        cycle_network_operator = "Niet Stad Genk"
    }, 0.52631578947368);

    unit_test_weights(determine_weights_networks_node_network, {
        highway = "residential",
        cycle_network = "yes",
        cycle_network_node_network = "yes",
    }, 0.084033613445378);

    unit_test_weights(determine_weights_anyways_network, { highway = "residential", cycle_network = "yes", Anyways = "yes" }, 0.090909090909091);
    unit_test_weights(determine_weights_anyways_network, { highway = "residential" }, 1.0);


    unit_test_weights(determine_weights_networks_node_network, { highway = "residential" }, 0.52631578947368);
    unit_test_weights(determine_weights_networks_node_network, { highway = "residential", cycle_network="yes", cycle_network_node_network="yes" }, 0.084033613445378);

end



