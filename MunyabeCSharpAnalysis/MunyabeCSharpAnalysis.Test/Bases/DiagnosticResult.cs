using Microsoft.CodeAnalysis;

namespace Munyabe.CSharp.Analysis.Test.Bases
{
    /// <summary>
    /// ソースコードの解析結果を格納する構造体です。
    /// </summary>
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] _locations;
        /// <summary>
        /// 解析結果が示すソースコードの位置を取得します。
        /// </summary>
        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (_locations == null)
                {
                    _locations = new DiagnosticResultLocation[] { };
                }
                return _locations;
            }
        }

        /// <summary>
        /// 解析したルールの重大度を取得します。
        /// </summary>
        public DiagnosticSeverity? Severity { get; }

        /// <summary>
        /// 解析したルールの識別子を取得します。
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 解析結果のメッセージを取得します。
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// 解析対象のファイルパスを取得します。
        /// </summary>
        public string Path
        {
            get
            {
                return Locations.Length > 0 ? Locations[0].Path : "";
            }
        }

        /// <summary>
        /// 解析結果が示すソースコードの列数を取得します。
        /// </summary>
        public int Column
        {
            get
            {
                return Locations.Length > 0 ? Locations[0].Column : -1;
            }
        }

        /// <summary>
        /// 解析結果が示すソースコードの行数を取得します。
        /// </summary>
        public int Line
        {
            get
            {
                return Locations.Length > 0 ? Locations[0].Line : -1;
            }
        }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="id">解析したルールの識別子</param>
        /// <param name="locations">解析結果が示すソースコードの位置</param>
        public DiagnosticResult(string id, params DiagnosticResultLocation[] locations)
            : this(id, string.Empty, null, locations)
        {
        }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="id">解析したルールの識別子</param>
        /// <param name="severity">解析したルールの重大度</param>
        /// <param name="locations">解析結果が示すソースコードの位置</param>
        public DiagnosticResult(string id, DiagnosticSeverity severity, params DiagnosticResultLocation[] locations)
            : this(id, string.Empty, severity, locations)
        {
        }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="id">解析したルールの識別子</param>
        /// <param name="message">解析結果のメッセージ</param>
        /// <param name="severity">解析したルールの重大度</param>
        /// <param name="locations">解析結果が示すソースコードの位置</param>
        public DiagnosticResult(string id, string message, DiagnosticSeverity? severity, params DiagnosticResultLocation[] locations)
        {
            Id = id;
            Message = message;
            Severity = severity;
            _locations = locations;
        }
    }
}