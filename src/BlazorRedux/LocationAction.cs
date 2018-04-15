using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorRedux
{
    public class LocationAction
    {
    }

    public class NewLocationAction : LocationAction
    {
        public string Location { get; set; }
    }
}
