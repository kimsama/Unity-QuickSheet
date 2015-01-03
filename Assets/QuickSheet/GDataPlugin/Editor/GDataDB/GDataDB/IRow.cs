namespace GDataDB {
    /// <summary>
    /// Row in the spreadsheet
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRow<T> {
        /// <summary>
        /// Element stored in the row
        /// </summary>
        T Element { get; set; }

        /// <summary>
        /// Updates the row in the spreadsheet using the current <see cref="Element"/>
        /// </summary>
        void Update();

        /// <summary>
        /// Deletes this row
        /// </summary>
        void Delete();
    }
}