# Prop Motion Simulator

![Specimen 4](https://github.com/nulluser/PropMotion/blob/master/Images/Build.jpg)

Motion Control Simulator for Vixen Lights and similar programs

- [Demo Video](https://www.youtube.com/watch?v=O-natc-rBuk)
- [Build Images](https://imgur.com/a/GDeLQwt)

## Goal
Provide general animatronics and RGB LED control for Vixen Lights

This program emulates the Vixen Streaming ACN E1.31 output module

## Install
Configure VixenLights to output to an E1.31
Set appropriate number of channels and configure universe

Modify VixenMotion.exe.config and DefaultScene.cfg to match the patching

Launch VixenMotion
Launch Vixen


## Simulation
The program provides a 3D simulation of the robot and lights to aid in sequencing. The 3D scene us user configurable via DefaultScene.cfg. Also, VixenMotion.exe.config can be modified to point to a different scene config.  The program supports 3d models in STL format. Default models are stored in Models.  Textures are stored in Textures. 
The Scene config file allows the specification of lights, materials and geometry, and camera placement.


Keys:

- Escape: Exit
- Manual Axis Test (With Vixen Not Running)
- Axis 0: Q/A/Z
- Axis 1: W/S/X
- And so on
- Space: Random Control data
- Up/Down Arrow: Move Camera
- Left/Right Arrow: Rotate Camera
- Delete / Page Down: Strafe Camera


## Operation
PropController runs on the arduino and connects to the PC via a USB serial port. It is responsible for motion planning,
driving the servo controller and running the NeoPixels.  The PropController code sends a sync byte
when it is ready for new data. This is needed because both the RGB LED update and I2C servo controller update are interrupt based, and tend to lose serial data if new data comes in while they are busy.

VixenMotion runs on the PC. It is responsible for redirecting the control data to the PropController as well as 
emulating a ACN E1.31 device.

VixenMotion sends the latest control data to the serial port when it receives the sync byte from PropController.

The system update at around 100hz

## Libraries
GL Provided by OpenTK


## Demo Show Configuration
8 Motion Axis, 23 RGB LED Axis (3 Channels Each), 2 DMX Axis
80 Total Mapped Outputs

### Motion Axis
- 01: Eye Pan
- 02: Eye Tilt
- 03: Eye Blink
- 04: Nose Curl
- 05: Brain Pulse
- 06: Mouth
- 07: Neck Pan
- 08: Neck TIlt

### Light Axis (Each 3 channels)
- 01: Brain 1
- 02: Brain 2
- 03: Power Bar 1
- 04: Power Bar 2
- 05: Power Bar 3
- 06: Power Bar 4
- 07: Power Bar 5
- 08: Power Bar 6
- 09: Power Bar 7
- 10: Reactor 1
- 11: Reactor 2
- 12: Reactor 3
- 13: Powerbox 1
- 14: Powerbox 2
- 15: Tube 1 Lower
- 16: Tube 1 Upper
- 17: Tube 2 Lower
- 18: Tube 2 Upper
- 19: Logo 1
- 20: Logo 2
- 21: Logo 3
- 22: Logo 4
- 23: Logo 5
- 24: Eyes 

### DMX Axis
- 01: Left Flood
- 02: Right Flood

## Parts Reference
- Wowwee chimp
- Hitec HS 81 Servos For Eye Pan / Blink
- Hitec HS 322 Servos for Eye Tilt / Mouth / Nose / Brain
- Generic DS3218 20kg Servos for Neck Pan / Tilt
- Arduino Nano
- Adafruit PCA9685 Servo Controller
- Pololu 5V DC-DC Converter S7V7F5
- WS2812B RGB LEDS
- 5A 5V power supply
- Fiber Optic Cable
- 3D printed parts
- Hose
- Cable mesh
- M3/M4 Hardware
- 3/8 Bearings

## Safety Warning
Use of this product does not enable you to fly.

## Build Images
