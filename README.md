# Snippet Generator
A small console application that generates consistent C# interfaces and implementations for typical Repository/Service patterns.
It uses doc comments on interfaces and <mark>// inherit</mark> on implementation classes to keep code organized and DRY.

## Features
- Repository Interface
  - <mark>GetAllXxxAsync</mark>, <mark>GetXxxByIdAsync</mark>, <mark>AddXxxAsync</mark>, <mark>UpdateXxx</mark>, <mark>DeleteXxxAsync</mark>, <mark>SaveChangesAsync</mark>
  - Contains rich XML documentation comments
  - <mark>AddXxxAsync</mark> calls <mark>SaveChangesAsync</mark> in the implementation so the new record’s ID is immediately available
- Repository Implementation
  - <mark>// inherit</mark> instead of repeating doc comments
  - Uses a <mark>DbContext</mark> instance (<mark>YourDbContext</mark> placeholder)
  - Splits out saving changes so only <mark>AddXxxAsync</mark> calls <mark>SaveChangesAsync</mark>.
  - Other operations rely on an external call (usually from the service) to <mark>SaveChangesAsync()</mark>.
- Service Interface
  - <mark>GetAllXxxAsync</mark>, <mark>GetXxxByIdAsync</mark>, <mark>AddXxxAsync</mark>, <mark>UpdateXxxAsync</mark>, <mark>DeleteXxxAsync</mark>
  - Also includes detailed XML documentation comments
- Service Implementation
  - <mark>// inherit</mark> for doc comments
  - Calls corresponding repository operations
  - For <mark>Update</mark> and <mark>Delete</mark>, calls the repository’s method and then <mark>SaveChangesAsync()</mark>.
  - For <mark>Add</mark>, the repository already saves changes to retrieve the new ID.
## How It Works
1. Run the console application.
2. The app prompts: <mark>Enter the entity name:</mark>
3. Type your entity class name (e.g., <mark>CampaignAttribute</mark> or <mark>Product</mark>).
4. Press Enter; the program will generate four code snippets:
  1. Repository Interface
  2. Repository Implementation
  3. Service Interface
  4. Service Implementation
5. Copy/paste these snippets into your solution where desired.
  - Replace any placeholders (<mark>YourDbContext</mark>) with the actual context type from your application.
  - Adjust namespaces to match your project’s organization.
## Comment Generation & Naming Conventions
- Doc comments are generated based on your entity’s name with some transformations:
  - Splits PascalCase into spaced-lowercase for user-friendly doc text (e.g., <mark>CampaignAttribute</mark> => _“campaign attribute”_).
  - Parameter names are camelCase (<mark>campaignAttribute</mark>) to match standard .NET style.
- <mark>// inherit</mark> in implementation classes ensures you don’t duplicate doc comments.
## Example Output
If you enter <mark>CampaignAttribute</mark>, you will see console output like:

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
    - <mark>NormalizeNameForMethods</mark>
    - <mark>GetPluralName</mark>
    - <mark>ToCamelCase</mark>
    - <mark>ToSpacedLowercase</mark>
- Save changes logic:
  - Currently, <mark>Add</mark> calls <mark>SaveChangesAsync</mark>; <mark>Update</mark> and <mark>Delete</mark> do not.
  - Adjust as desired if you prefer a different approach (e.g., always from the service, or always from the repository).
## Requirements
- .NET 6+ (or .NET Core 3.1+)
- A reference to <mark>Microsoft.EntityFrameworkCore for <mark>DbContext</mark>, <mark>DbSet</mark>, and <mark>DbUpdateException</mark> usage.
## How To Build and Run
1. Clone or download this repository.
2. Open a terminal/command prompt in the repository root.
3. Build the project (e.g., <mark>dotnet build</mark>).
4. Run the snippet generator (e.g., <mark>dotnet run</mark>).
5. Provide the entity name at the prompt.
6. Copy/paste your generated snippets into your project.
