function sum(list)
    local sum = 0
    for _, value in pairs(list) do
        if(value ~= nil) then
            if(value == 'yes' or value == 'true') then
                value = 1
            end
            sum = sum + value
        end
    end
    return sum;
end