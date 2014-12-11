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
			var docService = new DocumentsService("database");
			docService.setUserCredentials(username, password);
			documentService = docService;

			var ssService = new SpreadsheetsService("database");
			ssService.setUserCredentials(username, password);
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
			var feed = DocumentService.Query(new SpreadsheetQuery {TitleExact = true, Title = name });
			if (feed.Entries.Count == 0)
				return null;
			return new Database(this, feed.Entries[0]);
		}
	}
}