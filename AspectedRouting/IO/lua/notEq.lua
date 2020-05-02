function notEq(a, b)
    if (b == nil) then
        b = "yes"
    end
    
    if (a ~= b) then
        return "yes"
    else
        return "no"
    end
end