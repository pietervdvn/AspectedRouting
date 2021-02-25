function string_start(strt, s)
    return string.sub(s, 1, string.len(strt)) == strt
end


-- every key starting with "_relation:<name>:XXX" is rewritten to "_relation:XXX"
function remove_relation_prefix(tags, name)

    local new_tags = {}
    for k, v in pairs(tags) do
        local prefix = "_relation:" .. name .. ":";
        if (string_start(prefix, k)) then
            local new_key = "_relation:" .. string.sub(k, string.len(prefix) + 1) -- plus 1: sub uses one-based indexing to select the start
            new_tags[new_key] = v
        else
            new_tags[k] = v
        end
    end
    return new_tags
end