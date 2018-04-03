using System;
using System.Threading.Tasks;

namespace BlazorRedux
{
    public interface IModel
    {
        Task ProcessAsync(object action);
    }
}