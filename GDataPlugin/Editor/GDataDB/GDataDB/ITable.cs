using System;
using System.Collections.Generic;

namespace GDataDB {
    /// <summary>
    /// Worksheet in a spreadsheet document
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITable<T> {
        /// <summary>
        /// Deletes this worksheet
        /// </summary>
        void Delete();

        /// <summary>
        /// Adds a new row
        /// </summary>
        /// <param name="e">Object to store</param>
        /// <returns>Row stored</returns>
        IRow<T> Add(T e);

        /// <summary>
        /// Gets a row by index number
        /// </summary>
        /// <param name="rowNumber"></param>
        /// <returns></returns>
        IRow<T> Get(int rowNumber);

        /// <summary>
        /// Gets all stored rows in this worksheet
        /// </summary>
        /// <returns></returns>
        IList<IRow<T>> FindAll();

        /// <summary>
        /// Gets all stored rows in this worksheet, paged
        /// </summary>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        IList<IRow<T>> FindAll(int start, int count);

        /// <summary>
        /// Free text row search
        /// </summary>
        /// <param name="query">text to search</param>
        /// <returns>Matching rows</returns>
        IList<IRow<T>> Find(string query);

        /// <summary>
        /// Searches rows using a structured query
        /// Syntax: http://code.google.com/apis/spreadsheets/data/2.0/reference.html#ListParameters
        /// </summary>
        /// <param name="query">structured query</param>
        /// <returns>Matching rows</returns>
        IList<IRow<T>> FindStructured(string query);

        /// <summary>
        /// Searches rows using a structured query, paged
        /// Syntax: http://code.google.com/apis/spreadsheets/data/2.0/reference.html#ListParameters
        /// </summary>
        /// <param name="query"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <returns>Matching rows</returns>
        IList<IRow<T>> FindStructured(string query, int start, int count);

        /// <summary>
        /// Searches rows
        /// </summary>
        /// <param name="q">query parameters</param>
        /// <returns>Matching rows</returns>
        IList<IRow<T>> Find(Query q);


        Uri GetFeedUrl();
    }
}