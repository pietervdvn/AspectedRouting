function must_match(tags, result, needed_keys, table)
    for _, key in ipairs(needed_keys) do
        local v = tags[key]
        if (v == nil) then
            return false
        end

        local mapping = table[key]
        if (type(mapping) == "table") then
            local resultValue = mapping[v]
            if (resultValue == nil or
                    resultValue == false or
                    resultValue == "no" or
                    resultValue == "false") then
                return false
            end
        elseif (type(mapping) == "string") then
            local bool = mapping
            if (bool == "yes" or bool == "1") then
                return true
            elseif (bool == "no" or bool == "0") then
                return false
            end
            error("MustMatch got a string value it can't handle: " .. bool)
        else
            error("The mapping is not a table. This is not supported. We got " .. mapping)
        end
    end

        -- Now that we know for sure that every key matches, we add them all
        for _, key in ipairs(needed_keys) do
            local v = tags[key]
            result.attributes_to_keep[key] = v
        end

    return true;
end