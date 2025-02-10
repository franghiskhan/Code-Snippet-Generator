using System.Text.RegularExpressions;

namespace SnippetGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Enter the entity name: ");
            string entityName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(entityName))
            {
                Console.WriteLine("Entity name cannot be empty.");
                return;
            }

            GenerateRepositoryInterfaceSnippet(entityName);
            GenerateRepositoryImplementationSnippet(entityName);
            GenerateServiceInterfaceSnippet(entityName);
            GenerateServiceImplementationSnippet(entityName);
        }

        /// <summary>
        /// Normalizes the name for method or property naming.
        /// If the entity begins with "Ref_", we drop that prefix for naming conventions.
        /// </summary>
        /// <param name="entityName">The original entity name (may include Ref_).</param>
        /// <returns>A name suitable for display in method names (no Ref_ prefix).</returns>
        static string NormalizeNameForMethods(string entityName)
        {
            if (entityName.StartsWith("Ref_", StringComparison.OrdinalIgnoreCase))
            {
                return entityName.Substring(4); // remove 'Ref_'
            }
            return entityName;
        }

        /// <summary>
        /// Given a singular form of the entity (no Ref_), returns its pluralized form.
        /// Rules:
        /// - If it ends with 'y' (and not 's'), remove 'y' and append 'ies'.
        /// - If it ends with 's', leave it as is.
        /// - Otherwise, append 's'.
        /// </summary>
        /// <param name="cleanName">The normalized entity name without Ref_.</param>
        /// <returns>Pluralized entity name.</returns>
        static string GetPluralName(string cleanName)
        {
            if (cleanName.EndsWith("y", StringComparison.OrdinalIgnoreCase)
                && !cleanName.EndsWith("sy", StringComparison.OrdinalIgnoreCase))
            {
                // Drop 'y', add 'ies'
                return cleanName.Substring(0, cleanName.Length - 1) + "ies";
            }
            else if (cleanName.EndsWith("s", StringComparison.OrdinalIgnoreCase))
            {
                // Already ends with 's', just use as is
                return cleanName;
            }
            else
            {
                // Default: just add 's'
                return cleanName + "s";
            }
        }

        /// <summary>
        /// Converts a PascalCase or TitleCase name to camelCase.
        /// E.g., "CampaignAttribute" => "campaignAttribute"
        /// </summary>
        static string ToCamelCase(string pascalCase)
        {
            if (string.IsNullOrEmpty(pascalCase)) return pascalCase;
            if (pascalCase.Length == 1) return pascalCase.ToLower();
            return char.ToLower(pascalCase[0]) + pascalCase.Substring(1);
        }

        /// <summary>
        /// Converts PascalCase or TitleCase into spaced-lowercase words.
        /// E.g.: "CampaignAttribute" => "campaign attribute"
        ///       "MyGreatClass" => "my great class"
        /// </summary>
        static string ToSpacedLowercase(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            // Insert a space before each capital letter that follows a lower or digit.
            // This splits "CampaignAttribute" into "Campaign Attribute"
            var withSpaces = Regex.Replace(input, "(?<=[a-z0-9])([A-Z])", " $1");
            return withSpaces.ToLower();
        }

        /// <summary>
        /// Generates the REPOSITORY INTERFACE snippet:
        /// - Doc comments live on the interface.
        /// - The Add method returns the new entity's ID and calls SaveChangesAsync internally.
        /// - Other methods do not auto-save; the consumer (e.g. service) can call SaveChangesAsync.
        /// </summary>
        static void GenerateRepositoryInterfaceSnippet(string entityName)
        {
            string cleanName = NormalizeNameForMethods(entityName);
            string pluralName = GetPluralName(cleanName);

            // For doc comments, we want the spaced-lowercase variant
            string docName = ToSpacedLowercase(cleanName);        // e.g. "campaign attribute"
            string docPluralName = ToSpacedLowercase(pluralName); // e.g. "campaign attributes"

            // Camel-case parameter name
            string camelParamName = ToCamelCase(cleanName);

            string snippet = $@"
            // Repository Interface Snippet
            public interface I{cleanName}Repository
            {{
                /// <summary>
                /// Gets all {docPluralName} asynchronous.
                /// </summary>
                /// <returns>a collection of <see cref=""{entityName}""/> records</returns>
                Task<IEnumerable<{entityName}>> GetAll{pluralName}Async();

                /// <summary>
                /// Gets the {docName} by identifier asynchronous.
                /// </summary>
                /// <param name=""id"">The identifier.</param>
                /// <returns>the <see cref=""{entityName}""/> with the requested <paramref name=""id""/></returns>
                Task<{entityName}?> Get{cleanName}ByIdAsync(int id);

                /// <summary>
                /// Adds the {docName} asynchronously, saves changes, and returns the new ID.
                /// </summary>
                /// <param name=""{camelParamName}"">The {docName}.</param>
                /// <returns>the newly assigned ID of the <see cref=""{entityName}""/></returns>
                Task<int> Add{cleanName}Async({entityName} {camelParamName});

                /// <summary>
                /// Updates the {docName}, but does not save changes automatically.
                /// </summary>
                /// <param name=""{camelParamName}"">The {docName}.</param>
                void Update{cleanName}({entityName} {camelParamName});

                /// <summary>
                /// Deletes the {docName} asynchronously, but does not save changes automatically.
                /// </summary>
                /// <param name=""id"">The identifier.</param>
                Task Delete{cleanName}Async(int id);

                /// <summary>
                /// Explicitly saves changes to the data store.
                /// </summary>
                Task SaveChangesAsync();
            }}";

            Console.WriteLine("\nGenerated Repository Interface Snippet:");
            Console.WriteLine(snippet);
        }

        /// <summary>
        /// Generates the REPOSITORY IMPLEMENTATION snippet:
        /// - Uses // inherit for doc comments.
        /// - The Add method calls SaveChangesAsync so the new ID is available.
        /// - Other methods do not save automatically.
        /// - SaveChangesAsync is available for explicit calls.
        /// </summary>
        static void GenerateRepositoryImplementationSnippet(string entityName)
        {
            string cleanName = NormalizeNameForMethods(entityName);
            string pluralName = GetPluralName(cleanName);

            // Camel-case parameter name
            string camelParamName = ToCamelCase(cleanName);
            // Plural for DbSet
            string dbSetName = GetPluralName(cleanName);

            string snippet = $@"
            // Repository Implementation Snippet
            public class {cleanName}Repository : I{cleanName}Repository
            {{
                private readonly YourDbContext _context;

                // inherit
                public {cleanName}Repository(YourDbContext context)
                {{
                    _context = context;
                }}

                // inherit
                public async Task<IEnumerable<{entityName}>> GetAll{pluralName}Async()
                {{
                    return await _context.{dbSetName}.ToListAsync();
                }}

                // inherit
                public async Task<{entityName}?> Get{cleanName}ByIdAsync(int id)
                {{
                    return await _context.{dbSetName}.FirstOrDefaultAsync(e => e.Id == id);
                }}

                // inherit
                public async Task<int> Add{cleanName}Async({entityName} {camelParamName})
                {{
                    // Add and immediately save so the ID is generated
                    await _context.{dbSetName}.AddAsync({camelParamName});
                    await _context.SaveChangesAsync();
                    return {camelParamName}.Id;
                }}

                // inherit
                public void Update{cleanName}({entityName} {camelParamName})
                {{
                    _context.{dbSetName}.Update({camelParamName});
                    // No SaveChanges here; caller (service) decides.
                }}

                // inherit
                public async Task Delete{cleanName}Async(int id)
                {{
                    var entity = await Get{cleanName}ByIdAsync(id);
                    if (entity != null)
                    {{
                        _context.{dbSetName}.Remove(entity);
                    }}
                    // No SaveChanges here; caller (service) decides.
                }}

                #region Save Changes

                // inherit
                public async Task SaveChangesAsync()
                {{
                    try
                    {{
                        await _context.SaveChangesAsync();
                    }}
                    catch (DbUpdateException ex)
                    {{
                        // Log or handle exception as necessary
                        throw new InvalidOperationException(""An error occurred while saving changes."", ex);
                    }}
                }}

                #endregion
            }}";

            Console.WriteLine("\nGenerated Repository Implementation Snippet:");
            Console.WriteLine(snippet);
        }

        /// <summary>
        /// Generates the SERVICE INTERFACE snippet:
        /// - Contains doc comments.
        /// - Notice that Add returns int because the repository returns the new ID.
        /// - Update/Delete are async tasks that call SaveChangesAsync in the service.
        /// </summary>
        static void GenerateServiceInterfaceSnippet(string entityName)
        {
            string cleanName = NormalizeNameForMethods(entityName);
            string pluralName = GetPluralName(cleanName);

            // For doc comments
            string docName = ToSpacedLowercase(cleanName);
            string docPluralName = ToSpacedLowercase(pluralName);

            // Camel-case parameter name
            string camelParamName = ToCamelCase(cleanName);

            string snippet = $@"
            // Service Interface Snippet
            public interface I{cleanName}Service
            {{
                /// <summary>
                /// Gets all {docPluralName} asynchronously.
                /// </summary>
                /// <returns>a collection with all <see cref=""{entityName}""/> records</returns>
                Task<IEnumerable<{entityName}>> GetAll{pluralName}Async();

                /// <summary>
                /// Gets the {docName} by identifier asynchronously.
                /// </summary>
                /// <param name=""id"">The identifier.</param>
                /// <returns>the <see cref=""{entityName}""/> with the requested <paramref name=""id""/></returns>
                Task<{entityName}?> Get{cleanName}ByIdAsync(int id);

                /// <summary>
                /// Creates a new {docName}, automatically saves changes in the repository, and returns the new ID.
                /// </summary>
                /// <param name=""{camelParamName}"">The {docName}.</param>
                Task<int> Add{cleanName}Async({entityName} {camelParamName});

                /// <summary>
                /// Updates the {docName} and saves changes asynchronously.
                /// </summary>
                /// <param name=""{camelParamName}"">The {docName}.</param>
                Task Update{cleanName}Async({entityName} {camelParamName});

                /// <summary>
                /// Deletes the {docName} and saves changes asynchronously.
                /// </summary>
                /// <param name=""id"">The identifier.</param>
                Task Delete{cleanName}Async(int id);
            }}";

            Console.WriteLine("\nGenerated Service Interface Snippet:");
            Console.WriteLine(snippet);
        }

        /// <summary>
        /// Generates the SERVICE IMPLEMENTATION snippet:
        /// - Add calls repository.Add (which internally saves changes) to get the ID.
        /// - Update calls repository.Update, then calls SaveChangesAsync in the service.
        /// - Delete calls repository.Delete, then calls SaveChangesAsync in the service.
        /// - Each method uses // inherit for doc comments.
        /// </summary>
        static void GenerateServiceImplementationSnippet(string entityName)
        {
            string cleanName = NormalizeNameForMethods(entityName);
            string pluralName = GetPluralName(cleanName);

            // Camel-case parameter name
            string camelParamName = ToCamelCase(cleanName);

            string snippet = $@"
            // Service Implementation Snippet
            public class {cleanName}Service : I{cleanName}Service
            {{
                private readonly I{cleanName}Repository _{camelParamName}Repository;

                // inherit
                public {cleanName}Service(I{cleanName}Repository {camelParamName}Repository)
                {{
                    _{camelParamName}Repository = {camelParamName}Repository;
                }}

                // inherit
                public async Task<IEnumerable<{entityName}>> GetAll{pluralName}Async()
                {{
                    return await _{camelParamName}Repository.GetAll{pluralName}Async();
                }}

                // inherit
                public async Task<{entityName}?> Get{cleanName}ByIdAsync(int id)
                {{
                    return await _{camelParamName}Repository.Get{cleanName}ByIdAsync(id);
                }}

                // inherit
                public async Task<int> Add{cleanName}Async({entityName} {camelParamName})
                {{
                    // Repository call saves changes immediately and returns ID
                    return await _{camelParamName}Repository.Add{cleanName}Async({camelParamName});
                }}

                // inherit
                public async Task Update{cleanName}Async({entityName} {camelParamName})
                {{
                    _{camelParamName}Repository.Update{cleanName}({camelParamName});
                    // Now we save changes in the service
                    await _{camelParamName}Repository.SaveChangesAsync();
                }}

                // inherit
                public async Task Delete{cleanName}Async(int id)
                {{
                    await _{camelParamName}Repository.Delete{cleanName}Async(id);
                    // Now we save changes in the service
                    await _{camelParamName}Repository.SaveChangesAsync();
                }}
            }}";

            Console.WriteLine("\nGenerated Service Implementation Snippet:");
            Console.WriteLine(snippet);
        }
    }
}
