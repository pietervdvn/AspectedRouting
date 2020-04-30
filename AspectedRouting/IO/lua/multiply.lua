function multiply(list)
    local factor = 1
    for _, value in ipairs(list) do
        factor = factor * value
    end
    return factor;
end