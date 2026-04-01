# SpoofMess

## What is it?
A cross-platform messenger with any type of chats, stickers/emoji - that's just a dream for now.

## Quick Start
1. Install required software:
    - [Docker](https://www.docker.com/get-started/)
2. Clone this project to your pc:
    ```bash
    git clone https://github.com/SpoofCorp/SpoofMess
    cd ./SpoofMess
    ```
3. Open cmd/powershell/terminal in root project
4. Write 
    ```bash
    docker compose up -d
    ```
5. You can use swagger for next api:
    - [Spoof Entrance Service](https://localhost:7217/swagger/index.html)
    - [Spoof File Service](https://localhost:7138/swagger/index.html)
    - [Spoof Message Service](https://localhost:7146/swagger/index.html)
    - [Spoof Settings Service](https://localhost:7082/swagger/index.html)

### Core Features
Now you can registration and authorize in app, also create you're chat, add members to chat, connect to other user chats and send messages to your chats.

### Roadmap
Also I want add message parser to find needable rules for send this message and check it.
Soon I want realize ChatType, after you can create a unique types chats with properties.

## What stage of the project at?
### While on Alpha 0.0.6 Implemented first prototypes of:
- Common services:
    - Data services(db, cache, cached-aside repository);
    - Logger service(file, console loggers);
    - RabbitMQ service

- Microservice of entrance(SpoofEntranceService):
    - Db models services
    - Hash passwords and keys.
    - Publisher/Consumer services
    - Model validators

- Microservice of settings(SpoofSettingsService):
    - Db models services
    - DTO setters
    - Publisher/Consumer services
    - Model validators

- Microservice of files(SpoofFileService):
    - Db models services
    - Publisher/Consumer services
    - Model validators

- Microservice of message(SpoofMessageService):
    - Db models services
    - Publisher/Consumer services
    - Model validators