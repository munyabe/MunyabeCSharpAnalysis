using System;
using Microsoft.CodeAnalysis;

namespace TestHelper
{
    /// <summary>
    /// ソースコードの位置を表す構造体です。
    /// </summary>
    public struct DiagnosticResultLocation
    {
        /// <summary>
        /// 列数を取得します。
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// 行数を取得します。
        /// </summary>
        public int Line { get; }

        /// <summary>
        /// ファイルパスを取得します。
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// インスタンスを初期化します。
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <param name="line">行数</param>
        /// <param name="column">列数</param>
        public DiagnosticResultLocation(string path, int line, int column)
        {
            if (line < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "line must be >= -1");
            }

            if (column < -1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), "column must be >= -1");
            }

            this.Path = path;
            this.Line = line;
            this.Column = column;
        }
    }

    /// <summary>
    /// ソースコードの解析結果を格納する構造体です。
    /// </summary>
    public struct DiagnosticResult
    {
        private DiagnosticResultLocation[] locations;
        /// <summary>
        /// 解析結果が示すソースコードの位置を取得または設定します。
        /// </summary>
        public DiagnosticResultLocation[] Locations
        {
            get
            {
                if (this.locations == null)
                {
                    this.locations = new DiagnosticResultLocation[] { };
                }
                return this.locations;
            }
            set
            {
                this.locations = value;
            }
        }

        /// <summary>
        /// 解析したルールの重大度を取得または設定します。
        /// </summary>
        public DiagnosticSeverity Severity { get; set; }

        /// <summary>
        /// 解析したルールの識別子を取得または設定します。
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 解析結果のメッセージを取得または設定します。
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 解析対象のファイルパスを取得します。
        /// </summary>
        public string Path
        {
            get
            {
                return this.Locations.Length > 0 ? this.Locations[0].Path : "";
            }
        }

        /// <summary>
        /// 解析結果が示すソースコードの列数を取得します。
        /// </summary>
        public int Column
        {
            get
            {
                return this.Locations.Length > 0 ? this.Locations[0].Column : -1;
            }
        }

        /// <summary>
        /// 解析結果が示すソースコードの行数を取得します。
        /// </summary>
        public int Line
        {
            get
            {
                return this.Locations.Length > 0 ? this.Locations[0].Line : -1;
            }
        }
    }
}