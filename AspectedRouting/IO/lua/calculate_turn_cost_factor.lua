--[[ 

  
 Returns '0' if the turn is allowed and '-1' if the turn is forbidden.
 Only used by itinero 2.0.
 
 The itinero 2.0 profile outputs a `turn_cost_factor` which immediately calls this one (see LuaPrinter2.MainFunction).
 
 Dependencies: str_split, containedIn
]]
function calculate_turn_cost_factor(attributes, vehicle_types)

    if (attributes["type"] ~= "restriction") then
        -- not a turn restriction; no cost to turn,
        return 0
    end
    
    for _, vehicle_type in pairs(vehicle_types) do
        if (attributes["restriction:"..vehicle_type] ~= nil) then
            -- There is a turn restriction specifically for one of our vehicle types!
            return -1
        end
    end	

    if (attributes["restriction"] == nil) then
        -- Not a turn restriction; no cost to turn
        return 0
    end
    if (attributes["except"] ~= nil) then
        local except_types = str_split(attributes["except"],";")
        for _, vehicle_type in pairs(vehicle_types) do
            if (containedIn(except_types, vehicle_type)) then
                -- This vehicle is exempt from this turn restriction
                return 0
            end
        end
    end

    return -1
end


