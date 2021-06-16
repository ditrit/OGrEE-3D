# OGREE-3D
OGREE 3D is a data-center viewer  
- Build windows in v2.2
- Build to come : macOS / Android
- Gamer gpu needed
- VR version to come  

*Unity version: 2020.3.6f1*  

# Table of Contents
- [Getting started](#Getting-Started)
- [Command line arguments](#Command-line-arguments)
- [Build in CLI](#Build-in-CLI)
- [Templates definition](#Templates-definition)
- [Controls](#Controls)
    - [Free mode movement](#Free-mode-movement)
    - [Human mode movement](#Human-mode-movement)

# Getting started
Object hierarchy, each level is mandatory
```
Tenant
|_ Site
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
You can assign a tenant to an OGrEE object. If so, all children will have the same tenant by default.  
More levels will come in next releases.

# Command line arguments
You can override the config file parameters with command line arguments
```
--verbose [true|false]
--fullscreen [true|false]
--serverUrl [url]
--serverToken [token]
--file [path to ocli file to load]
```  

# Build in CLI
CLI langage documentation and example are available [on the wiki](https://github.com/ditrit/OGREE-3D/wiki/CLI-langage).


# Templates definition
Templates are json files describing an object.  
JSON template definitions are available [on the wiki](https://github.com/ditrit/OGREE-3D/wiki/JSON-template-definitions).

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
