using System.Linq;
using System.Text;

namespace Gosu.MsTestRunner.Core.Extensions
{
    public static class StringExtensions
    {
        public static string PrettifyIdentifier(this string identifierName)
        {
            if (string.IsNullOrWhiteSpace(identifierName))
                return identifierName;

            var intermediate = identifierName.Replace("_", " ");

            if (char.IsLower(intermediate[0]))
            {
                var capitalizedFirstLetter = char.ToUpper(intermediate[0]).ToString();

                intermediate = intermediate.Length > 1 ? capitalizedFirstLetter + intermediate.Substring(1) : capitalizedFirstLetter;
            }

            var result = "";

            // Skip the first letter, since that should be upper case without having a space before it
            for (int i = 0; i < intermediate.Length; i++)
            {
                var currentChar = intermediate[i];

                if (char.IsUpper(intermediate[i]) && result.Length > 0)
                {
                    var isFollowedByUpperCaseLetter = intermediate.Length > i + 1 && char.IsUpper(intermediate[i + 1]);
                    var isPreceededByUpperCaseLetter =  i > 0 && char.IsUpper(intermediate[i - 1]);
                    var isStandAloneUpperCaseLetter = !isPreceededByUpperCaseLetter && !isFollowedByUpperCaseLetter;
                    var isFirstUpperCaseLetterInLongerSequence = !isPreceededByUpperCaseLetter && isFollowedByUpperCaseLetter;
                    var isLastUpperCaseLetterInLongerSequence = isPreceededByUpperCaseLetter && !isFollowedByUpperCaseLetter;
                    var isUpperCaseLetterInContinuingSequence = isFollowedByUpperCaseLetter;

                    if (isStandAloneUpperCaseLetter || isLastUpperCaseLetterInLongerSequence || isFirstUpperCaseLetterInLongerSequence)
                        result += " ";

                    if (!isUpperCaseLetterInContinuingSequence)
                        currentChar = char.ToLower(currentChar);
                }

                result += currentChar;
            }

            return result;
        }
    }
}