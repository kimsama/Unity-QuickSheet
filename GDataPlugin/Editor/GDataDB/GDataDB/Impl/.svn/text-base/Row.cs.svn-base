using Google.GData.Spreadsheets;

namespace GDataDB.Impl {
    public class Row<T> : IRow<T> {
        public T Element { get; set; }
        private readonly ListEntry entry;
        private readonly Serializer<T> serializer = new Serializer<T>();

        public Row(ListEntry entry) {
            this.entry = entry;
        }

        public void Update() {
            serializer.Serialize(Element, entry);
            entry.Update();
        }

        public void Delete() {
            entry.Delete();
        }
    }
}