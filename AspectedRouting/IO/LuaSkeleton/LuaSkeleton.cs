using System.Collections.Generic;
using System.IO;
using System.Linq;
using AspectedRouting.Language;

namespace AspectedRouting.IO.LuaSkeleton
{
    /// <summary>
    ///     The 'LuaSkeleton' is a class which is used in Lua generation of profiles.
    ///     The lua skeleton basically keeps track of dependencies, and added functions.
    ///     Once done, all these can be retrieved as code.
    ///     E.g. if an expression is turned into lua with 'ToExpression', then the dependencies will be automatically added.
    /// </summary>
    public partial class LuaSkeleton
    {
        private readonly HashSet<string> _alreadyAddedFunctions = new HashSet<string>();

        private readonly List<string> _constants = new List<string>();
        private readonly Context _context;

        private readonly HashSet<string> _dependencies = new HashSet<string>();
        private readonly List<string> _functionImplementations = new List<string>();

        /// <summary>
        ///     It turns out that creating lua tables is a huge performance overhead.
        ///     Lots of functions however need a constant table to be invoked. Creating this table over and over is performance
        ///     issue.
        ///     If this flag is set, those constant tables are exported so they are created only once
        /// </summary>
        private readonly bool _staticTables;

        public LuaSkeleton(Context context, bool staticTables = false)
        {
            _context = context;
            _staticTables = staticTables;
        }

        internal void AddDep(string name)
        {
            _dependencies.Add(name);
        }

        public List<string> GenerateFunctions()
        {
            return _functionImplementations;
        }

        public bool ContainsFunction(string name)
        {
            return _alreadyAddedFunctions.Contains(name);
        }

        public void AddDependenciesFor(IExpression e)
        {
            var (_, functionNames) = e.InList().DirectlyAndInderectlyCalled(_context);
            foreach (var functionName in functionNames) {
                if (_context.DefinedFunctions.TryGetValue(functionName, out var aspectMeta)) {
                    AddFunction(aspectMeta);
                }
                else {
                    AddDep(functionName);
                }
            }
        }

        public List<string> GenerateDependencies()
        {
            var imps = new List<string>();

            foreach (var name in _dependencies) {
                var path = $"IO/lua/{name}.lua";
                if (File.Exists(path)) {
                    imps.Add(File.ReadAllText(path));
                }
                else {
                    throw new FileNotFoundException(path);
                }
            }

            return imps;
        }

        public string AddConstant(string luaExpression)
        {
            _constants.Add(luaExpression);
            return "c" + (_constants.Count - 1);
        }

        public IEnumerable<string> GenerateConstants()
        {
            return _constants.Select((c, i) => $"c{i} = {c}");
        }
    }
}