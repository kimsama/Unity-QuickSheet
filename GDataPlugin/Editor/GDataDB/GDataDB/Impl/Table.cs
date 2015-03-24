using System;
using System.Collections.Generic;
using Google.GData.Client;
using Google.GData.Spreadsheets;

namespace GDataDB.Impl {
    public class Table<T> : ITable<T> {
        private readonly IService svc;
        private readonly WorksheetEntry entry;
        private readonly Serializer<T> serializer = new Serializer<T>();

        public Table(IService svc, WorksheetEntry entry) {
            this.svc = svc;
            this.entry = entry;
        }

        public void Delete() {
            var wsFeed = (WorksheetFeed)svc.Query(new WorksheetQuery(entry.SelfUri.ToString()));
            wsFeed.Entries[0].Delete();
        }

        private ListQuery GetQuery() {
            return new ListQuery(GetLink().HRef.Content);
        }

        private ListFeed GetFeed() {
            return (ListFeed) svc.Query(GetQuery());
        }

        private AtomLink GetLink() {
            return entry.Links.FindService(GDataSpreadsheetsNameTable.ListRel, null);
        }

        public IRow<T> Add(T e) {
            var feed = GetFeed();
            var newEntry = serializer.Serialize(e);
            var rowEntry = feed.Insert(newEntry);
            return new Row<T>((ListEntry) rowEntry) {Element = e};
        }

        public IRow<T> Get(int rowNumber) {
            var q = GetQuery();
            q.StartIndex = rowNumber;
            q.NumberToRetrieve = 1;
            var results = Find(q);
            if (results.Count == 0)
                return null;
            return results[0];
        }

        public IList<IRow<T>> FindAll() {
            return Find(GetQuery());
        }

        public IList<IRow<T>> FindAll(int start, int count) {
            return Find(new Query {
                Start = start,
                Count = count,
            });
        }

        public IList<IRow<T>> Find(string query) {
            return Find(new Query {FreeQuery = query});
        }

        public IList<IRow<T>> FindStructured(string query) {
            return Find(new Query {StructuredQuery = query});
        }

        public IList<IRow<T>> FindStructured(string query, int start, int count) {
            return Find(new Query {
                StructuredQuery = query,
                Start = start,
                Count = count,
            });
        }

        public IList<IRow<T>> Find(Query q) {
            var fq = GetQuery();
            fq.Query = q.FreeQuery;
            fq.SpreadsheetQuery = q.StructuredQuery;
            fq.StartIndex = q.Start;
            fq.NumberToRetrieve = q.Count;
            if (q.Order != null) {
                fq.OrderByColumn = q.Order.ColumnName;
                fq.Reverse = q.Order.Descending;
            }
            return Find(fq);
        }

        public Uri GetFeedUrl() {
            return new Uri(GetFeed().Feed);
        }

        private IList<IRow<T>> Find(FeedQuery q) {
            var feed = (ListFeed) svc.Query(q);
            var l = new List<IRow<T>>();
            foreach (ListEntry e in feed.Entries) {
                l.Add(new Row<T>(e) { Element = serializer.Deserialize(e) });
            }
            return l;
        }
    }
}