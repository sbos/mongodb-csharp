using System;
using System.Collections.Generic;

using MongoDB.Driver.Bson;
using MongoDB.Driver.IO;

namespace MongoDB.Driver
{
	
	
	public class Cursor : IDisposable
	{
		private Connection connection;
		
		private long id = -1;
		public long Id{
			get {return id;}
		}		
		
		public string FullCollectionName {
            get { return _QueryMessage.FullCollectionName; }
            set { _QueryMessage.FullCollectionName = value; }
		}
		
		private String collName;
		public string CollName {
			get {return collName;}
			set {collName = value;}
		}
		
		private Document spec;
		public Document Spec{
            get { return spec; }
            set { spec = value; }
		}
		
		public int Limit{
            get { return _QueryMessage.NumberToReturn; }
            set { _QueryMessage.NumberToReturn = value; }
		}
		
		public int Skip{
			get {return _QueryMessage.NumberToSkip;}
            set { _QueryMessage.NumberToSkip = value; }
		}

		private Document fields;
		public Document Fields{
			get {return fields;}
			set {fields = value;}
		}
		
		private bool modifiable = true;
		public bool Modifiable{
			get {return modifiable;}
		}

        private bool _Special;

        private QueryMessage _QueryMessage;
		
		private ReplyMessage reply;

        private Document _OrderBy;
		
		public Cursor(Connection conn, String fullCollectionName, Document spec, int limit, int skip, Document fields){

            _QueryMessage = new QueryMessage();

            if (spec == null) spec = new Document();
            _QueryMessage.FullCollectionName = fullCollectionName;
            _QueryMessage.NumberToReturn = limit;
            _QueryMessage.NumberToSkip = skip;

			this.connection = conn;
            this.fields = fields;
            this.spec = spec;

            _Special = false;
            _OrderBy = null;
		}

        public void Sort(Document orderBy)
        {
            if (orderBy == null)
                throw new ArgumentNullException("orderBy");

            _OrderBy = orderBy;
            _Special = true;
        }
		
		public IEnumerable<Document> Documents{
			get{
				if(this.reply == null){
					RetrieveData();
				}
				int docsReturned = 0;
				BsonDocument[] bdocs = this.reply.Documents;
				Boolean shouldBreak = false;
				while(!shouldBreak){
					foreach(BsonDocument bdoc in bdocs){
						if((this.Limit == 0) || (this.Limit != 0 && docsReturned < this.Limit)){
							docsReturned++;
							yield return (Document)bdoc.ToNative();
						}else{
							shouldBreak = true;
							yield break;
						}
					}
					if(this.Id != 0 && shouldBreak == false){
						RetrieveMoreData();					
						bdocs = this.reply.Documents;
						if(bdocs == null){
							shouldBreak = true;	
						}
					}else{
						shouldBreak = true;
					}
				}
			}			
		}

        private QueryMessage BuildQueryMessage()
        {
            Document query = spec;
            if (_Special)
            {
                query = new Document();
                query.Add("query", spec);
                query.Add("orderby", _OrderBy);
            }
            _QueryMessage.Query = BsonConvert.From(query);
            if (fields != null) _QueryMessage.ReturnFieldSelector = BsonConvert.From(fields);
            return _QueryMessage;
        }
		
		private void RetrieveData(){
            QueryMessage query = BuildQueryMessage();
			if(this.Fields != null){
				query.ReturnFieldSelector = BsonConvert.From(this.Fields);
			}
			
			this.reply = connection.SendTwoWayMessage(query);
			this.id = this.reply.CursorID;
			if(this.Limit < 0)this.Limit = this.Limit * -1;
		}
		
		private void RetrieveMoreData(){
			GetMoreMessage gmm = new GetMoreMessage(this.FullCollectionName, this.Id, this.Limit);

			this.reply = connection.SendTwoWayMessage(gmm);
			this.id = this.reply.CursorID;
		}
		
		
		public void Dispose(){
			if(this.Id == 0) return; //All server side resources disposed of.
			KillCursorsMessage kcm = new KillCursorsMessage(this.Id);			
			connection.SendMessage(kcm);
			this.id = 0;
		}
	}
}
