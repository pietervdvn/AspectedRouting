function table_to_list(tags, result, factor_table)
    local list = {}
    if(tags == nil) then
        return list
    end
    for key, mapping in pairs(factor_table) do
        local v = tags[key]
        if (v ~= nil) then
            if (type(mapping) == "table") then
                local f = mapping[v]
                if (f ~= nil) then
                    table.insert(list, f);
                    result.attributes_to_keep[key] = v
                end
            else
                table.insert(list, mapping);
                result.attributes_to_keep[key] = v
            end
        end
    end

    return list;
end