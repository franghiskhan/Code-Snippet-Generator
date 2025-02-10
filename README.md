# Snippet Generator
A small console application that generates consistent C# interfaces and implementations for typical Repository/Service patterns.
It uses doc comments on interfaces and <mark>// inherit</mark> on implementation classes to keep code organized and DRY.

## Features
- Repository Interface
  - GetAllXxxAsync, GetXxxByIdAsync, AddXxxAsync, UpdateXxx, DeleteXxxAsync, SaveChangesAsync
  - Contains rich XML documentation comments
  - AddXxxAsync calls SaveChangesAsync in the implementation so the new record’s ID is immediately available
- Repository Implementation
  - // inherit instead of repeating doc comments
  - Uses a DbContext instance (YourDbContext placeholder)
  - Splits out saving changes so only AddXxxAsync calls SaveChangesAsync.
  - Other operations rely on an external call (usually from the service) to SaveChangesAsync().
- Service Interface
  - GetAllXxxAsync, GetXxxByIdAsync, AddXxxAsync, UpdateXxxAsync, DeleteXxxAsync
  - Also includes detailed XML documentation comments
- Service Implementation
  - // inherit for doc comments
  - Calls corresponding repository operations
  - For Update and Delete, calls the repository’s method and then SaveChangesAsync().
  - For Add, the repository already saves changes to retrieve the new ID.
## How It Works
1. Run the console application.
2. The app prompts: Enter the entity name:
3. Type your entity class name (e.g., CampaignAttribute or Product).
4. Press Enter; the program will generate four code snippets:
  1. Repository Interface
  2. Repository Implementation
  3. Service Interface
  4. Service Implementation
5. Copy/paste these snippets into your solution where desired.
  - Replace any placeholders (YourDbContext) with the actual context type from your application.
  - Adjust namespaces to match your project’s organization.
## Comment Generation & Naming Conventions
- Doc comments are generated based on your entity’s name with some transformations:
  - Splits PascalCase into spaced-lowercase for user-friendly doc text (e.g., CampaignAttribute => “campaign attribute”).
  - Parameter names are camelCase (campaignAttribute) to match standard .NET style.
- // inherit in implementation classes ensures you don’t duplicate doc comments.
## Example Output
If you enter CampaignAttribute, you will see console output like:

```
// Repository Interface Snippet
public interface ICampaignAttributeRepository
{
    /// <summary>
    /// Gets all campaign attributes asynchronous.
    /// </summary>
    /// <returns>a collection of <see cref="CampaignAttribute"/> records</returns>
    Task<IEnumerable<CampaignAttribute>> GetAllCampaignAttributesAsync();

    ...
    Task<int> AddCampaignAttributeAsync(CampaignAttribute campaignAttribute);

    /// <summary>
    /// Explicitly saves changes to the data store.
    /// </summary>
    Task SaveChangesAsync();
}

// Repository Implementation Snippet
public class CampaignAttributeRepository : ICampaignAttributeRepository
{
    private readonly YourDbContext _context;

    // inherit
    public CampaignAttributeRepository(YourDbContext context)
    {
        _context = context;
    }

    ...
    // inherit
    public async Task<int> AddCampaignAttributeAsync(CampaignAttribute campaignAttribute)
    {
        await _context.CampaignAttributes.AddAsync(campaignAttribute);
        await _context.SaveChangesAsync();
        return campaignAttribute.Id;
    }

    // inherit
    public async Task SaveChangesAsync()
    {
        ...
    }
}
```
## Extensibility
- Change the code generation:
  - If you have special naming rules (e.g., prefixes, suffixes, or different pluralization logic), edit the helper methods:
    - NormalizeNameForMethods
    - GetPluralName
    - ToCamelCase
    - ToSpacedLowercase
- Save changes logic:
  - Currently, Add calls SaveChangesAsync; Update and Delete do not.
  - Adjust as desired if you prefer a different approach (e.g., always from the service, or always from the repository).
## Requirements
- .NET 6+ (or .NET Core 3.1+)
- A reference to Microsoft.EntityFrameworkCore for DbContext, DbSet, and DbUpdateException usage.
## How To Build and Run
1. Clone or download this repository.
2. Open a terminal/command prompt in the repository root.
3. Build the project (e.g., dotnet build).
4. Run the snippet generator (e.g., dotnet run).
5. Provide the entity name at the prompt.
6. Copy/paste your generated snippets into your project.
