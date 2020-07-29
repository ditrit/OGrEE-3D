# OGREE-3D

- Unity version: *2019.2.11*

OGREE 3D is a data-center viewer

For DitRit members, demo builds are available in Nextcloud (Windows only).

## Build in CLI

### Glossary  
[name] can include the whole path of the object if starting with a '/'  
[pos] is a Vector3: [x,y,z]  
[size] is a Vector3: [x,y,z]  
[orientation] is a cardinal point: N, S, E, W  
[reserved] is a vector4: [N,S,E,W]  
[technical] is a vector4: [N,S,E,W]  

### Commands
- Create a new Customer at root.  
```
+customer:[name]  
+cu:[name]
```

- Create a datacenter child of a customer.  
```
+datacenter:[name]@[orientation]  
+dc:[name]@[orientation]
```

- Create a buiding child of a datacenter.  
```
+building:[name]@[pos]@[size]  
+bd:[name]@[pos]@[size]
```

- Create a room child of a building  
```
+room:[name]@[pos]@[size]@[orientation]  
+ro:[name]@[pos]@[size]@[orientation]
```

- Set reserved and technical zones of a room.  
```
+zones:[reserved]@[technical]  
+zones:[full name]@[reserved]@[technical]
```

### Examples
```
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
```

## Controls
- Right clic : rotate camera
- zsqd or arrow keys : move camera
