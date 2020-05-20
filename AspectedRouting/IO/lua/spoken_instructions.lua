-- instruction generators
instruction_generators = {
    {
        applies_to = "", -- applies to all profiles when empty
        generators = {
            {
                name = "start",
                function_name = "get_start"
            },
            {
                name = "stop",
                function_name = "get_stop"
            },
            {
                name = "roundabout",
                function_name = "get_roundabout"
            },
            {
                name = "turn",
                function_name = "get_turn"
            }
        }
    }
}

-- gets the first instruction
function get_start(route_position, language_reference, instruction)
    if route_position.is_first() then
        local direction = route_position.direction()
        instruction.text = itinero.format(language_reference.get("Start {0}."), language_reference.get(direction));
        instruction.shape = route_position.shape
        return 1
    end
    return 0
end

-- gets the last instruction
function get_stop(route_position, language_reference, instruction)
    if route_position.is_last() then
        instruction.text = language_reference.get("Arrived at destination.");
        instruction.shape = route_position.shape
        return 1
    end
    return 0
end


-- gets a roundabout instruction
function get_roundabout(route_position, language_reference, instruction)
    if route_position.attributes.junction == "roundabout" and
            (not route_position.is_last()) then
        local attributes = route_position.next().attributes
        if attributes.junction then
        else
            local exit = 1
            local count = 1
            local previous = route_position.previous()
            while previous and previous.attributes.junction == "roundabout" do
                local branches = previous.branches
                if branches then
                    branches = branches.get_traversable()
                    if branches.count > 0 then
                        exit = exit + 1
                    end
                end
                count = count + 1
                previous = previous.previous()
            end

            instruction.text = itinero.format(language_reference.get("Take the {0}th exit at the next roundabout."), "" .. exit)
            if exit == 1 then
                instruction.text = itinero.format(language_reference.get("Take the first exit at the next roundabout."))
            elseif exit == 2 then
                instruction.text = itinero.format(language_reference.get("Take the second exit at the next roundabout."))
            elseif exit == 3 then
                instruction.text = itinero.format(language_reference.get("Take the third exit at the next roundabout."))
            end
            instruction.type = "roundabout"
            instruction.shape = route_position.shape
            return count
        end
    end
    return 0
end

-- gets a turn
function get_turn(route_position, language_reference, instruction)
    local relative_direction = route_position.relative_direction().direction

    local turn_relevant = false
    local branches = route_position.branches
    if branches then
        branches = branches.get_traversable()
        if relative_direction == "straighton" and
                branches.count >= 2 then
            turn_relevant = true -- straight on at cross road
        end
        if relative_direction ~= "straighton" and
                branches.count > 0 then
            turn_relevant = true -- an actual normal turn
        end
    end

    if relative_direction == "unknown" then
        turn_relevant = false -- turn could not be calculated.
    end

    if turn_relevant then
        local next = route_position.next()
        local name = nil
        if next then
            name = next.attributes.name
        end
        if name then
            instruction.text = itinero.format(language_reference.get("Go {0} on {1}."),
                language_reference.get(relative_direction), name)
            instruction.shape = route_position.shape
        else
            instruction.text = itinero.format(language_reference.get("Go {0}."),
                language_reference.get(relative_direction))
            instruction.shape = route_position.shape
        end

        return 1
    end
    return 0
end
