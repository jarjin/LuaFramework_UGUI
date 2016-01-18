local protobuf = require "protobuf"

addr = io.open("../../build/addressbook.pb","rb")
buffer = addr:read "*a"
addr:close()
protobuf.register(buffer)

local person = {
	name = "Alice",
	id = 123,
	phone = {
		{ number = "123456789" , type = "MOBILE" },
		{ number = "87654321" , type = "HOME" },
	}
}

local buffer = protobuf.encode("tutorial.Person", person)

local t = protobuf.decode("tutorial.Person", buffer)

for k,v in pairs(t) do
	if type(k) == "string" then
		print(k,v)
	end
end

print(t.phone[2].type)

for k,v in pairs(t.phone[1]) do
	print(k,v)
end

