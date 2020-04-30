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