using System;

namespace Munyabe.CSharp.Analysis.Test.Analyzers.Performance
{
    class AvoidEnumHasFlagAnalyzerTarget
    {
        public bool Method()
        {
            return TestEnum.Hoge.HasFlag(TestEnum.Fuga);
        }
    }

    [Flags]
    enum TestEnum
    {
        Hoge,
        Fuga
    }
}
