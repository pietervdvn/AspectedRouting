function first_match_of(tags, result, order_of_keys, table)
    for _, key in ipairs(order_of_keys) do
        local v = tags[key]
        if (v ~= nil) then

            local mapping = table[key]
            if (type(mapping) == "table") then
                local resultValue = mapping[v]
                if (resultValue ~= nil) then
                    result.attributes_to_keep[key] = v
                    return resultValue
                end
            else
                result.attributes_to_keep[key] = v
                return mapping
            end
        end
    end
    return nil;
end