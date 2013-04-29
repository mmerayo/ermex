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

namespace ermeX.Models
{
	internal sealed class AppComponentInfo : ModelBase,IEquatable<AppComponentInfo>
	{
		internal const int DefaultLatencyMilliseconds = 5000;

		public AppComponentInfo() : this(DefaultLatencyMilliseconds)
		{
		}

		public AppComponentInfo(int latencyMilliseconds = DefaultLatencyMilliseconds)
		{
			Latency = latencyMilliseconds;
		}


		public Guid ComponentId { get; set; }

		public int Latency { get; set; }

		public bool IsRunning { get; set; }

		public bool ExchangedDefinitions { get; set; }

		/// <summary>
		/// Only one component exchanges the definitions, this is done by the one holded here
		/// </summary>
		public Guid? ComponentExchanges { get; set; }

		public static AppComponentInfo NewFromExisting(AppComponentInfo componentInfo)
		{
			var result = new AppComponentInfo(componentInfo.Latency)
				{
					ComponentId = componentInfo.ComponentId,
					OwnedBy = componentInfo.OwnedBy,
					IsRunning = componentInfo.IsRunning,
					Latency = componentInfo.Latency,
					ExchangedDefinitions = componentInfo.ExchangedDefinitions
				};
			return result;
		}

		#region Equatable

		public bool Equals(AppComponentInfo other)
		{
			if (other == null)
				return false;

			return ComponentId == other.ComponentId
				   && Latency == other.Latency
				   && OwnedBy == other.OwnedBy
				   && IsRunning == other.IsRunning
				   && ExchangedDefinitions == other.ExchangedDefinitions;
		}

		public static bool operator ==(AppComponentInfo a, AppComponentInfo b)
		{
			if ((object)a == null || ((object)b) == null)
				return Equals(a, b);

			return a.Equals(b);
		}

		public static bool operator !=(AppComponentInfo a, AppComponentInfo b)
		{
			if (a == null || b == null)
				return !Equals(a, b);

			return !(a.Equals(b));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof(AppComponentInfo)) return false;
			return Equals((AppComponentInfo)obj);
		}

		public override int GetHashCode()
		{
			return ComponentId.GetHashCode();
		}

		#endregion
		
	}
}