function if_then_else_dotted(conditionf, thnf, elsef, arg)
    local condition = conditionf(arg);
    if (condition) then
        return thnf(arg)
    else
        if(elsef == nil) then
            return nil
         end
        return elsef(arg) -- if no third parameter is given, 'els' will be nil
    end
end