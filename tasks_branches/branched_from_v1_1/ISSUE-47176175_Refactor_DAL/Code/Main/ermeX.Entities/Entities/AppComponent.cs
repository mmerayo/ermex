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
using System.Linq.Expressions;
using ermeX.Entities.Base;

namespace ermeX.Entities.Entities
{
	internal class AppComponent : ModelBase, IEquatable<AppComponent>
	{
		internal const int DefaultLatencyMilliseconds = 5000;

		public AppComponent() : this(DefaultLatencyMilliseconds)
		{
		}

		public AppComponent(int latencyMilliseconds = DefaultLatencyMilliseconds)
		{
			Latency = latencyMilliseconds;
		}


		public virtual Guid ComponentId { get; set; }

		public static string TableName
		{
			get { return "Components"; }
		}

		public virtual int Latency { get; set; }
		public virtual bool IsRunning { get; set; }

		public virtual bool ExchangedDefinitions { get; set; }

		/// <summary>
		/// Only one component exchanges the definitions, this is done by the one holded here
		/// </summary>
		public virtual Guid? ComponentExchanges { get; set; }

		#region Equatable

		public virtual bool Equals(AppComponent other)
		{
			if (other == null)
				return false;

			return ComponentId == other.ComponentId
			       && Latency == other.Latency
			       && ComponentOwner == other.ComponentOwner
			       && Version == other.Version
			       && IsRunning == other.IsRunning
			       && ExchangedDefinitions == other.ExchangedDefinitions;
		}

		public static bool operator ==(AppComponent a, AppComponent b)
		{
			if ((object) a == null || ((object) b) == null)
				return Equals(a, b);

			return a.Equals(b);
		}

		public static bool operator !=(AppComponent a, AppComponent b)
		{
			if (a == null || b == null)
				return !Equals(a, b);

			return !(a.Equals(b));
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != typeof (AppComponent)) return false;
			return Equals((AppComponent) obj);
		}

		public override int GetHashCode()
		{
			return ComponentId.GetHashCode();
		}

		#endregion

		public static AppComponent FromDataRow(DataRow dataRow)
		{
			var result = new AppComponent
				{
					Id = Convert.ToInt32(dataRow[GetDbFieldName("Id")]),
					ComponentOwner = (Guid) dataRow[GetDbFieldName("ComponentOwner")],
					Version = (long) dataRow[GetDbFieldName("Version")],
					ComponentId = (Guid) dataRow[GetDbFieldName("ComponentId")],
					Latency = Convert.ToInt32(dataRow[GetDbFieldName("Latency")]),
					IsRunning = (bool) dataRow[GetDbFieldName("IsRunning")],
					ExchangedDefinitions = (bool) dataRow[GetDbFieldName("ExchangedDefinitions")]
				};
			return result;
		}

		protected internal static string GetDbFieldName(string fieldName)
		{
			return String.Format("{0}_{1}", TableName, fieldName);
		}


		public static AppComponent NewFromExisting(AppComponent component)
		{
			var result = new AppComponent(component.Latency)
				{
					ComponentId = component.ComponentId,
					ComponentOwner = component.ComponentOwner,
					Version = component.Version,
					IsRunning = component.IsRunning,
					Latency = component.Latency,
					ExchangedDefinitions = component.ExchangedDefinitions
				};
			return result;
		}

		internal override Expression<Func<object, bool>> FindByBizKey
		{
			get
			{
				return x => ((AppComponent) x).ComponentOwner == ComponentOwner
				            && ((AppComponent) x).ComponentId == this.ComponentId;
			}
		}
	}
}