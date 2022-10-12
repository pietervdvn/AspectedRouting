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
                    if (result.attributes_to_keep ~= nil) then
                        result.attributes_to_keep[key] = v
                    end
                end
            else
                table.insert(list, mapping);
                if (result.attributes_to_keep ~= nil) then
                    result.attributes_to_keep[key] = v
                end
            end
        end
    end

    return list;
end