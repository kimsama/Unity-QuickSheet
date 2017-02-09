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
            
            documentService = docService;
             
            var ssService = new SpreadsheetsService("database");

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

        /// <summary>
        /// @kims 2017.02.09. Added exception handling to smoothly handle abnormal error.
        ///                   If oauth2 setting does not correctly done in the GoogleDataSetting.asset file or missing, 
        ///                   you will get the 'Null reference object' error. 
        /// @kims 2016.08.09. Added second parameter to pass error message by reference.
        /// </summary>
        /// <returns>Null, if any error has been occured.</returns>
        public IDatabase GetDatabase(string name, ref string error) 
        {
            try
            {
                Google.GData.Spreadsheets.SpreadsheetQuery query = new Google.GData.Spreadsheets.SpreadsheetQuery();

                // Make a request to the API and get all spreadsheets.
                SpreadsheetsService service = spreadsheetService as SpreadsheetsService;

                SpreadsheetFeed feed = service.Query(query);

                if (feed.Entries.Count == 0)
                {
                    error = @"There are no spreadsheets in your docs.";
                    return null;
                }

                AtomEntry spreadsheet = null;
                foreach (AtomEntry sf in feed.Entries)
                {
                    if (sf.Title.Text == name)
                        spreadsheet = sf;
                }

                if (spreadsheet == null)
                {
                    error = @"There is no such spreadsheet with such title in your docs.";
                    return null;
                }

                return new Database(this, spreadsheet);
            }
            catch(Exception e)
            {
                error = e.Message;
                return null;
            }
        }
    }
}