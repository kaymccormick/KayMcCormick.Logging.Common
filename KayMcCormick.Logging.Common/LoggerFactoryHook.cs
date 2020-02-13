﻿using System ;
using System.Reflection ;
using Castle.DynamicProxy ;

namespace KayMcCormick.Logging.Common
{
    /// <summary></summary>
    /// <seealso cref="Castle.DynamicProxy.IProxyGenerationHook" />
    /// <autogeneratedoc />
    /// TODO Edit XML Comment Template for LoggerFactoryHook
    public class LoggerFactoryHook : IProxyGenerationHook
    {
        private readonly LogDelegates.LogMethod _useLogMethod ;

        /// <summary>Initializes a new instance of the <see cref="LoggerFactoryHook"/> class.</summary>
        /// <param name="useLogMethod">The use log method.</param>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for #ctor
        public LoggerFactoryHook ( LogDelegates.LogMethod useLogMethod )
        {
            _useLogMethod = useLogMethod ;
        }

        /// <summary>
        ///     Invoked by the generation process to notify that the whole process has
        ///     completed.
        /// </summary>
        public void MethodsInspected ( ) { }

        /// <summary>
        ///     Invoked by the generation process to notify that a member was not marked
        ///     as
        ///     virtual.
        /// </summary>
        /// <param name="type">The type which declares the non-virtual member.</param>
        /// <param name="memberInfo">The non-virtual member.</param>
        /// <remarks>
        ///     This method gives an opportunity to inspect any non-proxy able member of
        ///     a
        ///     type that has
        ///     been requested to be proxied, and if appropriate - throw an exception to
        ///     notify the caller.
        /// </remarks>
        public void NonProxyableMemberNotification ( Type type , MemberInfo memberInfo )
        {
            // _useLogMethod ( $"Unproxyable: {memberInfo.Name}" );
        }

        /// <summary>
        ///     Invoked by the generation process to determine if the specified method
        ///     should be proxied.
        /// </summary>
        /// <param name="type">The type which declares the given method.</param>
        /// <param name="methodInfo">The method to inspect.</param>
        /// <returns>True if the given method should be proxied; false otherwise.</returns>
        public bool ShouldInterceptMethod ( Type type , MethodInfo methodInfo )
        {
            _useLogMethod ( $"{type}.{methodInfo.Name}" ) ;
            if ( methodInfo.Name == "ToString" )
            {
                return false ;
            }

            return true ;
        }
    }
}