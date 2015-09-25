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
            /*
            var link = entry.Links.FindService(GDataSpreadsheetsNameTable.WorksheetRel, null);
            var wsFeed = (WorksheetFeed) client.SpreadsheetService.Query(new WorksheetQuery(link.HRef.ToString()) {Title = name, Exact = true});
            if (wsFeed.Entries.Count == 0)
                return null;

            return (WorksheetEntry)wsFeed.Entries [0];
            */

            SpreadsheetEntry spreadsheet = this.entry as SpreadsheetEntry;
            WorksheetFeed wsFeed = spreadsheet.Worksheets;

            // Iterate through each worksheet in the spreadsheet.
            WorksheetEntry worksheetEntry = null;
            foreach(WorksheetEntry entry in wsFeed.Entries)
            {
                // Retrieve worksheet with the given name.
                if (entry.Title.Text == name)
                    worksheetEntry = entry;
            }

            return worksheetEntry;
        }

        public void Delete() {
            // cannot call "entry.Delete()" directly after modification as the EditUri is invalid
            var feed = client.DocumentService.Query(new DocumentsListQuery(entry.SelfUri.ToString()));
            feed.Entries[0].Delete();
        }
    }
}