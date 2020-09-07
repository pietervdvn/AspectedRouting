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
        if(elsef == nil) then
            return nil
         end
        return applyIfNeeded(elsef, arg) -- if no third parameter is given, 'els' will be nil
    end
end