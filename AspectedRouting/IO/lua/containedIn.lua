function containedIn(list, a)
    for _, value in ipairs(list) do
        if (value == a) then
            return true
        end
    end

    return false;
end