
--[[
copies all attributes from the source-table into the target-table,
but only if the key is listed in 'whitelist' (which is a set)
]]
function copy_tags(source, target, whitelist)
    for k, v in pairs(source) do
        if whitelist[k] then
            target[k] = v
        end
    end
end