using System;
using System.Collections.Generic;
using System.Linq;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

namespace AspectedRouting
{
    public static class Utils
    {
        public static string Indent(this string s)
        {
            return s.Replace("\n", "\n    ");
        }

        public static List<T> InList<T>(this T t)
        {
            return new List<T> {t};
        }

        public static string Lined(this IEnumerable<string> lines)
        {
            return string.Join("\n", lines);
        }

        public static string Lines(params string[] lines)
        {
            return string.Join("\n", lines);
        }

        public static int Multiply(this IEnumerable<int> ints)
        {
            var factor = 1;
            foreach (var i in ints) {
                factor += i;
            }

            return factor;
        }

        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static T[] SubArray<T>(this T[] data, int index)
        {
            return data.SubArray(index, data.Length - index);
        }

        /// <summary>
        ///     Generates a JSON file where all the profiles are listed, together with descriptions and other metadata.
        ///     Useful for other apps, e.g. the routing api to have
        /// </summary>
        /// <param name="profiles"></param>
        /// <param name="context"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        public static string GenerateExplanationJson(IEnumerable<ProfileMetaData> profiles, Context context)
        {
            var metaItems = new List<string>();

            foreach (var profile in profiles) {
                var profileName = profile.Name;
                var author = profile.Author;
                var profileDescription = profile.Description;


                foreach (var behaviour in profile.Behaviours) {
                    var behaviourDescription = behaviour.Value["description"].Evaluate(new Context()) as string;
                    behaviourDescription ??= "";
                    var keys = string.Join(", ",
                        profile.AllExpressions(context).PossibleTags().Select(tag => $"\"{tag.Key}\"")
                    );
                    var meta = new Dictionary<string, string> {
                        {"name", behaviour.Key},
                        {"type", profileName},
                        {"author", author},
                        {"description", behaviourDescription + " (" + profileDescription + ")"}
                    };

                    var json = string.Join(",", meta.Select(d =>
                        $"\"{d.Key}\": \"{d.Value}\""));
                    metaItems.Add("{" + json + ", \"usedKeys\": [" + keys + "] }\n");
                }
            }

            return "[" + string.Join(",\n", metaItems) + "]";
        }
    }
}