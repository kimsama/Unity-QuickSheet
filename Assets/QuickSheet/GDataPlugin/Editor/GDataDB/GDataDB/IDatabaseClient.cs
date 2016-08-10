using Google.GData.Client;

namespace GDataDB {
    /// <summary>
    /// Google spreadsheet service entry point
    /// </summary>
    public interface IDatabaseClient {

        IService DocumentService { get; }
        IService SpreadsheetService { get; }

        /// <summary>
        /// Creates a new <see cref="IDatabase"/> (spreadsheet document)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        IDatabase CreateDatabase(string name);

        /// <summary>
        /// Gets an existing <see cref="IDatabase"/> (spreadsheet document)
        /// </summary>
        /// <param name="name"></param>
        /// <returns>IDocument instance or null if not found</returns>
        IDatabase GetDatabase(string name, ref string error);
    }
}