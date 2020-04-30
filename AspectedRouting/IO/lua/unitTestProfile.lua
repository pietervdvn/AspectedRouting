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