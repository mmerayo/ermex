using System;

using ermeX.Models.Entities;

namespace ermeX.DAL.Interfaces.Component
{
    internal interface ICanWriteComponents
    {
	    void Save(AppComponent component);
    }
}
