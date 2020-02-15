using System ;
using System.Collections.Generic ;
using System.Diagnostics ;
using System.IO ;
using System.Linq ;
using System.Reflection ;
using System.Runtime.CompilerServices ;
using System.Text ;
using System.Text.RegularExpressions ;
using Castle.DynamicProxy ;
using DynamicData ;
using JetBrains.Annotations ;
using KayMcCormick.Logging.Common.Properties ;
using NLog ;
using NLog.Common ;
using NLog.Config ;
using NLog.Layouts ;
using NLog.Targets ;

namespace KayMcCormick.Logging.Common
{
    /// <summary></summary>
    /// <seealso cref="NLog.Config.LoggingConfiguration" />
    /// <autogeneratedoc />
    /// TODO Edit XML Comment Template for AppLoggingConfigHelper
    /// TODO should not use inheritance
    // ReSharper disable once ClassNeverInstantiated.Global
    public static class AppLoggingConfigHelper
    {
        /// <summary>The string writer</summary>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for StringWriter
        public static StringWriter Writer { get ; set ; }

        private const string JsonTargetName = "json_out" ;

        // ReSharper disable once InconsistentNaming
        [ UsedImplicitly ] private static Logger Logger ;

        [ ThreadStatic ]
        private static int ? _numTimesConfigured ;


        /// <summary>Gets or sets a value indicating whether [debugger target enabled].</summary>
        /// <value>
        ///   <see language="true"/> if [debugger target enabled]; otherwise, <see language="false"/>.</value>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for DebuggerTargetEnabled
        public static bool DebuggerTargetEnabled { get ; } = false ;


        /// <summary>Gets or sets a value indicating whether [logging is configured].</summary>
        /// <value>
        ///   <see language="true"/> if [logging is configured]; otherwise, <see language="false"/>.</value>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for LoggingIsConfigured
        public static bool LoggingIsConfigured { get ; set ; }

        /// <summary>Gets or sets a value indicating whether [dump existing configuration].</summary>
        /// <value>
        ///   <see language="true"/> if [dump existing configuration]; otherwise, <see language="false"/>.</value>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for DumpExistingConfig
        public static bool DumpExistingConfig { get ; } = true ;


        /// <summary>Gets or sets a value indicating whether [force code configuration].</summary>
        /// <value>
        ///   <see language="true"/> if [force code configuration]; otherwise, <see language="false"/>.</value>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for ForceCodeConfig
        public static bool ForceCodeConfig { get ; } = false ;

        private static void DoLogMessage ( string message )
        {
            System.Diagnostics.Debug.WriteLine (
                                                nameof ( AppLoggingConfigHelper ) + ":" + message
                                               ) ;
            // System.Diagnostics.Debug.WriteLine ( nameof(AppLoggingConfigHelper) + ":" + message ) ;
        }

        // ReSharper disable once MemberCanBePrivate.Global
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal static void ConfigureLogging (
            LogDelegates.LogMethod logMethod
          , bool                   proxyLogging = false
        )
        {
            logMethod (
                       Resource
                          .AppLoggingConfigHelper_ConfigureLogging_____Starting_logger_configuration_
                      ) ;
            InternalLogging ( ) ;

            LogFactory proxiedFactory = null ;
            if ( proxyLogging )
            {
                var proxyGenerator = new ProxyGenerator ( ) ;
                var loggerProxyHelper = new LoggerProxyHelper ( proxyGenerator , DoLogMessage ) ;
                var logFactory = new MyLogFactory ( DoLogMessage ) ;
                var lConfLogFactory = loggerProxyHelper.CreateLogFactory ( logFactory ) ;
                proxiedFactory = lConfLogFactory ;
            }

            var fieldInfo = typeof ( LogManager ).GetField (
                                                            "factory"
                                                          , BindingFlags.Static
                                                            | BindingFlags.NonPublic
                                                           ) ;
            if ( fieldInfo != null )
            {
                logMethod ( $"field info is {fieldInfo.DeclaringType} . {fieldInfo.Name}" ) ;
                var cur = fieldInfo.GetValue ( null ) ;
                logMethod ( $"cur is {cur}" ) ;

                if ( proxyLogging )
                {
                    fieldInfo.SetValue ( null , proxiedFactory ) ;
                    var newVal = fieldInfo.GetValue ( null ) ;
                    logMethod ( $"NewVal = {newVal}" ) ;
                }
            }

            var useFactory = proxyLogging ? proxiedFactory : LogManager.LogFactory ;
            var lConf = new CodeConfiguration ( useFactory ) ;

            var t = new List < Target > ( ) ;
            #region Cache Target
#if false
            var cacheTarget = new  MyCacheTarget ( ) ;
            t.Add ( cacheTarget );

#endif
            #endregion
            #region NLogViewer Target
            var viewer = Viewer ( ) ;
            t.Add ( viewer ) ;
            #endregion
            #region Debugger Target
            if ( DebuggerTargetEnabled )
            {
                var debuggerTarget =
                    new DebuggerTarget { Layout = new SimpleLayout ( "${message}" ) } ;
                t.Add ( debuggerTarget ) ;
            }
            #endregion
            #region Chainsaw Target
            var chainsawTarget = new ChainsawTarget ( ) ;
            SetupNetworkTarget ( chainsawTarget , "udp://192.168.10.1:4445" ) ;
            t.Add ( chainsawTarget ) ;
            #endregion
            t.Add ( MyFileTarget ( ) ) ;
            var jsonFileTarget = JsonFileTarget ( ) ;
            t.Add ( jsonFileTarget ) ;
            var byType = new Dictionary < Type , int > ( ) ;
            foreach ( var target in t )
            {
                // logMethod ( $"target is {target}" ) ;
                var type = target.GetType ( ) ;
                byType.TryGetValue ( type , out var count ) ;
                count          += 1 ;
                byType[ type ] =  count ;

                if ( target.Name == null )
                {
                    target.Name = $"{Regex.Replace ( type.Name , "Target" , "" )}{count:D2}" ;
                }

                lConf.AddTarget (
                                 target
                                ) ; //new AsyncTargetWrapper(target.Name + "AsyncWrapper", target)) ;
            }

            var loggingRules = t.AsQueryable ( ).AsEnumerable ( ).Select ( DefaultLoggingRule ) ;
            foreach ( var loggingRule in loggingRules ) { lConf.LoggingRules.Add ( loggingRule ) ; }

            LogManager.Configuration = lConf ;
            Logger                   = LogManager.GetCurrentClassLogger ( ) ;
        }

        private static LoggingRule DefaultLoggingRule ( Target target )
        {
            return new LoggingRule ( "*" , LogLevel.FromOrdinal ( 0 ) , target ) ;
        }

        private static void InternalLogging ( )
        {
            InternalLogger.LogLevel = LogLevel.Debug ;

            var id = Process.GetCurrentProcess ( ).Id ;
            var logFile = $@"c:\temp\nlog-internal-{id}.txt" ;
            InternalLogger.LogFile = logFile ;

            //InternalLogger.LogToConsole      = true ;
            //InternalLogger.LogToConsoleError = true ;
            //InternalLogger.LogToTrace        = true ;

            Writer                   = new StringWriter ( ) ;
            InternalLogger.LogWriter = Writer ;
        }

        private static void SetupNetworkTarget ( NetworkTarget target , string address )
        {
            target.Address = new SimpleLayout ( address ) ;
        }

        private static NLogViewerTarget Viewer ( string name = null )
        {
            return new NLogViewerTarget ( name )
                   {
                       Address              = new SimpleLayout ( "udp://10.25.0.102:9999" )
                     , IncludeAllProperties = true
                     , IncludeCallSite      = true
                     , IncludeSourceInfo    = true
                   } ;
        }

        /// <summary>JSON File Target</summary>
        /// <returns></returns>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for JsonFileTarget
        public static FileTarget JsonFileTarget ( )
        {
            var f = new FileTarget ( JsonTargetName )
                    {
                        FileName = Layout.FromString ( @"c:\data\logs\${processName}.json" )
                      , Layout   = SetupJsonLayout ( )
                    } ;

            return f ;
        }

        /// <summary>My File Target.</summary>
        /// <returns></returns>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for MyFileTarget
        public static FileTarget MyFileTarget ( )
        {
            var f = new FileTarget
                    {
                        Name     = "text_log"
                      , FileName = Layout.FromString ( @"c:\data\logs\log.txt" )
                      , Layout   = Layout.FromString ( "${message}" )
                    } ;

            return f ;
        }

        /// <summary>Removes the target.</summary>
        /// <param name="target">The target.</param>
        /// <exception cref="System.ArgumentNullException">target</exception>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for RemoveTarget
        // ReSharper disable once UnusedMember.Global
        public static void RemoveTarget ( [ NotNull ] Target target )
        {
            if ( target == null )
            {
                throw new ArgumentNullException ( nameof ( target ) ) ;
            }

            LogManager.Configuration.RemoveTarget ( target.Name ) ;
            LogManager.LogFactory.ReconfigExistingLoggers ( ) ;
#if LOGREMOVAL
            Logger.Debug ( "Removing target " + target ) ;
            foreach ( var t in LogManager.Configuration.AllTargets )
            {
                Logger.Debug ( "Target " + t ) ;
            }
#endif
        }

        /// <summary>Ensures the logging configured.</summary>
        /// <param name="logMethod">The log method.</param>
        /// <param name="callerFilePath">The caller file path.</param>
        /// <exception cref="Exception">no config loaded field found</exception>
        /// <autogeneratedoc />
        /// TODO Edit XML Comment Template for EnsureLoggingConfigured
        [ System.Diagnostics.CodeAnalysis.SuppressMessage (
                                                              "Microsoft.Naming"
                                                            , "CA2204:Literals should be spelled correctly"
                                                            , MessageId = "EnsureLoggingConfigured"
                                                          ) ]
        public static void EnsureLoggingConfigured (
            LogDelegates.LogMethod    logMethod      = null
          , [ CallerFilePath ] string callerFilePath = null
        )
        {
            if ( ! _numTimesConfigured.HasValue )
            {
                _numTimesConfigured = 1 ;
            }
            else
            {
                _numTimesConfigured += 1 ;
            }

            if ( logMethod == null )
            {
                logMethod = DoLogMessage ;
            }

            logMethod (
                       $"[time {_numTimesConfigured.Value}]\t{nameof ( EnsureLoggingConfigured )} called from {callerFilePath}"
                      ) ;


            var fieldInfo2 = LogManager.LogFactory.GetType ( )
                                       .GetField (
                                                  "_config"
                                                , BindingFlags.Instance | BindingFlags.NonPublic
                                                 ) ;

            if ( fieldInfo2 == null )
            {
                System.Diagnostics.Debug.WriteLine (
                                                    "no field _configLoaded for "
                                                    + LogManager.LogFactory
                                                   ) ;
                // throw new Exception ( Resources.AppLoggingConfigHelper_EnsureLoggingConfigured_no_config_loaded_field_found ) ;
            }

            if ( fieldInfo2 != null )
            {
                var config = fieldInfo2.GetValue ( LogManager.LogFactory ) ;

                //LogManager.ThrowConfigExceptions = true;
                //LogManager.ThrowExceptions = true;
                var fieldInfo = LogManager.LogFactory.GetType ( )
                                          .GetField (
                                                     "_configLoaded"
                                                   , BindingFlags.Instance | BindingFlags.NonPublic
                                                    ) ;

                bool configLoaded ;
                if ( fieldInfo == null )
                {
                    configLoaded = config != null ;

                    System.Diagnostics.Debug.WriteLine (
                                                        "no field _configLoaded for "
                                                        + LogManager.LogFactory
                                                       ) ;
                    // throw new Exception ( "no config loaded field found" ) ;
                }
                else
                {
                    configLoaded = ( bool ) fieldInfo.GetValue ( LogManager.LogFactory ) ;
                }

                LoggingIsConfigured = configLoaded ;
                var isMyConfig =
                    ! configLoaded || LogManager.Configuration is CodeConfiguration ;
                var doConfig = ! LoggingIsConfigured || ForceCodeConfig && ! isMyConfig ;
                logMethod (
                           $"{nameof ( LoggingIsConfigured )} = {LoggingIsConfigured}; {nameof ( ForceCodeConfig )} = {ForceCodeConfig}; {nameof ( isMyConfig )} = {isMyConfig});"
                          ) ;
                if ( DumpExistingConfig )
                {
                    void Collect ( string s ) { System.Diagnostics.Debug.WriteLine ( s ) ; }

                    DoDumpConfig ( Collect ) ;
                }

                if ( doConfig )
                {
                    ConfigureLogging ( logMethod ) ;
                    return ;
                }
            }

            DumpPossibleConfig ( LogManager.Configuration ) ;
        }

        private static void DoDumpConfig ( Action < string > collect )
        {
            var config = LogManager.Configuration ;
            if ( config == null )
            {
                return ;
            }

            foreach ( var aTarget in config.AllTargets )
            {
                collect ( aTarget.Name ) ;
                collect ( aTarget.GetType ( ).ToString ( ) ) ;
                if ( aTarget is TargetWithLayout a )
                {
                    if ( a.Layout is JsonLayout jl )
                    {
                        string Selector ( JsonAttribute attribute , int i )
                        {
                            if ( attribute == null )
                            {
                                throw new ArgumentNullException ( nameof ( attribute ) ) ;
                            }

                            var b = new StringBuilder ( ) ;
                            var propertyInfos = attribute
                                               .GetType ( )
                                               .GetProperties (
                                                               BindingFlags.Public
                                                               | BindingFlags.Instance
                                                              ) ;
                            foreach ( var propertyInfo in propertyInfos )
                            {
                                var val2 = propertyInfo.GetValue ( attribute ) ;
                                b.Append ( $"{propertyInfo.Name} = {val2}; " ) ;
                            }

                            return b.ToString ( ) ;
                        }

                        var enumerable = jl.Attributes.Select ( Selector ) ;
                        collect ( string.Join ( "--" , enumerable ) ) ;
                    }
                }

                if ( aTarget is FileTarget gt )
                {
                    collect ( gt.FileName.ToString ( ) ) ;
                }
            }
        }

        private static void DumpPossibleConfig ( LoggingConfiguration configuration )
        {
            var candidateConfigFilePaths = LogManager.LogFactory.GetCandidateConfigFilePaths ( ) ;
            foreach ( var q in candidateConfigFilePaths )
            {
                Debug ( $"{q}" ) ;
            }

            var fieldInfo = configuration.GetType ( )
                                         .GetField (
                                                    "_originalFileName"
                                                  , BindingFlags.NonPublic | BindingFlags.Instance
                                                   ) ;
            if ( fieldInfo != null )
            {
                if ( fieldInfo.GetValue ( configuration ) != null )
                {
                    {
                        Debug ( "Original NLog configuration filename" ) ;
                    }
                }
            }

            Debug ( $"{configuration}" ) ;
        }


        // ReSharper disable once UnusedParameter.Local
#pragma warning disable IDE0060 // Remove unused parameter
        private static void Debug ( string s ) { }
#pragma warning restore IDE0060 // Remove unused parameter

        /// <summary>Adds the supplied target to the current NLog configuration.</summary>
        /// <param name="target">The target.</param>
        /// <param name="minLevel"></param>
        // ReSharper disable once RedundantNameQualifier
        // ReSharper disable once UnusedMember.Global
        public static void AddTarget ( NLog.Targets.Target target , LogLevel minLevel )
        {
            if ( minLevel == null )
            {
                minLevel = LogLevel.Trace ;
            }

            LogManager.Configuration.AddTarget ( target ) ;

            LogManager.Configuration.AddRule ( minLevel , LogLevel.Fatal , target ) ;

            LogManager.LogFactory.ReconfigExistingLoggers ( ) ;
        }

        /// <summary>Removes a target by name from the current NLog configuration.</summary>
        /// <param name="name">The name of the target to remove.</param>
        // ReSharper disable once UnusedMember.Global
        public static void RemoveTarget ( string name )
        {
            LogManager.Configuration.RemoveTarget ( name ) ;
            LogManager.Configuration.LogFactory.ReconfigExistingLoggers ( ) ;
        }

        /// <summary>Set up a <seealso cref="NLog.Layouts.JsonLayout"/> for json loggers.</summary>
        /// <returns>Configured JSON layout</returns>
        public static JsonLayout SetupJsonLayout ( )
        {
            var atts = new[]
                       {
                           Tuple.Create ( "logger" ,    ( string ) null )
                         , Tuple.Create ( "@t" ,        "${longdate}" )
                         , Tuple.Create ( "logger" ,    ( string ) null )
                         , Tuple.Create ( "@mt" ,       "${message}" )
                         , Tuple.Create ( "exception" , "${exception}" )
                       } ;


            var l = new JsonLayout { IncludeAllProperties = true , MaxRecursionLimit = 3 } ;
            l.Attributes.AddRange (
                                   atts.Select (
                                                tuple => new JsonAttribute (
                                                                            tuple.Item1
                                                                          , Layout.FromString (
                                                                                               tuple
                                                                                                  .Item2
                                                                                               ?? $"${{{tuple.Item1}}}"
                                                                                              )
                                                                           )
                                               )
                                  ) ;

            return l ;
        }
    }
}