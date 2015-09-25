using System;
using System.IO;
using GDataDB.Impl;
using Google.GData.Client;
using Google.GData.Documents;
using Google.GData.Spreadsheets;
using SpreadsheetQuery=Google.GData.Documents.SpreadsheetQuery;

namespace GDataDB {
    public class DatabaseClient : IDatabaseClient {
        private readonly IService documentService;
        private readonly IService spreadsheetService;

        public IService DocumentService
        {
            get { return documentService; }
        }

        public IService SpreadsheetService
        {
            get { return spreadsheetService; }
        }

        public DatabaseClient(string username, string password) {

            GOAuth2RequestFactory requestFactory = GDataDBRequestFactory.RefreshAuthenticate();

            var docService = new DocumentsService("database");
            docService.RequestFactory = requestFactory;
            
            //@kims
            //docService.setUserCredentials(username, password);
            documentService = docService;
             
            var ssService = new SpreadsheetsService("database");

            //@kims
            //ssService.setUserCredentials(username, password);
            
            ssService.RequestFactory = requestFactory;
            spreadsheetService = ssService;
        }

        public IDatabase CreateDatabase(string name) {
            using (var ms = new MemoryStream()) {
                using (var sw = new StreamWriter(ms)) {
                    sw.WriteLine(",,,");
                    var spreadSheet = DocumentService.Insert(new Uri(DocumentsListQuery.documentsBaseUri), ms, "text/csv", name);
                    return new Database(this, spreadSheet);
                }
            }
        }

        public IDatabase GetDatabase(string name) {
            /*
            var feed = DocumentService.Query(new SpreadsheetQuery {TitleExact = true, Title = name });
            if (feed.Entries.Count == 0)
                return null;
            return new Database(this, feed.Entries[0]);
             */
            Google.GData.Spreadsheets.SpreadsheetQuery query = new Google.GData.Spreadsheets.SpreadsheetQuery();

            // Make a request to the API and get all spreadsheets.
            SpreadsheetsService service = spreadsheetService as SpreadsheetsService;
            SpreadsheetFeed feed = service.Query(query);
            
            if (feed.Entries.Count == 0)
            {
                //Debug.Log("There are no spreadsheets in your docs.");
                return null;
            }

            //SpreadsheetEntry spreadsheet = null;
            AtomEntry spreadsheet = null;
            foreach (AtomEntry sf in feed.Entries)
            {
                if (sf.Title.Text == name)
                    spreadsheet = sf;
            }

            if (spreadsheet == null)
            {
                //Debug.Log("There is no such spreadsheet with such title in your docs.");
                return null;
            }

            return new Database(this, spreadsheet);
        }
    }
}