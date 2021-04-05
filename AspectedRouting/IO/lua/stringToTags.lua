function stringToTags(table, tags)
    if (tags == nil) then
        return table
    end
    return  table_to_list(tags, {}, table)
end