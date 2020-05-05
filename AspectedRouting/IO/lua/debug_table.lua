function debug_table(table, prefix)
    if (prefix == nil) then
        prefix = ""
    end
    for k, v in pairs(table) do

        if (type(v) == "table") then
            debug_table(v, "   ")
        else
            print(prefix .. tostring(k) .. " = " .. tostring(v))
        end
    end
    print("")
end

function debug_table_str(table, prefix)
    if (prefix == nil) then
        prefix = ""
    end
    local str = "";
    for k, v in pairs(table) do

        if (type(v) == "table") then
            str = str .. "," .. debug_table_str(v, "   ")
        else
            str = str .. "," .. (prefix .. tostring(k) .. " = " .. tostring(v))
        end
    end
    return str
end