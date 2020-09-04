# OGREE-3D
OGREE 3D is a data-center viewer  
- Unity version: *2019.2.11*

# Table of Contents
- [Getting started](#Getting-Started)
- [Build in CLI](#Build-in-CLI)
    - [Glossary](#Glossary)
    - [Loading commands](#Loading-commands)
        - [Load commands from a text file](#Load-commands-from-a-text-file)
        - [Load template from JSON](#Load-template-from-JSON)
    - [Hierarchy commands](#Hierarchy-commands)
        - [Select an object](#Select-an-object)
        - [Select parent object](#Select-parent-object)
        - [Delete object](#Delete-object)
    - [Create commands](#Create-commands)
        - [Create a Customer](#Create-a-Customer)
        - [Create a Datacenter](#Create-a-Datacenter)
        - [Create a Building](#Create-a-Building)
        - [Create a Room](#Create-a-Room)
        - [Create a Rack](#Create-a-Rack)
    - [Set commands](#Set-commands)
        - [Set colors for zones of all rooms in a datacenter](#Set-colors-for-zones-of-all-rooms-in-a-datacenter)
        - [Set reserved and technical zones of a room](#Set-reserved-and-technical-zones-of-a-room)
        - [Modify an object’s attribute](#Modify-an-object’s-attribute)
    - [Examples](#Examples)
- [Controls](#Controls)

# Getting started
Object hierarchy, each level is mandatory
```
Customer
|_ Datacenter
    |_ Building
        |_ Room
            |_ Rack
```
You have to create each object either with the build in CLI or by loading a file containing these commands.  
You can assign a tenant to a Datacenter, a room or a rack. If so, all children will have the same tenant by default.  
More levels will come in next releases.

# Build in CLI

## Glossary
`[name]` is case sensitive. It can include the whole path of the object if starting with a '/'. In that case, the selected object doesn't change  
`[full name]` is the full name containing the path of an object: `cu.dc.bd.ro.rk`  
`[orientation]` is a definition of an orthonormal with cardinal points (+x,+y): **EN**, **NW**, **WS**, **SE**  
`[color]` is a hexadecimal code (*ffffff*)  

## Loading commands
### Load commands from a text file
*`[path]` path of the file*  
```
.cmds:[path]  
```

### Load template from JSON
*`[type]` type of template: rack (more to come)  
`[path]` path of the file*  
```
.template:[type]@[path]
```  

## Hierarchy commands
### Select an object
*If `[full name]` is empty, go back to root*  
```
=[full name]
```

### Select parent object
```
..
```

### Delete object
```
-[full name]  
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
`[size]` is a Vector3 [width,length,height] (m,m,m)*  
```
+room:[name]@[pos]@[size]@[orientation]  
+ro:[name]@[pos]@[size]@[orientation]
```

### Create a Rack
Rack must be child of a room
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

### Create a Tenant  
```
+tenant:[name]@[color]
```  

## Set commands
### Set colors for zones of all rooms in a datacenter
```
[datacenter].usableColor=[color]
[datacenter].reservedColor=[color]
[datacenter].technicalColor=[color]
```  

### Set reserved and technical zones of a room  
*[reserved] is a vector4: [front,back,right,left] (tile,tile,tile,tile)  
[technical] is a vector4: [front,back,right,left] (tile,tile,tile,tile)*  
```
+zones:[reserved]@[technical]  
+zones:[full name]@[reserved]@[technical]
```

### Modify an object's attribute

*`[full name]` can be `current` for modifying selected objects attributes  
`[datacenter].[attribute]` can be comment / address / zipcode / city / country / gps(format:[x,y,z]) / tenant / usableColor / reservedColor / technicalColor  
`[building].[attribute]` can be description / nbfloors  
`[room].[attribute]` can be description / nbfloors / tenant  
`[object].[attribute]` can be description / vendor / type / model / serial / tenant / alpha  
`[tenant].[attribute]` can be mainContact / mainPhone / mainEmail*  
```  
[full name].[attribute]=[value]
current.[attribute]=[value]
```  

## Examples
```
+customer:DEMO
DEMO.mainContact=Ced
DEMO.mainPhone=0612345678
DEMO.mainEmail=ced@ogree3D.com

+tenant:Marcus@42ff42
Marcus.mainContact=Marcus Pandora
Marcus.mainPhone=0666666666
Marcus.mainEmail=marcus@pandora.com

+tenant:Billy@F0C300

+datacenter:ALPHA@NW
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

+building:/DEMO.ALPHA.A@[0,0,0]@[12,12,5]
DEMO.ALPHA.A.description=Building A
DEMO.ALPHA.A.nbFloors=1
+ro:/DEMO.ALPHA.A.R0_EN@[6,6,0]@[4.2,5.4,1]@EN
+ro:/DEMO.ALPHA.A.R0_NW@[6,6,0]@[4.2,5.4,1]@NW
+ro:/DEMO.ALPHA.A.R0_WS@[6,6,0]@[4.2,5.4,1]@WS
+ro:/DEMO.ALPHA.A.R0_SE@[6,6,0]@[4.2,5.4,1]@SE

+rk:/DEMO.ALPHA.A.R0_EN.TEST_EN@[ 1,1]@[60,120,42]@front
+rk:/DEMO.ALPHA.A.R0_NW.TEST_NW@[1 ,1]@[60,120,42]@front
+rk:/DEMO.ALPHA.A.R0_WS.TEST_WS@[1, 1]@[60,120,42]@front
+rk:/DEMO.ALPHA.A.R0_SE.TEST_SE@[1,1 ]@[60,120,42]@front

// Building B (will stay selected object)

+bd:B@[-30,10,0]@[25,29.4,5]
DEMO.ALPHA.B.description=Building B
DEMO.ALPHA.B.nbFloors=1

+room:R1@[0,0,0]@[22.8,19.8,4]@NW
+zones:[2,1,5,2]@[3,3,1,1]
DEMO.ALPHA.B.R1.description=First room

+ro:/DEMO.ALPHA.B.R2@[22.8,19.8,0]@[9.6,22.8,3]@WS
+zones:/DEMO.ALPHA.B.R2@[3,1,1,3]@[5,0,0,0]
DEMO.ALPHA.B.R2.description=Second room, owned by Marcus
DEMO.ALPHA.B.R2.tenant=Marcus

// Racks for R1

+rack:/DEMO.ALPHA.B.R1.A01@[1,1]@[60,120,42]@front
DEMO.ALPHA.B.R1.A01.description=Rack A01
DEMO.ALPHA.B.R1.A01.vendor=someVendor
DEMO.ALPHA.B.R1.A01.type=someType
DEMO.ALPHA.B.R1.A01.model=someModel
DEMO.ALPHA.B.R1.A01.serial=someSerial

+rk:/DEMO.ALPHA.B.R1.A02@[2,1]@[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.A03@[3,1]@[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.A04@[4,1]@[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.A05@[5,1]@[60,120,42]@front
DEMO.ALPHA.B.R1.A05.tenant=Billy

+rk:/DEMO.ALPHA.B.R1.B05 @[8,6] @[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R1.B09 @[9,6] @[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R1.B010@[10,6]@[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R1.B011@[11,6]@[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R1.B012@[12,6]@[60,120,42]@rear

+rk:/DEMO.ALPHA.B.R1.C08 @[8,9] @[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.C09 @[9,9] @[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.C010@[10,9]@[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.C011@[11,9]@[60,120,42]@front
+rk:/DEMO.ALPHA.B.R1.C012@[12,9]@[60,120,42]@front

+rk:/DEMO.ALPHA.B.R1.D01@[20,5]@[60,120,42]@left
DEMO.ALPHA.B.R1.D01.tenant=Marcus
+rk:/DEMO.ALPHA.B.R1.D02@[20,6]@[60,120,42]@left
DEMO.ALPHA.B.R1.D02.tenant=Marcus
+rk:/DEMO.ALPHA.B.R1.D03@[20,7]@[60,120,42]@left
DEMO.ALPHA.B.R1.D03.tenant=Marcus

+rk:/DEMO.ALPHA.B.R1.E01@[23,5]@[60,120,42]@right
DEMO.ALPHA.B.R1.E01.tenant=Marcus
+rk:/DEMO.ALPHA.B.R1.E02@[23,6]@[60,120,42]@right
DEMO.ALPHA.B.R1.E02.tenant=Marcus
+rk:/DEMO.ALPHA.B.R1.E03@[23,7]@[60,120,42]@right
DEMO.ALPHA.B.R1.E03.tenant=Marcus

// Racks for R2

+rk:/DEMO.ALPHA.B.R2.A01@[1,3]@[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R2.A02@[2,3]@[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R2.A03@[3,3]@[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R2.A04@[4,3]@[60,120,42]@rear
+rk:/DEMO.ALPHA.B.R2.A05@[5,3]@[60,120,42]@rear

+rk:/DEMO.ALPHA.B.R2.B01@[1,5]@[60,120,42]@front
DEMO.ALPHA.B.R2.B01.tenant=Billy
DEMO.ALPHA.B.R2.B01.alpha=50
```

# Controls  
- Right click: rotate camera
- zsqd or arrow keys: move camera  
- Left click: select an object (Room or Rack)
- LeftControl + LeftClick: Add/Remove object from selection  
- Alt + drag: Move selected rack(s)  
