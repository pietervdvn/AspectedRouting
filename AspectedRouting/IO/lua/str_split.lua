--[[
 Splits a string on the specified character, e.g.
 str_split("abc;def;ghi", ";") will result in a table ["abc","def","ghi"]
]]
function str_split (inputstr, sep)
        if sep == nil then
                sep = "%s"
        end
        local t={}
        for str in string.gmatch(inputstr, "([^"..sep.."]+)") do
                table.insert(t, str)
        end
        return t
end
