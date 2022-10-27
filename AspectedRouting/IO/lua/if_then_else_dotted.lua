function applyIfNeeded(f, arg)
    if(f == nil) then
        return nil
    end
    if(type(f) == "function") then
        return f(arg)
     else
        return f
    end
end

function if_then_else_dotted(conditionf, thnf, elsef, arg)
    local condition = applyIfNeeded(conditionf, arg); 
    if (condition) then
        return applyIfNeeded(thnf, arg)
    else
        return applyIfNeeded(elsef, arg) -- if no third parameter is given, 'else' will be nil
    end
end