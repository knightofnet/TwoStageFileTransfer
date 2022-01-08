using System.Text.RegularExpressions;

namespace TwoStageFileTransferCore.constant
{
    public class AppCst
    {

        public const int BufferSize = 8192;

        public static readonly Regex FilePatternRegex = new Regex(@"(?'name'.+?\..+?)\.(?'size'\d+?)\.part(?'part'\d+)", RegexOptions.Compiled);
        public static readonly Regex FirstFilePatternRegex = new Regex(@"(?'name'.+?\..+?)\.(?'size'\d+?)\.part0", RegexOptions.Compiled);



        public static readonly char[] TrimPathChars = new[] { ' ', '"' };

        public static readonly string PassPhraseCharset =
            "ABCDEFGHJLMNPQRSTUVWXYZabcdefghikmnpqrstuvwxyz0123456789+-*/&.,:!?§%()[]<>";

        public static readonly string DefaultPassPhrase = "DefaultPassphrase";
    }
}
