local sproto = require "sproto"
local core = require "sproto.core"
local print_r = require "print_r"

local sp = sproto.parse [[
.Person {
	name 0 : string
	id 1 : integer
	email 2 : string

	.PhoneNumber {
		number 0 : string
		type 1 : integer
	}

	phone 3 : *PhoneNumber
}

.AddressBook {
	person 0 : *Person(id)
	others 1 : *Person
}
]]

-- core.dumpproto only for debug use
core.dumpproto(sp.__cobj)

local def = sp:default "Person"
print("default table for Person")
print_r(def)
print("--------------")

local ab = {
	person = {
		[10000] = {
			name = "Alice",
			id = 10000,
			phone = {
				{ number = "123456789" , type = 1 },
				{ number = "87654321" , type = 2 },
			}
		},
		[20000] = {
			name = "Bob",
			id = 20000,
			phone = {
				{ number = "01234567890" , type = 3 },
			}
		}
	},
	others = {
		{
			name = "Carol",
			id = 30000,
			phone = {
				{ number = "9876543210" },
			}
		},
	}
}

collectgarbage "stop"

local code = sp:encode("AddressBook", ab)
local addr = sp:decode("AddressBook", code)
print_r(addr)
