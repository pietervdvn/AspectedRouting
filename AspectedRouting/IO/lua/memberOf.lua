function memberOf(calledIn, parameters, tags, result)
    local k = "_relation:" .. calledIn
    -- This tag is conveniently setup by all the preprocessors, which take the parameters into account
    local doesMatch = tags[k]
    if (doesMatch == "yes") then
        result.attributes_to_keep[k] = "yes"
        return true
    end
    return false
end