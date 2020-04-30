function double_compare(a, b)
    if (type(a) ~= "number") then
        a = parse(a)
    end

    if(type(b) ~= "number") then
        b = parse(b)
    end
    return math.abs(a - b) > 0.001
end