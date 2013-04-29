// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/

using System;
using System.Data;
using ermeX.LayerMessages;

namespace ermeX.Models
{
	[Serializable]
	internal class OutgoingMessage : Message, IEquatable<OutgoingMessage>
	{
		public const string FinalTableName = "OutgoingMessages";

		public OutgoingMessage()
		{
		}

		//for testing

		public OutgoingMessage(BusMessage message)
			: base(message)
		{
			Tries = 0;
		}


		public virtual int Tries { get; set; }

		protected override string TableName
		{
			get { return FinalTableName; }
		}

		protected internal static string GetDbFieldName(string fieldName) //TODO: REFACTOR
		{
			return String.Format("{0}_{1}", FinalTableName, fieldName);
		}

		public virtual OutgoingMessage GetClone()
		{
			var result = new OutgoingMessage()
				{
					Version = Version,
					ComponentOwner = ComponentOwner,
					MessageId = MessageId,
					CreatedTimeUtc = CreatedTimeUtc,
					Status = Status,
					JsonMessage = JsonMessage,
					PublishedBy = PublishedBy,
					PublishedTo = PublishedTo,

				};

			return result;
		}

		public static OutgoingMessage FromDataRow(DataRow dataRow)
		{
			var result = new OutgoingMessage
				{
					Id = Convert.ToInt32(dataRow[GetDbFieldName("Id")]),
					//TODO: SET SQL SERVER TO LONG AND RECAST, CREATE TEST WITH INT32 OVERFLOW
					//BusMessageId = Convert.ToInt32(dataRow[GetDbFieldName("BusMessageId")]),
					CreatedTimeUtc = new DateTime((long) dataRow[GetDbFieldName("CreatedTimeUtc")]),
					Status = (MessageStatus) Convert.ToInt32(dataRow[GetDbFieldName("Status")]),
					JsonMessage = dataRow[GetDbFieldName("JsonMessage")].ToString(),
					MessageId = (Guid) dataRow[GetDbFieldName("MessageId")],
					PublishedBy = (Guid) dataRow[GetDbFieldName("PublishedBy")],
					PublishedTo = (Guid) dataRow[GetDbFieldName("PublishedTo")],
					ComponentOwner = (Guid) dataRow[GetDbFieldName("ComponentOwner")],
					Tries = Convert.ToInt32(dataRow[GetDbFieldName("Tries")]),
					Version = (long) dataRow[GetDbFieldName("Version")],
					//TODO: TO BASE CLASS
				};
			return result;
		}

		#region Equatable

		//TODO: refactor to base

		public virtual bool Equals(OutgoingMessage other)
		{
			if (other == null)
				return false;

			return
				ComponentOwner == other.ComponentOwner && Version == other.Version &&
				Status == other.Status && CreatedTimeUtc == other.CreatedTimeUtc && JsonMessage == other.JsonMessage &&
				MessageId == other.MessageId;
			//TODO: FINISH
		}

		public static bool operator ==(OutgoingMessage a, OutgoingMessage b)
		{
			if ((object) a == null || ((object) b) == null)
				return Equals(a, b);

			return a.Equals(b);
		}

		public static bool operator !=(OutgoingMessage a, OutgoingMessage b)
		{
			if (a == null || b == null)
				return !Equals(a, b);

			return !(a.Equals(b));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (OutgoingMessage)) return false;
			return Equals((OutgoingMessage) obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion

		public virtual bool Expired(TimeSpan sendExpiringTime)
		{
			return DateTime.UtcNow.Subtract(CreatedTimeUtc) > sendExpiringTime;
		}
	}
}