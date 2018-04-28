using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace BlazorRedux
{
    /// <summary>
    /// Extension methods for adding typed middleware.
    /// </summary>
    public static class UseMiddlewareExtensions
    {
        internal const string InvokeMethodName = "Invoke";
        internal const string InvokeAsyncMethodName = "InvokeAsync";

        private static readonly MethodInfo GetServiceInfo = typeof(UseMiddlewareExtensions).GetMethod(nameof(GetService), BindingFlags.NonPublic | BindingFlags.Static);

        /// <summary>
        /// Adds a middleware type to the application's request pipeline.
        /// </summary>
        /// <typeparam name="TMiddleware">The middleware type.</typeparam>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="args">The arguments to pass to the middleware type instance's constructor.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IStoreBuilder<TState, TAction> UseMiddleware<TMiddleware, TState, TAction>(this IStoreBuilder<TState, TAction> builder, IServiceProvider provider, params object[] args)
        {
            return builder.UseMiddleware(provider, typeof(TMiddleware), args);
        }

        /// <summary>
        /// Adds a middleware type to the application's request pipeline.
        /// </summary>
        /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
        /// <param name="middleware">The middleware type.</param>
        /// <param name="args">The arguments to pass to the middleware type instance's constructor.</param>
        /// <returns>The <see cref="IApplicationBuilder"/> instance.</returns>
        public static IStoreBuilder<TState, TAction> UseMiddleware<TState, TAction>(this IStoreBuilder<TState, TAction> builder, IServiceProvider provider, Type middleware, params object[] args)
        {
            //Disabled IMiddleware for now
            /*if (typeof(IMiddleware<TState, TAction>).GetTypeInfo().IsAssignableFrom(middleware.GetTypeInfo()))
            {
                // IMiddleware doesn't support passing args directly since it's
                // activated from the container
                if (args.Length > 0)
                {
                    //throw new NotSupportedException(Resources.FormatException_UseMiddlewareExplicitArgumentsNotSupported(typeof(IMiddleware)));
                    throw new NotSupportedException();
                }

                return UseMiddlewareInterface(builder, middleware);
            }*/
            return builder.Use(next =>
            {
                var methods = middleware.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                var invokeMethods = methods.Where(m =>
                    string.Equals(m.Name, InvokeMethodName, StringComparison.Ordinal)
                    || string.Equals(m.Name, InvokeAsyncMethodName, StringComparison.Ordinal)
                    ).ToArray();

                if (invokeMethods.Length > 1)
                {
                    //throw new InvalidOperationException(Resources.FormatException_UseMiddleMutlipleInvokes(InvokeMethodName, InvokeAsyncMethodName));
                    throw new InvalidOperationException();
                }

                if (invokeMethods.Length == 0)
                {
                    //throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoInvokeMethod(InvokeMethodName, InvokeAsyncMethodName, middleware));
                    throw new InvalidOperationException();
                }

                var methodinfo = invokeMethods[0];
                if (!typeof(Task).IsAssignableFrom(methodinfo.ReturnType))
                {
                    //throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNonTaskReturnType(InvokeMethodName, InvokeAsyncMethodName, nameof(Task)));
                    throw new InvalidOperationException();
                }

                var parameters = methodinfo.GetParameters();
                if (parameters.Length == 0 || parameters.Length == 1 || parameters[0].ParameterType != typeof(TState) || parameters[1].ParameterType != typeof(TAction))
                {
                    //throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoParameters(InvokeMethodName, InvokeAsyncMethodName, nameof(HttpContext)));
                    throw new InvalidOperationException();
                }

                var ctorArgs = new object[args.Length + 1];
                ctorArgs[0] = next;
                Array.Copy(args, 0, ctorArgs, 1, args.Length);

                var instance = ActivatorUtilities.CreateInstance(provider, middleware, ctorArgs);
                if (parameters.Length == 2)
                {
                    return (StoreEventDelegate<TState, TAction>)methodinfo.CreateDelegate(typeof(StoreEventDelegate<TState, TAction>), instance);
                }

                //This below doesn't work yet
                var factory = Compile<object, TState, TAction>(methodinfo, parameters);

                return (state, action) =>
                {
                    /*var serviceProvider = context.RequestServices ?? applicationServices;
                    if (serviceProvider == null)
                    {
                        throw new InvalidOperationException(Resources.FormatException_UseMiddlewareIServiceProviderNotAvailable(nameof(IServiceProvider)));
                    }*/

                    return factory(instance, state, action, provider);
                };
            });
        }

        /*private static IStoreBuilder<TState, TAction> UseMiddlewareInterface<TState, TAction>(IStoreBuilder<TState, TAction> app, Type middlewareType, IServiceProvider provider)
        {
            return app.Use(next =>
            {
                return async (state, action) =>
                {
                    var middlewareFactory = (IMiddlewareFactory)provider.GetService(typeof(IMiddlewareFactory));
                    if (middlewareFactory == null)
                    {
                        // No middleware factory
                        //throw new InvalidOperationException(Resources.FormatException_UseMiddlewareNoMiddlewareFactory(typeof(IMiddlewareFactory)));
                        throw new InvalidOperationException();
                    }

                    var middleware = middlewareFactory.Create(middlewareType);
                    if (middleware == null)
                    {
                        // The factory returned null, it's a broken implementation
                        //throw new InvalidOperationException(Resources.FormatException_UseMiddlewareUnableToCreateMiddleware(middlewareFactory.GetType(), middlewareType));
                        throw new InvalidOperationException();
                    }

                    try
                    {
                        await middleware.InvokeAsync(state, action, next);
                    }
                    finally
                    {
                        middlewareFactory.Release(middleware);
                    }
                };
            });
        }*/

        private static Func<T, TState, TAction, IServiceProvider, Task> Compile<T, TState, TAction>(MethodInfo methodinfo, ParameterInfo[] parameters)
        {
            // If we call something like
            //
            // public class Middleware
            // {
            //    public Task Invoke(HttpContext context, ILoggerFactory loggeryFactory)
            //    {
            //
            //    }
            // }
            //

            // We'll end up with something like this:
            //   Generic version:
            //
            //   Task Invoke(Middleware instance, HttpContext httpContext, IServiceprovider provider)
            //   {
            //      return instance.Invoke(httpContext, (ILoggerFactory)UseMiddlewareExtensions.GetService(provider, typeof(ILoggerFactory));
            //   }

            //   Non generic version:
            //
            //   Task Invoke(object instance, HttpContext httpContext, IServiceprovider provider)
            //   {
            //      return ((Middleware)instance).Invoke(httpContext, (ILoggerFactory)UseMiddlewareExtensions.GetService(provider, typeof(ILoggerFactory));
            //   }
            var middleware = typeof(T);

            var stateArg = Expression.Parameter(typeof(TState), "state");
            var actionArg = Expression.Parameter(typeof(TAction), "action");
            var providerArg = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
            var instanceArg = Expression.Parameter(middleware, "middleware");

            var methodArguments = new Expression[parameters.Length];
            methodArguments[0] = stateArg;
            methodArguments[1] = actionArg;
            for (int i = 2; i < parameters.Length; i++)
            {
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsByRef)
                {
                    //throw new NotSupportedException(Resources.FormatException_InvokeDoesNotSupportRefOrOutParams(InvokeMethodName));
                    throw new NotSupportedException();
                }

                var parameterTypeExpression = new Expression[]
                {
                    providerArg,
                    Expression.Constant(parameterType, typeof(Type)),
                    Expression.Constant(methodinfo.DeclaringType, typeof(Type))
                };

                var getServiceCall = Expression.Call(GetServiceInfo, parameterTypeExpression);
                methodArguments[i] = Expression.Convert(getServiceCall, parameterType);
            }

            Expression middlewareInstanceArg = instanceArg;
            if (methodinfo.DeclaringType != typeof(T))
            {
                middlewareInstanceArg = Expression.Convert(middlewareInstanceArg, methodinfo.DeclaringType);
            }

            var body = Expression.Call(middlewareInstanceArg, methodinfo, methodArguments);

            var lambda = Expression.Lambda<Func<T, TState, TAction, IServiceProvider, Task>>(body, instanceArg, stateArg, actionArg, providerArg);

            return lambda.Compile();
        }

        private static object GetService(IServiceProvider sp, Type type, Type middleware)
        {
            var service = sp.GetService(type);
            if (service == null)
            {
                //throw new InvalidOperationException(Resources.FormatException_InvokeMiddlewareNoService(type, middleware));
                throw new InvalidOperationException();
            }

            return service;
        }
    }
}
