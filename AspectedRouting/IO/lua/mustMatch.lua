--[[
must_match checks that a collection of tags matches a specification.

The function is not trivial and contains a few subtilities.

Consider the following source:

{"$mustMatch":{ "a":"X", "b":{"not":"Y"}}}

This is desugared into

{"$mustMatch":{ "a":{"$eq":"X"}, "b":{"not":"Y"}}}

When applied on the tags {"a" : "X"}, this yields the table {"a":"yes", "b":"yes} (as `notEq` "Y" "nil") yields "yes"..
MustMatch checks that every key in this last table yields yes - even if it is not in the original tags!


]]
function must_match(tags, result, needed_keys, table)
    for _, key in ipairs(needed_keys) do
        local v = table[key] -- use the table here, as a tag that must _not_ match might be 'nil' in the tags
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
            if (bool == "no" or bool == "0") then
                return false
            end

            if (bool ~= "yes" and bool ~= "1") then
                error("MustMatch got a string value it can't handle: " .. bool)
            end
        elseif (type(mapping) == "boolean") then
            if(not mapping) then
                return false
            end
        else
            error("The mapping is not a table. This is not supported. We got " .. tostring(mapping) .. " (" .. type(mapping)..")")
        end
    end

    -- Now that we know for sure that every key matches, we add them all
    for _, key in ipairs(needed_keys) do
        local v = tags[key] -- this is the only place where we use the original tags
        if (v ~= nil) then
            result.attributes_to_keep[key] = v
        end
    end

    return true
end