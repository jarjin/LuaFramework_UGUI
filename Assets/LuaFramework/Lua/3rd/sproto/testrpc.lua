local sproto = require "sproto"
local print_r = require "print_r"

local server_proto = sproto.parse [[
.package {
	type 0 : integer
	session 1 : integer
}

foobar 1 {
	request {
		what 0 : string
	}
	response {
		ok 0 : boolean
	}
}

foo 2 {
	response {
		ok 0 : boolean
	}
}

bar 3 {}

blackhole 4 {
}
]]

local client_proto = sproto.parse [[
.package {
	type 0 : integer
	session 1 : integer
}
]]

print("=== default table")

print_r(server_proto:default("package"))
print_r(server_proto:default("foobar", "REQUEST"))
assert(server_proto:default("foo", "REQUEST")==nil)
assert(server_proto:request_encode("foo")=="")
server_proto:response_encode("foo", { ok = true })
assert(server_proto:request_decode("blackhole")==nil)
assert(server_proto:response_decode("blackhole")==nil)

print("=== test 1")

-- The type package must has two field : type and session
local server = server_proto:host "package"
local client = client_proto:host "package"
local client_request = client:attach(server_proto)

print("client request foobar")
local req = client_request("foobar", { what = "foo" }, 1)
print("request foobar size =", #req)
local type, name, request, response = server:dispatch(req)
assert(type == "REQUEST" and name == "foobar")
print_r(request)
print("server response")
local resp = response { ok = true }
print("response package size =", #resp)
print("client dispatch")
local type, session, response = client:dispatch(resp)
assert(type == "RESPONSE" and session == 1)
print_r(response)

local req = client_request("foo", nil, 2)
print("request foo size =", #req)
local type, name, request, response = server:dispatch(req)
assert(type == "REQUEST" and name == "foo" and request == nil)
local resp = response { ok = false }
print("response package size =", #resp)
print("client dispatch")
local type, session, response = client:dispatch(resp)
assert(type == "RESPONSE" and session == 2)
print_r(response)

local req = client_request("bar")	-- bar has no response
print("request bar size =", #req)
local type, name, request, response = server:dispatch(req)
assert(type == "REQUEST" and name == "bar" and request == nil and response == nil)

local req = client_request "blackhole"
print("request blackhole size = ", #req)

print("=== test 2")
local v, tag = server_proto:request_encode("foobar", { what = "hello"})
print("tag =", tag)
print_r(server_proto:request_decode("foobar", v))
local v, tag = server_proto:response_encode("foobar", { ok = true })
print("tag =", tag)
print_r(server_proto:response_decode("foobar", v))
