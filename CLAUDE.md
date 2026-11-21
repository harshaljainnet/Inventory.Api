# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

ASP.NET Core Web API on .NET 10 (`Inventory.Api.csproj`, solution file `Inventory.Api.slnx`). There is no test project.

- Build: `dotnet build -c Release`
- Run: `dotnet run` (see `Properties/launchSettings.json` for ports). In Development, Swagger UI is served at the root URL `/` and the OpenAPI document at `/openapi/v1.json`.
- Sample requests: `Inventory.Api.http`

## Architecture

Controllers → services → in-memory data. There is no database or EF Core.

- **Services hold the data.** `ProductService`, `CategoryService` and `SupplierService` are registered as **singletons** in `Program.cs` and each owns its own seeded `List<T>` plus a `_nextId` counter. State is lost on restart. Every access is guarded by a per-service `_sync` lock.
- **Cross-service rules live in controllers or the product service.** `ProductService` depends on `ICategoryService` and `ISupplierService` to validate references and to resolve names in `ToDto`. The category and supplier services do not know about products. Their `Delete(id, hasProducts)` methods take the answer from the controller, which calls `IProductService.GetByCategory/GetBySupplier`. Follow this pattern to avoid circular dependencies.
- **`ServiceResult` / `ServiceResult<T>`** (`Services/ServiceResult.cs`) is how services report `Success`, `NotFound`, `Invalid` or `Conflict`. Controllers turn failures into 404, 400 or 409 ProblemDetails with `this.Failure(result)` (`Controllers/ControllerExtensions.cs`). Add new failure kinds in both places. Simple lookups (`GetById`) return `null` and the controller returns 404 itself.
- **DTOs** (`Dtos/`) are the API contract. Entities (`Product.cs`, `Category.cs`, `Supplier.cs`, `StockAdjustmentRecord.cs`) are in the project root and never leave the services. Services map entities to DTOs. `ProductService.ToDto` must be called while holding `_sync`.
- **Stock** is changed only through `ProductService.AdjustStock`, which appends to the in-memory `_stockHistory`. `Update` deliberately does not change `StockQuantity`. Deleting a product also removes its history.
- Controllers use `[ApiController]`, `[Route("[controller]")]` and named routes (`Name = "..."`) so `CreatedAtRoute` can reference them.


## Rules

- After adding or changing an endpoint, add a sample request to `Inventory.Api.http`
