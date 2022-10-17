using System.Collections.Generic;
using AspectedRouting.Language;
using AspectedRouting.Language.Expression;

namespace AspectedRouting.IO.itinero1
{
    public class LuaParameterPrinter
    {
        private readonly ProfileMetaData _profile;
        private readonly LuaSkeleton.LuaSkeleton _skeleton;

        public LuaParameterPrinter(ProfileMetaData profile, LuaSkeleton.LuaSkeleton skeleton)
        {
            _profile = profile;
            _skeleton = skeleton;
        }


        public string GenerateDefaultParameters()
        {
            var impl = new List<string> {
                "function default_parameters()",
                "    local parameters = {}",
                DeclareParametersFor(_profile.DefaultParameters),
                "    return parameters",
                "end"
            };

            return string.Join("\n", impl);
        }

        /// <summary>
        ///     Generates a piece of code of the following format:
        ///     parameters["x"] = a;
        ///     parameters["y"] = b:
        ///     ...
        ///     Where x=a and y=b are defined in the profile
        ///     Dependencies are added.
        ///     Note that the caller should still add `local paramaters = default_parameters()`
        /// </summary>
        /// <param name="behaviour"></param>
        /// <returns></returns>
        public string DeclareParametersFor(Dictionary<string, IExpression> subParams)
        {
            var impl = "";
            foreach (var (paramName, value) in subParams)
            {
                if (paramName.Equals("description"))
                {
                    continue;
                }

                var paramNameTrimmed = paramName.TrimStart('#').AsLuaIdentifier();
                if (!string.IsNullOrEmpty(paramNameTrimmed))
                {
                    impl += $"    parameters.{paramNameTrimmed} = {_skeleton.ToLua(value)}\n";
                }
            }

            return impl;
        }
    }
}