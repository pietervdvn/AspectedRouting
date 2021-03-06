failed_tests = false
function unit_test(f, fname, index, expected, parameters, tags)
    if (f == nil) then
        print("Trying to unit test " .. fname .. " but this function is not defined")
        failed_tests = true
        return
    end
    local result = {attributes_to_keep = {}}
    local actual = f(parameters, tags, result)
    if(expected == "null" and actual == nil) then
        -- OK!
    elseif(tonumber(actual) and tonumber(expected) and math.abs(tonumber(actual) - tonumber(expected)) < 0.1) then
        -- OK!
    elseif (tostring(actual) ~= expected) then
        print("[" .. fname .. "] " .. index .. " failed: expected " .. expected .. " but got " .. tostring(actual))
        failed_tests = true
    end
end