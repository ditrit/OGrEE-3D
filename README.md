# OGREE-3D
OGREE 3D is a data-center viewer  
- Build windows in v2.0
- Build to come : macOS / Android
- Gamer gpu needed
- VR version to come  

*Unity version: 2019.2.11*  

# Table of Contents
- [Getting started](#Getting-Started)
- [Command line arguments](#Command-line-arguments)
- [Build in CLI](#Build-in-CLI)
    - [Glossary](#Glossary)
    - [Variable](#Variable)
        - [Set a variable](#Set-a-variable)
        - [Use a variable](#Use-a-variable)
    - [Loading commands](#Loading-commands)
        - [Load commands from a text file](#Load-commands-from-a-text-file)
        - [Load template from JSON](#Load-template-from-JSON)
    - [Hierarchy commands](#Hierarchy-commands)
        - [Select an object](#Select-an-object)
        - [Select child / children object](#Select-child-/-children-object)
        - [Select parent object](#Select-parent-object)
        - [Delete object](#Delete-object)
    - [Create commands](#Create-commands)
        - [Create a Customer](#Create-a-Customer)
        - [Create a Datacenter](#Create-a-Datacenter)
        - [Create a Building](#Create-a-Building)
        - [Create a Room](#Create-a-Room)
        - [Create a Separator](#Create-a-Separator)
        - [Create a Rack](#Create-a-Rack)
        - [Create a Device](#Create-a-Device)
        - [Create a Tenant](#Create-a-Tenant)
    - [Set commands](#Set-commands)
        - [Set colors for zones of all rooms in a datacenter](#Set-colors-for-zones-of-all-rooms-in-a-datacenter)
        - [Set reserved and technical zones of a room](#Set-reserved-and-technical-zones-of-a-room)
        - [Modify object’s attribute](#Modify-object’s-attribute)
            - [Display room’s tiles name or color](#Display-room’s-tiles-name-or-color) 
            - [Display rack's U helpers](#Display-rack's-U-helpers) 
            - [Display object's slots display](#Display-object's-slots-display) 
            - [Display object's local coordinate system](#Display-object's-local-coordinate-system) 
    - [Manipulate UI](#Manipulate-UI)
        - [Enable/Disable wireframe mode](#Enable/Disable-wireframe-mode)
        - [Display infos panel](#Display-infos-panel)
        - [Display debug panel](#Display-debug-panel)
    - [Manipulate camera](#Manipulate-camera)
        - [Move camera](#Move-camera)
        - [Translate camera](#Translate-camera)
        - [Wait between two translations](#Wait-between-two-translations)
    - [Communicate with API](#Communicate-with-API)
        - [Get](#Get)
        - [Post](#Post)
        - [Put](#Put)
        - [Delete](#Delete)
    - [Examples](#Examples)
- [Templates definition](#Templates-definition)
    - [Room template](#Room-template)
    - [Rack template](#Rack-template)
    - [Device template](#Device-template)
- [Controls](#Controls)
    - [Free mode movement](#Free-mode-movement)
    - [Human mode movement](#Human-mode-movement)

# Getting started
Object hierarchy, each level is mandatory
```
Customer
|_ Datacenter
    |_ Building
        |_ Room
            |_ Rack                                 Rack
                |_ Device                            |_ Chassis
                |   |_ Sub-device                    |   |_ Blade
                |   |   |_ Sub-device                |   |   |_ processor
                |   |   |_ Sub-device                |   |   |_ memory
                |   |_ ...                           |   |   |_ ...
                |_ Device                            |_ Server
                |_ Device                            |_ LANswitch
```
You have to create each object either with the build in CLI or by loading a file containing these commands.  
You can assign a tenant to a Datacenter, a room or a rack. If so, all children will have the same tenant by default.  
More levels will come in next releases.

# Command line arguments
You can override the config file parameters with command line arguments
```
--verbose [true|false]
--fullscreen [true|false]
--serverUrl [url]
--file [path to ocli file to load]
```  

# Build in CLI

## Glossary
`[name]` is case sensitive. It include the whole path of the object (for example: `cu.dc.bd.ro.rk`)  
`[orientation]` is a definition of an orthonormal with cardinal points (+x,+y, anti-clockwise): **EN**, **NW**, **WS**, **SE**
`[color]` is a hexadecimal code (*ffffff*)  

## Variables
### Set a variable:  
```
.var:[name]=[value]
```  
### Use a variable:  
```
${[name]}
```  

## Loading commands
### Load commands from a text file
*`[path]` path of the file*  
```
.cmds:[path]  
```

### Load template from JSON
*`[type]` type of template: rack, room or device  
`[path]` path of the file*  
```
.template:[type]@[path]
```  

## Hierarchy commands
### Select an object
*If `[name]` is empty, go back to root*  
```
=[name]
```

### Select child / children object
Select one or several children of current selected object.  
*`[relativeName]` is the hierarchy name without the selected object part*  
```
={[relativeName]}
={[relativeName],[relativeName],...}
```  

### Select parent object
```
..
```

### Delete object
Works with single or multi selection.  
If `@server` is used, also delete item in server
```
-[name]  
-selection  
-[name]@server  
-selection@server  
```  

## Create commands
### Create a Customer
Customer will be created as a new root.
```
+customer:[name]  
+cu:[name]
```

### Create a Datacenter
Datacenter must be child of a Customer.
```
+datacenter:[name]@[orientation]  
+dc:[name]@[orientation]
```

### Create a Building
Building must be child of a Datacenter.  
*`[pos]` is a Vector3 [x,y,z] (m,m,m)  
`[size]` is a Vector3 [width,length,height] (m,m,m)*  
```
+building:[name]@[pos]@[size]  
+bd:[name]@[pos]@[size]
```

### Create a Room
Room must be child of a building.  
Its name will be displayed in the center of the room in its local coordinates system.  
*`[pos]` is a Vector3 [x,y,z] (m,m,m)  
`[size]` is a Vector3 [width,length,height] (m,m,m)
`[template]` is the name of the room template*  
```
+room:[name]@[pos]@[size]@[orientation]  
+room:[name]@[pos]@[template]  
+ro:[name]@[pos]@[size]@[orientation]
+ro:[name]@[pos]@[template]
```

### Create a Separator  
A separator is a wire mesh wall inside a room.  
*`[pos1]` is a vector2 : the starting point of the separator in meter  
`[pos1]` is a vector2 :the ending point of the separator in meter*  
```
+separator:[name]@[pos1]@[pos2]
+sp:[name]@[pos1]@[pos2]
```  

### Create a Rack
Rack must be child of a room.  
*`[pos]` is a Vector2 [x,y] (tile,tile) can be decimal or fraction. Can also be negative  
`[size]` is a Vector3 [width,length,height (cm,cm,u)  
`[template]` is the name of the rack template  
`[orientation]` is front|rear|left|right*  
```
+rack:[name]@[pos]@[size]@[orientation]  
+rack:[name]@[pos]@[template]@[orientation]  
+rk:[name]@[pos]@[size]@[orientation]  
+rk:[name]@[pos]@[template]@[orientation]
```  

### Create a Device
A chassis is a *parent* device racked at a defined U position.  
*`[posU]` is the position in U in a rack  
`[sizeU]` is the height in U in a rack  
`[slot]` is the name of the slot in which you want to place the device  
`[template]` is the name of the device template  
`[side]` is from which side you can see the device if not "fullsize". This value is for overriding the one defined in the template. It can be front | rear | frontflipped | rearflipped*  
If the parent rack has slots:  
```
+device:[name]@[posU]@[sizeU]
+device:[name]@[posU]@[template]
```
If the parent rack doesn't have slots:
```
+device:[name]@[slot]@[sizeU]
+device:[name]@[slot]@[template]
```  

All other devices (blades / components like processor, memory, adapters, disks...) have to be declared with a parent's slot and a template.  
```
+device:[name]@[slot]@[template]
+device:[name]@[slot]@[template]@[side]
+dv:[name]@[slot]@[template]
+dv:[name]@[slot]@[template]@[side]
```  

### Create a Tenant  
```
+tenant:[name]@[color]
+tn:[name]@[color]
```  

## Set commands  
You have to modify an object in the same file than its creation.   
### Set colors for zones of all rooms in a datacenter
```
[datacenter].usableColor=[color]
[datacenter].reservedColor=[color]
[datacenter].technicalColor=[color]
```  

### Set reserved and technical zones of a room  
Enables tiles edges display.  
You can modify areas only if the room has no racks in it.  
**Technical** area : typically a restricted zone where power panels and AC systems are installed. separated from "IT space" with either a wall or a wire mesh  
**Reserved** area : some tiles around the room that must be kept free to move racks and walk (usually 2 or 3 tiles)  

*`[reserved]` is a vector4: [front,back,right,left] (tile,tile,tile,tile)  
`[technical]` is a vector4: [front,back,right,left] (tile,tile,tile,tile)*  
```
[room].areas=[reserved]@[technical]  
```

### Modify object's attribute
Works with single or multi selection.  
*`[name]` can be `selection` or `_` for modifying selected objects attributes  
`[datacenter].[attribute]` can be comment / address / zipcode / city / country / gps(format:[x,y,z]) / tenant / usableColor / reservedColor / technicalColor  
`[building].[attribute]` can be description / nbfloors  
`[room].[attribute]` can be description / floors / tenant  
`[object].[attribute]` can be description / vendor / type / model / serial / tenant / alpha  
`[tenant].[attribute]` can be mainContact / mainPhone / mainEmail*  
```  
[full name].[attribute]=[value]

selection.[attribute]=[value]
_.[attribute]=[value]
```  

#### Display room's tiles name or color  
```
[room].tilesName=[true|false]
[room].tilesColor=[true|false]
```  

#### Display rack's U helpers
Display or hide U location dummies to simply identify objects in a rack.  
```
[rack].U=[true|false]
```  

#### Display object's slots display
```
[name].slots=[true|false]
```  

#### Display object's local coordinate system
```
[name].localCS=[true|false]
```  

## Manipulate UI
### Delay commands
You can put delay before each command: up to 2 seconds.  
```
ui.delay=[time]
```  

### Enable/Disable wireframe mode
```
ui.wireframe=[true|false]
```  

### Display infos panel
```
ui.infos=[true|false]
```  

### Display debug panel
```
ui.debug=[true|false]
```  

## Manipulate camera
### Move camera
Move the camera to the given point.  
*`[position]` is a Vector3: the new position of the camera  
`[rotation]` is a Vector2: the rotation of the camera*  
```
camera.move=[position]@[rotation]
```  

### Translate camera
Move the camera to the given destination. You can stack several destinations, the camera will move to each point in the given order.  
*`[position]` is a Vector3: the position of the camera's destination  
`[rotation]` is a Vector2: the rotation of the camera's destination*  
```
camera.translate=[position]@[rotation]
```  

### Wait between two translations 
You can define a delay between two camera translations.  
*`[time]` is the time to wait in seconds*  
```
camera.wait=[time]
```   

## Communicate with API  
The server url is given in config file / command line argument.  
Works only with customers and datacenters.  

### Get  
Get an item from server. Create the corresponding object in scene.  
```
api.get=[type/id]
```  
### Post  
Post the item on server.  
Automatically called on creation.  
```
api.post=[name]
```  

### Put  
Put the item on server.  
Automatically called 2 seconds after the last item modification.  
```
api.put=[name]
```  

### Delete  
Delete the item on server only.  
Automatically called if '@server' used on delete command.  
```
api.delete=[name]
```  


## Examples
```
+cu:DEMO
    DEMO.mainContact=Ced
    DEMO.mainPhone=0612345678
    DEMO.mainEmail=ced@ogree3D.com

+tn:Marcus@42ff42
    Marcus.mainContact=Marcus Pandora
    Marcus.mainPhone=0666666666
    Marcus.mainEmail=marcus@pandora.com

+tenant:Billy@F0C300

+dc:DEMO.ALPHA@NW
    DEMO.ALPHA.comment=This is a demo...
    DEMO.ALPHA.address=1 rue bidule
    DEMO.ALPHA.zipcode=42000
    DEMO.ALPHA.city=Truc
    DEMO.ALPHA.country=FRANCE
    DEMO.ALPHA.gps=[1,2,0]
    DEMO.ALPHA.usableColor=5BDCFF
    DEMO.ALPHA.reservedColor=AAAAAA
    DEMO.ALPHA.technicalColor=D0FF78

// Building A

+bd:DEMO.ALPHA.A@[0,0,0]@[12,12,5]
    DEMO.ALPHA.A.description=Building A
    DEMO.ALPHA.A.nbFloors=1
+ro:DEMO.ALPHA.A.R0_EN@[6,6,0]@[4.2,5.4,1]@EN
+ro:DEMO.ALPHA.A.R0_NW@[6,6,0]@[4.2,5.4,1]@NW
+ro:DEMO.ALPHA.A.R0_WS@[6,6,0]@[4.2,5.4,1]@WS
+ro:DEMO.ALPHA.A.R0_SE@[6,6,0]@[4.2,5.4,1]@SE

+rk:DEMO.ALPHA.A.R0_EN.TEST_EN@[ 1,1]@[60,120,42]@front
+rk:DEMO.ALPHA.A.R0_NW.TEST_NW@[1 ,1]@[60,120,42]@front
+rk:DEMO.ALPHA.A.R0_WS.TEST_WS@[1, 1]@[60,120,42]@front
+rk:DEMO.ALPHA.A.R0_SE.TEST_SE@[1,1 ]@[60,120,42]@front

// Building B

+bd:DEMO.ALPHA.B@[-30,10,0]@[25,29.4,5]
    DEMO.ALPHA.B.description=Building B
    DEMO.ALPHA.B.nbFloors=1

+ro:DEMO.ALPHA.B.R1@[0,0,0]@[22.8,19.8,4]@NW
    DEMO.ALPHA.B.R1.areas=[2,1,5,2]@[3,3,1,1]
    DEMO.ALPHA.B.R1.description=First room

+ro:DEMO.ALPHA.B.R2@[22.8,19.8,0]@[9.6,22.8,3]@WS
    DEMO.ALPHA.B.R2.areas=[3,1,1,3]@[5,0,0,0]
    DEMO.ALPHA.B.R2.description=Second room, owned by Marcus
    DEMO.ALPHA.B.R2.tenant=Marcus

// Racks for R1

+rk:DEMO.ALPHA.B.R1.A01@[1,1]@[60,120,42]@front
    DEMO.ALPHA.B.R1.A01.description=Rack A01
    DEMO.ALPHA.B.R1.A01.vendor=someVendor
    DEMO.ALPHA.B.R1.A01.type=someType
    DEMO.ALPHA.B.R1.A01.model=someModel
    DEMO.ALPHA.B.R1.A01.serial=someSerial

+rk:DEMO.ALPHA.B.R1.A02@[2,1]@[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.A03@[3,1]@[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.A04@[4,1]@[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.A05@[5,1]@[60,120,42]@front
    DEMO.ALPHA.B.R1.A05.tenant=Billy

+rk:DEMO.ALPHA.B.R1.B05 @[8,6] @[60,120,42]@rear
+rk:DEMO.ALPHA.B.R1.B09 @[9,6] @[60,120,42]@rear
+rk:DEMO.ALPHA.B.R1.B010@[10,6]@[60,120,42]@rear
+rk:DEMO.ALPHA.B.R1.B011@[11,6]@[60,120,42]@rear
+rk:DEMO.ALPHA.B.R1.B012@[12,6]@[60,120,42]@rear

+rk:DEMO.ALPHA.B.R1.C08 @[8,9] @[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.C09 @[9,9] @[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.C010@[10,9]@[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.C011@[11,9]@[60,120,42]@front
+rk:DEMO.ALPHA.B.R1.C012@[12,9]@[60,120,42]@front

+rk:DEMO.ALPHA.B.R1.D01@[20,5]@[60,120,42]@left
    DEMO.ALPHA.B.R1.D01.tenant=Marcus
+rk:DEMO.ALPHA.B.R1.D02@[20,6]@[60,120,42]@left
    DEMO.ALPHA.B.R1.D02.tenant=Marcus
+rk:DEMO.ALPHA.B.R1.D03@[20,7]@[60,120,42]@left
    DEMO.ALPHA.B.R1.D03.tenant=Marcus

+rk:DEMO.ALPHA.B.R1.E01@[23,5]@[60,120,42]@right
    DEMO.ALPHA.B.R1.E01.tenant=Marcus
+rk:DEMO.ALPHA.B.R1.E02@[23,6]@[60,120,42]@right
    DEMO.ALPHA.B.R1.E02.tenant=Marcus
+rk:DEMO.ALPHA.B.R1.E03@[23,7]@[60,120,42]@right
    DEMO.ALPHA.B.R1.E03.tenant=Marcus

// Racks for R2

+rk:DEMO.ALPHA.B.R2.A01@[1,3]@[60,120,42]@rear
+rk:DEMO.ALPHA.B.R2.A02@[2,3]@[60,120,42]@rear
+rk:DEMO.ALPHA.B.R2.A03@[3,3]@[60,120,42]@rear
+rk:DEMO.ALPHA.B.R2.A04@[4,3]@[60,120,42]@rear
+rk:DEMO.ALPHA.B.R2.A05@[5,3]@[60,120,42]@rear

+rk:DEMO.ALPHA.B.R2.B01@[1,5]@[60,120,42]@front
    DEMO.ALPHA.B.R2.B01.tenant=Billy
    DEMO.ALPHA.B.R2.B01.alpha=50

// Edit description of several racks in R1
={B05,B09,B10,B11,B12}
selection.description=Row B
```

# Templates definition
Templates are json files describing an object.  

## Room template
```
{
  "slug"          : "Slug",
  "orientation"   : "EN|NW|WS|SE",
  "sizeWDHm"      : [width,depth,height],
  "technicalArea" : [front,back,right,left],
  "reservedArea"  : [front,back,right,left],
  "separators"    : [
      { "name" : "Name", "pos1XYm" : [x,y], pos2XYm : [x,y] },
      ...
  ],
  "tiles" : [
      { "location": "x/y",   "name": "Name", "label" : "Label", "type" : "plain|perf30|perf50", "color": "standard|color", "comment": "Comment"},
      ...
  ],
  "aisles" : [
      { "name" : "Name", "locationY" : y, "orientation" : "rear|front" },
      ...
  ]
}
```  

## Rack template
```
{ 
 "name"       : "Name can have spaces",
 "slug"       : "slug-is-lowercase-with-no-space",
 "vendor"     : "Vendor",
 "model"      : "Model",
 "type"       : "rack",
 "role"       : "parent",
 "orientation": "horizontal",      
 "side"       : "top",
 "fulldepth" : "yes/no",
 "sizeWDHmm"  : [width ,depth ,height],
 "sizeWDU"  : [width ,depth ,heightU],
 "components" : [
   { "location": "location",  "family": "u/ou", "role": "rack", "installed" : "fulldepth/rear/front/left/right/rearleft/rearright",  "elemPos" : [x,y,z] , "elemSize" : [x,y,z], "mandatory":"yes/no", "labelPos":"front/rear/frontrear/top/right/left/none" },
    ...
  ]
}

```  

## Device template
```
{ 
 "slug"        : "slug-is-lowercase-with-no-space",
 "description" : "Description can have spaces",
 "vendor"      : "Vendor",
 "model"       : "Model",
 "type"        : "chassis/blade",
 "role"        : "parent/child",
 "side"        : "front/rear/frontflipped/rearflipped",
 "fulldepth"  : "yes/no",
 "sizeWDHmm"   : [x,y,z],
 "components"  : [
  { "location": "location",  "type": "chassis/blade",   "role": "parent/child", "position" : "front/rear",  "elemPos" : [x,y,z],  "elemSize" : [x,y,z], "mandatory":"yes/no",  "labelPos":"front/rear/frontrear/left/right/top/none" },
  ...    
  ]
}
```  

# Controls  
- Left click: select an object (Room or Rack)
- Left control + Left click: Add/Remove object from selection  
- Alt + drag: Move selected rack(s)  

## Free mode movement 
- Right click + drag: rotate camera
- Middle click + drag: move camera on X & Z local axis
- Scroll wheel: move camera on Y local axis
- zsqd / arrow keys: move camera on X & Y local axis 
- Left shift + zsqd / arrow keys: rotate camera

## Human mode movement
- Right click + drag: rotate camera
- zsqd / arrow keys: move camera on X & Y axis 
- Left shift + zsqd / arrow keys: rotate camera
