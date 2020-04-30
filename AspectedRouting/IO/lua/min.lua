function min(list)
    local min
    for _, value in ipairs(list) do
        if (min == nil) then
            min = value
        elseif (min > value) then
            min = value
        end
    end

    return min;
end