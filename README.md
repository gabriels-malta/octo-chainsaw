# Acesso-Bankly
## Tech Case - solutions proposal

### Requirements
- SDK .net5.0
- Docker
- Docker Compose
- Visual Studio 2019 or Visual Studio Code
- Windows Terminal _(this is required to execute all queue workers in at once)_

### Getting started
Get system started by following the steps below:
- run the file ./src/**docker-compose.yml**
- execute the file ./src/**run-system.cmd**

### Dashboards and API's Swagger
- Fund Transfer API: http:localhost:5000/swagger
- Acesso API: http://localhost:6123/swagger
- Logs: http://localhost:6124
- Message Broker: http://localhost:6125
  - user: guest
  - passwd: guest
