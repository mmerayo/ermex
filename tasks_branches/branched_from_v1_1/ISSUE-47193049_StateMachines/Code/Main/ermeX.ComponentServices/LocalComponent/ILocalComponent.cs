﻿using System;

namespace ermeX.ComponentServices.LocalComponent
{
	internal interface ILocalComponent : IErmexComponent
	{
		void Start();
		void PublishMyServices(Guid componentId);
		void PublishMySubscriptions(Guid componentId);
	}
}