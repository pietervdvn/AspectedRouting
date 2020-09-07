function multiply(list)
    local factor = 1
    for _, value in pairs(list) do
        if (value ~= nil) then
            factor = factor * value
        end
    end
    return factor;
end