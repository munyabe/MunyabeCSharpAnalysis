using System;

namespace Munyabe.CSharp.Analysis.Test.Bases
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
        /// <param name="line">行数</param>
        /// <param name="column">列数</param>
        public DiagnosticResultLocation(int line, int column) : this(string.Empty, line, column)
        {
        }

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

            Path = path;
            Line = line;
            Column = column;
        }
    }
}
