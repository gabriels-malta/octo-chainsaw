# Acesso-Bankly
## Tech Case - solution proposal

The system follows an event-driven architecture, coordinated by choreography a message broker.

Important to notice that this isn't a bunch of microservices. It is a monolith that has a loosely coupled approach to handle each step of the workflow.

### Requirements
- SDK .net5.0
- Docker
- Docker Compose
- Visual Studio 2019 or Visual Studio Code
- Windows Terminal _(this is required to execute all queue workers in at once)_

### Getting started
Get the system started by following the steps below:
- run the file ./src/**docker-compose.yml**
- execute the file ./src/**run-system.cmd**

### Dashboards and API's Swagger
- Fund Transfer API: http:localhost:5000/swagger
- Acesso API: http://localhost:6123/swagger
- Logs: http://localhost:6124
- Message Broker: http://localhost:6125
  - user: guest
  - passwd: guest
