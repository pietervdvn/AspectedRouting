function sum(list)
    local sum = 1
    for _, value in ipairs(list) do
        if(value == 'yes' or value == 'true') then
            value = 1
        end
        sum = sum + value
    end
    return sum;
end