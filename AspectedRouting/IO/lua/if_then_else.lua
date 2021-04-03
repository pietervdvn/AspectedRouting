function if_then_else(condition, thn, els)
    if (condition ~= nil and (condition == "yes" or condition == true or condition == "true")) then
        return thn
    else
        return els -- if no third parameter is given, 'els' will be nil
    end
end