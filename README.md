# Project Laundry Management System

Description: Laundry Management System is a system that helps to manage the laundry process.
Technology: .NET 8, Entity Framework Core, Docker, Clean Architecture

1. Setup the project for windows
   - Download [msys2](https://www.msys2.org/)
   - Install the following packages:
     - `pacman -S --needed make`
   - Add msys2 Path to the environment variable `PATH` in the `C:\msys64\usr\bin`
2. How to run the project
   - Open the terminal in the root of the project
   - Run `make dev SERVICE="redis aspire-dashboard database" `
   - Run `make dev SERVICE="pgadmin"` if use pgadmin web
   - Run `make external`
   - Run `make update`
   - Run all service

## Overview

- Micro is a .NET 8 project that contains the backend services for the Micro e-commerce platform.
- It is built using the .NET 8 SDK and the Entity Framework Core for database operations.
- It uses Docker to containerize the services and Docker Compose to orchestrate them.
- It uses the Clean Architecture approach to design the services.

## Services

- AuthService: This service is responsible for authentication and authorization of users.
- ProjectService: This service is responsible for managing project as log,...
- EcommerceService: This service is responsible for e-commerce.
- FinanceService: This service is responsible for managing finance.
