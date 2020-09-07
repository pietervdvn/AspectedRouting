function containedIn(list, a)
    for _, value in pairs(list) do
        if (value == a) then
            return true
        end
    end

    return false;
end