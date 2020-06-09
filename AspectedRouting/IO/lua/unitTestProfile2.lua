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