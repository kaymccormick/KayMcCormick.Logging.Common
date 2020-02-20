using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KayMcCormick.Logging.Common;
using NLog.Layouts;

namespace KayMcCormick.Logging.Common
{
    class RegConfigSource : IConfigSource
    {
        public string PathForLogFile(ILogFileSpecification spec) { return ""; }
    }

    internal interface IConfigSource
    {
        string PathForLogFile(ILogFileSpecification spec);



    }

    internal interface ILogEntrySpecification
    {
        FunctionalArea GetFunctionalArea();
        CrosscuttingConcern GetCrosscuttingConcern();
        ExecutionContext GetExecutionContext();
    }

    public interface ExecutionContext
    {
        Application Application { get; }
    }

    public class ExecutionContextImpl : ExecutionContext
    {
        public Application Application { get; set; }
    }

    public enum Application
    {
        MainApplication,
        Tests,
    }

    internal interface ILogFileSpecification : ILogEntrySpecification
    {
    }

    internal enum CrosscuttingConcern
    {


    }

    internal enum FunctionalArea
    {
        Configuration,
        UserInterface,
        DependencyInjection,
        Model,
        TypeConversion,
        Xaml,



    }
}
