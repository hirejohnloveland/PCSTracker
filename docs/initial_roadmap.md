# Initial Roadmap

## Purpose

- This roadmap is to guide the agent on the steps necessary to build out the golden pattern in .NET.  It heavily relies on using code and patterns from "C:\Users\John.Loveland\source\repos\ziosk-portal"
- It presents a series of steps and features that should be added one by one to build up the golden pattern, which is supposed to be a clone ready package that ultimately contains its own
data sources, CI / CD pipeline, front end, etc.  The goal is a "clone-and-click" application that is read to go.


## 1. Initial Structure -

- API host project
- Class library for the actual buiness code
- Migration classs library sekeleton based on ziosk portal migratoin project
- NUnit Unit test project


## 2. API Surface

### Validattion
- Add Fluent Validation (see Ziosk Portal) in line with Ziosk Portal project. I belive Fluent Validation should live in the class library, but not sure here.

### API Surface
- We should define a standard API surface in line with http best practices.  This project prefers proper HTTP resposne codes and standard verb handling.  Part of the planning process should be an anlysis
of what enterprise best practice is

### Exception handling
- Add a Global Exception handler.  Exceptions should generally not provide a lot of external info to callers outside of maybe error ids and transaction ids or timestamps.
However, a boolean should be able to provide an optional error array in the response. 

### Pagination
How should we hanlde Pagination in our controllers?

### Controller
Should we define a base controller that expects to a call service wrapper and returns a error object or gnereically typed API wrapper response?

## 3. Domain Driven Design

### Namespaces
- Create Domain driven namespaces in the class libary. Each namespace should have an implementation namespace and an abstratctions name space.  As part of implementation we need to think through the namespaces

### Domain Layer Enforcemennt 
Tests in the test project or a separate architest project should enforce boundaries between the namespaces
Additioanlly, a custom Rozlyn analyzer would be a good way to introduce compile time safety at the boundaries
Enforcement should be at the highest namespace and all of its children. Namespace rules should automatically extend to sub-namespaces.
Adding a new root namespace may not be forbidden, but it needs some rules around it.

### Docuemntation
The project structure document should clarify the role of each namespace, when to use it, when not to use it, how it is enforced, and what is necessary for exceptions





## 4. Initial Integration of Testing
- Set up a few tests and a pre-commit hook that requires passing tests
- Set up unit test types. The Ziosk portal has multiple types. Unit is only one.  Is this a fixed list or defined by the implementor?  Decide on the types we will use, and whether NUnit covers our use cases
- We conceivably will also need TestContainers and Architest.  
