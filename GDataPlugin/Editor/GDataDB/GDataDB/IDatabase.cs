namespace GDataDB {
    /// <summary>
    /// Spreadsheet document
    /// </summary>
    public interface IDatabase {
        /// <summary>
        /// Creates a new worksheet in this document
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns></returns>
        ITable<T> CreateTable<T>(string name);

        /// <summary>
        /// Gets an existing worksheet in this document.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <returns>Searched worksheet or null if not found</returns>
        ITable<T> GetTable<T>(string name);

        /// <summary>
        /// Deletes this spreadsheet document
        /// </summary>
        void Delete();
    }
}