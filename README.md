# Ness Messenger
Simple LAN messenger built with C# and Windows Forms. Supports multiple clients, asynchronous message processing, and configurable server settings via a configuration file. Works with physical LAN or LAN emulators like Radmin VPN.

[![Windows](https://img.shields.io/badge/platform-Windows-blue)]()
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-Attribution--NonCommercial-orange)](LICENSE)
[![Status](https://img.shields.io/badge/Status-W.I.P.-yellow)]()

## Tech stack

- **Language**: C# (.NET 8.0)
- **UI**: Windows Forms
- **Network**: TCP protocol (asynchronous I/O)

## Features
- Creating server for chatting
- Configurable maximum number of concurrent clients
- Client kick button
- UI customization

## Roadmap
- ~~v1.0 - Stable MVP~~ **Done**
- ~~v1.1 - Markdown formatting~~ **Done**
- ~~Message indicators (delivered/read)~~ deffered: Faced the limitation of current UI realization
- ~~v1.2 - "Typing..." indicator~~ **Done**
- ~~v1.3 - Reliability: packet framing, validation, client limit through config.txt~~ **Done**
- ~~v1.4 - notification (sound + flash)~~ **Done** (sounds need polish)
- ~~v1.5 Server commands (ban/kick etc)~~ **Done**
- v2.0 - File sending
- v2.1 - Customization(UI improvements)
- v2.2 - Customization(Dark/Light theme)

## Getting Started
You can find and download latest release[here](https://github.com/Banghyang/Ness/releases)

## Why does this project exist?
- **Primary goal**: — educational purposes, portfolio. 
- **Secondary goal**: — to provide an easy way for anyone to set up their own chat server.

## Author
Banghyang - [github profile](https://github.com/Banghyang)

## License
Software distributed under [Attribution-NonCommercial license](http://creativecommons.org/licenses/by-nc/4.0/)

## Current version
v1.5
