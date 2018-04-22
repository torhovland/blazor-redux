using System;
using System.Collections.Generic;
using System.Text;

namespace BlazorRedux
{
    public class Location
    {
        public static string Reducer(string location, IAction action)
        {
            switch (action)
            {
                case NewLocationAction a:
                    return a.Location;
                default:
                    return location;
            }
        }
    }
}
