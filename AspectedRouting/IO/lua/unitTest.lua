failed_tests = false
function unit_test(f, fname, index, expected, parameters, tags)
    local result = {attributes_to_keep = {}}
    local actual = f(parameters, tags, result)
    if (tostring(actual) ~= expected) then
        print("[" .. fname .. "] " .. index .. " failed: expected " .. expected .. " but got " .. tostring(actual))
        failed_tests = true
    end
end