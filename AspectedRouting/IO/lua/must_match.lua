--[[
must_match checks that a collection of tags matches a specification.

The function is not trivial and contains a few subtilities.

Consider the following source:

{"$mustMatch":{ "a":"X", "b":{"not":"Y"}}}

This is desugared into

{"$mustMatch":{ "a":{"$eq":"X"}, "b":{"not":"Y"}}}

When applied on the tags {"a" : "X"}, this yields the table {"a":"yes", "b":"yes} (as `notEq` "Y" "nil") yields "yes"..
MustMatch checks that every key in this last table yields yes - even if it is not in the original tags!


Arguments:
- The tags of the feature
- The result table, where 'attributes_to_keep' might be set
- needed_keys which indicate which keys must be present in 'tags'
- table which is the table to match

]]
function must_match(needed_keys, table, tags, result)
    for _, key in ipairs(needed_keys) do
        local v = tags[key]
        if (v == nil) then
            -- a key is missing...

            -- this probably means that we must return false... unless the mapping returns something for null!
            local mappng = table[key]
            if (mappng ~= nil) then
                -- there is a mapping! We might be in luck...
                local resultValue = mappng[v]
                if (resultValue == nil or resultValue == false) then
                    -- nope, no luck after all
                    return false
                end
                if (resultValue == true or resultValue == "yes") then
                    return true
                end
            else
                return false
            end
        end

        local mapping = table[key]
        if (mapping == nil) then
            -- the mapping is nil! That is fine, the key is present anyway
            -- we ignore
        elseif (type(mapping) == "table") then
            -- we have to map the value with a function:
            local resultValue = mapping[v]
            if (resultValue ~= nil or -- actually, having nil for a mapping is fine for this function!.
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
            if (not mapping) then
                return false
            end
        else
            error("The mapping is not a table. This is not supported. We got " .. tostring(mapping) .. " (" .. type(mapping) .. ")")
        end
    end

    -- Now that we know for sure that every key matches, we add them all to the 'attributes_to_keep'
    if (result == nil) then
        -- euhm, well, seems like we don't are about the attributes_to_keep; early return!
        return true
    end
    for _, key in ipairs(needed_keys) do
        local v = tags[key] -- this is the only place where we use the original tags
        if (v ~= nil) then
            result.attributes_to_keep[key] = v
        end
    end

    return true
end