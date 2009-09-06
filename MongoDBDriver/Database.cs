
using System;
using System.Collections;
using System.Collections.Generic;

using MongoDB.Driver.Bson;

namespace MongoDB.Driver
{
	public class Database
	{
		private Connection connection;
		
		private String name;		
		public string Name {
			get { return name; }
		}
				
		public Database(Connection conn, String name){
			this.connection = conn;
			this.name = name;
		}
		
		public List<String> GetCollectionNames(){
			Collection namespaces = this.GetCollection("system.namespaces");
			Cursor cursor = namespaces.Find(null);
			List<String> names = new List<string>();
			foreach (Document doc in cursor.Documents){
				names.Add((String)doc["name"]); //Fix Me: Should filter built-ins
			}
			return names;
		}
		
		public Collection this[ String name ]  {
			get{
				return this.GetCollection(name);
			}
		}	
		public Collection GetCollection(String name){
			Collection col = new Collection(name, this.connection, this.Name);
			return col;
		}
		
		public Collection CreateCollection(String name){
			return this.CreateCollection(name,null);
		}
		
		public Collection CreateCollection(String name, Document options){
			Document command = new Document();
			command.Append("create", name).Update(options);			
			//this.connection.SendCommand(command);
			//TODO send command to DB.
			
			return new Collection(name, connection, this.Name);
		}
		
		public Boolean DropCollection(String name){
			Collection col = this.GetCollection(name);
			return this.DropCollection(col);
		}
		public Boolean DropCollection(Collection col){
			throw new NotImplementedException();
		}
		public void Close(){
			throw new NotImplementedException();
		}
		
		public Boolean DropDatabase(){
			throw new NotImplementedException();
		}
		
	}
}
