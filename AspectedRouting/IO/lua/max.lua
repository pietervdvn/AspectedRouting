function max(list)
    local max
    for _, value in ipairs(list) do
        if (max == nil) then
            max = value
        elseif (max < value) then
            max = value
        end
    end

    return max;
end