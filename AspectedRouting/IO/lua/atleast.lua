function atleast(minimumExpected, actual, thn, els)
    if (minimumExpected <= actual) then
        return thn;
    end
    return els
end