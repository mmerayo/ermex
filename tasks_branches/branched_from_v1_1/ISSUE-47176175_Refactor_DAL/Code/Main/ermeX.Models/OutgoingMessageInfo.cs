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
	internal class OutgoingMessageInfo : MessageInfo, IEquatable<OutgoingMessageInfo>
	{
		public const string FinalTableName = "OutgoingMessages";

		public OutgoingMessageInfo()
		{
		}

		//for testing

		public OutgoingMessageInfo(BusMessage message)
			: base(message)
		{
			Tries = 0;
		}


		public  int Tries { get; set; }


		public  OutgoingMessageInfo GetClone()
		{
			var result = new OutgoingMessageInfo()
				{
					OwnedBy = OwnedBy,
					MessageId = MessageId,
					CreatedTimeUtc = CreatedTimeUtc,
					Status = Status,
					JsonMessage = JsonMessage,
					PublishedBy = PublishedBy,
					PublishedTo = PublishedTo,

				};

			return result;
		}

		#region Equatable

		//TODO: refactor to base

		public  bool Equals(OutgoingMessageInfo other)
		{
			if (other == null)
				return false;

			return
				OwnedBy == other.OwnedBy && Version == other.Version &&
				Status == other.Status && CreatedTimeUtc == other.CreatedTimeUtc && JsonMessage == other.JsonMessage &&
				MessageId == other.MessageId;
			//TODO: FINISH
		}

		public static bool operator ==(OutgoingMessageInfo a, OutgoingMessageInfo b)
		{
			if ((object) a == null || ((object) b) == null)
				return Equals(a, b);

			return a.Equals(b);
		}

		public static bool operator !=(OutgoingMessageInfo a, OutgoingMessageInfo b)
		{
			if (a == null || b == null)
				return !Equals(a, b);

			return !(a.Equals(b));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (OutgoingMessageInfo)) return false;
			return Equals((OutgoingMessageInfo) obj);
		}

		public override int GetHashCode()
		{
			return Id.GetHashCode();
		}

		#endregion

		public  bool Expired(TimeSpan sendExpiringTime)
		{
			return DateTime.UtcNow.Subtract(CreatedTimeUtc) > sendExpiringTime;
		}

		
	}
}