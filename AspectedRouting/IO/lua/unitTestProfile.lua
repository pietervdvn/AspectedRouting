failed_profile_tests = false
--[[
expected should be a table containing 'access', 'speed' and 'priority'
]]
function unit_test_profile(profile_function, profile_name, index, expected, tags)
    local result = { attributes_to_keep = {} }
    local profile_failed = false
    profile_function(tags, result)

    local accessCorrect = (result.access == 0 and (expected.access == "no" or expected.priority <= 0)) or result.access == 1
    if (not accessCorrect) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".access: expected " .. expected.access .. " but got " .. result.access)
        profile_failed = true
        failed_profile_tests = true
    end

    if (expected.access == "no" or expected.priority <= 0) then
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


    local actualOneway = result.direction
    
    if(actualOneway == nil) then
        print("Fail: result.direction is nil")
        profile_failed = true;
    end
    
    if (result.direction == 0) then
        actualOneway = "both"
    elseif (result.direction == 1) then
        actualOneway = "with"
    elseif (result.direction == 2) then
        actualOneway = "against"
    end

    if (expected.oneway ~= actualOneway) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".oneway: expected " .. expected.oneway .. " but got " .. actualOneway)
        failed_profile_tests = true
        profile_failed = true
    end


    if (not double_compare(result.factor, 1/expected.priority)) then
        print("Test " .. tostring(index) .. " failed for " .. profile_name .. ".factor: expected " .. expected.priority .. " but got " .. 1/result.factor)
        failed_profile_tests = true
        profile_failed = true
    end

    if (profile_failed == true) then
        print("The used tags for test " .. tostring(index) .. " are:")
        debug_table(tags)
    end
end