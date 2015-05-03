
local env = {}

-- standard stuffs
env.assert   = assert
env.ipairs   = ipairs
env.next     = next
env.pairs    = pairs
env.pcall    = pcall
env.select   = select
env.tonumber = tonumber
env.tostring = tostring
env.type     = type
env.unpack   = unpack
env.xpcall   = xpcall

-- string
env.string.byte    = string.byte
env.string.char    = string.char
env.string.find    = string.find
env.string.format  = string.format
env.string.gmatch  = string.gmatch
env.string.gsub    = string.gsub
env.string.len     = string.len
env.string.lower   = string.lower
env.string.match   = string.match
env.string.rep     = string.rep
env.string.reverse = string.reverse
env.string.sub     = string.sub
env.string.upper   = string.upper

-- table
env.table.insert = table.insert
env.table.maxn   = table.maxn
env.table.remove = table.remove
env.table.sort   = table.sort

-- math
env.math.abs   = math.abs
env.math.acos  = math.acos
env.math.asin  = math.asin
env.math.atan  = math.atan
env.math.atan2 = math.atan2
env.math.ceil  = math.ceil
env.math.cos   = math.cos
env.math.cosh  = math.cosh
env.math.deg   = math.deg
env.math.exp   = math.exp
env.math.floor = math.floor
env.math.fmod  = math.fmod
env.math.frexp = math.frexp
env.math.huge  = math.huge
env.math.ldexp = math.ldexp
env.math.log   = math.log
env.math.log10 = math.log10
env.math.max   = math.max
env.math.min   = math.min
env.math.modf  = math.modf
env.math.pi    = math.pi
env.math.pow   = math.pow
env.math.rad   = math.rad
env.math.sin   = math.sin
env.math.sinh  = math.sinh
env.math.sqrt  = math.sqrt
env.math.tan   = math.tan
env.math.tanh  = math.tanh
