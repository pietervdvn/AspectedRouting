function default(defaultValue, realValue)
    if(realValue ~= nil) then
        return realValue
    end
    return defaultValue
end