Most names are not case sensitive.

The starting point files are *deps.lua* and *world.lua*.

# deps.lua
A list of the packages that should be loaded before the one this file is in.
### Example

    doors = true
    cats  = true
    body_parts = true

# world.json
A map of entity containers to add to the world.
### Container values:
* `"type"`: Either `"list"` or `"grid"`.
* `"properties"`: A list of properties.
* `"bases"`: A list of entity bases.
* `"size"`: Only for grid containers. A length 3 list of integers for the dimensions of the grid.
### Property values:
* `"name"`: The name of the property.
* `"type"`: Either `"int"`, `"float"`, `"bool"` or `"string"`.
* `"default"`: The default value for that property. This is optional, and if none is supplied
               then int, float, bool and string properties default to `0`, `0`, `false` and `""` respectively.
### Entity base values:
* "name": The name of the base.
* "values": A map of property names to values.

### Example

    require "item_props.lua"
    require "item_bases.lua"

    items = {
      type = "list"
      properties = item_props
      bases      = item_bases
    }

    phys = {
      type = "list",
      properties = {
          { name = "x", type = "float" },
          { name = "y", type = "float" },
          { name = "xvel", type = "float" },
          { name = "yvel", type = "float" },
      },
    }
    phys.bases = {}
    phys.bases.sanic = { xvel = 999999999, yvel = 999999999 }
