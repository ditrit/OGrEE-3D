# OGREE-3D
OGREE 3D is a data-center viewer, using Unity 3D  
- Build windows / macOS / Linux: 3.0.0
- Build to come: Android
- Gamer gpu needed
- VR version to come  

## Work on OGrEE-3D
- Install Unity version **2022.3.12f1**
- For loading fbx models at runtime, we are using a third party plugin: [**TriLib 2**](https://assetstore.unity.com/packages/tools/modeling/trilib-2-model-loading-package-157548) **v2.3.6**. You'll have to purchase it.
- Open the project in Unity Editor. A warning message will ask you to enter into safe mode. Deny it and continue to open the project.
- Import your TriLib2 package in the *Assets* folder. Then, add *TRILIB* in the scripting define symbols to enable related code if needed.

# Table of Contents
- [OGREE-3D](#ogree-3d)
  - [Work on OGrEE-3D](#work-on-ogree-3d)
- [Table of Contents](#table-of-contents)
- [Getting started](#getting-started)
- [Command line arguments](#command-line-arguments)
- [Draw objects](#draw-objects)
- [Controls](#controls)
  - [Free mode movement](#free-mode-movement)
  - [Human mode movement](#human-mode-movement)

# Getting started
Object hierarchy, each level is mandatory
```
Site
 |_ Building
     |_ Room
         |_ Rack                                 Rack
             |_ Device                            |_ Chassis
             |   |_ Device                        |   |_ Blade
             |   |   |_ Device                    |   |   |_ processor
             |   |   |_ Device                    |   |   |_ memory
             |   |_ ...                           |   |   |_ ...
             |_ Device                            |_ Server
             |_ Device                            |_ LANswitch
```
You have to create each object either with the build in CLI or by loading a file containing these commands.  
You can assign a domain to an OGrEE object. If so, all children will have the same domain by default.  
More levels will come in next releases.

# Command line arguments
You can override the config file parameters with command line arguments
```
--verbose [true|false]
--fullscreen [true|false]
--file [path to ocli file to load]
```  

# Draw objects

For drawing objects in OGrEE-3D, we need to use the [CLI](https://github.com/ditrit/OGrEE-Core/tree/main/CLI) from the [OGrEE-Core](https://github.com/ditrit/OGrEE-Core) repository.

# Controls  
- Left click: select an object (Room or Rack)
- Left control + Left click: Add/Remove object from selection   

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
