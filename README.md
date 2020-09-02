# OGREE-3D

- Unity version: *2019.2.11*

OGREE 3D is a data-center viewer

For DitRit members, demo builds are available in Nextcloud (Windows only).

## Build in CLI

### Glossary  
[name] is case sensitive. It can include the whole path of the object if starting with a '/'. In that case, the selected object doesn't change  
[orientation] is a definition of an orthonormal with cardinal points (+x,+y): EN, NW, WS, SE  

### Commands  
- Load commands from a text file  
*[path] path of the file*  
```
.cmds:[path]  
```

- Load template from JSON  
*[type] type of template: rack (more to come)  
[path] path of the file*  
```
.template:[type]@[path]
```

- Select an object  
*If [full name] is empty, go back to root*  
```
=[full name]
```

- Select parent object
```
..
```

- Delete object  
```
-[full name]  
```  

- Create a new Customer at root  
```
+customer:[name]  
+cu:[name]
```

- Create a datacenter child of a customer  
```
+datacenter:[name]@[orientation]  
+dc:[name]@[orientation]
```

- Create a buiding child of a datacenter  
*[pos] is a Vector3 [x,y,z] (m,m,m)  
[size] is a Vector3 [x,y,z] (m,m,m)*  
```
+building:[name]@[pos]@[size]  
+bd:[name]@[pos]@[size]
```

- Create a room child of a building  
*[pos] is a Vector3 [x,y,z] (m,m,m)  
[size] is a Vector3 [x,y,z] (m,m,m)*  
Its name will be displayed in its local coordinates system.  
```
+room:[name]@[pos]@[size]@[orientation]  
+ro:[name]@[pos]@[size]@[orientation]
```

- Set reserved and technical zones of a room  
*[reserved] is a vector4: [front,back,right,left] (tile,tile,tile,tile)  
[technical] is a vector4: [front,back,right,left] (tile,tile,tile,tile)*  
```
+zones:[reserved]@[technical]  
+zones:[full name]@[reserved]@[technical]
```

- Create a rack, child of a room  
*[pos] is a Vector2 [x,y] (tile,tile) can be decimal or fraction. Can also be negative  
[size] is a Vector3 [x,y,z] (cm,cm,u)  
[template] is the name of the rack template  
[orientation] is front|rear|left|right*  
```
+rack:[name]@[pos]@[size]@[orientation]  
+rack:[name]@[pos]@[template]@[orientation]  
+rk:[name]@[pos]@[size]@[orientation]  
+rk:[name]@[pos]@[template]@[orientation]
```  

- Create a tenant  
*[color] is a hexadecimal code (ffffff)*  
```
+tenant:[name]@[color]
```  

- Modify an object's attribute  
*[full name] can be current for modifying selected objects attributes  
[datacenter].[attribute] can be comment / address / zipcode / city / country / gps(format:[x,y,z]) / usableColor / reservedColor / technicalColor  
[building].[attribute] can be description / nbfloors  
[room].[attribute] can be description / nbfloors / tenant  
[object].[attribute] can be description / vendor / type / model / serial / tenant / alpha 
[tenant].[attribute] can be mainContact / mainPhone / mainEmail*  
```  
[full name].[attribute]=[value]
current.[attribute]=[value]
```  


### Examples
```
.cmds:C:\Users\Cedrok\Desktop\testCmds.txt

.template:rack@K:\IBM Rack 1410-PRB\ibm-rack.json

+customer:DEMO
+datacenter:BETA@N
+building:/DEMO.BETA.A@[0,80,0]@[20,30,4]
+building:/DEMO.BETA.B@[0,20,0]@[20,30,4]
+bd:C@[30,0,0]@[60,135,5]
+room:R1@[0,15,0]@[60,60,5]@W
+room:/DEMO.BETA.C.R2@[0,75,0]@[60,60,5]@W
+ro:/DEMO.BETA.C.Office@[60,0,0]@[20,75,4]@N
+zones:[2,1,3,3]@[4,4,4,4]
+zones:/DEMO.BETA.C.R2@[3,3,3,3]@[5,0,0,0]

+rack:A01@[1,1]@[60,120,42]@rear
+rack:/DEMO.BETA.C.R1.A02@[2,1]@[60,120,42]@rear
+rack:/DEMO.BETA.C.R1.B01@[1,3]@[60,120,42]@front
+rack:/DEMO.BETA.C.R1.B02@[2,3]@[60,120,42]@front
+rack:/DEMO.BETA.C.R2.A01@[1,1]@[60,120,42]@rear

+rack:/DEMO.BETA.C.R1.C01@[1,6]@ibm-rack42u@rear
+rack:/DEMO.BETA.C.R1.C02@[2,6]@ibm-rack42u@rear
+rack:/DEMO.BETA.C.R1.C03@[3,6]@ibm-rack42u@rear
+rack:/DEMO.BETA.C.R1.C04@[4,6]@ibm-rack42u@rear
+rack:/DEMO.BETA.C.R1.C05@[5,6]@ibm-rack42u@rear
```

## Controls  
- Right click: rotate camera  
- zsqd or arrow keys: move camera  
  
- Left click: select an object (Room or Rack)
- LeftControl + LeftClick: Add/Remove object from selection  
