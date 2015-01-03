namespace GDataDB {
    /// <summary>
    /// Query parameters
    /// </summary>
    public class Query {
        /// <summary>
        /// Start index, for paging
        /// </summary>
        public int Start { get; set; }

        /// <summary>
        /// Record count to fetch, for paging
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Free text query
        /// </summary>
        public string FreeQuery { get; set; }

        /// <summary>
        /// Structured query
        /// </summary>
        public string StructuredQuery { get; set; }

        /// <summary>
        /// Sort order
        /// </summary>
        public Order Order { get; set; }
    }
}