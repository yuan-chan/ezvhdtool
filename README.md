![ezvhdtrans2](https://user-images.githubusercontent.com/59510695/232297256-3ad30397-ec84-4788-8019-b205392662a4.png)
# EzVHDTool

EzVHDTool is a tool to simplify the process of mounting Virtual Hard Disk (VHD) files. 
If you have a VHD on a network drive, you can still use EzVHDTool to mount VHD files directly from the Windows ContextMenu.
___
## Table of Contents
- [TL;DR](https://github.com/yuan-chan/ezvhdtool#tldr)
- [Motivation](https://github.com/yuan-chan/ezvhdtool#Motivation)
- [Usage](https://github.com/yuan-chan/ezvhdtool#Usage)

## TL;DR
Software that allows you to easily mount a VHD on a network drive without errors.
>There appears to be a bug in Windows 10 with VHD's on mounted network locations (ie. mounted and assigned a network location a disk letter). Using the full location, preferably the IP address, will allow you to mount the virtual disk.
___
## Motivation

EzVHDTool was created to simplify the process of mounting VHD files that are located on network drives. In some cases, attempting to mount a VHD file by double-clicking on it will result in an error, unless you are using a UNC path. This can be inconvenient, especially if you frequently work with VHD files. EzVHDTool simplifies the process by allowing you to mount VHD files directly from the ContextMenu, without having to worry about UNC paths or other technical details.
___

## Usage

To use EzVHDTool, simply right-click on the VHD file and select "Attach VHD" in "EzVHD Tool" from the ContextMenu; the VHD file will be automatically mounted and ready for use.
___
