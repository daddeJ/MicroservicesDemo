# JWT Authentication System - Development Progress

## Tech Stack

![.NET](https://img.shields.io/badge/.NET-7.0-blue)
![C#](https://img.shields.io/badge/C%23-10.0-green)
![SQL Server](https://img.shields.io/badge/SQL%20Server-2019-red)
![Docker](https://img.shields.io/badge/Docker-24.0-blue)
![GitHub](https://img.shields.io/badge/GitHub-SSH-black)

## Phases Completed

### Phase 1 – Environment Setup
* Created Web API project
* Installed Identity, EF Core, JWT packages
* Configured Identity with in-memory DB

### Phase 2 – Registration Endpoint
* Added `RegisterDto`
* `AccountController.Register` implemented
* Basic user creation working

### Phase 3 – JWT Authentication
* Added JWT Authentication
* `IJwtTokenService` + `JwtTokenService` for token generation
* Basic claims: `sub`, `unique_name`, `email`

### Phase 4 – Role + Tier Registration
* Added `Role` + `Tier` to registration
* Validates tier vs role using `DataSeeder.RoleTierMap`
* Adds Tier claim automatically
* JWT includes Role + Tier claims
* `/api/account/me` endpoint verifies claims

### Phase 5 – Login
* Added `LoginDto`
* `/api/account/login` endpoint
* Validates username/password
* Generates JWT with Role + Tier

### Phase 5.1 – Tier-Based Authorization Policies
* Policies based on Tier claim:
    * `ExecutivesOnly` → Tier 0–1
    * `ManagerAndAbove` → Tier 0–3
    * `RegularAndAbove` → Tier 0–5
* Refactored into **helper class** for reusability

## Current Status

* **Registration & Login working**
* JWT contains **Role + Tier claims**
* Tier-based authorization policies implemented
* `/me` endpoint confirms claim storage
* Middleware fully set up for **authentication + authorization**

## Future Phases / Next Steps

### Phase 6 – User & Role Management
* Admin endpoints to:
    * List users, roles, tiers
    * Update roles/tier of users
    * Soft delete users

### Phase 7 – Refresh Token & Token Expiry Handling
* Implement refresh tokens
* Automatic JWT renewal without forcing login

### Phase 8 – Microservices Ready
* Split into microservices:
    * `AuthService` → handles registration/login/roles
    * `UserService` → manages user profiles, roles, tiers
    * Any future services can validate JWT independently

### Phase 9 – Logging & Monitoring
* Add logging for authentication/authorization events
* Monitor suspicious activity (failed logins, token tampering)

### Phase 10 – Database Migration & Persistence
* Move from InMemory DB to **SQL Server/PostgreSQL**
* Seed default users (SuperAdmin, CEO, HR, etc.)

### Phase 11 – Advanced Policies / Feature Flags
* Hierarchical access based on tier
* Conditional feature access per role/tier
* Multi-service authorization

## Summary

✅ **Current State:** We have a fully functional **JWT-based authentication system with role & tier claims**, tier-based authorization policies, and a clean helper for scalable policy management.

🎯 **Next Steps:** Ready to move towards **Phase 6 – User & Role Management** or **seeding default users** to test policies across multiple tiers.