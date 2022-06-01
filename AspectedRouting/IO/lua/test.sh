#! /bin/bash

echo "" > temp.lua

for f in `ls`
do
	if [[ $f != "temp.lua" && $f != "test.sh" && $f != "not.lua" ]]
	then
		cat $f >> temp.lua
		echo -e "\n\n" >> temp.lua
	fi
done

cat << TESTCODE >> temp.lua

print("------------ TESTS --------------")

function expect(expected, actual)
	if (actual ~= expected) then
		print("Expected "..expected.." but got "..actual)
	else
		print("OK")
	end
end

expect(-1, calculate_turn_cost_factor({["type"] = "restriction", restriction = "no_left_turn"}, {}))
expect(0, calculate_turn_cost_factor({["type"] = "restriction", restriction = "no_left_turn", except="bicycle;cargo_bike"}, {"bicycle"}))
TESTCODE

lua temp.lua
