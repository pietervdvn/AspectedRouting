function all(list)
    for _, value in pairs(list) do
        if (value == nil) then
            return false
        end
        
        if(value ~= "yes" and value ~= "true") then
            return false
        end
    end

    return true;
end