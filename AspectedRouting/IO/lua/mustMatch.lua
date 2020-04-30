function must_match(tags, result, needed_keys, table)
    local result_list = {}
    for _, key in ipairs(needed_keys) do
        local v = tags[key]
        if (v == nil) then
            return false
        end

        local mapping = table[key]
        if (type(mapping) == "table") then
            local resultValue = mapping[v]
            if (v == nil or v == false) then
                return false
            end
            if (v == "no" or v == "false") then
                return false
            end

            result.attributes_to_keep[key] = v
        else
            error("The mapping is not a table. This is not supported")
        end
    end
    return true;
end