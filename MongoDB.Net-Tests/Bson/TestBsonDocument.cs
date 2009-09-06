/*
 * User: scorder
 * Date: 7/15/2009
 */
using System;
using System.IO;

using NUnit.Framework;

namespace MongoDB.Driver.Bson
{
	/// <summary>
	/// Description of TestBsonDocument.
	/// </summary>
	[TestFixture()]
	public class TestBsonDocument
	{
		[Test()]
		public void TestAdds(){
			BsonDocument doc = new BsonDocument();
			for(int i = 1; i < 6; i++){
				BsonElement be = new BsonElement(Convert.ToString(i),new BsonInteger(i));
				doc.Add(be.Name, be);
			}
			Assert.AreEqual(5,doc.Count,"Not all elements there");
			
			doc.Add(new BsonElement("6", new BsonInteger(6)));
			Assert.AreEqual(6,doc.Count,"Not all elements there");
			
			doc.Add("7", new BsonInteger(7));
			Assert.AreEqual(7,doc.Count,"Not all elements there");
			
		}
		
		[Test]
		public void TestSize(){
			BsonDocument doc = new BsonDocument();
			Assert.AreEqual(5, doc.Size);
			doc.Add("test", new BsonString("test"));
			Assert.AreEqual(20, doc.Size);
			for(int i = 1; i < 6; i++){
				doc.Add(Convert.ToString(i),new BsonInteger(i));
			}
			Assert.AreEqual(55, doc.Size);
			
			BsonDocument sub = new BsonDocument();
			sub.Add("test", new BsonString("sub test"));
			Assert.AreEqual(24,sub.Size);
			doc.Add("sub",sub);
			Assert.AreEqual(84,doc.Size);
		}
		
		[Test]
		public void TestFormatting(){
			BsonDocument doc = new BsonDocument();
			MemoryStream buf = new MemoryStream();
			BsonWriter writer = new BsonWriter(buf);
			
			
			doc.Add("test", new BsonString("test"));
			doc.Write(writer);
			writer.Flush();
			
			Byte[] output = buf.ToArray();
			String hexdump = BitConverter.ToString(output);
			hexdump = hexdump.Replace("-","");
			Assert.AreEqual(20,output[0],"Size didn't take into count null terminator");
			Assert.AreEqual("1400000002746573740005000000746573740000",hexdump, "Dump not correct");
		}		
		
		[Test]
		public void TestElements(){
			BsonDocument bdoc = new BsonDocument();
			MemoryStream buf = new MemoryStream();
			BsonWriter writer = new BsonWriter(buf);
			
			Oid oid = new Oid("4a753ad8fac16ea58b290351");
			
			bdoc.Append("_id", new BsonElement("_id",new BsonOid(oid)))
				.Append("a", new BsonElement("a",new BsonNumber(1)))
				.Append("b", new BsonElement("b",new BsonString("test")));
			bdoc.Write(writer);


			writer.Flush();
			
			Byte[] output = buf.ToArray();
			String hexdump = BitConverter.ToString(output);
			hexdump = hexdump.Replace("-","");
							 //0         1         2         3         4         5         6         7         8         9
			                 //0123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789
			string expected = "2D000000075F6964004A753AD8FAC16EA58B290351016100000000000000F03F02620005000000746573740000";
			Assert.AreEqual(expected,hexdump, "Dump not correct");
		}
		
		[Test]
		public void TestNumberElements(){
			BsonDocument bdoc = new BsonDocument();
			MemoryStream buf = new MemoryStream();
			BsonWriter writer = new BsonWriter(buf);
			
			Oid oid = new Oid("4a75384cfac16ea58b290350");
			
			bdoc.Append("_id", new BsonElement("_id",new BsonOid(oid)))
				.Append("a", new BsonElement("a",new BsonNumber(1)))
				.Append("b", new BsonElement("b",new BsonNumber(2)));
			bdoc.Write(writer);


			writer.Flush();
			
			Byte[] output = buf.ToArray();
			String hexdump = BitConverter.ToString(output);
			hexdump = hexdump.Replace("-","");
			
			Assert.AreEqual("2C000000075F6964004A75384CFAC16EA58B290350016100000000000000F03F016200000000000000004000",hexdump, "Dump not correct");
		}
		
		[Test]
		public void TestArrayElements(){
			String hexdoc = "82000000075f6964004a78937917220000000061cf0461005d0000" +
							"00013000000000000000f03f013100000000000000004001320000" +
							"000000000008400133000000000000001040013400000000000000" +
							"145001350000000000000018400136000000000000001c40013700" +
							"00000000000020400002620005000000746573740000";

			byte[] bytes = HexToBytes(hexdoc);
			MemoryStream buf = new MemoryStream(bytes);
			BsonReader reader = new BsonReader(buf);
			
			BsonDocument bdoc = new BsonDocument();
			bdoc.Read(reader);
			Assert.AreEqual(BsonDataType.Array, (BsonDataType)bdoc["a"].Val.TypeNum);
			
			buf = new MemoryStream();
			BsonWriter writer = new BsonWriter(buf);
			bdoc.Write(writer);

			String hexdump = BitConverter.ToString(buf.ToArray());
			hexdump = hexdump.Replace("-","").ToLower();
			
			Assert.AreEqual(hexdoc, hexdump);
		}
		
		[Test]
		public void TestArraysWithHoles(){
			String hexdoc = 
				"46000000075F6964004A79BFD517220000000061D304617272617900" +
				"29000000023000020000006100023100020000006200023200020000" + 
				"0063000234000200000065000000";
			byte[] bytes = HexToBytes(hexdoc);
			MemoryStream buf = new MemoryStream(bytes);
			BsonReader reader = new BsonReader(buf);
			BsonDocument bdoc = new BsonDocument();
			bdoc.Read(reader);
			Assert.AreEqual(BsonDataType.Array, (BsonDataType)bdoc["array"].Val.TypeNum);
			
			buf = new MemoryStream();
			BsonWriter writer = new BsonWriter(buf);
			bdoc.Write(writer);

			String hexdump = BitConverter.ToString(buf.ToArray());
			hexdump = hexdump.Replace("-","");
			
			Assert.AreEqual(hexdoc, hexdump);			
		}

		private byte[] HexToBytes(string hex){
			//TODO externalize somewhere.
			if(hex.Length % 2 == 1){
				System.Console.WriteLine("uneven number of hex pairs.");
				hex = "0" + hex;
			}			
			int numberChars = hex.Length;
			byte[] bytes = new byte[numberChars / 2];
			for (int i = 0; i < numberChars; i += 2){
				try{
					bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
				}
				catch{
					//failed to convert these 2 chars, they may contain illegal charracters
					bytes[i / 2] = 0;
				}
			}			
			return bytes;			
		}
	}
}
