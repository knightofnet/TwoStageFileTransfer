using System.Text.RegularExpressions;

namespace TwoStageFileTransfer.constant
{
    class AppCst
    {

        public const int BufferSize = 8192;

        public static readonly Regex FilePatternRegex = new Regex(@"(?'name'.+?\..+?)\.(?'size'\d+?)\.part(?'part'\d+)", RegexOptions.Compiled);
        public static readonly Regex FirstFilePatternRegex = new Regex(@"(?'name'.+?\..+?)\.(?'size'\d+?)\.part0", RegexOptions.Compiled);



        public static readonly char[] TrimPathChars = new[] { ' ', '"' };
    }
}
