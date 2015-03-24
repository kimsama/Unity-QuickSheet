using System;
using Google.GData.Client;
using Google.GData.Documents;
using Google.GData.Spreadsheets;

namespace GDataDB.Impl {
    public class Database : IDatabase {
        private readonly IDatabaseClient client;
        private readonly AtomEntry entry;

        public Database(IDatabaseClient client, AtomEntry entry) {
            this.client = client;
            this.entry = entry;
        }

        public ITable<T> CreateTable<T>(string name) {
            var link = entry.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            var wsFeed = (WorksheetFeed) client.SpreadsheetService.Query(new WorksheetQuery(link.HRef.ToString()));
            var length = typeof (T).GetProperties().Length;
            var ws = wsFeed.Insert(new WorksheetEntry(1, (uint) length, name));
            var cellLink = new AtomLink(ws.CellFeedLink);
            var cFeed = client.SpreadsheetService.Query(new CellQuery(cellLink.HRef.ToString()));
            {
                uint c = 0;
                foreach (var p in typeof (T).GetProperties()) {
                    var entry1 = new CellEntry(1, ++c, p.Name);
                    cFeed.Insert(entry1);
                }
            }
            return new Table<T>(client.SpreadsheetService, ws);
        }

        public ITable<T> GetTable<T>(string name) {
            var link = entry.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            var wsFeed = (WorksheetFeed) client.SpreadsheetService.Query(new WorksheetQuery(link.HRef.ToString()) {Title = name, Exact = true});
            if (wsFeed.Entries.Count == 0)
                return null;
            return new Table<T>(client.SpreadsheetService, (WorksheetEntry) wsFeed.Entries[0]);
        }

		public WorksheetEntry GetWorksheetEntry(string name)
		{
			var link = entry.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
			var wsFeed = (WorksheetFeed) client.SpreadsheetService.Query(new WorksheetQuery(link.HRef.ToString()) {Title = name, Exact = true});
			if (wsFeed.Entries.Count == 0)
				return null;

			return (WorksheetEntry)wsFeed.Entries [0];
/*
			WorksheetEntry worksheet = (WorksheetEntry)wsFeed.Entries [0];

			// Fetch the cell feed of the worksheet.
			CellQuery cellQuery = new CellQuery(worksheet.CellFeedLink);
			CellFeed cellFeed = client.SpreadsheetService.Query(cellQuery);

			// Iterate through each cell, printing its value.
			foreach (CellEntry cell in cellFeed.Entries)
			{

				// Print the cell's address in A1 notation
				Console.WriteLine(cell.Title.Text);
				// Print the cell's address in R1C1 notation
				Console.WriteLine(cell.Id.Uri.Content.Substring(cell.Id.Uri.Content.LastIndexOf("/") + 1));
				// Print the cell's formula or text value
				Console.WriteLine(cell.InputValue);
				// Print the cell's calculated value if the cell's value is numeric
				// Prints empty string if cell's value is not numeric
				Console.WriteLine(cell.NumericValue);
				// Print the cell's displayed value (useful if the cell has a formula)
				Console.WriteLine(cell.Value);

			}
*/
		}

        public void Delete() {
            // cannot call "entry.Delete()" directly after modification as the EditUri is invalid
            var feed = client.DocumentService.Query(new DocumentsListQuery(entry.SelfUri.ToString()));
            feed.Entries[0].Delete();
        }
    }
}