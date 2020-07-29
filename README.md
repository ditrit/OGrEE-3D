# OGREE-3D

- Unity version: *2019.2.11*

OGREE 3D is a data-center viewer

For DitRit members, demo builds are available in Nextcloud (Windows only).

## Build in CLI

- Glossary
[pos] is a Vector3: [x,y,z]
[size] is a Vector3: [x,y,z]
[orientation] is a cardinal point: N, S, E, W
[reserved] is a vector4: [N,S,E,W]
[technical] is a vector4: [N,S,E,W]

- Create a new Customer at root.
+customer:[name]
+cu[name]

- Create a datacenter child of a customer.
+datacenter:[name]@[orientation]
+dc:[name]@[orientation]

- Create a buiding child of a datacenter.
+building:[name]@[pos]@[size]
+bd:[name]@[pos]@[size]

- Create a room child of a building
+room:[name]@[pos]@[size]@[orientation]
+ro:[name]@[pos]@[size]@[orientation]

- Set reserved and technical zones of a room.
+zones:[reserved]@[technical]


## Controls
- Right clic : rotate camera
- wasd or arrow keys : move camera
